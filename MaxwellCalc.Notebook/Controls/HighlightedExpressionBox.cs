using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Documents;
using Avalonia.Controls.Primitives;
using Avalonia.Data;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Media;
using Avalonia.Threading;
using MaxwellCalc.Core.Dictionaries;
using MaxwellCalc.Core.Parsers;
using MaxwellCalc.Core.Workspaces;
using System;
using System.Collections.Generic;

namespace MaxwellCalc.Notebook.Controls;

/// <summary>
/// The notebook's inline-highlighting expression editor. A real, editable <see cref="TextBox"/> (with
/// transparent text so only its caret and selection show) is layered over a colored
/// <see cref="TextBlock"/> that is rebuilt from the <see cref="Core.Parsers.Lexer"/>'s tokens on every
/// keystroke. This is the same <see cref="Run"/>-rebuild technique <see cref="QuantityView"/> uses, so
/// the editor and the gutter share one highlighting approach and identical mono metrics.
/// <para>
/// Tokens are colored per REDESIGN_UI.md → Inline syntax highlighting: numbers, variables, constants
/// and functions in <see cref="TemplatedControl.Foreground"/> (ink); known input units in
/// <see cref="UnitForeground"/> (the unit hue); the <c>in</c> keyword in <see cref="KeywordForeground"/>
/// (accent); operators/brackets/commas/<c>=</c> in <see cref="PunctuationForeground"/> (faint); and
/// unrecognized identifiers in <see cref="UnknownForeground"/> (error). Word tokens are classified
/// against the active <see cref="Workspace"/>'s dictionaries, and the display re-highlights whenever
/// those dictionaries change (e.g. after an input unit is added in the overlay).
/// </para>
/// </summary>
public class HighlightedExpressionBox : TemplatedControl
{
    private TextBlock? _display;
    private TextBox? _editor;
    private readonly List<Action> _unsubscribe = [];
    private IWorkspace? _hooked;
    private bool _isAttached;
    private bool _renderQueued;

    /// <summary>
    /// Raised when Enter is pressed. The sheet splits the line at <see cref="CaretIndex"/>: the text up
    /// to the caret stays, the remainder moves to a new line below, and focus follows it (Step 7).
    /// </summary>
    public event EventHandler? EnterPressed;

    /// <summary>
    /// Raised when Backspace is pressed with the caret at column 0 and nothing selected. The sheet
    /// merges this line into the previous one, placing the caret at the join point (Step 7).
    /// </summary>
    public event EventHandler? MergeBackRequested;

    /// <summary>Raised when the Up arrow is pressed: the sheet moves focus to the previous line.</summary>
    public event EventHandler? NavigateUpRequested;

    /// <summary>Raised when the Down arrow is pressed: the sheet moves focus to the next line.</summary>
    public event EventHandler? NavigateDownRequested;

    /// <summary>Identifies the <see cref="Text"/> property.</summary>
    public static readonly StyledProperty<string?> TextProperty =
        AvaloniaProperty.Register<HighlightedExpressionBox, string?>(
            nameof(Text), defaultBindingMode: BindingMode.TwoWay);

    /// <summary>Identifies the <see cref="CaretIndex"/> property.</summary>
    public static readonly StyledProperty<int> CaretIndexProperty =
        AvaloniaProperty.Register<HighlightedExpressionBox, int>(
            nameof(CaretIndex), defaultBindingMode: BindingMode.TwoWay);

    /// <summary>Identifies the <see cref="Workspace"/> property.</summary>
    public static readonly StyledProperty<IWorkspace?> WorkspaceProperty =
        AvaloniaProperty.Register<HighlightedExpressionBox, IWorkspace?>(nameof(Workspace));

    /// <summary>Identifies the <see cref="UnitForeground"/> property.</summary>
    public static readonly StyledProperty<IBrush?> UnitForegroundProperty =
        AvaloniaProperty.Register<HighlightedExpressionBox, IBrush?>(nameof(UnitForeground));

    /// <summary>Identifies the <see cref="KeywordForeground"/> property.</summary>
    public static readonly StyledProperty<IBrush?> KeywordForegroundProperty =
        AvaloniaProperty.Register<HighlightedExpressionBox, IBrush?>(nameof(KeywordForeground));

