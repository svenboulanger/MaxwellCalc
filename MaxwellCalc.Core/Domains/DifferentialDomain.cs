using MaxwellCalc.Core.Units;
using MaxwellCalc.Core.Workspaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;

namespace MaxwellCalc.Core.Domains;

/// <summary>
/// An <see cref="IDomain{T}"/> that works for <see cref="Differential{T}"/> values.
/// </summary>
/// <typeparam name="T">The scalar type.</typeparam>
public class DifferentialDomain<T> : IDomain<Differential<T>> where T : IFormattable, IEquatable<T>
{
    private readonly IWorkspace<T>.BuiltInFunctionDelegate _ln;

    /// <summary>
    /// Gets the domain for <typeparamref name="T"/> that should be used.
    /// </summary>
    public IDomain<T> Domain { get; }

    /// <inheritdoc />
    public Differential<T> One { get; }

    /// <inheritdoc />
    public Quantity<Differential<T>> Default { get; }

    /// <inheritdoc />
    public JsonConverter<Differential<T>> Converter => throw new NotImplementedException();

    /// <summary>
    /// Creates a new <see cref="DifferentialDomain{T}"/>.
    /// </summary>
    /// <param name="domain">The scalar domain.</param>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="domain"/> is <c>null</c>.</exception>
    public DifferentialDomain(IDomain<T> domain, IWorkspace<T>.BuiltInFunctionDelegate ln)
    {
        Domain = domain ?? throw new ArgumentNullException(nameof(domain));
        Default = new Quantity<Differential<T>>(
            new Differential<T>(Domain.Default.Scalar), Unit.UnitNone);
        One = new Differential<T>(Domain.One);
        _ln = ln ?? throw new ArgumentNullException();
    }

    /// <inheritdoc />
    public bool TryScalar(string scalar, IDiagnosticsHandler? diagnostics, out Quantity<Differential<T>> result)
    {
        if (!Domain.TryScalar(scalar, diagnostics, out var domainResult))
        {
            result = Default;
            return false;
        }
        result = new Quantity<Differential<T>>(new Differential<T>(domainResult.Scalar), domainResult.Unit);
        return true;
    }

    /// <inheritdoc />
    public bool TryPlus(Quantity<Differential<T>> a, IDiagnosticsHandler? diagnostics, out Quantity<Differential<T>> result)
    {
        result = a;
        return true;
    }

    /// <inheritdoc />
    public bool TryMinus(Quantity<Differential<T>> a, IDiagnosticsHandler? diagnostics, out Quantity<Differential<T>> result)
    {
        if (!Domain.TryMinus(new(a.Scalar.Value, a.Unit), diagnostics, out var value))
        {
            result = Default;
            return false;
        }
        var derivatives = new (string, T)[a.Scalar.Derivatives.Count];
        int index = 0;
        foreach (var pair in a.Scalar.Derivatives)
        {
            if (!Domain.TryMinus(new(pair.Value, a.Unit), diagnostics, out var derivative))
            {
                result = Default;
                return false;
            }
            derivatives[index++] = (pair.Key, pair.Value);
        }
        result = new Quantity<Differential<T>>(new Differential<T>(value.Scalar, derivatives), a.Unit);
        return true;
    }

    /// <inheritdoc />
    public bool TryFactorial(Quantity<Differential<T>> a, IDiagnosticsHandler? diagnostics, out Quantity<Differential<T>> result)
    {
        if (!CheckNoDerivatives(a, diagnostics, out result))
            return false;
        if (!Domain.TryFactorial(new Quantity<T>(a.Scalar.Value, a.Unit), diagnostics, out var sub))
        {
            result = Default;
            return false;
        }
        result = new Quantity<Differential<T>>(new Differential<T>(sub.Scalar), sub.Unit);
        return true;
    }

