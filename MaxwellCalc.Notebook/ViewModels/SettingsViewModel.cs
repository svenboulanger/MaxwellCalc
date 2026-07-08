using Avalonia;
using Avalonia.Styling;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Material.Icons;
using MaxwellCalc.Core.Units;
using MaxwellCalc.Core.Workspaces;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Numerics;
using System.Text.Json;
using System.Windows.Input;

namespace MaxwellCalc.Notebook.ViewModels;

/// <summary>
/// One row in the settings dialog's live preview: a caption and the sample quantity formatted with the
/// edited workspace's current format string.
/// </summary>
/// <param name="Label">The sample caption (e.g. "Avogadro").</param>
/// <param name="Value">The formatted sample quantity, ready for a <c>QuantityView</c>.</param>
public readonly record struct PreviewSample(string Label, Quantity<string> Value);

/// <summary>The selectable unit-hue preset (the color used for unit symbols in the editor and gutter).</summary>
public enum UnitHue
{
    /// <summary>Amber, H=60 (the default).</summary>
    Amber,

    /// <summary>Green, H=150.</summary>
    Green,

    /// <summary>Violet, H=300.</summary>
    Violet,
}

/// <summary>The numeric domain a workspace calculates over, chosen once at creation.</summary>
public enum WorkspaceDomain
{
    /// <summary>Real numbers only (<see cref="System.Double"/>).</summary>
    Double,

    /// <summary>Complex numbers (allows imaginary results).</summary>
    Complex,
}

/// <summary>The scalar display format for a workspace (Step 12): how numeric results are rendered.</summary>
public enum ScalarFormat
{
    /// <summary>General format: plain decimal for mid-range magnitudes, exponent for very large/small.</summary>
    Auto,

    /// <summary>Fixed-point format (always <c>digits</c> decimal places).</summary>
    Fixed,

    /// <summary>Scientific / exponential format.</summary>
    Scientific,
}

/// <summary>Which kind of unit a "Clear units" row targets (Workspace settings → Clear units).</summary>
public enum ClearUnitKind
{
    /// <summary>The input units (symbols resolved to base SI on input).</summary>
    Input,

    /// <summary>The output units (candidates the app auto-picks from for display).</summary>
    Output,
}

/// <summary>
/// One entry in the workspace switcher: a named workspace, its per-workspace display settings (scalar
/// format + significant digits, Step 12), and the per-entry commands the switcher rows bind to. The
/// commands capture this entry so the "Physics ▾" flyout rows can bind
/// <c>Command="{Binding SelectCommand}"</c> directly against their own data context, without reaching
/// across the flyout's popup boundary back to the shell.
/// </summary>
public partial class WorkspaceEntry : ObservableObject
{
    /// <summary>Gets the display name shown in the title caption, the Physics button, and the switcher.</summary>
    [ObservableProperty]
    private string _name;

    /// <summary>Gets or sets whether this is the active workspace (drives the switcher check mark).</summary>
    [ObservableProperty]
    private bool _isSelected;

