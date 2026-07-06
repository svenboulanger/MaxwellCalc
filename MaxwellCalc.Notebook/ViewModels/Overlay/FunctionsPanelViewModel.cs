using MaxwellCalc.Core.Dictionaries;
using MaxwellCalc.Core.Workspaces;
using System;
using System.Collections.Generic;
using System.Linq;

namespace MaxwellCalc.Notebook.ViewModels.Overlay;

/// <summary>
/// The command palette's Functions panel: the workspace's user-defined functions plus the read-only
/// <c>from sheet</c> functions defined on the sheet this session. Reads <c>workspace.UserFunctions</c>.
/// </summary>
public sealed class FunctionsPanelViewModel : FilteredPanelViewModel<FunctionItem, UserFunctionKey, UserFunction>
{
    private readonly SheetViewModel _sheet;

    /// <summary>
    /// Creates a new <see cref="FunctionsPanelViewModel"/>.
    /// </summary>
    /// <param name="workspaceState">The shared workspace state.</param>
    /// <param name="sheet">The sheet, for the <c>from sheet</c> functions.</param>
    public FunctionsPanelViewModel(WorkspaceState workspaceState, SheetViewModel sheet)
        : base(workspaceState)
    {
        _sheet = sheet;
        _sheet.SheetSymbolsChanged += ScheduleRebuild;
        ScheduleRebuild();
    }

    /// <inheritdoc />
    protected override IReadOnlyObservableDictionary<UserFunctionKey, UserFunction>? GetDictionary(IWorkspace workspace)
        => workspace.UserFunctions.AsReadOnly();

    /// <inheritdoc />
    protected override FunctionItem Project(UserFunctionKey key, UserFunction value)
        => new()
        {
            Signature = $"{key.Name}({string.Join(", ", value.Parameters)})",
            Body = string.Join("; ", value.Body.Select(node => node.Content.ToString())),
            CanRemove = true,
            FromSheet = false,
        };

    /// <inheritdoc />
    protected override IEnumerable<FunctionItem> ExtraItems()
        // Null-safe: the base constructor's first Rebuild runs before this derived constructor assigns
        // _sheet (see VariablesPanelViewModel for the same pattern).
        => (_sheet?.SheetFunctions ?? []).Select(signature => new FunctionItem
        {
            Signature = signature,
            Body = null,
            CanRemove = false,
            FromSheet = true,
        });

    /// <inheritdoc />
    protected override bool Matches(FunctionItem item, string filter)
        => item.Signature.Contains(filter, StringComparison.OrdinalIgnoreCase)
        || (item.Body?.Contains(filter, StringComparison.OrdinalIgnoreCase) ?? false);

    /// <inheritdoc />
    protected override int Compare(FunctionItem a, FunctionItem b)
    {
        // Workspace functions first, sheet functions after; alphabetical within each group.
        if (a.FromSheet != b.FromSheet)
            return a.FromSheet ? 1 : -1;
        return StringComparer.OrdinalIgnoreCase.Compare(a.Signature, b.Signature);
    }
}
