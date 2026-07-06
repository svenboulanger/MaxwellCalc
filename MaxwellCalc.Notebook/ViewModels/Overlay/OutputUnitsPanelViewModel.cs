using MaxwellCalc.Core.Dictionaries;
using MaxwellCalc.Core.Units;
using MaxwellCalc.Core.Workspaces;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

namespace MaxwellCalc.Notebook.ViewModels.Overlay;

/// <summary>
/// The command palette's Output-units panel: the candidates the app auto-picks from, grouped by physical
/// quantity (Length, Mass, …). Reads <c>workspace.OutputUnits</c>; <see cref="Items"/> holds the flat,
/// filtered rows and <see cref="Groups"/> the same rows arranged into labelled sections for the view.
/// </summary>
public sealed class OutputUnitsPanelViewModel : FilteredPanelViewModel<OutputUnitItem, OutputUnitKey, string>
{
    /// <summary>Gets the filtered rows grouped by physical quantity, for the view.</summary>
    public ObservableCollection<OutputUnitGroup> Groups { get; } = [];

    /// <summary>
    /// Creates a new <see cref="OutputUnitsPanelViewModel"/>.
    /// </summary>
    /// <param name="workspaceState">The shared workspace state.</param>
    public OutputUnitsPanelViewModel(WorkspaceState workspaceState)
        : base(workspaceState)
    {
    }

    /// <inheritdoc />
    protected override IReadOnlyObservableDictionary<OutputUnitKey, string>? GetDictionary(IWorkspace workspace)
        => workspace.OutputUnits;

    /// <inheritdoc />
    protected override OutputUnitItem Project(OutputUnitKey key, string value)
        => new() { Label = key.OutputUnit.ToString(), Definition = new Quantity<string>(value, key.BaseUnit) };

    /// <inheritdoc />
    protected override bool Matches(OutputUnitItem item, string filter)
        => item.Label.Contains(filter, StringComparison.OrdinalIgnoreCase)
        || item.Definition.Unit.ToString().Contains(filter, StringComparison.OrdinalIgnoreCase);

    /// <inheritdoc />
    protected override int Compare(OutputUnitItem a, OutputUnitItem b)
    {
        // Order by physical-quantity group first, then by unit label, so grouping is contiguous.
        int byGroup = StringComparer.OrdinalIgnoreCase.Compare(
            PhysicalQuantity.Label(a.Definition.Unit),
            PhysicalQuantity.Label(b.Definition.Unit));
        if (byGroup != 0)
            return byGroup;
        return StringComparer.OrdinalIgnoreCase.Compare(a.Label, b.Label);
    }

    /// <inheritdoc />
    protected override void OnItemsChanged()
    {
        Groups.Clear();
        foreach (var group in Items.GroupBy(item => PhysicalQuantity.Label(item.Definition.Unit)))
            Groups.Add(new OutputUnitGroup { Label = group.Key, Items = group.ToList() });
    }
}