    /// <inheritdoc />
    public bool TryAdd(Quantity<Differential<T>> a, Quantity<Differential<T>> b, IDiagnosticsHandler? diagnostics, out Quantity<Differential<T>> result)
    {
        if (a.Unit != b.Unit)
        {
            diagnostics?.PostDiagnosticMessage(new("The units do not match"));
            result = Default;
            return false;
        }
        if (!Domain.TryAdd(new Quantity<T>(a.Scalar.Value, Unit.UnitNone), new Quantity<T>(b.Scalar.Value, Unit.UnitNone), diagnostics, out var value))
        {
            result = Default;
            return false;
        }
        var derivatives = new Dictionary<string, T>();
        foreach (var pair in a.Scalar.Derivatives)
            derivatives[pair.Key] = pair.Value;
        foreach (var pair in b.Scalar.Derivatives)
        {
            if (derivatives.TryGetValue(pair.Key, out var left))
            {
                if (!Domain.TryAdd(new Quantity<T>(left, Unit.UnitNone), new Quantity<T>(pair.Value, Unit.UnitNone), diagnostics, out var sum))
                {
                    result = Default;
                    return false;
                }
                derivatives[pair.Key] = sum.Scalar;
            }
            else
                derivatives[pair.Key] = pair.Value;
        }
        result = new Quantity<Differential<T>>(new Differential<T>(value.Scalar, [.. derivatives.Select(p => (p.Key, p.Value))]), a.Unit);
        return true;
    }

    /// <inheritdoc />
    public bool TrySubtract(Quantity<Differential<T>> a, Quantity<Differential<T>> b, IDiagnosticsHandler? diagnostics, out Quantity<Differential<T>> result)
    {
        if (a.Unit != b.Unit)
        {
            diagnostics?.PostDiagnosticMessage(new("The units do not match"));
            result = Default;
            return false;
        }
        if (!Domain.TryAdd(new Quantity<T>(a.Scalar.Value, Unit.UnitNone), new Quantity<T>(b.Scalar.Value, Unit.UnitNone), diagnostics, out var value))
        {
            result = Default;
            return false;
        }
        var derivatives = new Dictionary<string, T>();
        foreach (var pair in a.Scalar.Derivatives)
            derivatives[pair.Key] = pair.Value;
        foreach (var pair in b.Scalar.Derivatives)
        {
            if (derivatives.TryGetValue(pair.Key, out var left))
            {
                if (!Domain.TrySubtract(new Quantity<T>(left, Unit.UnitNone), new Quantity<T>(pair.Value, Unit.UnitNone), diagnostics, out var sum))
                {
                    result = Default;
                    return false;
                }
                derivatives[pair.Key] = sum.Scalar;
            }
            else
            {
                if (!Domain.TryMinus(new Quantity<T>(pair.Value, Unit.UnitNone), diagnostics, out var sum))
                {
                    result = Default;
                    return false;
                }
                derivatives[pair.Key] = sum.Scalar;
            }
        }
        result = new Quantity<Differential<T>>(new Differential<T>(value.Scalar, [.. derivatives.Select(p => (p.Key, p.Value))]), a.Unit);
        return true;
    }

    /// <inheritdoc />
    /// <remarks>Rule: d(f * g) = f * dg + g * df</remarks>
    public bool TryMultiply(Quantity<Differential<T>> a, Quantity<Differential<T>> b, IDiagnosticsHandler? diagnostics, out Quantity<Differential<T>> result)
    {
        var left = new Quantity<T>(a.Scalar.Value, Unit.UnitNone);
        var right = new Quantity<T>(b.Scalar.Value, Unit.UnitNone);

        // Calculate the value
        if (!Domain.TryMultiply(left, right, diagnostics, out var value))
        {
            result = Default;
            return false;
        }

        var dict = new Dictionary<string, T>();
        foreach (var pair in a.Scalar.Derivatives)
        {
            if (!Domain.TryMultiply(new Quantity<T>(pair.Value, Unit.UnitNone), right, diagnostics, out var fdg))
            {
                result = Default;
                return false;
            }
            dict[pair.Key] = fdg.Scalar;
        }
        foreach (var pair in b.Scalar.Derivatives)
        {
            if (!Domain.TryMultiply(new Quantity<T>(pair.Value, Unit.UnitNone), left, diagnostics, out var gdf))
            {
                result = Default;
                return false;
            }
            if (dict.TryGetValue(pair.Key, out var fdg))
            {
                if (!Domain.TryAdd(new Quantity<T>(fdg, Unit.UnitNone), gdf, diagnostics, out var sum))
                {
                    result = Default;
                    return false;
                }
                dict[pair.Key] = sum.Scalar;
            }
            else
                dict[pair.Key] = gdf.Scalar;
        }
        result = new Quantity<Differential<T>>(new Differential<T>(value.Scalar, [.. dict.Select(p => (p.Key, p.Value))]), a.Unit * b.Unit);
        return true;
    }

