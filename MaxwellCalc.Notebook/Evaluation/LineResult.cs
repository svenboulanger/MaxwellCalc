using MaxwellCalc.Core.Units;

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
}

/// <summary>
/// The immutable outcome of evaluating a single sheet line. Produced by
/// <see cref="SheetEvaluator"/> and copied onto a line view model for rendering.
/// </summary>
/// <param name="Kind">The kind of result.</param>
/// <param name="Quantity">The formatted quantity for <see cref="LineKind.Value"/> / <see cref="LineKind.Assign"/> lines.</param>
/// <param name="IsConstBadge">Whether the whole line is a single constant identifier (renders a <c>const</c> pill).</param>
/// <param name="AutoUnitSelected">Whether the output unit was auto-selected by the workspace (value ≥ 1 rule).</param>
/// <param name="ErrorMessage">The joined diagnostic message for <see cref="LineKind.Error"/> lines.</param>
public readonly record struct LineResult(
    LineKind Kind,
    Quantity<string> Quantity,
    bool IsConstBadge,
    bool AutoUnitSelected,
    string? ErrorMessage)
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
}
