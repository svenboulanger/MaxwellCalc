using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Documents;
using Avalonia.Controls.Primitives;
using Avalonia.Input;
using Avalonia.Input.Platform;
using Avalonia.Media;
using Avalonia.Threading;
using MaxwellCalc.Core.Units;
using MaxwellCalc.Notebook.Evaluation;
using System;
using System.Collections.Generic;

namespace MaxwellCalc.Notebook.Controls;

/// <summary>
/// The read-only rendered form of a notebook text line: literal prose interleaved with the evaluated
/// results of its inline <c>{…}</c> expressions. This is what a text line shows when it is not being
/// edited.
/// <para>
/// Literal spans are drawn in <see cref="TemplatedControl.Foreground"/>; inline value/assignment
/// results reuse the gutter's quantity layout (<see cref="QuantityInlines"/>) with the scalar in
/// <see cref="ValueForeground"/> and units in <see cref="UnitForeground"/>; a function definition shows
/// its signature; and a failed expression shows the diagnostic itself (<c>⚠</c>-prefixed) in
/// <see cref="ErrorForeground"/>, the same way the result gutter does.
/// </para>
/// <para>
/// The view is interactive even though it is "read-only" text: clicking an inline value/assignment result
/// copies it to the clipboard (like the gutter, and with a hand cursor over it), while clicking anywhere
/// in the prose asks the sheet to switch to the raw-text editor with the caret at the clicked column.
/// Pointer input does not route to controls embedded in a <see cref="TextBlock"/>, so both gestures are
/// resolved here from a single hit-test against the rendered→raw region map (see <see cref="ResolveClick"/>).
/// </para>
/// </summary>
public class InlineTextView : TemplatedControl
{
    // How long the "Copied!" tooltip stays up after a click (matches QuantityView's gutter feedback).
    private static readonly TimeSpan CopiedFeedbackDuration = TimeSpan.FromSeconds(1.1);

    private readonly Cursor _handCursor = new(StandardCursorType.Hand);
    private readonly Cursor _textCursor = new(StandardCursorType.Ibeam);

    private TextBlock? _output;
    private bool _renderQueued;

    // Pending revert of the "Copied!" tooltip; disposed/replaced on each copy so rapid clicks don't leave
    // the feedback stuck or revert early.
    private IDisposable? _feedbackReset;

    // Maps ranges of the rendered text back to offsets in the raw line text, in rendered order. Rebuilt
    // on every Render so a click on the prose can be reverse-calculated to a caret in the source (see
    // GetRawCaretIndex). Empty when the raw text and rendered segments can't be aligned (falls back to
    // placing the caret at the end).
    private readonly List<RenderRegion> _regions = [];

    /// <summary>Identifies the <see cref="Segments"/> property.</summary>
    public static readonly StyledProperty<IReadOnlyList<TextSegment>?> SegmentsProperty =
        AvaloniaProperty.Register<InlineTextView, IReadOnlyList<TextSegment>?>(nameof(Segments));

    /// <summary>Identifies the <see cref="Text"/> property.</summary>
    public static readonly StyledProperty<string?> TextProperty =
        AvaloniaProperty.Register<InlineTextView, string?>(nameof(Text));

    /// <summary>Identifies the <see cref="ValueForeground"/> property.</summary>
    public static readonly StyledProperty<IBrush?> ValueForegroundProperty =
        AvaloniaProperty.Register<InlineTextView, IBrush?>(nameof(ValueForeground));

    /// <summary>Identifies the <see cref="UnitForeground"/> property.</summary>
    public static readonly StyledProperty<IBrush?> UnitForegroundProperty =
        AvaloniaProperty.Register<InlineTextView, IBrush?>(nameof(UnitForeground));

    /// <summary>Identifies the <see cref="ErrorForeground"/> property.</summary>
    public static readonly StyledProperty<IBrush?> ErrorForegroundProperty =
        AvaloniaProperty.Register<InlineTextView, IBrush?>(nameof(ErrorForeground));

    /// <summary>Gets or sets the rendered segments of the text line.</summary>
    public IReadOnlyList<TextSegment>? Segments
    {
        get => GetValue(SegmentsProperty);
        set => SetValue(SegmentsProperty, value);
    }

    /// <summary>
    /// Gets or sets the raw (editable) source of the text line — marker, prose and <c>{…}</c> braces and
    /// all. Used only to reverse-map a click on the rendered prose to a caret column in the source (see
    /// <see cref="GetRawCaretIndex"/>); the visible content comes from <see cref="Segments"/>.
    /// </summary>
    public string? Text
    {
        get => GetValue(TextProperty);
        set => SetValue(TextProperty, value);
    }