    /// <inheritdoc />
    /// <remarks>Rule: d(f / g) = (g * df - f * dg) / g^2</remarks>
    public bool TryDivide(Quantity<Differential<T>> a, Quantity<Differential<T>> b, IDiagnosticsHandler? diagnostics, out Quantity<Differential<T>> result)
    {
        var left = new Quantity<T>(a.Scalar.Value, Unit.UnitNone);
        var right = new Quantity<T>(b.Scalar.Value, Unit.UnitNone);

        // Calculate the value
        if (!Domain.TryDivide(left, right, diagnostics, out var value))
        {
            result = Default;
            return false;
        }

        var dict = new Dictionary<string, T>();

        // First calculate the numerator
        foreach (var pair in a.Scalar.Derivatives)
        {
            if (!Domain.TryMultiply(right, new Quantity<T>(pair.Value, Unit.UnitNone), diagnostics, out var gdf))
            {
                result = Default;
                return false;
            }
            dict[pair.Key] = gdf.Scalar;
        }
        foreach (var pair in b.Scalar.Derivatives)
        {
            if (!Domain.TryMultiply(left, new Quantity<T>(pair.Value, Unit.UnitNone), diagnostics, out var fdg))
            {
                result = Default;
                return false;
            }
            if (dict.TryGetValue(pair.Key, out var gdf))
            {
                if (!Domain.TrySubtract(new Quantity<T>(gdf, Unit.UnitNone), fdg, diagnostics, out var numerator))
                {
                    result = Default;
                    return false;
                }
                dict[pair.Key] = numerator.Scalar;
            }
            else
            {
                if (!Domain.TryMinus(new Quantity<T>(pair.Value, Unit.UnitNone), diagnostics, out var numerator))
                {
                    result = Default;
                    return false;
                }
                dict[pair.Key] = numerator.Scalar;
            }
        }

        // Divide all by the denominator squared
        if (!Domain.TryMultiply(right, right, diagnostics, out right))
        {
            result = Default;
            return false;
        }
        foreach (var pair in dict.ToArray())
        {
            if (!Domain.TryDivide(new Quantity<T>(pair.Value, Unit.UnitNone), right, diagnostics, out var diff))
            {
                result = Default;
                return false;
            }
            dict[pair.Key] = diff.Scalar;
        }
        result = new Quantity<Differential<T>>(new Differential<T>(value.Scalar, [.. dict.Select(p => (p.Key, p.Value))]), a.Unit * b.Unit);
        return true;
    }

    /// <inheritdoc />
    /// <remarks>Rule: d(!f) does not exist.</remarks>
    public bool TryInvert(Quantity<Differential<T>> a, IDiagnosticsHandler? diagnostics, out Quantity<Differential<T>> result)
    {
        if (!CheckNoDerivatives(a, diagnostics, out result))
            return false;
        if (!Domain.TryInvert(new Quantity<T>(a.Scalar.Value, a.Unit), diagnostics, out var scalar))
            return false;
        result = new Quantity<Differential<T>>(new Differential<T>(scalar.Scalar), scalar.Unit);
        return true;
    }

    /// <inheritdoc />
    /// <remarks>Rule: d(a % b) does not exist.</remarks>
    public bool TryModulo(Quantity<Differential<T>> a, Quantity<Differential<T>> b, IDiagnosticsHandler? diagnostics, out Quantity<Differential<T>> result)
    {
        if (!CheckNoDerivatives(a, diagnostics, out result))
            return false;
        if (!Domain.TryModulo(new Quantity<T>(a.Scalar.Value, a.Unit), new Quantity<T>(b.Scalar.Value, b.Unit), diagnostics, out var scalar))
            return false;
        result = new Quantity<Differential<T>>(new Differential<T>(scalar.Scalar), scalar.Unit);
        return true;
    }