    /// <summary>Identifies the <see cref="PunctuationForeground"/> property.</summary>
    public static readonly StyledProperty<IBrush?> PunctuationForegroundProperty =
        AvaloniaProperty.Register<HighlightedExpressionBox, IBrush?>(nameof(PunctuationForeground));

    /// <summary>Identifies the <see cref="UnknownForeground"/> property.</summary>
    public static readonly StyledProperty<IBrush?> UnknownForegroundProperty =
        AvaloniaProperty.Register<HighlightedExpressionBox, IBrush?>(nameof(UnknownForeground));

    /// <summary>Gets or sets the raw expression text being edited.</summary>
    public string? Text
    {
        get => GetValue(TextProperty);
        set => SetValue(TextProperty, value);
    }

    /// <summary>
    /// Gets or sets the caret position within the editable text. Kept in sync with the inner editor's
    /// caret so the sheet's keyboard model can split at, preserve, and restore the column (Step 7).
    /// </summary>
    public int CaretIndex
    {
        get => GetValue(CaretIndexProperty);
        set => SetValue(CaretIndexProperty, value);
    }

    /// <summary>Gets or sets the workspace used to classify identifiers (units / consts / vars / funcs).</summary>
    public IWorkspace? Workspace
    {
        get => GetValue(WorkspaceProperty);
        set => SetValue(WorkspaceProperty, value);
    }

    /// <summary>Gets or sets the brush for known input units (the unit hue).</summary>
    public IBrush? UnitForeground
    {
        get => GetValue(UnitForegroundProperty);
        set => SetValue(UnitForegroundProperty, value);
    }

    /// <summary>Gets or sets the brush for the <c>in</c> keyword (accent).</summary>
    public IBrush? KeywordForeground
    {
        get => GetValue(KeywordForegroundProperty);
        set => SetValue(KeywordForegroundProperty, value);
    }

    /// <summary>Gets or sets the brush for operators, brackets, commas and <c>=</c> (faint).</summary>
    public IBrush? PunctuationForeground
    {
        get => GetValue(PunctuationForegroundProperty);
        set => SetValue(PunctuationForegroundProperty, value);
    }

    /// <summary>Gets or sets the brush for unrecognized identifiers and characters (error).</summary>
    public IBrush? UnknownForeground
    {
        get => GetValue(UnknownForegroundProperty);
        set => SetValue(UnknownForegroundProperty, value);
    }

    /// <inheritdoc />
    protected override void OnApplyTemplate(TemplateAppliedEventArgs e)
    {
        base.OnApplyTemplate(e);

        if (_editor is not null)
            _editor.RemoveHandler(KeyDownEvent, OnEditorKeyDown);

        _display = e.NameScope.Find<TextBlock>("PART_Display");
        _editor = e.NameScope.Find<TextBox>("PART_Editor");

        // Handle on the tunnel (preview) route so the notebook keys pre-empt the TextBox's own bubble
        // handling — otherwise the TextBox would consume/act on them before we could.
        if (_editor is not null)
            _editor.AddHandler(KeyDownEvent, OnEditorKeyDown, RoutingStrategies.Tunnel);

        Render();
    }

    /// <summary>
    /// Moves keyboard focus to the editable surface and places the caret at <see cref="CaretIndex"/>.
    /// Called by the sheet view after a split/merge/navigation so the freshly focused line's caret
    /// lands where the keyboard model decided (Step 7).
    /// </summary>
    public void FocusEditor()
    {
        if (_editor is null)
            return;
        _editor.Focus();
        int caret = Math.Clamp(CaretIndex, 0, (Text ?? string.Empty).Length);
        // Collapse any select-all the focus may have applied so the caret shows at the target column.
        _editor.SelectionStart = caret;
        _editor.SelectionEnd = caret;
        _editor.CaretIndex = caret;
    }

