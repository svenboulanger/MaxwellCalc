using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using MaxwellCalc.Core.Dictionaries;
using MaxwellCalc.Core.Parsers;
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
/// The footer (Step 9) adds an output unit; rows remove one.
/// </summary>
public sealed partial class OutputUnitsPanelViewModel : FilteredPanelViewModel<OutputUnitItem, OutputUnitKey, string>
{
    /// <summary>Gets the filtered rows grouped by physical quantity, for the view.</summary>
    public ObservableCollection<OutputUnitGroup> Groups { get; } = [];

    /// <summary>Gets or sets the footer's unit field.</summary>
    [ObservableProperty]
    private string _newUnit = string.Empty;

    /// <summary>Gets or sets the footer's definition field (value in base units).</summary>
    [ObservableProperty]
    private string _newDefinition = string.Empty;

    /// <summary>Gets or sets the inline error shown when an add fails.</summary>
    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(HasError))]
    private string? _errorMessage;

    /// <summary>Gets whether an inline error is showing.</summary>
    public bool HasError => !string.IsNullOrEmpty(ErrorMessage);

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
        => new() { Key = key, Label = key.OutputUnit.ToString(), Definition = new Quantity<string>(value, key.BaseUnit) };

    /// <summary>
    /// Adds an output unit from the footer fields: the unit label and its value in base units, parsed
    /// under a restricted workspace then assigned via <c>TryAssignOutputUnit</c>, mirroring the old
    /// <c>OutputUnitsViewModel.AddOutputUnit</c> flow.
    /// </summary>
    [RelayCommand]
    private void Add()
    {
        if (string.IsNullOrWhiteSpace(NewUnit) || string.IsNullOrWhiteSpace(NewDefinition))
            return;

        ErrorMessage = RunWithDiagnostics(workspace =>
        {
            var oldState = workspace.Restrict(false, false, true, false, false);
            try
            {
                var unitNode = Parser.Parse(new Lexer(NewUnit), workspace);
                if (unitNode is null)
                    return false;
                var valueNode = Parser.Parse(new Lexer(NewDefinition), workspace);
                return valueNode is not null && workspace.TryAssignOutputUnit(unitNode, valueNode);
            }
            finally
            {
                workspace.Restore(oldState);
            }
        });

        if (ErrorMessage is null)
        {
            NewUnit = string.Empty;
            NewDefinition = string.Empty;
        }
    }

    /// <summary>Removes an output unit (the row × button).</summary>
    /// <param name="item">The row to remove.</param>
    [RelayCommand]
    private void Remove(OutputUnitItem item)
        => WorkspaceState.Workspace?.TryRemoveOutputUnit(item.Key);

    /// <inheritdoc />
    protected override bool Matches(OutputUnitItem item, string filter)
        => item.Label.Contains(filter, StringComparison.OrdinalIgnoreCase)
        || item.Definition.Unit.ToString().Contains(filter, StringComparison.OrdinalIgnoreCase);

    /// <inheritdoc />
    protected override int Compare(OutputUnitItem a, OutputUnitItem b)
    {
        // Order by physical-quantity group first, then by unit label, so grouping is contiguous.
        int byGroup = StringComparer.OrdinalIgnoreCase.Compare(
            CategoryLabel(a.Definition.Unit),
            CategoryLabel(b.Definition.Unit));
        if (byGroup != 0)
            return byGroup;
        return StringComparer.OrdinalIgnoreCase.Compare(a.Label, b.Label);
    }

    /// <inheritdoc />
    protected override void OnItemsChanged()
    {
        Groups.Clear();
        foreach (var group in Items.GroupBy(item => CategoryLabel(item.Definition.Unit)))
            Groups.Add(new OutputUnitGroup { Label = group.Key, Items = group.ToList() });
    }

    /// <summary>
    /// Gets the display label for the physical quantity a base unit represents, used to group the output
    /// units. Looks the base unit up in the active workspace's <see cref="IWorkspace.UnitCategories"/>
    /// (upper-casing the stored lower-case category), falling back to the base unit's own string so every
    /// output unit still lands in a labelled group.
    /// </summary>
    /// <param name="baseUnit">The base unit.</param>
    /// <returns>The category label, or the base unit's string when it has no category.</returns>
    private string CategoryLabel(Unit baseUnit)
    {
        var categories = WorkspaceState.Workspace?.UnitCategories;
        if (categories is not null && categories.TryGetValue(baseUnit, out var category))
            return category.ToUpperInvariant();
        string key = baseUnit.ToString();
        return key.Length == 0 ? "Dimensionless" : key;
    }
}
