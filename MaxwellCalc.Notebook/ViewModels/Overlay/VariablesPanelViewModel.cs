using CommunityToolkit.Mvvm.Input;
using MaxwellCalc.Core.Dictionaries;
using MaxwellCalc.Core.Workspaces;
using MaxwellCalc.Core.Workspaces.Variables;
using System;
using System.Collections.Generic;
using System.Linq;

namespace MaxwellCalc.Notebook.ViewModels.Overlay;

/// <summary>
/// The command palette's Variables panel: the workspace's variables (e.g. the answer variable and any
/// loaded from persistence) plus the read-only <c>from sheet</c> variables assigned on the sheet this
/// session. Reads <c>workspace.Variables.Local</c>. Variables are defined on the sheet (<c>x = …</c>),
/// not added here — the footer adds constants instead (see <see cref="ConstantsPanelViewModel"/>). Rows
/// remove workspace variables; <c>from sheet</c> rows are read-only.
/// </summary>
public sealed partial class VariablesPanelViewModel : FilteredPanelViewModel<VariableItem, string, Variable<string>>
{
    private readonly SheetViewModel _sheet;

    /// <summary>
    /// Creates a new <see cref="VariablesPanelViewModel"/>.
    /// </summary>
    /// <param name="workspaceState">The shared workspace state.</param>
    /// <param name="sheet">The sheet, for the <c>from sheet</c> variables.</param>
    public VariablesPanelViewModel(WorkspaceState workspaceState, SheetViewModel sheet)
        : base(workspaceState)
    {
        _sheet = sheet;
        _sheet.SheetSymbolsChanged += ScheduleRebuild;
        // The sheet has already evaluated once by now; fold in whatever it defined.
        ScheduleRebuild();
    }

    /// <summary>Removes a workspace variable (the row × button). <c>from sheet</c> rows have no button.</summary>
    /// <param name="item">The row to remove.</param>
    [RelayCommand]
    private void Remove(VariableItem item)
        => WorkspaceState.Workspace?.Variables.TryRemoveVariable(item.Name);

    /// <inheritdoc />
    protected override IReadOnlyObservableDictionary<string, Variable<string>>? GetDictionary(IWorkspace workspace)
        => workspace.Variables.Local;

    /// <inheritdoc />
    protected override VariableItem Project(string key, Variable<string> value)
        => new() { Name = key, Value = value.Value, CanRemove = true, FromSheet = false };

    /// <inheritdoc />
    protected override IEnumerable<VariableItem> ExtraItems()
        // Null-safe: the base constructor runs its first Rebuild (which calls this) before this
        // derived constructor assigns _sheet; the constructor's own ScheduleRebuild folds the sheet
        // variables in immediately afterwards.
        => (_sheet?.SheetVariables ?? []).Select(v => new VariableItem
        {
            Name = v.Name,
            Value = v.Value,
            CanRemove = false,
            FromSheet = true,
        });

    /// <inheritdoc />
    protected override bool Matches(VariableItem item, string filter)
        => item.Name.Contains(filter, StringComparison.OrdinalIgnoreCase);

    /// <inheritdoc />
    protected override int Compare(VariableItem a, VariableItem b)
    {
        // Workspace variables first, sheet variables after; alphabetical within each group.
        if (a.FromSheet != b.FromSheet)
            return a.FromSheet ? 1 : -1;
        return StringComparer.OrdinalIgnoreCase.Compare(a.Name, b.Name);
    }
}
