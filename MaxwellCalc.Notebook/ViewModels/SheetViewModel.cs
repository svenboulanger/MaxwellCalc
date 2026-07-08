using CommunityToolkit.Mvvm.ComponentModel;
using MaxwellCalc.Core.Units;
using MaxwellCalc.Notebook.Evaluation;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Text.Json;

namespace MaxwellCalc.Notebook.ViewModels;

/// <summary>
/// A variable defined on the sheet (via an <c>x = …</c> line): its name and last formatted value.
/// Surfaced read-only in the command palette tagged <c>from sheet</c> (Step 8).
/// </summary>
/// <param name="Name">The variable name.</param>
/// <param name="Value">The formatted value the line resolved to.</param>
public readonly record struct SheetVariable(string Name, Quantity<string> Value);

/// <summary>
/// The notebook sheet: an ordered collection of live calculation lines. Whenever a line's text
/// changes (or the active workspace changes), the whole sheet is re-evaluated top to bottom against
/// a transient copy of the workspace via <see cref="SheetEvaluator"/>.
/// </summary>
public partial class SheetViewModel : ViewModelBase
{
    private readonly WorkspaceState _workspaceState;
    private readonly string _sheetFile;
    private bool _suppressEvaluation;

    private static readonly JsonSerializerOptions SaveOptions = new() { WriteIndented = true };

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
    /// Gets or sets whether the auto-selected-output-unit caption may show under a focused row. Driven
    /// by the user setting owned by <c>SettingsViewModel</c> (Step 10, default on); the view ANDs this
    /// with each line's <see cref="LineViewModel.ShowAutoCaption"/>.
    /// </summary>
    [ObservableProperty]
    private bool _autoCaptionEnabled = true;

    /// <summary>
    /// Gets the variables defined on the sheet this session (from <c>x = …</c> lines), deduplicated by
    /// name with the last assignment winning. These live only in the transient evaluation scope, so
    /// they are surfaced read-only in the command palette tagged <c>from sheet</c>.
    /// </summary>
    public IReadOnlyList<SheetVariable> SheetVariables { get; private set; } = [];

    /// <summary>
    /// Gets the function signatures (<c>name(params)</c>) defined on the sheet this session (from
    /// <c>f(x) = …</c> lines). Surfaced read-only in the command palette tagged <c>from sheet</c>.
    /// </summary>
    public IReadOnlyList<string> SheetFunctions { get; private set; } = [];

    /// <summary>
    /// Raised after a re-evaluation whenever the set of sheet-defined symbols changes, so the overlay's
    /// Variables/Functions panels can refresh their <c>from sheet</c> rows.
    /// </summary>
    public event Action? SheetSymbolsChanged;

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
        _sheetFile = Path.Combine(Directory.GetCurrentDirectory(), "sheet.json");
        _workspaceState.PropertyChanged += OnWorkspaceStateChanged;
        Lines.CollectionChanged += OnLinesChanged;

        // Restore the saved sheet (Step 11); if there is none, start with a single empty line so the
        // sheet is immediately editable.
        if (!TryLoad())
        {
            Lines.Add(new LineViewModel());
            Evaluate();
        }
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

    /// <summary>
    /// Clears the sheet back to a single empty, focused line (the "Clear sheet" chip and ⌘N / Ctrl+N) and
    /// persists immediately. Only the current sheet's editor lines are reset — the active workspace and its
    /// units, variables, functions and settings, and the Saved Sheets library, are all left untouched.
    /// There is deliberately no confirmation: clearing is cheap and recoverable by retyping or recalling a
    /// saved sheet.
    /// </summary>
    public void ClearSheet()
    {
        _suppressEvaluation = true;
        try
        {
            Lines.Clear();
            Lines.Add(new LineViewModel { CaretIndex = 0 });
        }
        finally
        {
            _suppressEvaluation = false;
        }

        Evaluate();
        Save();
        FocusedLineIndex = 0;
        FocusRequested?.Invoke(0);
    }

