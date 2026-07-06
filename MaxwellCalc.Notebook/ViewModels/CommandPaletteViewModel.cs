using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using MaxwellCalc.Notebook.ViewModels.Overlay;

namespace MaxwellCalc.Notebook.ViewModels;

/// <summary>The section the command palette is showing.</summary>
public enum PaletteSection
{
    /// <summary>Variables and constants.</summary>
    Variables,

    /// <summary>Input and output units.</summary>
    Units,

    /// <summary>User and built-in functions.</summary>
    Functions,
}

/// <summary>The active tab within the Units section.</summary>
public enum UnitTab
{
    /// <summary>Input units.</summary>
    Input,

    /// <summary>Output units.</summary>
    Output,
}

/// <summary>
/// Hosts the command palette overlay (Step 8): the open/closed state, the active section and unit tab,
/// and a single <see cref="Search"/> string pushed into every hosted panel's <c>Filter</c>. ⌘K/Ctrl+K
/// opens it on the Units section; the access-bar chips open it pre-scoped to their section; Esc or a
/// scrim click closes it. Add/remove flows arrive in Step 9.
/// </summary>
public partial class CommandPaletteViewModel : ViewModelBase
{
    /// <summary>Gets whether the overlay is open.</summary>
    [ObservableProperty]
    private bool _paletteOpen;

    /// <summary>Gets the active section.</summary>
    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(IsVariables))]
    [NotifyPropertyChangedFor(nameof(IsUnits))]
    [NotifyPropertyChangedFor(nameof(IsFunctions))]
    [NotifyPropertyChangedFor(nameof(SectionTitle))]
    private PaletteSection _section;

    /// <summary>Gets the active tab within the Units section.</summary>
    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(IsInputTab))]
    [NotifyPropertyChangedFor(nameof(IsOutputTab))]
    private UnitTab _unitTab;

    /// <summary>Gets the live search string, mirrored into every panel's filter.</summary>
    [ObservableProperty]
    private string _search = string.Empty;

    /// <summary>Gets the Variables panel.</summary>
    public VariablesPanelViewModel Variables { get; }

    /// <summary>Gets the Constants panel.</summary>
    public ConstantsPanelViewModel Constants { get; }

    /// <summary>Gets the Input-units panel.</summary>
    public InputUnitsPanelViewModel InputUnits { get; }

    /// <summary>Gets the Output-units panel.</summary>
    public OutputUnitsPanelViewModel OutputUnits { get; }

    /// <summary>Gets the user Functions panel.</summary>
    public FunctionsPanelViewModel Functions { get; }

    /// <summary>Gets the Built-in functions panel.</summary>
    public BuiltInFunctionsPanelViewModel BuiltInFunctions { get; }

    /// <summary>Gets whether the Variables section is active.</summary>
    public bool IsVariables => Section is PaletteSection.Variables;

    /// <summary>Gets whether the Units section is active.</summary>
    public bool IsUnits => Section is PaletteSection.Units;

    /// <summary>Gets whether the Functions section is active.</summary>
    public bool IsFunctions => Section is PaletteSection.Functions;

    /// <summary>Gets whether the Input-units tab is active.</summary>
    public bool IsInputTab => UnitTab is UnitTab.Input;

    /// <summary>Gets whether the Output-units tab is active.</summary>
    public bool IsOutputTab => UnitTab is UnitTab.Output;

    /// <summary>Gets the right-aligned section title shown in the header.</summary>
    public string SectionTitle => Section switch
    {
        PaletteSection.Variables => "Variables & constants",
        PaletteSection.Units => "Units",
        PaletteSection.Functions => "Functions",
        _ => string.Empty,
    };

    /// <summary>
    /// Creates a new <see cref="CommandPaletteViewModel"/>.
    /// </summary>
    public CommandPaletteViewModel(
        VariablesPanelViewModel variables,
        ConstantsPanelViewModel constants,
        InputUnitsPanelViewModel inputUnits,
        OutputUnitsPanelViewModel outputUnits,
        FunctionsPanelViewModel functions,
        BuiltInFunctionsPanelViewModel builtInFunctions)
    {
        Variables = variables;
        Constants = constants;
        InputUnits = inputUnits;
        OutputUnits = outputUnits;
        Functions = functions;
        BuiltInFunctions = builtInFunctions;
    }

    // The search box drives one shared string; mirror it into every panel so the visible section
    // filters live and the others are already filtered when the user switches to them.
    partial void OnSearchChanged(string value)
    {
        Variables.Filter = value;
        Constants.Filter = value;
        InputUnits.Filter = value;
        OutputUnits.Filter = value;
        Functions.Filter = value;
        BuiltInFunctions.Filter = value;
    }

    // Switching section or unit tab clears the search, matching the prototype.
    partial void OnSectionChanged(PaletteSection value) => Search = string.Empty;

    partial void OnUnitTabChanged(UnitTab value) => Search = string.Empty;

    /// <summary>Opens the overlay on the Variables section (the Variables chip).</summary>
    [RelayCommand]
    private void OpenVariables() => Open(PaletteSection.Variables);

    /// <summary>Opens the overlay on the Units section (the Units chip and ⌘K default).</summary>
    [RelayCommand]
    private void OpenUnits() => Open(PaletteSection.Units);

    /// <summary>Opens the overlay on the Functions section (the Functions chip).</summary>
    [RelayCommand]
    private void OpenFunctions() => Open(PaletteSection.Functions);

    /// <summary>Selects the Input-units tab.</summary>
    [RelayCommand]
    private void ShowInputUnits() => UnitTab = UnitTab.Input;

    /// <summary>Selects the Output-units tab.</summary>
    [RelayCommand]
    private void ShowOutputUnits() => UnitTab = UnitTab.Output;

    /// <summary>Closes the overlay (Esc or scrim click).</summary>
    [RelayCommand]
    private void Close() => PaletteOpen = false;

    // Opens the overlay pre-scoped to a section, with the search cleared.
    private void Open(PaletteSection section)
    {
        Search = string.Empty;
        Section = section;
        PaletteOpen = true;
    }
}
