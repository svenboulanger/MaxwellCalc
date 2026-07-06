using Avalonia.Threading;
using MaxwellCalc.Core.Dictionaries;
using MaxwellCalc.Core.Workspaces;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;

namespace MaxwellCalc.Notebook.ViewModels.Overlay;

/// <summary>
/// Fresh base for the command-palette panels (Step 8). Reads one observable dictionary from the active
/// workspace, projects each entry into an item view model, keeps the projection in sync by subscribing
/// to <see cref="IReadOnlyObservableDictionary{TKey, TValue}.DictionaryChanged"/>, re-projects when the
/// active workspace switches (via <see cref="WorkspaceState"/>), and exposes a <see cref="Filter"/>
/// string that live-filters the visible <see cref="Items"/>.
/// <para>
/// This is a clean rewrite of the old <c>FilteredCollectionViewModel&lt;&gt;</c>, exposing only what the
/// overlay needs. Rebuilds are coalesced onto the dispatcher because the sheet evaluator briefly mutates
/// and restores the variable/function dictionaries on every keystroke (see
/// <c>SheetEvaluator</c>); coalescing means the panel rebuilds once, from the settled dictionary, rather
/// than once per transient change.
/// </para>
/// </summary>
/// <typeparam name="TItem">The row view-model type.</typeparam>
/// <typeparam name="TKey">The dictionary key type.</typeparam>
/// <typeparam name="TValue">The dictionary value type.</typeparam>
public abstract class FilteredPanelViewModel<TItem, TKey, TValue> : ViewModelBase
    where TKey : notnull
{
    private readonly WorkspaceState _workspaceState;
    private readonly List<TItem> _all = [];
    private IReadOnlyObservableDictionary<TKey, TValue>? _dictionary;
    private EventHandler<DictionaryChangedEventArgs<TKey, TValue>>? _handler;
    private bool _rebuildQueued;
    private string _filter = string.Empty;

    /// <summary>Gets the shared workspace state the panel reads its dictionary from.</summary>
    protected WorkspaceState WorkspaceState => _workspaceState;

    /// <summary>Gets the currently visible (filtered, sorted) rows.</summary>
    public ObservableCollection<TItem> Items { get; } = [];

    /// <summary>Gets the total number of rows before filtering (for the access-bar count badges, Step 10).</summary>
    public int Count => _all.Count;

    /// <summary>Gets whether there are no visible rows (drives the panel's empty-state text).</summary>
    public bool IsEmpty => Items.Count == 0;

    /// <summary>Gets or sets the live search filter applied to <see cref="Items"/>.</summary>
    public string Filter
    {
        get => _filter;
        set
        {
            if (_filter == value)
                return;
            _filter = value;
            OnPropertyChanged();
            ApplyFilter();
        }
    }

    /// <summary>
    /// Creates a new <see cref="FilteredPanelViewModel{TItem, TKey, TValue}"/> bound to the active workspace.
    /// </summary>
    /// <param name="workspaceState">The shared workspace state.</param>
    protected FilteredPanelViewModel(WorkspaceState workspaceState)
    {
        _workspaceState = workspaceState;
        _workspaceState.PropertyChanged += OnWorkspaceStateChanged;
        Attach(_workspaceState.Workspace);
        Rebuild();
    }

    /// <summary>Gets the observable dictionary this panel projects, or <c>null</c> if unavailable.</summary>
    /// <param name="workspace">The active workspace.</param>
    protected abstract IReadOnlyObservableDictionary<TKey, TValue>? GetDictionary(IWorkspace workspace);

    /// <summary>Projects one dictionary entry into a row view model.</summary>
    /// <param name="key">The entry key.</param>
    /// <param name="value">The entry value.</param>
    protected abstract TItem Project(TKey key, TValue value);

    /// <summary>Tests a row against the (non-empty) filter.</summary>
    /// <param name="item">The row.</param>
    /// <param name="filter">The filter text.</param>
    protected abstract bool Matches(TItem item, string filter);

    /// <summary>Orders two rows.</summary>
    /// <param name="a">The left row.</param>
    /// <param name="b">The right row.</param>
    protected abstract int Compare(TItem a, TItem b);

    /// <summary>
    /// Extra rows appended after the projected dictionary rows (e.g. the <c>from sheet</c> symbols on the
    /// Variables/Functions panels). Empty by default.
    /// </summary>
    protected virtual IEnumerable<TItem> ExtraItems() => [];

    /// <summary>
    /// Called after <see cref="Items"/> is repopulated (initial build, dictionary change, or filter
    /// change). Panels that present a derived shape (e.g. grouped output units) override this to rebuild
    /// it. No-op by default.
    /// </summary>
    protected virtual void OnItemsChanged() { }

    /// <summary>
    /// Runs an add/remove <paramref name="attempt"/> against the active workspace (Step 9) while capturing
    /// any diagnostics it posts, so a footer can surface them inline. Returns <c>null</c> on success, or the
    /// joined diagnostic text (falling back to a generic message) on failure.
    /// </summary>
    /// <param name="attempt">The Core call to run; returns whether it succeeded.</param>
    protected string? RunWithDiagnostics(Func<IWorkspace, bool> attempt)
    {
        var workspace = WorkspaceState.Workspace;
        if (workspace is null)
            return "No active workspace.";

        var diagnostics = new List<string>();
        void Collect(object? sender, DiagnosticMessagePostedEventArgs args) => diagnostics.Add(args.Message);
        workspace.DiagnosticMessagePosted += Collect;
        try
        {
            bool ok = attempt(workspace);
            if (ok && diagnostics.Count == 0)
                return null;
            if (diagnostics.Count > 0)
                return string.Join(Environment.NewLine, diagnostics);
            return "The expression could not be evaluated.";
        }
        finally
        {
            workspace.DiagnosticMessagePosted -= Collect;
        }
    }

    /// <summary>
    /// Queues a coalesced rebuild. Used by subclasses that observe an additional source (e.g. the sheet's
    /// symbol changes) so their extra rows refresh.
    /// </summary>
    protected void ScheduleRebuild()
    {
        if (_rebuildQueued)
            return;
        _rebuildQueued = true;
        Dispatcher.UIThread.Post(() =>
        {
            _rebuildQueued = false;
            Rebuild();
        }, DispatcherPriority.Background);
    }

    private void OnWorkspaceStateChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (e.PropertyName is nameof(WorkspaceState.Workspace))
        {
            Attach(_workspaceState.Workspace);
            Rebuild();
        }
    }

    private void Attach(IWorkspace? workspace)
    {
        Detach();
        if (workspace is null)
            return;

        var dictionary = GetDictionary(workspace);
        if (dictionary is null)
            return;

        _dictionary = dictionary;
        _handler = (_, _) => ScheduleRebuild();
        dictionary.DictionaryChanged += _handler;
    }

    private void Detach()
    {
        if (_dictionary is not null && _handler is not null)
            _dictionary.DictionaryChanged -= _handler;
        _dictionary = null;
        _handler = null;
    }

    private void Rebuild()
    {
        _all.Clear();

        // Enumerate the dictionary attached for the current workspace rather than calling
        // GetDictionary again: some workspace dictionaries hand out a fresh read-only wrapper per call
        // (each holding a live subscription), so re-fetching on every rebuild would leak.
        if (_dictionary is not null)
        {
            foreach (var pair in _dictionary)
                _all.Add(Project(pair.Key, pair.Value));
        }

        _all.AddRange(ExtraItems());
        _all.Sort(Compare);
        ApplyFilter();
        OnPropertyChanged(nameof(Count));
    }

    private void ApplyFilter()
    {
        Items.Clear();
        foreach (var item in _all)
        {
            if (string.IsNullOrWhiteSpace(_filter) || Matches(item, _filter))
                Items.Add(item);
        }
        OnPropertyChanged(nameof(IsEmpty));
        OnItemsChanged();
    }
}