    /// <inheritdoc />
    public bool TryIntDivide(Quantity<Differential<T>> a, Quantity<Differential<T>> b, IDiagnosticsHandler? diagnostics, out Quantity<Differential<T>> result)
    {
        if (!CheckNoDerivatives(a, diagnostics, out result))
            return false;
        if (!Domain.TryIntDivide(new Quantity<T>(a.Scalar.Value, a.Unit), new Quantity<T>(b.Scalar.Value, b.Unit), diagnostics, out var scalar))
            return false;
        result = new Quantity<Differential<T>>(new Differential<T>(scalar.Scalar), scalar.Unit);
        return true;
    }

    /// <inheritdoc />
    /// <remarks>Rule: d(a ^ b) = (a ^ b) * (db * ln(a) + b / a * da) = (a^b) * ln(a) * db + b * a^(b-1) * da</remarks>
    public bool TryPow(Quantity<Differential<T>> a, Quantity<Differential<T>> b, IDiagnosticsHandler? diagnostics, out Quantity<Differential<T>> result)
    {
        // First the value
        if (!Domain.TryPow(new Quantity<T>(a.Scalar.Value, a.Unit), new Quantity<T>(b.Scalar.Value, b.Unit), diagnostics, out var value))
        {
            result = Default;
            return false;
        }
        var left = new Quantity<T>(a.Scalar.Value, Unit.UnitNone);
        var right = new Quantity<T>(b.Scalar.Value, Unit.UnitNone);

        // d(a^b) = b * a^(n-1) * da
        var dict = new Dictionary<string, T>();
        foreach (var pair in a.Scalar.Derivatives)
        {
            if (!Domain.TrySubtract(right, new Quantity<T>(Domain.One, Unit.UnitNone), diagnostics, out var expMinusOne) ||
                !Domain.TryPow(left, expMinusOne, diagnostics, out var aPowNMinusOne) ||
                !Domain.TryMultiply(right, aPowNMinusOne, diagnostics, out var coefficient) ||
                !Domain.TryMultiply(coefficient, new Quantity<T>(pair.Value, Unit.UnitNone), diagnostics, out var derivative))
            {
                result = Default;
                return false;
            }
            dict[pair.Key] = derivative.Scalar;
        }

        // d(a^b) for b = (a^b) * ln(a) * db
        foreach (var pair in b.Scalar.Derivatives)
        {
            if (!_ln([new Quantity<T>(a.Scalar.Value, Unit.UnitNone)], diagnostics, out var ln) ||
                !Domain.TryMultiply(new Quantity<T>(value.Scalar, Unit.UnitNone), ln, diagnostics, out var coefficient) ||
                !Domain.TryMultiply(coefficient, new Quantity<T>(pair.Value, Unit.UnitNone), diagnostics, out var derivative))
            {
                result = Default;
                return false;
            }
            if (dict.TryGetValue(pair.Key, out var existing))
            {
                if (!Domain.TryAdd(new Quantity<T>(existing, Unit.UnitNone), derivative, diagnostics, out var sum))
                {
                    result = Default;
                    return false;
                }
                dict[pair.Key] = sum.Scalar;
            }
            else
                dict[pair.Key] = derivative.Scalar;
        }

        result = new Quantity<Differential<T>>(new Differential<T>(value.Scalar, [.. dict.Select(p => (p.Key, p.Value))]), value.Unit);
        return true;
    }

    /// <inheritdoc />
    public bool TryBitwiseOr(Quantity<Differential<T>> a, Quantity<Differential<T>> b, IDiagnosticsHandler? diagnostics, out Quantity<Differential<T>> result)
    {
        if (!CheckNoDerivatives(a, diagnostics, out result))
            return false;
        if (!Domain.TryBitwiseOr(new Quantity<T>(a.Scalar.Value, a.Unit), new Quantity<T>(b.Scalar.Value, b.Unit), diagnostics, out var scalar))
            return false;
        result = new Quantity<Differential<T>>(new Differential<T>(scalar.Scalar), scalar.Unit);
        return true;
    }

