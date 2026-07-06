using MaxwellCalc.Core.Dictionaries;
using MaxwellCalc.Core.Workspaces;
using MaxwellCalc.Core.Workspaces.Variables;
using System;

namespace MaxwellCalc.Notebook.ViewModels.Overlay;

/// <summary>
/// The command palette's Constants panel: the workspace's fixed constants. Read-only — constants can't
/// be reassigned or removed. Reads <c>workspace.Constants.Local</c>.
/// </summary>
public sealed class ConstantsPanelViewModel : FilteredPanelViewModel<ConstantItem, string, Variable<string>>
{
    /// <summary>
    /// Creates a new <see cref="ConstantsPanelViewModel"/>.
    /// </summary>
    /// <param name="workspaceState">The shared workspace state.</param>
    public ConstantsPanelViewModel(WorkspaceState workspaceState)
        : base(workspaceState)
    {
    }

    /// <inheritdoc />
    protected override IReadOnlyObservableDictionary<string, Variable<string>>? GetDictionary(IWorkspace workspace)
        => workspace.Constants.Local;

    /// <inheritdoc />
    protected override ConstantItem Project(string key, Variable<string> value)
        => new() { Name = key, Description = value.Description, Value = value.Value };

    /// <inheritdoc />
    protected override bool Matches(ConstantItem item, string filter)
        => item.Name.Contains(filter, StringComparison.OrdinalIgnoreCase)
        || (item.Description?.Contains(filter, StringComparison.OrdinalIgnoreCase) ?? false);

    /// <inheritdoc />
    protected override int Compare(ConstantItem a, ConstantItem b)
        => StringComparer.OrdinalIgnoreCase.Compare(a.Name, b.Name);
}