    // Turns the notebook navigation/edit keys into semantic events for the sheet to act on. Everything
    // else (typing, Left/Right, selection) is left to the underlying TextBox. Each handled key is
    // marked so the TextBox doesn't also process it.
    private void OnEditorKeyDown(object? sender, KeyEventArgs e)
    {
        if (e.KeyModifiers != KeyModifiers.None)
            return;

        switch (e.Key)
        {
            case Key.Enter:
                EnterPressed?.Invoke(this, EventArgs.Empty);
                e.Handled = true;
                break;

            // Only merge when the caret sits before the first character with no active selection;
            // otherwise let the TextBox delete normally.
            case Key.Back when _editor is { CaretIndex: 0 } && _editor.SelectionStart == _editor.SelectionEnd:
                MergeBackRequested?.Invoke(this, EventArgs.Empty);
                e.Handled = true;
                break;

            case Key.Up:
                NavigateUpRequested?.Invoke(this, EventArgs.Empty);
                e.Handled = true;
                break;

            case Key.Down:
                NavigateDownRequested?.Invoke(this, EventArgs.Empty);
                e.Handled = true;
                break;
        }
    }

    /// <inheritdoc />
    protected override void OnAttachedToVisualTree(VisualTreeAttachmentEventArgs e)
    {
        base.OnAttachedToVisualTree(e);
        _isAttached = true;
        HookWorkspace(Workspace);
        Render();
    }

    /// <inheritdoc />
    protected override void OnDetachedFromVisualTree(VisualTreeAttachmentEventArgs e)
    {
        base.OnDetachedFromVisualTree(e);
        _isAttached = false;
        Unhook();
    }

    /// <inheritdoc />
    protected override void OnPropertyChanged(AvaloniaPropertyChangedEventArgs change)
    {
        base.OnPropertyChanged(change);

        if (change.Property == WorkspaceProperty)
        {
            if (_isAttached)
                HookWorkspace(Workspace);
            Render();
        }
        else if (change.Property == TextProperty ||
                 change.Property == ForegroundProperty ||
                 change.Property == UnitForegroundProperty ||
                 change.Property == KeywordForegroundProperty ||
                 change.Property == PunctuationForegroundProperty ||
                 change.Property == UnknownForegroundProperty ||
                 change.Property == FontSizeProperty ||
                 change.Property == FontFamilyProperty ||
                 change.Property == FontWeightProperty)
        {
            Render();
        }
    }

    // ---- Workspace dictionary subscriptions --------------------------------------------------

    // Re-highlights when the workspace's dictionaries change so, e.g., adding an input unit in the
    // overlay recolors the lines that use it. The sheet evaluator briefly mutates and restores the
    // variable/function dictionaries during each pass, so these notifications are coalesced onto the
    // dispatcher (see ScheduleRender) to render once after the churn settles rather than per change.
    private void HookWorkspace(IWorkspace? workspace)
    {
        if (ReferenceEquals(_hooked, workspace))
            return;

        Unhook();
        _hooked = workspace;
        if (workspace is null)
            return;

        Subscribe(workspace.InputUnits);
        Subscribe(workspace.Constants.Local);
        Subscribe(workspace.Variables.Local);
        Subscribe(workspace.UserFunctions);
        Subscribe(workspace.BuiltInFunctions);
    }

    private void Subscribe<TKey, TValue>(IReadOnlyObservableDictionary<TKey, TValue> dictionary)
    {
        void Handler(object? sender, DictionaryChangedEventArgs<TKey, TValue> args) => ScheduleRender();
        dictionary.DictionaryChanged += Handler;
        _unsubscribe.Add(() => dictionary.DictionaryChanged -= Handler);
    }

    private void Subscribe<TKey, TValue>(IObservableDictionary<TKey, TValue> dictionary)
    {
        void Handler(object? sender, DictionaryChangedEventArgs<TKey, TValue> args) => ScheduleRender();
        dictionary.DictionaryChanged += Handler;
        _unsubscribe.Add(() => dictionary.DictionaryChanged -= Handler);
    }

    private void Unhook()
    {
        foreach (var unsubscribe in _unsubscribe)
            unsubscribe();
        _unsubscribe.Clear();
        _hooked = null;
    }

    private void ScheduleRender()
    {
        if (_renderQueued)
            return;
        _renderQueued = true;
        Dispatcher.UIThread.Post(() =>
        {
            _renderQueued = false;
            Render();
        }, DispatcherPriority.Background);
    }

    // ---- Highlighting ------------------------------------------------------------------------