    /// <summary>Gets or sets the scalar display format.</summary>
    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(FormatString))]
    [NotifyPropertyChangedFor(nameof(IsAuto))]
    [NotifyPropertyChangedFor(nameof(IsFixed))]
    [NotifyPropertyChangedFor(nameof(IsScientific))]
    private ScalarFormat _format;

    /// <summary>Gets or sets the number of significant digits / decimal places (1–12).</summary>
    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(FormatString))]
    private int _digits;

    /// <summary>Gets the workspace this entry activates.</summary>
    public IWorkspace Workspace { get; }

    /// <summary>Gets the command that makes this the active workspace.</summary>
    public ICommand SelectCommand { get; }

    /// <summary>Gets the command that opens this workspace's settings dialog.</summary>
    public ICommand OpenSettingsCommand { get; }

    /// <summary>Gets the command that removes this workspace.</summary>
    public ICommand RemoveCommand { get; }

    /// <summary>
    /// Gets the workspace's numeric-domain label ("Complex" / "Double"), derived from its scalar type.
    /// The domain is fixed at creation, so this never changes for a given entry.
    /// </summary>
    public string DomainLabel =>
        Workspace.ScalarType == typeof(Complex) ? "Complex" : "Double";

    /// <summary>Gets the uppercase form of <see cref="DomainLabel"/> shown in the switcher-row badge.</summary>
    public string DomainBadge => DomainLabel.ToUpperInvariant();

    /// <summary>Gets the switcher-row badge tooltip describing this entry's domain.</summary>
    public string DomainTooltip => Workspace.ScalarType == typeof(Complex)
        ? "Complex — allows imaginary results"
        : "Real / Double precision";

    /// <summary>
    /// Gets the standard .NET numeric format string this entry's setting maps to (handed straight to
    /// <c>double.ToString</c> by the domain formatters).
    /// </summary>
    public string FormatString => Format switch
    {
        ScalarFormat.Fixed => "F" + Digits,        // 0.00012, 273.15000
        ScalarFormat.Scientific => "E" + Digits,   // 2.99792E+008
        _ => "G" + Digits,                          // .NET "general": auto fixed/scientific
    };

    /// <summary>Gets whether the Auto format is selected (drives the segmented control highlight).</summary>
    public bool IsAuto => Format is ScalarFormat.Auto;

    /// <summary>Gets whether the Fixed format is selected.</summary>
    public bool IsFixed => Format is ScalarFormat.Fixed;

    /// <summary>Gets whether the Scientific format is selected.</summary>
    public bool IsScientific => Format is ScalarFormat.Scientific;

    /// <summary>
    /// Creates a new <see cref="WorkspaceEntry"/>.
    /// </summary>
    /// <param name="name">The display name.</param>
    /// <param name="workspace">The workspace this entry activates.</param>
    /// <param name="format">The scalar display format.</param>
    /// <param name="digits">The significant-digit count.</param>
    /// <param name="selectCommand">The command that activates this workspace.</param>
    /// <param name="openSettingsCommand">The command that opens this workspace's settings.</param>
    /// <param name="removeCommand">The command that removes this workspace.</param>
    public WorkspaceEntry(
        string name,
        IWorkspace workspace,
        ScalarFormat format,
        int digits,
        ICommand selectCommand,
        ICommand openSettingsCommand,
        ICommand removeCommand)
    {
        _name = name;
        Workspace = workspace;
        _format = format;
        _digits = digits;
        SelectCommand = selectCommand;
        OpenSettingsCommand = openSettingsCommand;
        RemoveCommand = removeCommand;
    }
}

/// <summary>
/// Owns the user preferences that the access bar and title bar drive (Step 10): the workspace list and
/// selection, the light/dark theme, the unit hue, and the auto-caption toggle. Preferences persist to
/// <c>settings.json</c> in the working directory as a plain record; the workspace <em>contents</em> are
/// persisted separately in Step 11 via Core's converters.
/// </summary>
public partial class SettingsViewModel : ViewModelBase
{
    private readonly WorkspaceState _workspaceState;
    private readonly SheetViewModel _sheet;
    private readonly JsonSerializerOptions _workspaceJsonOptions;
    private readonly string _settingsFile;
    private readonly string _workspaceFile;
    private bool _suppressSave;
    private WorkspaceEntry? _observedEntry;

    /// <summary>Gets the available workspaces (the "Physics ▾" switcher list).</summary>
    public ObservableCollection<WorkspaceEntry> Workspaces { get; } = [];

    /// <summary>Gets or sets the active workspace entry.</summary>
    [ObservableProperty]
    private WorkspaceEntry? _selectedWorkspace;

