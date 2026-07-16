using CommunityToolkit.Mvvm.ComponentModel;
using MaxwellCalc.Core.Domains;
using MaxwellCalc.Core.Units;
using MaxwellCalc.Core.Workspaces;
using System.Numerics;

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
    /// Gets or sets the numeric format string handed to <see cref="IWorkspace.TryResolveAndFormat"/>.
    /// Driven by the active workspace entry's format/digits setting (Step 12); the sheet re-evaluates
    /// whenever it changes. Defaults to a five-significant-digit "general" format ("Auto", 5 digits).
    /// </summary>
    [ObservableProperty]
    private string _outputFormat = "G5";

    /// <summary>
    /// Creates a new <see cref="WorkspaceState"/> with a default real-valued workspace, seeded with
    /// the common units, constants, and built-in functions so the sheet has something to compute
    /// against. (Step 10 moves workspace creation into the settings view model.)
    /// </summary>
    public WorkspaceState()
    {
        Workspace = CreateDefaultWorkspace();
    }

    /// <summary>
    /// Creates a fresh real-valued workspace populated with the common units, constants, and
    /// built-in math functions.
    /// </summary>
    /// <returns>Returns the seeded workspace.</returns>
    public static Workspace<double> CreateDefaultWorkspace()
    {
        var workspace = new Workspace<double>(new DoubleDomain()) { AnswerVariable = "ans" };
        workspace.RegisterCommonPhysicsUnits();
        workspace.RegisterConstants(typeof(DoubleMathHelper));
        workspace.RegisterBuiltInMethods(typeof(DoubleMathHelper));
        return workspace;
    }

    /// <summary>
    /// Creates a fresh complex-valued workspace populated with the common units, constants, and
    /// built-in math functions. Used by the settings view model's workspace list (Step 10).
    /// </summary>
    /// <returns>Returns the seeded workspace.</returns>
    public static Workspace<Complex> CreateComplexWorkspace()
    {
        var workspace = new Workspace<Complex>(new ComplexDomain()) { AnswerVariable = "ans" };
        workspace.RegisterCommonPhysicsUnits();
        workspace.RegisterConstants(typeof(ComplexMathHelper));
        workspace.RegisterBuiltInMethods(typeof(ComplexMathHelper));
        return workspace;
    }
}
