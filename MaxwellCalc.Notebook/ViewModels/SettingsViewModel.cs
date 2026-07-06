using Avalonia;
using Avalonia.Styling;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Material.Icons;
using MaxwellCalc.Core.Workspaces;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Windows.Input;

namespace MaxwellCalc.Notebook.ViewModels;

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

/// <summary>
/// One entry in the workspace switcher: a named workspace plus a command that selects it. The command
/// is per-entry (it captures this entry) so the "Physics ▾" menu items can bind
/// <c>Command="{Binding SelectCommand}"</c> directly against their own data context, without reaching
/// across the flyout's popup boundary back to the shell.
/// </summary>
public sealed class WorkspaceEntry(string name, IWorkspace workspace, ICommand selectCommand)
{
    /// <summary>Gets the display name shown in the title caption, the Physics button, and the menu.</summary>
    public string Name { get; } = name;

    /// <summary>Gets the workspace this entry activates.</summary>
    public IWorkspace Workspace { get; } = workspace;

    /// <summary>Gets the command that makes this the active workspace.</summary>
    public ICommand SelectCommand { get; } = selectCommand;
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
    private readonly string _settingsFile;
    private bool _suppressSave;

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
    public SettingsViewModel(WorkspaceState workspaceState, SheetViewModel sheet)
    {
        _workspaceState = workspaceState;
        _sheet = sheet;
        _settingsFile = Path.Combine(Directory.GetCurrentDirectory(), "settings.json");

        // Adopt the workspace WorkspaceState already seeded as the default "Physics" entry (so the panels
        // that attached to it at startup stay valid) and offer a complex-valued workspace alongside it.
        var physics = _workspaceState.Workspace ?? WorkspaceState.CreateDefaultWorkspace();
        Workspaces.Add(MakeEntry("Physics", physics));
        Workspaces.Add(MakeEntry("Complex", WorkspaceState.CreateComplexWorkspace()));

        _suppressSave = true;
        _selectedWorkspace = Workspaces[0];

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
    }

    /// <summary>Makes this the active workspace (a "Physics ▾" menu item).</summary>
    /// <param name="entry">The workspace to activate.</param>
    [RelayCommand]
    private void SelectWorkspace(WorkspaceEntry? entry)
    {
        if (entry is not null)
            SelectedWorkspace = entry;
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
        ApplySelectedWorkspace();
        Save();
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

    // Builds a workspace entry whose SelectCommand captures the entry itself.
    private WorkspaceEntry MakeEntry(string name, IWorkspace workspace)
    {
        WorkspaceEntry entry = null!;
        entry = new WorkspaceEntry(name, workspace, new RelayCommand(() => SelectWorkspace(entry)));
        return entry;
    }

    private void ApplySelectedWorkspace()
    {
        if (SelectedWorkspace is null)
            return;
        _workspaceState.Workspace = SelectedWorkspace.Workspace;
        _workspaceState.Name = SelectedWorkspace.Name;
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

    private static readonly JsonSerializerOptions SaveOptions = new() { WriteIndented = true };

    // The plain, serializable shape of settings.json.
    private sealed class SettingsData
    {
        public bool IsDarkTheme { get; set; }

        public UnitHue UnitHue { get; set; }

        public bool AutoCaptionEnabled { get; set; } = true;

        public string? SelectedWorkspace { get; set; }
    }
}
