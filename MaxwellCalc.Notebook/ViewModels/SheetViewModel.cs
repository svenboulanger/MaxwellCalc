using CommunityToolkit.Mvvm.ComponentModel;
using MaxwellCalc.Notebook.Evaluation;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Linq;

namespace MaxwellCalc.Notebook.ViewModels;

/// <summary>
/// The notebook sheet: an ordered collection of live calculation lines. Whenever a line's text
/// changes (or the active workspace changes), the whole sheet is re-evaluated top to bottom against
/// a transient copy of the workspace via <see cref="SheetEvaluator"/>.
/// </summary>
public partial class SheetViewModel : ViewModelBase
{
    private readonly WorkspaceState _workspaceState;
    private bool _suppressEvaluation;

    /// <summary>
    /// Gets the lines of the sheet.
    /// </summary>
    public ObservableCollection<LineViewModel> Lines { get; } = [];

    /// <summary>
    /// Gets the active workspace the line editors classify their tokens against (for inline
    /// highlighting). Proxies <see cref="WorkspaceState.Workspace"/> and changes with it.
    /// </summary>
    public MaxwellCalc.Core.Workspaces.IWorkspace? Workspace => _workspaceState.Workspace;

    /// <summary>
    /// Gets or sets the index of the currently focused line, or <c>null</c> when nothing is focused.
    /// </summary>
    [ObservableProperty]
    private int? _focusedLineIndex;

    /// <summary>
    /// Creates a new <see cref="SheetViewModel"/>.
    /// </summary>
    /// <param name="workspaceState">The shared workspace state.</param>
    public SheetViewModel(WorkspaceState workspaceState)
    {
        _workspaceState = workspaceState;
        _workspaceState.PropertyChanged += OnWorkspaceStateChanged;
        Lines.CollectionChanged += OnLinesChanged;

        // Start with a single empty line so the sheet is immediately editable.
        Lines.Add(new LineViewModel());
        Evaluate();
    }

    /// <summary>
    /// Replaces the sheet contents with the given line texts and re-evaluates once. Used by
    /// persistence (Step 11) when loading a saved sheet.
    /// </summary>
    /// <param name="texts">The line texts to load.</param>
    public void SetLines(IEnumerable<string> texts)
    {
        _suppressEvaluation = true;
        try
        {
            Lines.Clear();
            foreach (var text in texts)
                Lines.Add(new LineViewModel(text));
            if (Lines.Count == 0)
                Lines.Add(new LineViewModel());
        }
        finally
        {
            _suppressEvaluation = false;
        }
        Evaluate();
    }

    private void OnWorkspaceStateChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (e.PropertyName is nameof(WorkspaceState.Workspace))
        {
            OnPropertyChanged(nameof(Workspace));
            Evaluate();
        }
    }

    private void OnLinesChanged(object? sender, NotifyCollectionChangedEventArgs e)
    {
        if (e.OldItems is not null)
        {
            foreach (LineViewModel line in e.OldItems)
                line.PropertyChanged -= OnLinePropertyChanged;
        }
        if (e.NewItems is not null)
        {
            foreach (LineViewModel line in e.NewItems)
                line.PropertyChanged += OnLinePropertyChanged;
        }

        Evaluate();
    }

    private void OnLinePropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (e.PropertyName is nameof(LineViewModel.Text))
            Evaluate();
    }

    /// <summary>
    /// Re-evaluates every line against a transient copy of the active workspace and pushes the
    /// results back onto the line view models. The persistent workspace is never mutated.
    /// </summary>
    private void Evaluate()
    {
        if (_suppressEvaluation)
            return;

        var texts = Lines.Select(line => line.Text).ToList();
        var results = SheetEvaluator.Evaluate(_workspaceState.Workspace, texts, _workspaceState.OutputFormat);
        for (int i = 0; i < Lines.Count && i < results.Count; i++)
            Lines[i].ApplyResult(results[i]);
    }
}
