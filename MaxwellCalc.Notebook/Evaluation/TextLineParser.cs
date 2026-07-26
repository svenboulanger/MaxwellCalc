using System.Collections.Generic;

namespace MaxwellCalc.Notebook.Evaluation;

/// <summary>
/// The kind of a raw span scanned out of a text line by <see cref="TextLineParser"/>.
/// </summary>
public enum TextSpanKind
{
    /// <summary>Literal prose (as written — may still contain <c>{{</c>/<c>}}</c> escapes).</summary>
    Literal,

    /// <summary>An inline <c>{…}</c> expression. The span covers the braces as well as the body.</summary>
    Expression,
}

/// <summary>
/// A span of a text line, in coordinates of the original (un-modified) line text.
/// </summary>
/// <param name="Kind">Whether the span is literal prose or an inline expression.</param>
/// <param name="Start">The start index in the original text.</param>
/// <param name="Length">
/// The length in the original text. For an <see cref="TextSpanKind.Expression"/> span this includes the
/// opening <c>{</c> at <see cref="Start"/> and the closing <c>}</c> at <c>Start + Length - 1</c>.
/// </param>
public readonly record struct RawSegment(TextSpanKind Kind, int Start, int Length);

/// <summary>
/// Parses a notebook "text line" — prose that starts with the <c>#</c> marker and mixes literal text
/// with inline <c>{…}</c> expressions. This is the one place the marker and brace grammar are defined,
/// shared by the evaluator (which resolves the inline expressions) and the editor's inline highlighter
/// (which colors the braces and their contents). It is intentionally UI-free.
/// <para>
/// Grammar: a line is prose when its first non-whitespace character is <c>#</c>. Everything after the
/// marker (and one optional following space) is literal text, except <c>{…}</c> regions which are
/// expressions. <c>{{</c> and <c>}}</c> are literal <c>{</c> / <c>}</c>. A <c>{</c> with no matching
/// <c>}</c> is treated as ordinary literal text (never an error).
/// </para>
/// </summary>
public static class TextLineParser
{
    /// <summary>The leading character that marks a line as prose.</summary>
    public const char Marker = '#';

    /// <summary>
    /// Gets whether the given text is a prose line (its first non-whitespace character is the marker).
    /// </summary>
    /// <param name="text">The raw line text.</param>
    /// <returns>Returns <c>true</c> if the line is a text line.</returns>
    public static bool IsTextLine(string? text) => TryGetMarker(text, out _);

    /// <summary>
    /// Finds the marker at the start of the line (allowing leading whitespace).
    /// </summary>
    /// <param name="text">The raw line text.</param>
    /// <param name="markerIndex">The index of the marker character, or <c>-1</c>.</param>
    /// <returns>Returns <c>true</c> if the line starts (after any whitespace) with the marker.</returns>
    public static bool TryGetMarker(string? text, out int markerIndex)
    {
        markerIndex = -1;
        if (string.IsNullOrEmpty(text))
            return false;

        for (int i = 0; i < text.Length; i++)
        {
            char c = text[i];
            if (c == ' ' || c == '\t')
                continue;
            if (c == Marker)
            {
                markerIndex = i;
                return true;
            }
            return false;
        }
        return false;
    }

    /// <summary>
    /// Returns the index where the line's content begins: just past the marker, skipping a single
    /// space if one follows (so <c>"# note"</c> renders as <c>"note"</c>, not <c>" note"</c>).
    /// </summary>
    /// <param name="text">The raw line text.</param>
    /// <param name="markerIndex">The marker index from <see cref="TryGetMarker"/>.</param>
    /// <returns>Returns the content start index.</returns>
    public static int GetContentStart(string text, int markerIndex)
    {
        int i = markerIndex + 1;
        if (i < text.Length && text[i] == ' ')
            i++;
        return i;
    }

    /// <summary>
    /// Scans the line's content into a contiguous list of literal / expression spans that together
    /// tile <c>[start, text.Length)</c>.
    /// </summary>
    /// <param name="text">The raw line text.</param>
    /// <param name="start">The index to begin scanning (typically <see cref="GetContentStart"/>).</param>
    /// <returns>Returns the ordered spans.</returns>
    public static List<RawSegment> ScanSegments(string text, int start)
    {
        var segments = new List<RawSegment>();
        int literalStart = start;
        int i = start;

        void FlushLiteral(int end)
        {
            if (end > literalStart)
                segments.Add(new RawSegment(TextSpanKind.Literal, literalStart, end - literalStart));
        }

        while (i < text.Length)
        {
            char c = text[i];

            // Escapes stay part of the surrounding literal run.
            if ((c == '{' || c == '}') && i + 1 < text.Length && text[i + 1] == c)
            {
                i += 2;
                continue;
            }

            if (c == '{')
            {
                int close = text.IndexOf('}', i + 1);
                if (close < 0)
                    break; // Unclosed brace: the rest is literal text.

                FlushLiteral(i);
                segments.Add(new RawSegment(TextSpanKind.Expression, i, close - i + 1));
                i = close + 1;
                literalStart = i;
                continue;
            }

            i++;
        }

        FlushLiteral(text.Length);
        return segments;
    }

    /// <summary>
    /// Collapses brace escapes (<c>{{</c> → <c>{</c>, <c>}}</c> → <c>}</c>) in a literal span for display.
    /// </summary>
    /// <param name="literal">The literal text as written.</param>
    /// <returns>Returns the display text.</returns>
    public static string Unescape(string literal) =>
        literal.IndexOf('{') < 0 && literal.IndexOf('}') < 0
            ? literal
            : literal.Replace("{{", "{").Replace("}}", "}");
}