    /// <summary>Gets or sets the brush for the scalar of an inline value/assignment result (accent).</summary>
    public IBrush? ValueForeground
    {
        get => GetValue(ValueForegroundProperty);
        set => SetValue(ValueForegroundProperty, value);
    }

    /// <summary>Gets or sets the brush for unit symbols of an inline result (the unit hue).</summary>
    public IBrush? UnitForeground
    {
        get => GetValue(UnitForegroundProperty);
        set => SetValue(UnitForegroundProperty, value);
    }

    /// <summary>Gets or sets the brush for a failed inline expression (error color).</summary>
    public IBrush? ErrorForeground
    {
        get => GetValue(ErrorForegroundProperty);
        set => SetValue(ErrorForegroundProperty, value);
    }

    /// <inheritdoc />
    protected override void OnApplyTemplate(TemplateAppliedEventArgs e)
    {
        base.OnApplyTemplate(e);
        _output = e.NameScope.Find<TextBlock>("PART_Text");
        Render();
    }

    /// <inheritdoc />
    protected override void OnPropertyChanged(AvaloniaPropertyChangedEventArgs change)
    {
        base.OnPropertyChanged(change);

        if (change.Property == IsVisibleProperty)
        {
            // A text line's rendered view is created collapsed and only revealed when the line goes
            // idle (see LineViewModel.ShowInlineText). The template is applied during the layout pass
            // that reveals it, so the OnApplyTemplate render below runs mid-measure and its freshly
            // built inline runs are not laid out until some later, unrelated pass — leaving the row
            // blank and near-zero height (so it can't even be clicked to edit) until the sheet is
            // re-evaluated. Re-render once out-of-band, after the reveal layout has settled, so the
            // runs measure normally — the same path a re-evaluation's Segments change already takes.
            if (change.GetNewValue<bool>())
                ScheduleRender();
            return;
        }

        if (change.Property == SegmentsProperty ||
            change.Property == TextProperty ||
            change.Property == ValueForegroundProperty ||
            change.Property == UnitForegroundProperty ||
            change.Property == ErrorForegroundProperty ||
            change.Property == ForegroundProperty ||
            change.Property == FontSizeProperty ||
            change.Property == FontFamilyProperty)
        {
            Render();
        }
    }

    // Coalesces a re-render onto the dispatcher so it runs after the current layout pass (when the
    // template has been applied and the row laid out), rather than synchronously mid-measure.
    private void ScheduleRender()
    {
        if (_renderQueued)
            return;
        _renderQueued = true;
        Dispatcher.UIThread.Post(
            () =>
            {
                _renderQueued = false;
                Render();
            },
            DispatcherPriority.Loaded);
    }

    // Rebuilds the inline runs from the current segments, and alongside them the rendered→raw region
    // map that GetRawCaretIndex uses to place the caret where the user clicked.
    private void Render()
    {
        if (_output is null)
            return;

        var inlines = _output.Inlines ??= [];
        inlines.Clear();
        _regions.Clear();

        if (Segments is null)
            return;

        // Re-scan the raw source to recover each segment's offsets. The scan tiles the source in the
        // same order and count as the evaluator built Segments, so rawSegments[i] locates Segments[i]
        // in the source. If the two can't be aligned (raw text out of sync, not a text line), leave the
        // region map empty — GetRawCaretIndex then falls back to the end of the line.
        var rawSegments = TryScanRaw(Text, Segments.Count);
        int rendered = 0;

        for (int i = 0; i < Segments.Count; i++)
        {
            var segment = Segments[i];
            RawSegment? raw = rawSegments is not null ? rawSegments[i] : null;

            if (segment.Literal is { } literal)
            {
                inlines.Add(new Run { Text = literal, Foreground = Foreground, FontSize = FontSize });
                AddRegion(raw, ref rendered, literal.Length, isLiteral: true);
                continue;
            }

            if (segment.Expression is not { } result)
                continue;

            switch (result.Kind)
            {
                case LineKind.Value:
                case LineKind.Assign:
                    // An assignment renders just its value (e.g. "{m = 5 kg}" → "5 kg"), laid out like the
                    // gutter. It is copyable: the region records the quantity, and ResolveClick copies it.
                    // (Copy is driven from here, not the QuantityView, because pointer input doesn't reach
                    // a control embedded in a TextBlock — CopyOnClick would never fire.)
                    var quantity = new QuantityView
                    {
                        Value = result.Quantity,
                        Foreground = ValueForeground,
                        UnitForeground = UnitForeground,
                        FontSize = FontSize,
                    };
                    inlines.Add(new InlineUIContainer(quantity)
                    {
                        BaselineAlignment = BaselineAlignment.TextBottom,
                    });
                    // An embedded control occupies a single position in the text layout.
                    AddRegion(raw, ref rendered, 1, isLiteral: false, copyValue: result.Quantity);
                    break;

                case LineKind.FuncDef:
                    string name = result.DefinedName ?? segment.RawSource;
                    inlines.Add(new Run
                    {
                        Text = name,
                        Foreground = ValueForeground,
                        FontWeight = FontWeight.Medium,
                        FontSize = FontSize,
                    });
                    AddRegion(raw, ref rendered, name.Length, isLiteral: false);
                    break;

                case LineKind.Error:
                    // Show the diagnostic itself (⚠ prefixed, in the error color) inline, the same way
                    // the result gutter does — not the raw {…} source in red. Falls back to the raw
                    // source only when there is no message. Rendered as a plain run so it flows with the
                    // prose; a click anywhere on it opens the editor (like the funcdef case).
                    string errorText = !string.IsNullOrEmpty(result.ErrorMessage)
                        ? "⚠ " + result.ErrorMessage
                        : segment.RawSource;
                    inlines.Add(new Run
                    {
                        Text = errorText,
                        Foreground = ErrorForeground,
                        FontSize = FontSize,
                    });
                    AddRegion(raw, ref rendered, errorText.Length, isLiteral: false);
                    break;
            }
        }
    }

