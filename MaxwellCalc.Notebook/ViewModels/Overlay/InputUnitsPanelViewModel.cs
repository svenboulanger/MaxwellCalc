using MaxwellCalc.Core.Dictionaries;
using MaxwellCalc.Core.Units;
using MaxwellCalc.Core.Workspaces;
using System;

namespace MaxwellCalc.Notebook.ViewModels.Overlay;

/// <summary>
/// The command palette's Input-units panel: the symbols that resolve to base SI on input. Reads
/// <c>workspace.InputUnits</c>.
/// </summary>
public sealed class InputUnitsPanelViewModel : FilteredPanelViewModel<InputUnitItem, string, Quantity<string>>
{
    /// <summary>
    /// Creates a new <see cref="InputUnitsPanelViewModel"/>.
    /// </summary>
    /// <param name="workspaceState">The shared workspace state.</param>
    public InputUnitsPanelViewModel(WorkspaceState workspaceState)
        : base(workspaceState)
    {
    }

    /// <inheritdoc />
    protected override IReadOnlyObservableDictionary<string, Quantity<string>>? GetDictionary(IWorkspace workspace)
        => workspace.InputUnits;

    /// <inheritdoc />
    protected override InputUnitItem Project(string key, Quantity<string> value)
        => new() { Symbol = key, Definition = value };

    /// <inheritdoc />
    protected override bool Matches(InputUnitItem item, string filter)
        => item.Symbol.Contains(filter, StringComparison.OrdinalIgnoreCase)
        || (item.Definition.Scalar?.Contains(filter, StringComparison.OrdinalIgnoreCase) ?? false)
        || item.Definition.Unit.ToString().Contains(filter, StringComparison.OrdinalIgnoreCase);

    /// <inheritdoc />
    protected override int Compare(InputUnitItem a, InputUnitItem b)
        => StringComparer.OrdinalIgnoreCase.Compare(a.Symbol, b.Symbol);
}
