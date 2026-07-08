using CommunityToolkit.Mvvm.Input;
using System.Windows.Input;

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
    /// Gets the Saved Sheets command palette (⌘O / the "Sheets" chip): the named line-snapshot library.
    /// </summary>
    public SavedSheetsViewModel SavedSheets { get; }

    /// <summary>
    /// Gets the user preferences (theme, unit hue, auto-caption) and workspace switcher.
    /// </summary>
    public SettingsViewModel Settings { get; }

    /// <summary>
    /// Gets the name of the active workspace, for the title caption and Physics button.
    /// </summary>
    public string WorkspaceName => WorkspaceState.WorkspaceName;

    /// <summary>
    /// Creates a new <see cref="ShellViewModel"/>.
    /// </summary>
    /// <param name="workspaceState">The shared workspace state.</param>
    /// <param name="sheet">The notebook sheet view model.</param>
    /// <param name="commandPalette">The command-palette overlay view model.</param>
    /// <param name="savedSheets">The Saved Sheets command palette.</param>
    /// <param name="settings">The user preferences and workspace switcher.</param>
    public ShellViewModel(
        WorkspaceState workspaceState,
        SheetViewModel sheet,
        CommandPaletteViewModel commandPalette,
        SavedSheetsViewModel savedSheets,
        SettingsViewModel settings)
    {
        WorkspaceState = workspaceState;
        Sheet = sheet;
        CommandPalette = commandPalette;
        SavedSheets = savedSheets;
        Settings = settings;
        WorkspaceState.PropertyChanged += (_, e) =>
        {
            if (e.PropertyName is nameof(WorkspaceState.WorkspaceName))
                OnPropertyChanged(nameof(WorkspaceName));
        };
    }

    /// <summary>
    /// Clears the current sheet back to a single empty, focused line (the "Clear sheet" access-bar chip
    /// and ⌘N / Ctrl+N). Every open overlay is closed first — the command palette, the Saved Sheets
    /// palette, and the workspace-settings / new-workspace dialogs — so the user lands directly on the
    /// fresh sheet. The action is friction-free: no confirmation, no undo prompt. The workspace and the
    /// Saved Sheets library are untouched.
    /// </summary>
    [RelayCommand]
    private void ClearSheet()
    {
        Close(CommandPalette.CloseCommand);
        Close(SavedSheets.CloseCommand);
        Close(Settings.CloseWorkspaceSettingsCommand);
        Close(Settings.CloseNewWorkspaceCommand);

        Sheet.ClearSheet();

        // Each overlay's close is guarded (CanExecute is false when it isn't open), so closing them all
        // unconditionally only dismisses the ones actually showing.
        static void Close(ICommand command)
        {
            if (command.CanExecute(null))
                command.Execute(null);
        }
    }

    /// <summary>
    /// Persists the session on shutdown (Step 11): the sheet's line texts, the workspace list and its
    /// contents, and the user preferences. Called from <c>ShellWindow</c>'s <c>Closing</c> handler.
    /// </summary>
    public void Save()
    {
        Sheet.Save();
        Settings.Persist();
    }
}