    // Records that the next <paramref name="renderedLength"/> rendered positions came from raw span
    // <paramref name="raw"/>, and advances the rendered cursor. A null span (alignment failed) is skipped
    // but the cursor still advances so later regions stay correctly placed. A non-null copyValue marks the
    // region as a clickable result carrying that quantity.
    private void AddRegion(RawSegment? raw, ref int rendered, int renderedLength, bool isLiteral, Quantity<string>? copyValue = null)
    {
        if (raw is { } span)
            _regions.Add(new RenderRegion(
                rendered, renderedLength, span.Start, span.Length, isLiteral, copyValue));
        rendered += renderedLength;
    }

    /// <summary>
    /// Resolves a click on the rendered prose at <paramref name="point"/> (in this control's coordinates).
    /// If it landed on an inline value/assignment result, this copies that result to the clipboard (in the
    /// same original text form as the gutter), flashes a "Copied!" hint, and returns <c>null</c> to signal
    /// the click was consumed. Otherwise it reverse-calculates and returns the caret column in the raw
    /// source (<see cref="Text"/>) so the sheet can open the editor there. The rendered prose differs from
    /// the source — literal escapes are collapsed and each <c>{…}</c> is replaced by its result — so this
    /// hit-tests the rendered text and maps the hit back through the region map built in <see cref="Render"/>.
    /// </summary>
    /// <param name="point">The click position, relative to this control.</param>
    /// <returns>The caret index into the raw source, or <c>null</c> if a result was copied instead.</returns>
    public int? ResolveClick(Point point)
    {
        string text = Text ?? string.Empty;
        if (_output is null || _regions.Count == 0)
            return text.Length;

        Point local = this.TranslatePoint(point, _output) ?? point;
        var hit = _output.TextLayout.HitTestPoint(local);

        // The glyph actually under the pointer (not the insertion point) decides whether a result was hit.
        if (hit.IsInside &&
            RegionContaining(hit.CharacterHit.FirstCharacterIndex) is { CopyValue: { } value })
        {
            Copy(value);
            return null;
        }

        int rendered = hit.CharacterHit.FirstCharacterIndex + hit.CharacterHit.TrailingLength;
        return MapRenderedToRaw(rendered, text);
    }

    /// <inheritdoc />
    protected override void OnPointerMoved(PointerEventArgs e)
    {
        base.OnPointerMoved(e);
        // A hand cursor over a copyable result makes it discoverable as clickable (like the gutter); the
        // I-beam elsewhere signals click-to-edit. This also covers the gaps between a result's glyphs,
        // which the embedded control itself doesn't fill.
        Cursor = IsOverResult(e.GetPosition(this)) ? _handCursor : _textCursor;
    }

    /// <inheritdoc />
    protected override void OnPointerExited(PointerEventArgs e)
    {
        base.OnPointerExited(e);
        Cursor = _textCursor;
    }

