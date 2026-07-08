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

        // Only one overlay may be open at a time. The command palettes (⌘K / ⌘O) are opened by
        // window-level key bindings that fire even while another overlay's scrim is already up, so
        // without this a second palette would stack on the first. Whenever any overlay reports that
        // it has opened, close the others (the just-opened one is exempted so it stays open). This
        // covers every entry point — hotkeys, access-bar chips, and the workspace flyout — without
        // any binding needing to know about the others.
        CommandPalette.PropertyChanged += (_, e) =>
        {
            if (e.PropertyName is nameof(CommandPaletteViewModel.PaletteOpen) && CommandPalette.PaletteOpen)
                CloseOverlays(except: CommandPalette.CloseCommand);
        };
        SavedSheets.PropertyChanged += (_, e) =>
        {
            if (e.PropertyName is nameof(SavedSheetsViewModel.PaletteOpen) && SavedSheets.PaletteOpen)
                CloseOverlays(except: SavedSheets.CloseCommand);
        };
        Settings.PropertyChanged += (_, e) =>
        {
            if (e.PropertyName is nameof(SettingsViewModel.WorkspaceSettingsOpen) && Settings.WorkspaceSettingsOpen)
                CloseOverlays(except: Settings.CloseWorkspaceSettingsCommand);
            else if (e.PropertyName is nameof(SettingsViewModel.NewWorkspaceOpen) && Settings.NewWorkspaceOpen)
                CloseOverlays(except: Settings.CloseNewWorkspaceCommand);
        };
    }

    /// <summary>
    /// Closes every open overlay except the one whose close command is passed as <paramref name="except"/>
    /// (pass <c>null</c> to close them all). Each overlay's close is guarded by its own <c>CanExecute</c>,
    /// so overlays that aren't showing are left untouched; closing an already-closed overlay is a no-op and
    /// raises no further open-state change, so this doesn't re-enter.
    /// </summary>
    private void CloseOverlays(ICommand? except = null)
    {
        Close(CommandPalette.CloseCommand);
        Close(SavedSheets.CloseCommand);
        Close(Settings.CloseWorkspaceSettingsCommand);
        Close(Settings.CloseNewWorkspaceCommand);

        void Close(ICommand command)
        {
            if (!ReferenceEquals(command, except) && command.CanExecute(null))
                command.Execute(null);
        }
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
        CloseOverlays();
        Sheet.ClearSheet();
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
