using CommunityToolkit.Mvvm.ComponentModel;
using MaxwellCalc.Notebook.Evaluation;
using System;
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
    /// Raised when the keyboard model has moved the logical focus to a different line (after a split,
    /// merge or arrow navigation). The argument is the index of the line whose editor should take
    /// keyboard focus; the view realizes that row and calls <c>FocusEditor</c> on its editor. The line's
    /// <see cref="LineViewModel.CaretIndex"/> is already set to the desired caret column.
    /// </summary>
    public event Action<int>? FocusRequested;

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

    // ---- Keyboard model (Step 7) -------------------------------------------------------------

    /// <summary>
    /// Splits the line at <paramref name="index"/> at <paramref name="caretIndex"/> (the Enter key):
    /// the text up to the caret stays on the line, the remainder moves to a new line inserted directly
    /// below, and focus follows to the start of that new line. When the caret is at the end of the line
    /// (the common case) the new line is simply empty.
    /// </summary>
    /// <param name="index">The index of the line being split.</param>
    /// <param name="caretIndex">The caret column at which to split.</param>
    public void SplitLine(int index, int caretIndex)
    {
        if (index < 0 || index >= Lines.Count)
            return;

        var line = Lines[index];
        string text = line.Text ?? string.Empty;
        caretIndex = Math.Clamp(caretIndex, 0, text.Length);

        var newLine = new LineViewModel(text[caretIndex..]) { CaretIndex = 0 };

        _suppressEvaluation = true;
        try
        {
            line.Text = text[..caretIndex];
            line.CaretIndex = caretIndex;
            Lines.Insert(index + 1, newLine);
        }
        finally
        {
            _suppressEvaluation = false;
        }

        Evaluate();
        FocusedLineIndex = index + 1;
        FocusRequested?.Invoke(index + 1);
    }

    /// <summary>
    /// Merges the line at <paramref name="index"/> into the previous line (Backspace at column 0): the
    /// current line's text is appended to the previous line, the current line is removed, and focus
    /// moves to the previous line with the caret at the join point. No-op for the first line.
    /// </summary>
    /// <param name="index">The index of the line being merged upward.</param>
    public void MergeWithPrevious(int index)
    {
        if (index <= 0 || index >= Lines.Count)
            return;

        var previous = Lines[index - 1];
        var current = Lines[index];
        int joinAt = (previous.Text ?? string.Empty).Length;

        _suppressEvaluation = true;
        try
        {
            previous.Text = (previous.Text ?? string.Empty) + (current.Text ?? string.Empty);
            previous.CaretIndex = joinAt;
            Lines.RemoveAt(index);
        }
        finally
        {
            _suppressEvaluation = false;
        }

        Evaluate();
        FocusedLineIndex = index - 1;
        FocusRequested?.Invoke(index - 1);
    }

    /// <summary>
    /// Moves focus to the previous line (Up arrow), preserving the caret column where the target line
    /// is long enough. No-op on the first line.
    /// </summary>
    /// <param name="index">The index of the line currently focused.</param>
    /// <param name="caretColumn">The caret column to preserve.</param>
    public void NavigateUp(int index, int caretColumn) => MoveFocus(index, index - 1, caretColumn);

    /// <summary>
    /// Moves focus to the next line (Down arrow), preserving the caret column where the target line is
    /// long enough. No-op on the last line.
    /// </summary>
    /// <param name="index">The index of the line currently focused.</param>
    /// <param name="caretColumn">The caret column to preserve.</param>
    public void NavigateDown(int index, int caretColumn) => MoveFocus(index, index + 1, caretColumn);

    // Shared navigation: focus the target line (if it exists), clamping the preserved column to its
    // length. Evaluation is untouched — navigation changes no text.
    private void MoveFocus(int fromIndex, int toIndex, int caretColumn)
    {
        if (fromIndex < 0 || fromIndex >= Lines.Count)
            return;
        if (toIndex < 0 || toIndex >= Lines.Count)
            return;

        var target = Lines[toIndex];
        target.CaretIndex = Math.Min(caretColumn, (target.Text ?? string.Empty).Length);
        FocusedLineIndex = toIndex;
        FocusRequested?.Invoke(toIndex);
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