    // Whether the pointer is over an inline result region (used for the hand-cursor affordance).
    private bool IsOverResult(Point point)
    {
        if (_output is null || _regions.Count == 0)
            return false;

        Point local = this.TranslatePoint(point, _output) ?? point;
        var hit = _output.TextLayout.HitTestPoint(local);
        return hit.IsInside &&
            RegionContaining(hit.CharacterHit.FirstCharacterIndex) is { CopyValue: not null };
    }

    // The region whose rendered range contains the given glyph index, or null if past the end.
    private RenderRegion? RegionContaining(int glyph)
    {
        foreach (var region in _regions)
        {
            if (glyph >= region.RenderedStart && glyph < region.RenderedStart + region.RenderedLength)
                return region;
        }
        return null;
    }

    // Copies a quantity to the clipboard in its original text form (e-exponent scalar, ^-notation units,
    // e.g. "1.5e21 m^2 s^-1"), the same form the gutter's QuantityView copies, then flashes the hint.
    private void Copy(Quantity<string> value)
    {
        if (TopLevel.GetTopLevel(this)?.Clipboard is not { } clipboard)
            return;

        string scalar = value.Scalar ?? string.Empty;
        string unit = value.Unit.ToString();
        _ = clipboard.SetTextAsync(unit.Length > 0 ? $"{scalar} {unit}" : scalar);
        ShowCopiedFeedback();
    }

    // Flashes a "Copied!" tooltip for a moment, then clears it. Replacing any pending revert keeps rapid
    // clicks from reverting early or leaving the confirmation stuck open.
    private void ShowCopiedFeedback()
    {
        _feedbackReset?.Dispose();
        ToolTip.SetTip(this, "Copied!");
        ToolTip.SetIsOpen(this, true);
        _feedbackReset = DispatcherTimer.RunOnce(() =>
        {
            ToolTip.SetIsOpen(this, false);
            ToolTip.SetTip(this, null);
        }, CopiedFeedbackDuration);
    }

    // Walks the region map to turn a rendered caret position into a raw source offset.
    private int MapRenderedToRaw(int rendered, string text)
    {
        foreach (var region in _regions)
        {
            if (rendered > region.RenderedStart + region.RenderedLength)
                continue;

            int within = Math.Clamp(rendered - region.RenderedStart, 0, region.RenderedLength);
            int raw = region.IsLiteral
                // Literal: map char-for-char, stepping over {{ / }} escapes that collapsed to one glyph.
                ? region.RawStart + UnescapedOffsetToRaw(text, region.RawStart, region.RawLength, within)
                // Inline result (value / funcdef / error): its rendered width bears no relation to the
                // source, so snap to whichever brace edge the click is nearer.
                : within * 2 <= region.RenderedLength ? region.RawStart : region.RawStart + region.RawLength;
            return Math.Clamp(raw, 0, text.Length);
        }

        return text.Length;
    }

    // Scans the raw text into its literal / expression spans, returning null unless the scan lines up
    // one-to-one with the rendered segments (so index i in each list refers to the same piece).
    private static IReadOnlyList<RawSegment>? TryScanRaw(string? text, int segmentCount)
    {
        if (string.IsNullOrEmpty(text) || !TextLineParser.TryGetMarker(text, out int markerIndex))
            return null;

        var spans = TextLineParser.ScanSegments(text, TextLineParser.GetContentStart(text, markerIndex));
        return spans.Count == segmentCount ? spans : null;
    }

    // Converts an offset into a literal span's *unescaped* (rendered) form back to an offset into its raw
    // form, by replaying the escape collapsing: a {{ or }} pair in the source is one glyph on screen.
    private static int UnescapedOffsetToRaw(string text, int rawStart, int rawLength, int within)
    {
        int raw = 0;
        for (int shown = 0; shown < within && raw < rawLength; shown++)
        {
            char c = text[rawStart + raw];
            bool escaped = (c == '{' || c == '}') && raw + 1 < rawLength && text[rawStart + raw + 1] == c;
            raw += escaped ? 2 : 1;
        }
        return raw;
    }

    // One rendered-text range and the raw-source span it came from. RenderedStart/RenderedLength index the
    // TextBlock's rendered text (each inline result counts as a single position); RawStart/RawLength index
    // the raw source. IsLiteral marks prose spans, which map char-for-char (modulo escapes). CopyValue is
    // set for a clickable value/assignment result and is the quantity a click on it copies.
    private readonly record struct RenderRegion(
        int RenderedStart, int RenderedLength, int RawStart, int RawLength, bool IsLiteral,
        Quantity<string>? CopyValue);
}