    /// <summary>Gets or sets whether the dark theme is active.</summary>
    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(ThemeToggleIcon))]
    private bool _isDarkTheme;

    /// <summary>Gets or sets the selected unit hue.</summary>
    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(IsAmber))]
    [NotifyPropertyChangedFor(nameof(IsGreen))]
    [NotifyPropertyChangedFor(nameof(IsViolet))]
    private UnitHue _unitHue;

    /// <summary>Gets or sets whether the auto-selected-output-unit caption is shown (default on).</summary>
    [ObservableProperty]
    private bool _autoCaptionEnabled = true;

    /// <summary>Gets or sets whether the workspace-settings dialog is open.</summary>
    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(CloseWorkspaceSettingsCommand))]
    private bool _workspaceSettingsOpen;

    /// <summary>Gets or sets the workspace entry the settings dialog is editing.</summary>
    [ObservableProperty]
    private WorkspaceEntry? _settingsTarget;

    /// <summary>Gets the sample quantities shown in the settings dialog's live preview.</summary>
    [ObservableProperty]
    private IReadOnlyList<PreviewSample> _preview = [];

    /// <summary>
    /// Gets or sets which "Clear units" row, if any, is armed for confirmation. Only one row can be armed
    /// at a time; set on the first "Clear all" click, cleared on Cancel, on a successful clear, and when
    /// the dialog closes.
    /// </summary>
    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(IsInputClearArmed))]
    [NotifyPropertyChangedFor(nameof(IsOutputClearArmed))]
    private ClearUnitKind? _clearConfirm;

    /// <summary>Gets the number of input units defined in the edited workspace.</summary>
    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(InputUnitCaption))]
    [NotifyPropertyChangedFor(nameof(HasInputUnits))]
    private int _inputUnitCount;

    /// <summary>Gets the number of output units defined in the edited workspace.</summary>
    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(OutputUnitCaption))]
    [NotifyPropertyChangedFor(nameof(HasOutputUnits))]
    private int _outputUnitCount;

    /// <summary>Gets whether the input-units row is armed for confirmation.</summary>
    public bool IsInputClearArmed => ClearConfirm is ClearUnitKind.Input;

    /// <summary>Gets whether the output-units row is armed for confirmation.</summary>
    public bool IsOutputClearArmed => ClearConfirm is ClearUnitKind.Output;

    /// <summary>Gets the "N unit(s) defined" caption under the input-units row.</summary>
    public string InputUnitCaption => $"{InputUnitCount} {(InputUnitCount == 1 ? "unit" : "units")} defined";

    /// <summary>Gets the "N unit(s) defined" caption under the output-units row.</summary>
    public string OutputUnitCaption => $"{OutputUnitCount} {(OutputUnitCount == 1 ? "unit" : "units")} defined";

    /// <summary>Gets whether the edited workspace has any input units (drives the button's enabled state).</summary>
    public bool HasInputUnits => InputUnitCount > 0;

    /// <summary>Gets whether the edited workspace has any output units (drives the button's enabled state).</summary>
    public bool HasOutputUnits => OutputUnitCount > 0;

    /// <summary>Gets or sets whether the "New workspace" dialog is open.</summary>
    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(CloseNewWorkspaceCommand))]
    private bool _newWorkspaceOpen;

    /// <summary>Gets or sets the name typed in the "New workspace" dialog.</summary>
    [ObservableProperty]
    private string _newWorkspaceName = string.Empty;

    /// <summary>Gets or sets the domain chosen in the "New workspace" dialog.</summary>
    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(IsNewDouble))]
    [NotifyPropertyChangedFor(nameof(IsNewComplex))]
    private WorkspaceDomain _newWorkspaceDomain;

    /// <summary>Gets whether the Double domain is selected in the new-workspace dialog.</summary>
    public bool IsNewDouble => NewWorkspaceDomain is WorkspaceDomain.Double;

    /// <summary>Gets whether the Complex domain is selected in the new-workspace dialog.</summary>
    public bool IsNewComplex => NewWorkspaceDomain is WorkspaceDomain.Complex;

    /// <summary>Gets the theme-toggle glyph: a moon in light mode (switch to dark), a sun in dark mode.</summary>
    public MaterialIconKind ThemeToggleIcon =>
        IsDarkTheme ? MaterialIconKind.WhiteBalanceSunny : MaterialIconKind.WeatherNight;

    /// <summary>Gets whether the amber unit hue is selected (drives the swatch highlight).</summary>
    public bool IsAmber => UnitHue is UnitHue.Amber;

    /// <summary>Gets whether the green unit hue is selected.</summary>
    public bool IsGreen => UnitHue is UnitHue.Green;

    /// <summary>Gets whether the violet unit hue is selected.</summary>
    public bool IsViolet => UnitHue is UnitHue.Violet;

    /// <summary>
    /// Creates a new <see cref="SettingsViewModel"/>.
    /// </summary>
    /// <param name="workspaceState">The shared workspace state to drive on selection.</param>
    /// <param name="sheet">The sheet, for pushing the auto-caption toggle.</param>
    /// <param name="workspaceJsonOptions">The JSON options carrying Core's workspace converters, used to
    /// persist and restore the workspace list (Step 11).</param>
    public SettingsViewModel(
        WorkspaceState workspaceState,
        SheetViewModel sheet,
        JsonSerializerOptions workspaceJsonOptions)
    {
        _workspaceState = workspaceState;
        _sheet = sheet;
        _workspaceJsonOptions = workspaceJsonOptions;
        _settingsFile = Path.Combine(Directory.GetCurrentDirectory(), "settings.json");
        _workspaceFile = Path.Combine(Directory.GetCurrentDirectory(), "workspace.json");

        _suppressSave = true;

        // Restore the saved workspace list (Step 11). If there is none, fall back to the default pair:
        // adopt the workspace WorkspaceState already seeded as "Physics" (so the panels that attached to
        // it at startup stay valid) and offer a complex-valued workspace alongside it.
        if (!TryLoadWorkspaces())
        {
            var physics = _workspaceState.Workspace ?? WorkspaceState.CreateDefaultWorkspace();
            Workspaces.Add(MakeEntry("Physics", physics));
            Workspaces.Add(MakeEntry("Complex", WorkspaceState.CreateComplexWorkspace()));
        }

        _selectedWorkspace = Workspaces.Count > 0 ? Workspaces[0] : null;

        // Keep every row's delete command in step with the count (the last workspace can't be removed).
        Workspaces.CollectionChanged += (_, _) => RefreshRemoveCommands();

        // Seed the toggle from the current (system) variant, then let persisted settings override it.
        _isDarkTheme = Application.Current?.ActualThemeVariant == ThemeVariant.Dark;

        Load();
        _suppressSave = false;

        // Apply the settled values to the app and workspace. These are idempotent, so it doesn't matter
        // that Load may already have applied some of them through the property setters.
        ApplySelectedWorkspace();
        ApplyTheme();
        ApplyUnitHue();
        _sheet.AutoCaptionEnabled = AutoCaptionEnabled;

        // Track the settled selection (Load may or may not have gone through the property setter, so we
        // attach here rather than relying on the change handler having fired) and mark the check.
        ObserveSelectedEntry(SelectedWorkspace);
        UpdateSelectedFlags();
    }

    /// <summary>Makes this the active workspace (a "Physics ▾" menu item).</summary>
    /// <param name="entry">The workspace to activate.</param>
    [RelayCommand]
    private void SelectWorkspace(WorkspaceEntry? entry)
    {
        if (entry is not null)
            SelectedWorkspace = entry;
    }

    /// <summary>
    /// Opens the "New workspace" dialog, seeding it with the next free "Untitled N" name and the
    /// default Complex domain. Creation is deferred to <see cref="CreateNewWorkspace"/>.
    /// </summary>
    [RelayCommand]
    private void OpenNewWorkspace()
    {
        NewWorkspaceName = UniqueName("Untitled");
        NewWorkspaceDomain = WorkspaceDomain.Complex;
        NewWorkspaceOpen = true;
    }

    /// <summary>Sets the domain selected in the new-workspace dialog (the Double/Complex control).</summary>
    /// <param name="domain">The domain to select.</param>
    [RelayCommand]
    private void SetNewWorkspaceDomain(WorkspaceDomain domain) => NewWorkspaceDomain = domain;

    /// <summary>
    /// Creates a fresh, seeded workspace against the chosen domain, appends it to the switcher,
    /// activates it, and closes the dialog.
    /// </summary>
    [RelayCommand]
    private void CreateNewWorkspace()
    {
        IWorkspace workspace = NewWorkspaceDomain == WorkspaceDomain.Complex
            ? WorkspaceState.CreateComplexWorkspace()
            : WorkspaceState.CreateDefaultWorkspace();
        string name = string.IsNullOrWhiteSpace(NewWorkspaceName)
            ? UniqueName("Untitled")
            : NewWorkspaceName.Trim();
        var entry = MakeEntry(name, workspace);
        Workspaces.Add(entry);
        SelectedWorkspace = entry;   // fires OnSelectedWorkspaceChanged → applies + saves preferences
        SaveWorkspaces();            // persist the new (empty) workspace into the list immediately
        NewWorkspaceOpen = false;
    }

    /// <summary>Closes the new-workspace dialog without creating (× button, scrim click, Cancel, or Esc).</summary>
    [RelayCommand(CanExecute = nameof(NewWorkspaceOpen))]
    private void CloseNewWorkspace() => NewWorkspaceOpen = false;

    /// <summary>
    /// Removes a workspace, guarding against deleting the last one. If the removed entry was active,
    /// selection falls back to the nearest remaining entry.
    /// </summary>
    /// <param name="entry">The workspace to remove.</param>
    [RelayCommand]
    private void RemoveWorkspace(WorkspaceEntry? entry)
    {
        if (entry is null || Workspaces.Count <= 1)
            return;

        int index = Workspaces.IndexOf(entry);
        if (index < 0)
            return;

        bool wasSelected = SelectedWorkspace == entry;
        if (WorkspaceSettingsOpen && SettingsTarget == entry)
            CloseWorkspaceSettings();

        Workspaces.Remove(entry);
        if (wasSelected)
            SelectedWorkspace = Workspaces[Math.Min(index, Workspaces.Count - 1)];

        SaveWorkspaces();
        Save();
    }

    /// <summary>Opens the settings dialog for a workspace.</summary>
    /// <param name="entry">The workspace to edit.</param>
    [RelayCommand]
    private void OpenWorkspaceSettings(WorkspaceEntry? entry)
    {
        if (entry is null)
            return;

        DetachSettingsTarget();
        SettingsTarget = entry;
        SettingsTarget.PropertyChanged += OnSettingsTargetChanged;
        ClearConfirm = null;
        RefreshPreview();
        RefreshUnitCounts();
        WorkspaceSettingsOpen = true;
    }

    /// <summary>Closes the settings dialog (× button, scrim click, or Esc).</summary>
    [RelayCommand(CanExecute = nameof(WorkspaceSettingsOpen))]
    private void CloseWorkspaceSettings()
    {
        DetachSettingsTarget();
        // Never leave a row armed, so the dialog never reopens mid-confirmation.
        ClearConfirm = null;
        WorkspaceSettingsOpen = false;
        // Persist any name / format / digit edits made in the dialog (edits to a non-active workspace
        // don't route through the active-entry change handler, so save the whole list here).
        SaveWorkspaces();
        Save();
    }

    /// <summary>Sets the edited workspace's scalar format (the Auto/Fixed/Scientific segmented control).</summary>
    /// <param name="format">The format to apply.</param>
    [RelayCommand]
    private void SetTargetFormat(ScalarFormat format)
    {
        if (SettingsTarget is { } target)
            target.Format = format;
    }

    /// <summary>Increases the edited workspace's significant digits (the stepper's +), capped at 12.</summary>
    [RelayCommand]
    private void IncrementDigits()
    {
        if (SettingsTarget is { } target)
            target.Digits = Math.Min(12, target.Digits + 1);
    }

    /// <summary>Decreases the edited workspace's significant digits (the stepper's −), floored at 1.</summary>
    [RelayCommand]
    private void DecrementDigits()
    {
        if (SettingsTarget is { } target)
            target.Digits = Math.Max(1, target.Digits - 1);
    }

    /// <summary>Arms a "Clear units" row for confirmation (the first "Clear all" click).</summary>
    /// <param name="kind">Which row (input or output) to arm.</param>
    [RelayCommand]
    private void ArmClearUnits(ClearUnitKind kind) => ClearConfirm = kind;

    /// <summary>Disarms the armed "Clear units" row without clearing anything (the "Cancel" button).</summary>
    [RelayCommand]
    private void CancelClearUnits() => ClearConfirm = null;

    /// <summary>
    /// Removes every input or output unit from the edited workspace (the confirm "Clear all" button),
    /// then persists and re-renders. Scoped to the edited workspace only; other workspaces are untouched.
    /// </summary>
    /// <param name="kind">Which units to clear.</param>
    [RelayCommand]
    private void ClearUnits(ClearUnitKind kind)
    {
        if (SettingsTarget?.Workspace is { } workspace)
        {
            if (kind == ClearUnitKind.Input)
            {
                // Snapshot the keys first: removal mutates the dictionary we would otherwise enumerate.
                foreach (var key in workspace.InputUnits.Keys.ToList())
                    workspace.TryRemoveInputUnit(key);
            }
            else
            {
                foreach (var key in workspace.OutputUnits.Keys.ToList())
                    workspace.TryRemoveOutputUnit(key);
            }
        }

        ClearConfirm = null;
        AfterUnitsChanged();
    }

    /// <summary>Registers the common physics / general unit bundle into the edited workspace.</summary>
    [RelayCommand]
    private void AddPhysicsUnits()
    {
        switch (SettingsTarget?.Workspace)
        {
            case IWorkspace<double> real:
                real.RegisterCommonUnits();
                break;
            case IWorkspace<Complex> complex:
                complex.RegisterCommonUnits();
                break;
            default:
                return;
        }
        AfterUnitsChanged();
    }

    /// <summary>Registers the common electronics unit bundle (fF, aF, siemens, amp-hours, …).</summary>
    [RelayCommand]
    private void AddElectronicsUnits()
    {
        switch (SettingsTarget?.Workspace)
        {
            case IWorkspace<double> real:
                UnitHelper.RegisterCommonElectronicsUnits(real);
                break;
            case IWorkspace<Complex> complex:
                UnitHelper.RegisterCommonElectronicsUnits(complex);
                break;
            default:
                return;
        }
        AfterUnitsChanged();
    }

    /// <summary>Flips between light and dark (the title-bar theme button).</summary>
    [RelayCommand]
    private void ToggleTheme() => IsDarkTheme = !IsDarkTheme;

    /// <summary>Selects a unit hue (the settings swatches).</summary>
    /// <param name="hue">The hue to select.</param>
    [RelayCommand]
    private void SetUnitHue(UnitHue hue) => UnitHue = hue;

    partial void OnSelectedWorkspaceChanged(WorkspaceEntry? value)
    {
        ObserveSelectedEntry(value);
        UpdateSelectedFlags();
        ApplySelectedWorkspace();
        Save();
    }

    // Subscribes to the active entry so a live rename / format / digit edit re-applies to the workspace
    // state (and re-evaluates the sheet). Idempotent: re-attaching the same entry is a no-op.
    private void ObserveSelectedEntry(WorkspaceEntry? entry)
    {
        if (_observedEntry == entry)
            return;
        if (_observedEntry is not null)
            _observedEntry.PropertyChanged -= OnSelectedEntryChanged;
        _observedEntry = entry;
        if (_observedEntry is not null)
            _observedEntry.PropertyChanged += OnSelectedEntryChanged;
    }

    private void OnSelectedEntryChanged(object? sender, System.ComponentModel.PropertyChangedEventArgs e)
    {
        if (e.PropertyName is nameof(WorkspaceEntry.Name)
            or nameof(WorkspaceEntry.Format)
            or nameof(WorkspaceEntry.Digits))
        {
            // Push the name into the title caption and the format string into WorkspaceState (which
            // re-evaluates the sheet), then persist the change.
            ApplySelectedWorkspace();
            Save();
        }
    }

    private void UpdateSelectedFlags()
    {
        foreach (var entry in Workspaces)
            entry.IsSelected = entry == SelectedWorkspace;
    }

    private void RefreshRemoveCommands()
    {
        foreach (var entry in Workspaces)
            (entry.RemoveCommand as IRelayCommand)?.NotifyCanExecuteChanged();
    }

    partial void OnIsDarkThemeChanged(bool value)
    {
        ApplyTheme();
        // The unit hue is theme-variant-specific, so re-point it for the new variant.
        ApplyUnitHue();
        Save();
    }

    partial void OnUnitHueChanged(UnitHue value)
    {
        ApplyUnitHue();
        Save();
    }

    partial void OnAutoCaptionEnabledChanged(bool value)
    {
        _sheet.AutoCaptionEnabled = value;
        Save();
    }

    // Builds a workspace entry whose per-row commands capture the entry itself, so the switcher flyout
    // rows bind directly to them without crossing the popup boundary back to the shell.
    private WorkspaceEntry MakeEntry(
        string name,
        IWorkspace workspace,
        ScalarFormat format = ScalarFormat.Auto,
        int digits = 5)
    {
        WorkspaceEntry entry = null!;
        entry = new WorkspaceEntry(
            name,
            workspace,
            format,
            digits,
            selectCommand: new RelayCommand(() => SelectWorkspace(entry)),
            openSettingsCommand: new RelayCommand(() => OpenWorkspaceSettings(entry)),
            removeCommand: new RelayCommand(() => RemoveWorkspace(entry), () => Workspaces.Count > 1));
        return entry;
    }

    // Returns a name not already used by any workspace: the base name, else "base 2", "base 3", …
    private string UniqueName(string baseName)
    {
        if (Workspaces.All(w => w.Name != baseName))
            return baseName;
        for (int n = 2; ; n++)
        {
            string candidate = $"{baseName} {n}";
            if (Workspaces.All(w => w.Name != candidate))
                return candidate;
        }
    }

    private void ApplySelectedWorkspace()
    {
        if (SelectedWorkspace is null)
            return;
        _workspaceState.Workspace = SelectedWorkspace.Workspace;
        _workspaceState.Name = SelectedWorkspace.Name;
        _workspaceState.OutputFormat = SelectedWorkspace.FormatString;
    }

    private void DetachSettingsTarget()
    {
        if (SettingsTarget is not null)
            SettingsTarget.PropertyChanged -= OnSettingsTargetChanged;
    }

    private void OnSettingsTargetChanged(object? sender, System.ComponentModel.PropertyChangedEventArgs e)
    {
        if (e.PropertyName is nameof(WorkspaceEntry.FormatString)
            or nameof(WorkspaceEntry.Format)
            or nameof(WorkspaceEntry.Digits))
        {
            RefreshPreview();
        }
    }

    // Rebuilds the live preview from the edited workspace's current format string. The sample scalars
    // are formatted directly (dimensionless) so the preview matches the gutter's scalar rendering.
    private void RefreshPreview()
    {
        if (SettingsTarget is not { } target)
        {
            Preview = [];
            return;
        }

        string format = target.FormatString;
        Preview = PreviewInputs
            .Select(sample => new PreviewSample(
                sample.Label,
                new Quantity<string>(
                    sample.Value.ToString(format, CultureInfo.InvariantCulture),
                    Unit.UnitNone)))
            .ToList();
    }

    // After the edited workspace's units change (a preset added, or a "Clear units" wipe): re-evaluate the
    // sheet (in case the edited workspace is the active one, so output-unit resolution refreshes), update
    // the unit counts and preview, and persist the changed workspace.
    private void AfterUnitsChanged()
    {
        _sheet.Recompute();
        RefreshUnitCounts();
        RefreshPreview();
        SaveWorkspaces();
        Save();
    }

    // Recomputes the input/output unit counts shown in the "Clear units" rows from the edited workspace.
    private void RefreshUnitCounts()
    {
        var workspace = SettingsTarget?.Workspace;
        InputUnitCount = workspace?.InputUnits.Count ?? 0;
        OutputUnitCount = workspace?.OutputUnits.Count ?? 0;
    }

    private void ApplyTheme()
    {
        if (Application.Current is { } app)
            app.RequestedThemeVariant = IsDarkTheme ? ThemeVariant.Dark : ThemeVariant.Light;
    }

    // Repoints the dynamic UnitHueBrush / UnitHueWashBrush at the selected preset for the current theme
    // variant. Application.Resources is consulted before the theme dictionaries in Theme.axaml, so
    // assigning here overrides the defaults defined there; we re-run this on theme changes to keep the
    // (variant-specific) preset in step.
    private void ApplyUnitHue()
    {
        if (Application.Current is not { } app)
            return;

        string suffix = UnitHue switch
        {
            UnitHue.Green => "Green",
            UnitHue.Violet => "Violet",
            _ => "Amber",
        };
        // Pick the variant from our own toggle rather than ActualThemeVariant, so a hue re-point that
        // runs alongside a theme change doesn't race the ActualThemeVariant update.
        var variant = IsDarkTheme ? ThemeVariant.Dark : ThemeVariant.Light;
        if (app.TryGetResource($"UnitHue{suffix}Brush", variant, out var main) && main is not null)
            app.Resources["UnitHueBrush"] = main;
        if (app.TryGetResource($"UnitHue{suffix}WashBrush", variant, out var wash) && wash is not null)
            app.Resources["UnitHueWashBrush"] = wash;
    }

    private void Load()
    {
        try
        {
            if (!File.Exists(_settingsFile))
                return;
            var data = JsonSerializer.Deserialize<SettingsData>(File.ReadAllText(_settingsFile));
            if (data is null)
                return;

            IsDarkTheme = data.IsDarkTheme;
            UnitHue = data.UnitHue;
            AutoCaptionEnabled = data.AutoCaptionEnabled;
            if (data.SelectedWorkspace is { } name
                && Workspaces.FirstOrDefault(w => w.Name == name) is { } entry)
                SelectedWorkspace = entry;
        }
        catch
        {
            // A malformed or unreadable settings file falls back to defaults.
        }
    }

    private void Save()
    {
        if (_suppressSave)
            return;
        try
        {
            var data = new SettingsData
            {
                IsDarkTheme = IsDarkTheme,
                UnitHue = UnitHue,
                AutoCaptionEnabled = AutoCaptionEnabled,
                SelectedWorkspace = SelectedWorkspace?.Name,
            };
            File.WriteAllText(_settingsFile, JsonSerializer.Serialize(data, SaveOptions));
        }
        catch
        {
            // Persistence is best-effort; a failed write must not break the UI.
        }
    }

    /// <summary>
    /// Persists everything this view model owns — the preferences (<c>settings.json</c>) and the
    /// workspace list with its contents (<c>workspace.json</c>). Called from the shell on window close
    /// (Step 11). Preferences also save eagerly on every change; the workspace <em>contents</em> only
    /// persist here, since they mutate through the overlay's direct Core calls.
    /// </summary>
    public void Persist()
    {
        Save();
        SaveWorkspaces();
    }

    // Serializes the workspace list (name + full contents) to workspace.json using Core's converters.
    private void SaveWorkspaces()
    {
        try
        {
            var data = Workspaces
                .Select(entry => new WorkspaceData
                {
                    Name = entry.Name,
                    Workspace = entry.Workspace,
                    Format = entry.Format,
                    Digits = entry.Digits,
                })
                .ToList();
            File.WriteAllText(_workspaceFile, JsonSerializer.Serialize(data, _workspaceJsonOptions));
        }
        catch
        {
            // Persistence is best-effort; a failed write must not break shutdown.
        }
    }

    // Restores the workspace list from workspace.json, returning whether any workspaces were loaded. A
    // missing, empty, or malformed file leaves Workspaces empty so the caller seeds the defaults.
    private bool TryLoadWorkspaces()
    {
        try
        {
            if (!File.Exists(_workspaceFile))
                return false;
            var data = JsonSerializer.Deserialize<List<WorkspaceData>>(
                File.ReadAllText(_workspaceFile), _workspaceJsonOptions);
            if (data is null)
                return false;

            Workspaces.Clear();
            foreach (var item in data)
            {
                if (item.Workspace is null)
                    continue;
                Workspaces.Add(MakeEntry(item.Name, item.Workspace, item.Format, item.Digits));
            }
            return Workspaces.Count > 0;
        }
        catch
        {
            Workspaces.Clear();
            return false;
        }
    }

    private static readonly JsonSerializerOptions SaveOptions = new() { WriteIndented = true };

    // The fixed sample scalars run through the edited workspace's format string for the settings preview:
    // a very large number (auto → scientific), a very small one, and a mid-range value (auto → decimal).
    private static readonly (string Label, double Value)[] PreviewInputs =
    [
        ("Avogadro", 6.02214076e23),
        ("Boltzmann", 1.380649e-23),
        ("small", 273.15),
    ];

    // The serializable shape of one workspace.json entry: its display name and per-workspace display
    // settings, plus the workspace itself, which round-trips through Core's WorkspaceJsonConverter
    // (registered on _workspaceJsonOptions).
    private sealed class WorkspaceData
    {
        public string Name { get; set; } = string.Empty;

        public IWorkspace? Workspace { get; set; }

        public ScalarFormat Format { get; set; } = ScalarFormat.Auto;

        public int Digits { get; set; } = 5;
    }

    // The plain, serializable shape of settings.json.
    private sealed class SettingsData
    {
        public bool IsDarkTheme { get; set; }

        public UnitHue UnitHue { get; set; }

        public bool AutoCaptionEnabled { get; set; } = true;

        public string? SelectedWorkspace { get; set; }
    }
}
