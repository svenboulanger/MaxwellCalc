using CommunityToolkit.Mvvm.ComponentModel;

namespace MaxwellCalc.Notebook.ViewModels;

/// <summary>
/// The top-level view model for the notebook window. Owns the shared workspace state and
/// (in later steps) the sheet and command-palette view models.
/// </summary>
public partial class ShellViewModel : ViewModelBase
{
    /// <summary>
    /// Gets the shared workspace state.
    /// </summary>
    public WorkspaceState WorkspaceState { get; }

    /// <summary>
    /// Gets the notebook sheet view model.
    /// </summary>
    public SheetViewModel Sheet { get; }

    /// <summary>
    /// Gets the command-palette overlay view model (⌘K / chip clicks).
    /// </summary>
    public CommandPaletteViewModel CommandPalette { get; }

    /// <summary>
    /// Gets the name of the active workspace, for the title caption and Physics button.
    /// </summary>
    public string WorkspaceName => WorkspaceState.WorkspaceName;

    /// <summary>Gets the Variables chip count. Placeholder until wired to the panel VM in Step 10.</summary>
    public string VariablesCount => string.Empty;

    /// <summary>Gets the Units chip count. Placeholder until wired to the panel VM in Step 10.</summary>
    public string UnitsCount => string.Empty;

    /// <summary>Gets the Functions chip count. Placeholder until wired to the panel VM in Step 10.</summary>
    public string FunctionsCount => string.Empty;

    /// <summary>
    /// Creates a new <see cref="ShellViewModel"/>.
    /// </summary>
    /// <param name="workspaceState">The shared workspace state.</param>
    /// <param name="sheet">The notebook sheet view model.</param>
    /// <param name="commandPalette">The command-palette overlay view model.</param>
    public ShellViewModel(WorkspaceState workspaceState, SheetViewModel sheet, CommandPaletteViewModel commandPalette)
    {
        WorkspaceState = workspaceState;
        Sheet = sheet;
        CommandPalette = commandPalette;
        WorkspaceState.PropertyChanged += (_, e) =>
        {
            if (e.PropertyName is nameof(WorkspaceState.WorkspaceName))
                OnPropertyChanged(nameof(WorkspaceName));
        };
    }
}
