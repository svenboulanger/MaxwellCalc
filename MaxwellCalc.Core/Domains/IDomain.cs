using MaxwellCalc.Core.Units;
using MaxwellCalc.Core.Workspaces;
using System;
using System.Text.Json.Serialization;

namespace MaxwellCalc.Core.Domains;

/// <summary>
/// A resolver that allows resolving nodes to a result.
/// </summary>
/// <typeparam name="T">The result type.</typeparam>
public interface IDomain<T> where T : IFormattable
{
    /// <summary>
    /// Gets a default value to return in case of an error.
    /// </summary>
    public Quantity<T> Default { get; }

    /// <summary>
    /// Gets a representation for a unit.
    /// </summary>
    /// <remarks>Used for example when making a quantity from units. a "m" becomes "1m".</remarks>
    public T One { get; }

    /// <summary>
    /// Gets a JSON converter that can be used by the diagnostics to translate scalars to JSON.
    /// </summary>
    public JsonConverter<T> Converter { get; }

    /// <summary>
    /// Evaluates a scalar value from a string.
    /// </summary>
    /// <param name="scalar">The scalar value.</param>
    /// <param name="diagnostics">The diagnostics.</param>
    /// <param name="result">The result.</param>
    /// <returns>Returns <c>true</c> if the evaluation happened; otherwise, <c>false</c>.</returns>
    public bool TryScalar(string scalar, IDiagnosticsHandler? diagnostics, out Quantity<T> result);

    /// <summary>
    /// Evaluates the unary plus operator.
    /// </summary>
    /// <param name="a">The argument.</param>
    /// <param name="diagnostics">The diagnostics.</param>
    /// <param name="result">The result.</param>
    /// <returns>Returns <c>true</c> if the evaluation happened; otherwise, <c>false</c>.</returns>
    public bool TryPlus(Quantity<T> a, IDiagnosticsHandler? diagnostics, out Quantity<T> result);

    /// <summary>
    /// Evaluates the unary minus operator.
    /// </summary>
    /// <param name="a">The argument.</param>
    /// <param name="diagnostics">The diagnostics.</param>
    /// <param name="result">The result.</param>
    /// <returns>Returns <c>true</c> if the evaluation happened; otherwise, <c>false</c>.</returns>
    public bool TryMinus(Quantity<T> a, IDiagnosticsHandler? diagnostics, out Quantity<T> result);

    /// <summary>
    /// Evaluates the unary factorial operator.
    /// </summary>
    /// <param name="a">The argument.</param>
    /// <param name="diagnostics">The diagnostics.</param>
    /// <param name="result">The result.</param>
    /// <returns>Returns <c>true</c> if the evaluation happened; otherwise, <c>false</c>.</returns>
    public bool TryFactorial(Quantity<T> a, IDiagnosticsHandler? diagnostics, out Quantity<T> result);

    /// <summary>
    /// Evaluates the binary addition operator.
    /// </summary>
    /// <param name="a">The left argument.</param>
    /// <param name="b">The right argument.</param>
    /// <param name="diagnostics">The diagnostics.</param>
    /// <param name="result">The result.</param>
    /// <returns>Returns <c>true</c> if the evaluation happened; otherwise, <c>false</c>.</returns>
    public bool TryAdd(Quantity<T> a, Quantity<T> b, IDiagnosticsHandler? diagnostics, out Quantity<T> result);

    /// <summary>
    /// Evaluates the binary subtraction operator.
    /// </summary>
    /// <param name="a">The left argument.</param>
    /// <param name="b">The right argument.</param>
    /// <param name="diagnostics">The diagnostics.</param>
    /// <param name="result">The result.</param>
    /// <returns>Returns <c>true</c> if the evaluation happened; otherwise, <c>false</c>.</returns>
    public bool TrySubtract(Quantity<T> a, Quantity<T> b, IDiagnosticsHandler? diagnostics, out Quantity<T> result);

    /// <summary>
    /// Evaluates the binary multiplication operator.
    /// </summary>
    /// <param name="a">The left argument.</param>
    /// <param name="b">The right argument.</param>
    /// <param name="diagnostics">The diagnostics.</param>
    /// <param name="result">The result.</param>
    /// <returns>Returns <c>true</c> if the evaluation happened; otherwise, <c>false</c>.</returns>
    public bool TryMultiply(Quantity<T> a, Quantity<T> b, IDiagnosticsHandler? diagnostics, out Quantity<T> result);

    /// <summary>
    /// Evaluates the binary division operator.
    /// </summary>
    /// <param name="a">The left argument.</param>
    /// <param name="b">The right argument.</param>
    /// <param name="diagnostics">The diagnostics.</param>
    /// <param name="result">The result.</param>
    /// <returns>Returns <c>true</c> if the evaluation happened; otherwise, <c>false</c>.</returns>
    public bool TryDivide(Quantity<T> a, Quantity<T> b, IDiagnosticsHandler? diagnostics, out Quantity<T> result);

