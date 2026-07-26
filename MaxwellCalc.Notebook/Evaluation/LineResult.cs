using MaxwellCalc.Core.Units;
using System.Collections.Generic;

namespace MaxwellCalc.Notebook.Evaluation;

/// <summary>
/// The kind of result a sheet line produced when it was evaluated.
/// </summary>
public enum LineKind
{
    /// <summary>The line is blank (or whitespace only).</summary>
    Empty,

    /// <summary>The line is an expression that resolved to a value.</summary>
    Value,

    /// <summary>The line assigns a value to a variable (e.g. <c>mass = 70 kg</c>).</summary>
    Assign,

    /// <summary>The line defines a function (e.g. <c>f(x) = x^2 + 1</c>).</summary>
    FuncDef,

    /// <summary>The line could not be parsed or evaluated.</summary>
    Error,

    /// <summary>
    /// The line is prose (starts with the <c>#</c> marker) that mixes literal text with inline
    /// <c>{…}</c> expressions. Its rendered form is carried in <see cref="LineResult.Segments"/>.
    /// </summary>
    Text,
}

/// <summary>
/// One piece of a <see cref="LineKind.Text"/> line: either a literal prose span or an evaluated
/// inline <c>{…}</c> expression. Exactly one of <see cref="Literal"/> / <see cref="Expression"/> is set.
/// </summary>
/// <param name="Literal">
/// The literal prose text (already un-escaped, so <c>{{</c>/<c>}}</c> are collapsed to <c>{</c>/<c>}</c>),
/// or <c>null</c> when this segment is an inline expression.
/// </param>
/// <param name="Expression">
/// The result of evaluating an inline <c>{…}</c> expression (a Value / Assign / FuncDef / Error), or
/// <c>null</c> when this segment is literal text. An inline assignment binds into the sheet's transient
/// scope exactly like an assignment line, so later segments and later lines can reference it.
/// </param>
/// <param name="RawSource">The original <c>{…}</c> text (braces included), used to render error segments.</param>
public readonly record struct TextSegment(
    string? Literal,
    LineResult? Expression,
    string RawSource);

/// <summary>
/// The immutable outcome of evaluating a single sheet line. Produced by
/// <see cref="SheetEvaluator"/> and copied onto a line view model for rendering.
/// </summary>
/// <param name="Kind">The kind of result.</param>
/// <param name="Quantity">The formatted quantity for <see cref="LineKind.Value"/> / <see cref="LineKind.Assign"/> lines.</param>
/// <param name="IsConstBadge">Whether the whole line is a single constant identifier (renders a <c>const</c> pill).</param>
/// <param name="AutoUnitSelected">Whether the output unit was auto-selected by the workspace (value ≥ 1 rule).</param>
/// <param name="ErrorMessage">The joined diagnostic message for <see cref="LineKind.Error"/> lines.</param>
/// <param name="DefinedName">
/// The name defined on this line, used by the overlay's <c>from sheet</c> listing (Step 8): the
/// variable name for an <see cref="LineKind.Assign"/> line, or the <c>name(params)</c> signature for a
/// <see cref="LineKind.FuncDef"/> line. <c>null</c> for every other kind.
/// </param>
/// <param name="Segments">
/// The rendered pieces of a <see cref="LineKind.Text"/> line (literal spans interleaved with evaluated
/// inline expressions), or <c>null</c> for every other kind.
/// </param>
public readonly record struct LineResult(
    LineKind Kind,
    Quantity<string> Quantity,
    bool IsConstBadge,
    bool AutoUnitSelected,
    string? ErrorMessage,
    string? DefinedName = null,
    IReadOnlyList<TextSegment>? Segments = null)
{
    /// <summary>
    /// Gets the result for an empty line.
    /// </summary>
    public static LineResult Empty { get; } = new(LineKind.Empty, default, false, false, null);

    /// <summary>
    /// Creates an error result with the given message.
    /// </summary>
    /// <param name="message">The diagnostic message.</param>
    /// <returns>Returns the error result.</returns>
    public static LineResult Error(string message) => new(LineKind.Error, default, false, false, message);

    /// <summary>
    /// Creates a prose (text) result from its rendered segments.
    /// </summary>
    /// <param name="segments">The literal / inline-expression pieces, in order.</param>
    /// <returns>Returns the text result.</returns>
    public static LineResult Text(IReadOnlyList<TextSegment> segments) =>
        new(LineKind.Text, default, false, false, null, null, segments);
}