    // Rebuilds the colored display runs from the current Text, spanning the whole string (including
    // the whitespace the lexer skips) so the display aligns character-for-character with the editor.
    private void Render()
    {
        if (_display is null)
            return;

        var inlines = _display.Inlines ??= [];
        inlines.Clear();

        string text = Text ?? string.Empty;
        if (text.Length == 0)
            return;

        var tokens = Tokenize(text);
        int cursor = 0;
        for (int i = 0; i < tokens.Count; i++)
        {
            var token = tokens[i];

            // Leading trivia (spaces/tabs) the lexer skipped between tokens.
            if (token.Start > cursor)
                inlines.Add(MakeRun(text.Substring(cursor, token.Start - cursor), Foreground, FontWeight.Normal));

            var (brush, weight) = Classify(tokens, i);
            inlines.Add(MakeRun(text.Substring(token.Start, token.Length), brush, weight));
            cursor = token.Start + token.Length;
        }

        // Trailing whitespace after the last token.
        if (cursor < text.Length)
            inlines.Add(MakeRun(text.Substring(cursor), Foreground, FontWeight.Normal));
    }

    private Run MakeRun(string text, IBrush? brush, FontWeight weight) => new()
    {
        Text = text,
        Foreground = brush,
        FontWeight = weight,
    };

    // Maps a token to its (brush, weight) per the highlighting class table.
    private (IBrush? Brush, FontWeight Weight) Classify(IReadOnlyList<TokenInfo> tokens, int index)
    {
        var token = tokens[index];
        switch (token.Type)
        {
            case TokenTypes.Scalar:
                return (Foreground, FontWeight.Normal);          // num — ink, 400

            case TokenTypes.Word:
                return ClassifyWord(tokens, index);

            case TokenTypes.Unknown:
                return (UnknownForeground, FontWeight.Normal);   // unknown — err, 400

            default:
                return (PunctuationForeground, FontWeight.Normal); // op / paren / punc / '=' — faint, 400
        }
    }

    // Classifies a word: the 'in' keyword, a function call (identifier followed by '('), or an
    // identifier resolved against the workspace as a variable / constant / unit / unknown.
    private (IBrush? Brush, FontWeight Weight) ClassifyWord(IReadOnlyList<TokenInfo> tokens, int index)
    {
        string name = tokens[index].Text;

        if (name == "in")
            return (KeywordForeground, FontWeight.SemiBold);     // kw — accent, 600

        var workspace = Workspace;

        // Followed by '(' ⇒ a function call: func if known, otherwise unknown.
        if (index + 1 < tokens.Count && tokens[index + 1].Type == TokenTypes.OpenParenthesis)
        {
            if (workspace is not null && IsFunction(workspace, name))
                return (Foreground, FontWeight.Medium);          // func — ink, 500
            return (UnknownForeground, FontWeight.Normal);       // unknown — err, 400
        }

        if (workspace is not null)
        {
            if (workspace.Variables.Local.ContainsKey(name))
                return (Foreground, FontWeight.Medium);          // var — ink, 500
            if (workspace.Constants.Local.ContainsKey(name))
                return (Foreground, FontWeight.Normal);          // const — ink, 400
            if (workspace.InputUnits.ContainsKey(name))
                return (UnitForeground, FontWeight.Medium);      // unit — unit hue, 500
        }

        return (UnknownForeground, FontWeight.Normal);           // unknown — err, 400
    }

    private static bool IsFunction(IWorkspace workspace, string name)
    {
        if (workspace.BuiltInFunctions.ContainsKey(name))
            return true;
        foreach (var key in workspace.UserFunctions.Keys)
        {
            if (key.Name == name)
                return true;
        }
        return false;
    }

    // Lexes the whole line into a flat token list. Every non-EndOfLine token consumes at least one
    // character, so the loop always terminates.
    private static List<TokenInfo> Tokenize(string text)
    {
        var tokens = new List<TokenInfo>();
        var lexer = new Lexer(text);
        while (lexer.Type != TokenTypes.EndOfLine)
        {
            var content = lexer.Content;
            tokens.Add(new TokenInfo(lexer.Index, content.Length, lexer.Type, content.ToString()));
            lexer.Next();
        }
        return tokens;
    }

    private readonly record struct TokenInfo(int Start, int Length, TokenTypes Type, string Text);
}