    /// <summary>
    /// Evaluates the inverted quantity.
    /// </summary>
    /// <param name="a">The argument.</param>
    /// <param name="diagnostics">The diagnostics.</param>
    /// <param name="result">The result.</param>
    /// <returns>Returns <c>true</c> if the evaluation happened; otherwise, <c>false</c>.</returns>
    public bool TryInvert(Quantity<T> a, IDiagnosticsHandler? diagnostics, out Quantity<T> result);

    /// <summary>
    /// Evaluates the binary modulo operator.
    /// </summary>
    /// <param name="a">The left argument.</param>
    /// <param name="b">The right argument.</param>
    /// <param name="diagnostics">The diagnostics.</param>
    /// <param name="result">The result.</param>
    /// <returns>Returns <c>true</c> if the evaluation happened; otherwise, <c>false</c>.</returns>
    public bool TryModulo(Quantity<T> a, Quantity<T> b, IDiagnosticsHandler? diagnostics, out Quantity<T> result);

    /// <summary>
    /// Evaluates the binary integer division operator.
    /// </summary>
    /// <param name="a">The left argument.</param>
    /// <param name="b">The right argument.</param>
    /// <param name="diagnostics">The diagnostics.</param>
    /// <param name="result">The result.</param>
    /// <returns>Returns <c>true</c> if the evaluation happened; otherwise, <c>false</c>.</returns>
    public bool TryIntDivide(Quantity<T> a, Quantity<T> b, IDiagnosticsHandler? diagnostics, out Quantity<T> result);

    /// <summary>
    /// Evaluates the binary exponentiation operator.
    /// </summary>
    /// <param name="a">The left argument.</param>
    /// <param name="b">The right argument.</param>
    /// <param name="diagnostics">The diagnostics.</param>
    /// <param name="result">The result.</param>
    /// <returns>Returns <c>true</c> if the evaluation happened; otherwise, <c>false</c>.</returns>
    public bool TryPow(Quantity<T> a, Quantity<T> b, IDiagnosticsHandler? diagnostics, out Quantity<T> result);

    /// <summary>
    /// Evaluates the binary bitwise OR operator.
    /// </summary>
    /// <param name="a">The left argument.</param>
    /// <param name="b">The right argument.</param>
    /// <param name="diagnostics">The diagnostics.</param>
    /// <param name="result">The result.</param>
    /// <returns>Returns <c>true</c> if the evaluation happened; otherwise, <c>false</c>.</returns>
    public bool TryBitwiseOr(Quantity<T> a, Quantity<T> b, IDiagnosticsHandler? diagnostics, out Quantity<T> result);

    /// <summary>
    /// Evaluates the binary logical OR operator.
    /// </summary>
    /// <param name="a">The left argument.</param>
    /// <param name="b">The right argument.</param>
    /// <param name="diagnostics">The diagnostics.</param>
    /// <param name="result">The result.</param>
    /// <returns>Returns <c>true</c> if the evaluation happened; otherwise, <c>false</c>.</returns>
    public bool TryLogicalOr(Quantity<T> a, Quantity<T> b, IDiagnosticsHandler? diagnostics, out Quantity<T> result);

    /// <summary>
    /// Evaluates the binary bitwise AND operator.
    /// </summary>
    /// <param name="a">The left argument.</param>
    /// <param name="b">The right argument.</param>
    /// <param name="diagnostics">The diagnostics.</param>
    /// <param name="result">The result.</param>
    /// <returns>Returns <c>true</c> if the evaluation happened; otherwise, <c>false</c>.</returns>
    public bool TryBitwiseAnd(Quantity<T> a, Quantity<T> b, IDiagnosticsHandler? diagnostics, out Quantity<T> result);

    /// <summary>
    /// Evaluates the binary logical AND operator.
    /// </summary>
    /// <param name="a">The left argument.</param>
    /// <param name="b">The right argument.</param>
    /// <param name="diagnostics">The diagnostics.</param>
    /// <param name="result">The result.</param>
    /// <returns>Returns <c>true</c> if the evaluation happened; otherwise, <c>false</c>.</returns>
    public bool TryLogicalAnd(Quantity<T> a, Quantity<T> b, IDiagnosticsHandler? diagnostics, out Quantity<T> result);

    /// <summary>
    /// Evaluates the binary left shift operator.
    /// </summary>
    /// <param name="a">The left argument.</param>
    /// <param name="b">The right argument.</param>
    /// <param name="diagnostics">The diagnostics.</param>
    /// <param name="result">The result.</param>
    /// <returns>Returns <c>true</c> if the evaluation happened; otherwise, <c>false</c>.</returns>
    public bool TryLeftShift(Quantity<T> a, Quantity<T> b, IDiagnosticsHandler? diagnostics, out Quantity<T> result);

    /// <summary>
    /// Evaluates the binary right shift operator.
    /// </summary>
    /// <param name="a">The left argument.</param>
    /// <param name="b">The right argument.</param>
    /// <param name="diagnostics">The diagnostics.</param>
    /// <param name="result">The result.</param>
    /// <returns>Returns <c>true</c> if the evaluation happened; otherwise, <c>false</c>.</returns>
    public bool TryRightShift(Quantity<T> a, Quantity<T> b, IDiagnosticsHandler? diagnostics, out Quantity<T> result);