    /// <inheritdoc />
    /// <remarks>Comparison removes any derivatives.</remarks>
    public bool TryLogicalOr(Quantity<Differential<T>> a, Quantity<Differential<T>> b, IDiagnosticsHandler? diagnostics, out Quantity<Differential<T>> result)
    {
        if (!Domain.TryLogicalOr(new Quantity<T>(a.Scalar.Value, a.Unit), new Quantity<T>(b.Scalar.Value, b.Unit), diagnostics, out var scalar))
        {
            result = Default;
            return false;
        }
        result = new Quantity<Differential<T>>(new Differential<T>(scalar.Scalar), scalar.Unit);
        return true;
    }

    /// <inheritdoc />
    public bool TryBitwiseAnd(Quantity<Differential<T>> a, Quantity<Differential<T>> b, IDiagnosticsHandler? diagnostics, out Quantity<Differential<T>> result)
    {
        if (!CheckNoDerivatives(a, diagnostics, out result))
            return false;
        if (!Domain.TryBitwiseAnd(new Quantity<T>(a.Scalar.Value, a.Unit), new Quantity<T>(b.Scalar.Value, b.Unit), diagnostics, out var scalar))
            return false;
        result = new Quantity<Differential<T>>(new Differential<T>(scalar.Scalar), scalar.Unit);
        return true;
    }

    /// <inheritdoc />
    public bool TryLogicalAnd(Quantity<Differential<T>> a, Quantity<Differential<T>> b, IDiagnosticsHandler? diagnostics, out Quantity<Differential<T>> result)
    {
        if (!Domain.TryLogicalAnd(new Quantity<T>(a.Scalar.Value, a.Unit), new Quantity<T>(b.Scalar.Value, b.Unit), diagnostics, out var scalar))
        {
            result = Default;
            return false;
        }
        result = new Quantity<Differential<T>>(new Differential<T>(scalar.Scalar), scalar.Unit);
        return true;
    }

    /// <inheritdoc />
    public bool TryLeftShift(Quantity<Differential<T>> a, Quantity<Differential<T>> b, IDiagnosticsHandler? diagnostics, out Quantity<Differential<T>> result)
    {
        if (!CheckNoDerivatives(a, diagnostics, out result))
            return false;
        if (!Domain.TryLeftShift(new Quantity<T>(a.Scalar.Value, a.Unit), new Quantity<T>(b.Scalar.Value, b.Unit), diagnostics, out var scalar))
            return false;
        result = new Quantity<Differential<T>>(new Differential<T>(scalar.Scalar), scalar.Unit);
        return true;
    }

    /// <inheritdoc />
    public bool TryRightShift(Quantity<Differential<T>> a, Quantity<Differential<T>> b, IDiagnosticsHandler? diagnostics, out Quantity<Differential<T>> result)
    {
        if (!CheckNoDerivatives(a, diagnostics, out result))
            return false;
        if (!Domain.TryRightShift(new Quantity<T>(a.Scalar.Value, a.Unit), new Quantity<T>(b.Scalar.Value, b.Unit), diagnostics, out var scalar))
            return false;
        result = new Quantity<Differential<T>>(new Differential<T>(scalar.Scalar), scalar.Unit);
        return true;
    }

    /// <inheritdoc />
    public bool TryGreaterThan(Quantity<Differential<T>> a, Quantity<Differential<T>> b, IDiagnosticsHandler? diagnostics, out Quantity<Differential<T>> result)
    {
        if (!Domain.TryGreaterThan(new Quantity<T>(a.Scalar.Value, a.Unit), new Quantity<T>(b.Scalar.Value, b.Unit), diagnostics, out var scalar))
        {
            result = Default;
            return false;
        }
        result = new Quantity<Differential<T>>(new Differential<T>(scalar.Scalar), scalar.Unit);
        return true;
    }

    /// <inheritdoc />
    public bool TryGreaterThanOrEqual(Quantity<Differential<T>> a, Quantity<Differential<T>> b, IDiagnosticsHandler? diagnostics, out Quantity<Differential<T>> result)
    {
        if (!Domain.TryGreaterThanOrEqual(new Quantity<T>(a.Scalar.Value, a.Unit), new Quantity<T>(b.Scalar.Value, b.Unit), diagnostics, out var scalar))
        {
            result = Default;
            return false;
        }
        result = new Quantity<Differential<T>>(new Differential<T>(scalar.Scalar), scalar.Unit);
        return true;
    }

