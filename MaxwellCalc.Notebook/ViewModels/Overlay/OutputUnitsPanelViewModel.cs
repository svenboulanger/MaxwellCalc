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
/// filtered rows and <see cref="Rows"/> the same rows flattened into a single virtualizable sequence —
/// section headers interleaved with their units — for the view. The footer (Step 9) adds an output unit;
/// rows remove one.
/// </summary>
public sealed partial class OutputUnitsPanelViewModel : FilteredPanelViewModel<OutputUnitItem, OutputUnitKey, string>
{
    /// <summary>
    /// Gets the filtered rows flattened for the view: an <see cref="OutputHeaderRow"/> per physical
    /// quantity followed by that group's <see cref="OutputUnitItem"/>s. A single flat collection (rather
    /// than nested groups) lets the view virtualize the list with one <c>VirtualizingStackPanel</c>.
    /// </summary>
    public ObservableCollection<OutputRow> Rows { get; } = [];

    /// <summary>Gets or sets the footer's unit field.</summary>
    [ObservableProperty]
    private string _newUnit = string.Empty;

    /// <summary>Gets or sets the footer's definition field (value in base units).</summary>
    [ObservableProperty]
    private string _newDefinition = string.Empty;

    /// <summary>The base unit of the category currently being renamed, or null when not editing.</summary>
    [ObservableProperty]
    private Unit? _editingCategoryKey;

    /// <summary>The text bound to the inline rename field.</summary>
    [ObservableProperty]
    private string _categoryDraft = string.Empty;