    // ---- Persistence (Step 11) ---------------------------------------------------------------

    /// <summary>
    /// Persists the sheet to <c>sheet.json</c> in the working directory as a plain JSON array of line
    /// texts. Only the raw text is stored; results are always recomputed on load. Best-effort: a failed
    /// write is swallowed so it can never break closing the window.
    /// </summary>
    public void Save()
    {
        try
        {
            var texts = Lines.Select(line => line.Text ?? string.Empty).ToList();
            File.WriteAllText(_sheetFile, JsonSerializer.Serialize(texts, SaveOptions));
        }
        catch
        {
            // Persistence is best-effort; a failed write must not break shutdown.
        }
    }

    // Loads the saved line texts from sheet.json and applies them, returning whether a sheet was
    // restored. A missing, empty, or malformed file leaves the sheet untouched (the caller seeds a
    // single empty line in that case).
    private bool TryLoad()
    {
        try
        {
            if (!File.Exists(_sheetFile))
                return false;
            var texts = JsonSerializer.Deserialize<List<string>>(File.ReadAllText(_sheetFile));
            if (texts is null || texts.Count == 0)
                return false;
            SetLines(texts);
            return true;
        }
        catch
        {
            return false;
        }
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
    /// Merges the next line into the line at <paramref name="index"/> (Delete at the last column): the
    /// next line's text is appended to the current line, the next line is removed, and focus stays on
    /// the current line with the caret at the join point. No-op for the last line.
    /// </summary>
    /// <param name="index">The index of the line the next line is merged into.</param>
    public void MergeWithNext(int index)
    {
        if (index < 0 || index >= Lines.Count - 1)
            return;

        var current = Lines[index];
        var next = Lines[index + 1];
        int joinAt = (current.Text ?? string.Empty).Length;

        _suppressEvaluation = true;
        try
        {
            current.Text = (current.Text ?? string.Empty) + (next.Text ?? string.Empty);
            current.CaretIndex = joinAt;
            Lines.RemoveAt(index + 1);
        }
        finally
        {
            _suppressEvaluation = false;
        }

        Evaluate();
        FocusedLineIndex = index;
        FocusRequested?.Invoke(index);
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

    /// <summary>
    /// Re-evaluates the whole sheet against the active workspace. Public entry point for callers that
    /// mutate the workspace out-of-band (e.g. the settings view model applying a unit preset, Step 12)
    /// and need the gutter to refresh.
    /// </summary>
    public void Recompute() => Evaluate();

    private void OnWorkspaceStateChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (e.PropertyName is nameof(WorkspaceState.Workspace))
        {
            OnPropertyChanged(nameof(Workspace));
            Evaluate();
        }
        else if (e.PropertyName is nameof(WorkspaceState.OutputFormat))
        {
            // The active entry's scalar-format / digit setting changed: re-run the pass so every
            // gutter value re-formats with the new format string.
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

        UpdateSheetSymbols(results);
    }

    /// <summary>
    /// Rebuilds <see cref="SheetVariables"/> / <see cref="SheetFunctions"/> from the pass results and
    /// notifies listeners if either changed. Variables are deduplicated by name (last assignment wins);
    /// functions by signature.
    /// </summary>
    private void UpdateSheetSymbols(IReadOnlyList<LineResult> results)
    {
        var variables = new List<SheetVariable>();
        var functions = new List<string>();
        foreach (var result in results)
        {
            switch (result.Kind)
            {
                case LineKind.Assign when result.DefinedName is { } name:
                    variables.RemoveAll(v => v.Name == name);
                    variables.Add(new SheetVariable(name, result.Quantity));
                    break;

                case LineKind.FuncDef when result.DefinedName is { } signature:
                    if (!functions.Contains(signature))
                        functions.Add(signature);
                    break;
            }
        }

        if (SheetVariables.SequenceEqual(variables) && SheetFunctions.SequenceEqual(functions))
            return;

        SheetVariables = variables;
        SheetFunctions = functions;
        SheetSymbolsChanged?.Invoke();
    }
}