    /// <summary>
    /// Evaluates the binary greater than operator.
    /// </summary>
    /// <param name="a">The left argument.</param>
    /// <param name="b">The right argument.</param>
    /// <param name="diagnostics">The diagnostics.</param>
    /// <param name="result">The result.</param>
    /// <returns>Returns <c>true</c> if the evaluation happened; otherwise, <c>false</c>.</returns>
    public bool TryGreaterThan(Quantity<T> a, Quantity<T> b, IDiagnosticsHandler? diagnostics, out Quantity<T> result);

    /// <summary>
    /// Evaluates the binary greater than or equals operator.
    /// </summary>
    /// <param name="a">The left argument.</param>
    /// <param name="b">The right argument.</param>
    /// <param name="diagnostics">The diagnostics.</param>
    /// <param name="result">The result.</param>
    /// <returns>Returns <c>true</c> if the evaluation happened; otherwise, <c>false</c>.</returns>
    public bool TryGreaterThanOrEqual(Quantity<T> a, Quantity<T> b, IDiagnosticsHandler? diagnostics, out Quantity<T> result);

    /// <summary>
    /// Evaluates the binary less than operator.
    /// </summary>
    /// <param name="a">The left argument.</param>
    /// <param name="b">The right argument.</param>
    /// <param name="diagnostics">The diagnostics.</param>
    /// <param name="result">The result.</param>
    /// <returns>Returns <c>true</c> if the evaluation happened; otherwise, <c>false</c>.</returns>
    public bool TryLessThan(Quantity<T> a, Quantity<T> b, IDiagnosticsHandler? diagnostics, out Quantity<T> result);

    /// <summary>
    /// Evaluates the binary less or equals operator.
    /// </summary>
    /// <param name="a">The left argument.</param>
    /// <param name="b">The right argument.</param>
    /// <param name="diagnostics">The diagnostics.</param>
    /// <param name="result">The result.</param>
    /// <returns>Returns <c>true</c> if the evaluation happened; otherwise, <c>false</c>.</returns>
    public bool TryLessThanOrEqual(Quantity<T> a, Quantity<T> b, IDiagnosticsHandler? diagnostics, out Quantity<T> result);

    /// <summary>
    /// Evaluates the binary equality operator.
    /// </summary>
    /// <param name="a">The left argument.</param>
    /// <param name="b">The right argument.</param>
    /// <param name="diagnostics">The diagnostics.</param>
    /// <param name="result">The result.</param>
    /// <returns>Returns <c>true</c> if the evaluation happened; otherwise, <c>false</c>.</returns>
    public bool TryEquals(Quantity<T> a, Quantity<T> b, IDiagnosticsHandler? diagnostics, out Quantity<T> result);

    /// <summary>
    /// Evaluates the binary inequality operator.
    /// </summary>
    /// <param name="a">The left argument.</param>
    /// <param name="b">The right argument.</param>
    /// <param name="diagnostics">The diagnostics.</param>
    /// <param name="result">The result.</param>
    /// <returns>Returns <c>true</c> if the evaluation happened; otherwise, <c>false</c>.</returns>
    public bool TryNotEquals(Quantity<T> a, Quantity<T> b, IDiagnosticsHandler? diagnostics, out Quantity<T> result);

    /// <summary>
    /// Evaluates whether a quantity should be treated as "true".
    /// </summary>
    /// <param name="a">The left argument.</param>
    /// <param name="diagnostics">The diagnostics.</param>
    /// <param name="result">The result.</param>
    /// <returns>Returns <c>true</c> if the evaluation happened; otherwise, <c>false</c>.</returns>
    public bool TryIsTrue(Quantity<T> a, IDiagnosticsHandler? diagnostics, out bool result);

    /// <summary>
    /// Evaluates a factor to determine which unit should be used to describe
    /// <paramref name="a"/>.
    /// </summary>
    /// <param name="a">The quantity to describe.</param>
    /// <param name="unit">The unit.</param>
    /// <param name="factor">The factor that should be considered when chosing the unit.</param>
    /// <returns>Returns <c>true</c> if the factor could be determined; otherwise, <c>false</c>.</returns>
    public bool TryFactor(Quantity<T> a, Quantity<T> unit, out double factor);

    /// <summary>
    /// Tries to format a quantity.
    /// </summary>
    /// <param name="value">The value.</param>
    /// <param name="format">The format, which will usually describe the number of digits.</param>
    /// <param name="formatProvider">The format provider.</param>
    /// <param name="result">The result.</param>
    /// <returns>Returns <c>true</c> if the formatting succeeded; otherwise, <c>false</c>.</returns>
    public bool TryFormat(Quantity<T> value, string? format, IFormatProvider? formatProvider, out Quantity<string> result);
}