    /// <summary>Gets or sets the inline error shown when an add fails.</summary>
    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(HasError))]
    private string? _errorMessage;

    /// <summary>Gets whether an inline error is showing.</summary>
    public bool HasError => !string.IsNullOrEmpty(ErrorMessage);

    /// <summary>
    /// Raised after a rename re-sorts and rebuilds <see cref="Rows"/>, carrying the new index of the
    /// renamed category header so the view can scroll it back into view. A rename can move the group far
    /// down the alphabetical order (past the viewport), so without this the renamed group would vanish.
    /// </summary>
    public event Action<int>? RevealRowRequested;

    /// <summary>The category whose header should be revealed on the next rebuild, or null.</summary>
    private Unit? _pendingRevealKey;

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
        || item.Definition.Unit.ToString().Contains(filter, StringComparison.OrdinalIgnoreCase)
        || CategoryLabel(item.Definition.Unit).Contains(filter, StringComparison.OrdinalIgnoreCase);

    /// <inheritdoc />
    protected override int Compare(OutputUnitItem a, OutputUnitItem b)
    {
        // Order alphabetically by the category as displayed (a rename re-sorts), then by unit label.
        int byGroup = StringComparer.OrdinalIgnoreCase.Compare(
            CategoryLabel(a.Definition.Unit),
            CategoryLabel(b.Definition.Unit));
        if (byGroup != 0)
            return byGroup;
        return StringComparer.OrdinalIgnoreCase.Compare(a.Label, b.Label);
    }

    /// <inheritdoc />
    protected override void OnItemsChanged() => RebuildRows();

    /// <summary>
    /// Flattens the (default-category-then-label sorted) items into header + unit rows, tagging each
    /// header with its rename/edit state, then reconciles in place. Re-called after a rename/reset since
    /// UnitCategories isn't observable.
    /// </summary>
    private void RebuildRows()
    {
        var rows = new List<OutputRow>(Items.Count);
        // Group by the base unit itself so each header maps 1:1 to a UnitCategories key.
        foreach (var group in Items.GroupBy(item => item.Definition.Unit))
        {
            Unit key = group.Key;
            rows.Add(new OutputHeaderRow
            {
                Label = CategoryLabel(key),
                CategoryKey = key,
                IsEditing = EditingCategoryKey is { } editing && editing.Equals(key),
            });
            foreach (var item in group)
                rows.Add(item);
        }
        CollectionReconciler.Reconcile(Rows, rows);

        // After a rename re-sort, ask the view to scroll the renamed group back into view.
        if (_pendingRevealKey is { } revealKey)
        {
            _pendingRevealKey = null;
            for (int i = 0; i < Rows.Count; i++)
            {
                if (Rows[i] is OutputHeaderRow header && header.CategoryKey.Equals(revealKey))
                {
                    RevealRowRequested?.Invoke(i);
                    break;
                }
            }
        }
    }

    /// <summary>Enters inline-edit mode for a category header (the ✎ pencil).</summary>
    [RelayCommand]
    private void BeginRename(OutputHeaderRow row)
    {
        EditingCategoryKey = row.CategoryKey;
        // Seed with the raw stored name (case preserved), not row.Label — the label is upper-cased purely
        // for display, but editing operates on the real under-the-hood value.
        CategoryDraft = RawCategoryName(row.CategoryKey);
        RebuildRows();
    }

    /// <summary>Commits the inline rename (Save button / Enter).</summary>
    [RelayCommand]
    private void CommitRename()
    {
        if (EditingCategoryKey is not { } key)
            return;

        var workspace = WorkspaceState.Workspace;
        string name = CategoryDraft.Trim();
        if (workspace is not null)
        {
            // Empty, or an exact (case-sensitive) match of the built-in default, clears the override;
            // otherwise store the new name verbatim so its casing is preserved under the hood.
            if (name.Length == 0 ||
                string.Equals(name, DefaultCategoryName(key), StringComparison.Ordinal))
            {
                ResetCategoryEntry(workspace, key);
            }
            else
            {
                workspace.UnitCategories[key] = name;
            }
        }

        EditingCategoryKey = null;
        CategoryDraft = string.Empty;
        // A rename changes the category label the list is ordered by, so re-sort (not just re-group):
        // ScheduleRebuild re-runs the full sort/filter and lands the group in its new alphabetical spot.
        // Flag the group so RebuildRows asks the view to scroll it back into view after the re-sort.
        _pendingRevealKey = key;
        ScheduleRebuild();
    }

    /// <summary>Cancels inline editing without saving (Esc).</summary>
    [RelayCommand]
    private void CancelRename()
    {
        EditingCategoryKey = null;
        CategoryDraft = string.Empty;
        RebuildRows();
    }

    // Restore the seeded default (or remove the entry entirely when the unit had no built-in category).
    // Used by CommitRename when the typed name is empty or matches the built-in default.
    private static void ResetCategoryEntry(IWorkspace workspace, Unit key)
    {
        if (DefaultCategories.TryGetValue(key, out var seeded))
            workspace.UnitCategories[key] = seeded;
        else
            workspace.UnitCategories.Remove(key);
    }

    // Built-in category names, captured once from a pristine workspace. Used to (a) order groups by a
    // STABLE key so a rename doesn't re-sort the list, and (b) restore the default on Reset.
    private static readonly Dictionary<Unit, string> DefaultCategories =
        WorkspaceState.CreateDefaultWorkspace().UnitCategories;

    /// <summary>The built-in (pre-rename) name for a base unit with its original casing; base-unit string as fallback.</summary>
    private static string DefaultCategoryName(Unit baseUnit)
    {
        if (DefaultCategories.TryGetValue(baseUnit, out var category))
            return category;
        string key = baseUnit.ToString();
        return key.Length == 0 ? "Dimensionless" : key;
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

    /// <summary>
    /// Gets the underlying category name with its original casing (what the inline rename field edits),
    /// as opposed to <see cref="CategoryLabel"/> which upper-cases it for display. Falls back to the base
    /// unit's own string when it has no category entry.
    /// </summary>
    /// <param name="baseUnit">The base unit.</param>
    /// <returns>The stored category name, or the base unit's string when it has no category.</returns>
    private string RawCategoryName(Unit baseUnit)
    {
        var categories = WorkspaceState.Workspace?.UnitCategories;
        if (categories is not null && categories.TryGetValue(baseUnit, out var category))
            return category;
        string key = baseUnit.ToString();
        return key.Length == 0 ? "Dimensionless" : key;
    }
}
