using CommunityToolkit.Mvvm.ComponentModel;
using MaxwellCalc.Core.Domains;
using MaxwellCalc.Core.Workspaces;

namespace MaxwellCalc.Notebook.ViewModels;

/// <summary>
/// Holds the currently active workspace and its formatting options, and notifies listeners
/// (the sheet, the overlay panels) when the active workspace changes.
/// This is the single shared source of truth for "which workspace am I calculating against".
/// </summary>
public partial class WorkspaceState : ViewModelBase
{
    /// <summary>
    /// Gets or sets the active workspace.
    /// </summary>
    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(WorkspaceName))]
    private IWorkspace? _workspace;

    /// <summary>
    /// Gets or sets the display name of the active workspace.
    /// </summary>
    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(WorkspaceName))]
    private string _name = "Workspace";

    /// <summary>
    /// Gets the name shown in the title caption and the Physics button.
    /// </summary>
    public string WorkspaceName => Name;

    /// <summary>
    /// Gets the numeric format string handed to <see cref="IWorkspace.TryResolveAndFormat"/>.
    /// </summary>
    public string OutputFormat => "g12";

    /// <summary>
    /// Creates a new <see cref="WorkspaceState"/> with a default real-valued workspace.
    /// </summary>
    public WorkspaceState()
    {
        Workspace = new Workspace<double>(new DoubleDomain()) { AnswerVariable = "ans" };
    }
}