    /// <inheritdoc />
    public bool TryLessThan(Quantity<Differential<T>> a, Quantity<Differential<T>> b, IDiagnosticsHandler? diagnostics, out Quantity<Differential<T>> result)
    {
        if (!Domain.TryLessThan(new Quantity<T>(a.Scalar.Value, a.Unit), new Quantity<T>(b.Scalar.Value, b.Unit), diagnostics, out var scalar))
        {
            result = Default;
            return false;
        }
        result = new Quantity<Differential<T>>(new Differential<T>(scalar.Scalar), scalar.Unit);
        return true;
    }

    /// <inheritdoc />
    public bool TryLessThanOrEqual(Quantity<Differential<T>> a, Quantity<Differential<T>> b, IDiagnosticsHandler? diagnostics, out Quantity<Differential<T>> result)
    {
        if (!Domain.TryLessThanOrEqual(new Quantity<T>(a.Scalar.Value, a.Unit), new Quantity<T>(b.Scalar.Value, b.Unit), diagnostics, out var scalar))
        {
            result = Default;
            return false;
        }
        result = new Quantity<Differential<T>>(new Differential<T>(scalar.Scalar), scalar.Unit);
        return true;
    }

    /// <inheritdoc />
    public bool TryEquals(Quantity<Differential<T>> a, Quantity<Differential<T>> b, IDiagnosticsHandler? diagnostics, out Quantity<Differential<T>> result)
    {
        if (!Domain.TryEquals(new Quantity<T>(a.Scalar.Value, a.Unit), new Quantity<T>(b.Scalar.Value, b.Unit), diagnostics, out var scalar))
        {
            result = Default;
            return false;
        }
        result = new Quantity<Differential<T>>(new Differential<T>(scalar.Scalar), scalar.Unit);
        return true;
    }

    /// <inheritdoc />
    public bool TryNotEquals(Quantity<Differential<T>> a, Quantity<Differential<T>> b, IDiagnosticsHandler? diagnostics, out Quantity<Differential<T>> result)
    {
        if (!Domain.TryNotEquals(new Quantity<T>(a.Scalar.Value, a.Unit), new Quantity<T>(b.Scalar.Value, b.Unit), diagnostics, out var scalar))
        {
            result = Default;
            return false;
        }
        result = new Quantity<Differential<T>>(new Differential<T>(scalar.Scalar), scalar.Unit);
        return true;
    }

    /// <inheritdoc />
    public bool TryIsTrue(Quantity<Differential<T>> a, IDiagnosticsHandler? diagnostics, out bool result)
        => Domain.TryIsTrue(new Quantity<T>(a.Scalar.Value, a.Unit), diagnostics, out result);

    /// <inheritdoc />
    public bool TryFactor(Quantity<Differential<T>> a, Quantity<Differential<T>> unit, out double factor)
        => Domain.TryFactor(new Quantity<T>(a.Scalar.Value, a.Unit), new Quantity<T>(unit.Scalar.Value, unit.Unit), out factor);

    /// <inheritdoc />
    public bool TryFormat(Quantity<Differential<T>> value, string? format, IFormatProvider? formatProvider, out Quantity<string> result)
    {
        // We will use the domain to format everything
        var sb = new StringBuilder();
        if (!Domain.TryFormat(new Quantity<T>(value.Scalar.Value, value.Unit), format, formatProvider, out result))
            return false;

        sb.Append(result.Scalar);

        // We are going to add to the value the derivative of each
        foreach (var pair in value.Scalar.Derivatives)
        {
            if (!Domain.TryFormat(new Quantity<T>(pair.Value, Unit.UnitNone), format, formatProvider, out var derivative))
                return false;
            sb.Append(" + ");
            sb.Append(derivative.Scalar);
            sb.Append(" d(");
            sb.Append(pair.Key);
            sb.Append(")");
        }
        result = new Quantity<string>(sb.ToString(), result.Unit);
        return true;
    }

    private bool CheckNoDerivatives(Quantity<Differential<T>> a, IDiagnosticsHandler? diagnostics, out Quantity<Differential<T>> result)
    {
        result = Default;
        if (a.Scalar.Derivatives.Count > 0)
        {
            diagnostics?.PostDiagnosticMessage(new("Cannot take the factorial with derivatives"));
            return false;
        }
        return true;
    }
}
