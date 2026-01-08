using MaxwellCalc.Core.Units;
using MaxwellCalc.Core.Workspaces;
using System;
using System.Numerics;
using System.Text;
using System.Text.Json.Serialization;

namespace MaxwellCalc.Core.Domains;

/// <summary>
/// Describes the complex domain for doubles.
/// </summary>
public class ComplexDomain : IDomain<Complex>
{
    /// <inheritdoc />
    public Quantity<Complex> Default { get; } = new Quantity<Complex>(0.0, Unit.UnitNone);

    /// <inheritdoc />
    public Complex One => 1.0;

    /// <inheritdoc />
    public JsonConverter<Complex> Converter { get; } = new ComplexJsonConverter();

    /// <inheritdoc />
    public bool TryScalar(string scalar, IDiagnosticsHandler? diagnostics, out Quantity<Complex> result)
    {
        // Parse the scalar
        if (!double.TryParse(scalar, System.Globalization.NumberStyles.Float, System.Globalization.CultureInfo.InvariantCulture, out double dbl))
        {
            diagnostics?.PostDiagnosticMessage(new($"Could not evaluate the scalar '{scalar}'."));
            result = Default;
            return false;
        }
        result = new Quantity<Complex>(dbl, Unit.UnitNone);
        return true;
    }

    /// <inheritdoc />
    public bool TryFactor(Quantity<Complex> a, Quantity<Complex> unit, out double factor)
    {
        var s = a.Scalar * unit.Scalar;
        factor = Math.Max(Math.Abs(s.Real), Math.Abs(s.Imaginary));
        return true;
    }

    /// <inheritdoc />
    public bool TryPlus(Quantity<Complex> a, IDiagnosticsHandler? diagnostics, out Quantity<Complex> result)
    {
        result = a;
        return true;
    }

    /// <inheritdoc />
    public bool TryMinus(Quantity<Complex> a, IDiagnosticsHandler? diagnostics, out Quantity<Complex> result)
    {
        result = new Quantity<Complex>(-a.Scalar, a.Unit);
        return true;
    }

    /// <inheritdoc />
    public bool TryFactorial(Quantity<Complex> a, IDiagnosticsHandler? diagnostics, out Quantity<Complex> result)
    {
        if (diagnostics is not null)
            return ComplexMathHelper.Factorial([a], diagnostics, out result);
        throw new NotImplementedException();
    }

    /// <inheritdoc />
    public bool TryAdd(Quantity<Complex> a, Quantity<Complex> b, IDiagnosticsHandler? diagnostics, out Quantity<Complex> result)
    {
        // Check units
        if (a.Unit != b.Unit)
        {
            // Units should match!
            diagnostics?.PostDiagnosticMessage(new("Units do not match for addition."));
            result = Default;
            return false;
        }
        result = new Quantity<Complex>(a.Scalar + b.Scalar, a.Unit);
        return true;
    }

    public bool TrySubtract(Quantity<Complex> a, Quantity<Complex> b, IDiagnosticsHandler? diagnostics, out Quantity<Complex> result)
    {
        // Check units
        if (a.Unit != b.Unit)
        {
            // Units should match!
            diagnostics?.PostDiagnosticMessage(new("Units do not match for subtraction."));
            result = Default;
            return false;
        }
        result = new Quantity<Complex>(a.Scalar - b.Scalar, a.Unit);
        return true;
    }

    /// <inheritdoc />
    public bool TryMultiply(Quantity<Complex> a, Quantity<Complex> b, IDiagnosticsHandler? diagnostics, out Quantity<Complex> result)
    {
        result = new Quantity<Complex>(a.Scalar * b.Scalar, a.Unit * b.Unit);
        return true;
    }

    /// <inheritdoc />
    public bool TryDivide(Quantity<Complex> a, Quantity<Complex> b, IDiagnosticsHandler? diagnostics, out Quantity<Complex> result)
    {
        result = new Quantity<Complex>(a.Scalar / b.Scalar, a.Unit / b.Unit);
        return true;
    }

    /// <inheritdoc />
    public bool TryInvert(Quantity<Complex> a, IDiagnosticsHandler? diagnostics, out Quantity<Complex> result)
    {
        result = new Quantity<Complex>(1.0 / a.Scalar, Unit.Inv(a.Unit));
        return true;
    }

    /// <inheritdoc />
    public bool TryModulo(Quantity<Complex> a, Quantity<Complex> b, IDiagnosticsHandler? diagnostics, out Quantity<Complex> result)
    {
        result = new Quantity<Complex>(
            Math.IEEERemainder(a.Scalar.Real, b.Scalar.Real),
            a.Unit);
        return true;
    }

    /// <inheritdoc />
    public bool TryIntDivide(Quantity<Complex> a, Quantity<Complex> b, IDiagnosticsHandler? diagnostics, out Quantity<Complex> result)
    {
        result = new Quantity<Complex>(
            Math.Truncate(a.Scalar.Real / b.Scalar.Real),
            a.Unit / b.Unit);
        return true;
    }

    /// <inheritdoc />
    public bool TryPow(Quantity<Complex> a, Quantity<Complex> b, IDiagnosticsHandler? diagnostics, out Quantity<Complex> result)
    {
        // Exponentiation can only happen with numbers that don't have units
        if (b.Unit != Unit.UnitNone)
        {
            // Cannot use exponent with units
            diagnostics?.PostDiagnosticMessage(new("Cannot raise to a power where the exponent contains units."));
            result = Default;
            return false;
        }

        if (b.Scalar.Equals(1.0))
        {
            result = a;
            return true;
        }

        if (a.Unit == Unit.UnitNone)
        {
            // The base does not have units, so we can simply raise to the power
            result = new Quantity<Complex>(
                Complex.Pow(a.Scalar, b.Scalar),
                a.Unit);
        }
        else
        {
            // We will be raising units to the power, so let's try to convert the exponent to a fraction
            if (!b.Scalar.Imaginary.Equals(0.0))
            {
                // Cannot raise units to an imaginary power
                diagnostics?.PostDiagnosticMessage(new("Cannot raise units to a complex power."));
                result = Default;
                return false;
            }

            if (!Fraction.TryConvert(b.Scalar.Real, out var fraction))
            {
                // Could not convert to a fraction
                diagnostics?.PostDiagnosticMessage(new("Cannot raise units to a power that is too complex."));
                result = Default;
                return false;
            }

            result = new Quantity<Complex>(
                Complex.Pow(a.Scalar, b.Scalar),
                Unit.Pow(a.Unit, fraction));
        }
        return true;
    }

    /// <inheritdoc />
    public bool TryBitwiseOr(Quantity<Complex> a, Quantity<Complex> b, IDiagnosticsHandler? diagnostics, out Quantity<Complex> result)
    {
        // First convert to integer
        if (a.Unit != Unit.UnitNone || b.Unit != Unit.UnitNone)
        {
            // Don't know what to do here
            diagnostics?.PostDiagnosticMessage(new("Cannot take a bitwise OR of quantities with units."));
            result = Default;
            return false;
        }
        var la = (long)Math.Truncate(a.Scalar.Real);
        var lb = (long)Math.Truncate(b.Scalar.Real);
        result = new Quantity<Complex>(la | lb, Unit.UnitNone);
        return true;
    }

    /// <inheritdoc />
    public bool TryBitwiseAnd(Quantity<Complex> a, Quantity<Complex> b, IDiagnosticsHandler? diagnostics, out Quantity<Complex> result)
    {
        // First convert to integer
        if (a.Unit != Unit.UnitNone || b.Unit != Unit.UnitNone)
        {
            // Don't know what to do here
            diagnostics?.PostDiagnosticMessage(new("Cannot take a bitwise AND of quantities with units."));
            result = Default;
            return false;
        }
        var la = (long)Math.Truncate(a.Scalar.Real);
        var lb = (long)Math.Truncate(b.Scalar.Real);
        result = new Quantity<Complex>(la & lb, Unit.UnitNone);
        return true;
    }

    /// <inheritdoc />
    public bool TryLeftShift(Quantity<Complex> a, Quantity<Complex> b, IDiagnosticsHandler? diagnostics, out Quantity<Complex> result)
    {
        // First convert to integer
        if (a.Unit != Unit.UnitNone || b.Unit != Unit.UnitNone)
        {
            // Don't know what to do here
            diagnostics?.PostDiagnosticMessage(new("Cannot take shift quantities with units."));
            result = Default;
            return false;
        }
        var la = (long)Math.Truncate(a.Scalar.Real);
        var lb = (int)Math.Truncate(b.Scalar.Real);
        result = new Quantity<Complex>(la << lb, Unit.UnitNone);
        return true;
    }

    public bool TryRightShift(Quantity<Complex> a, Quantity<Complex> b, IDiagnosticsHandler? diagnostics, out Quantity<Complex> result)
    {
        // First convert to integer
        if (a.Unit != Unit.UnitNone || b.Unit != Unit.UnitNone)
        {
            // Don't know what to do here
            diagnostics?.PostDiagnosticMessage(new("Cannot take shift quantities with units."));
            result = Default;
            return false;
        }
        var la = (long)Math.Truncate(a.Scalar.Real);
        var lb = (int)Math.Truncate(b.Scalar.Real);
        result = new Quantity<Complex>(la >> lb, Unit.UnitNone);
        return true;
    }

    /// <inheritdoc />
    public bool TryGreaterThan(Quantity<Complex> a, Quantity<Complex> b, IDiagnosticsHandler? diagnostics, out Quantity<Complex> result)
    {
        if (a.Unit != b.Unit)
        {
            // Cannot compare quantities with different units
            diagnostics?.PostDiagnosticMessage(new("Cannot compare quantities with different units."));
            result = Default;
            return false;
        }
        result = new Quantity<Complex>(a.Scalar.Real > b.Scalar.Real ? 1.0 : 0.0, Unit.UnitNone);
        return true;
    }

    /// <inheritdoc />
    public bool TryGreaterThanOrEqual(Quantity<Complex> a, Quantity<Complex> b, IDiagnosticsHandler? diagnostics, out Quantity<Complex> result)
    {
        if (a.Unit != b.Unit)
        {
            // Cannot compare quantities with different units
            diagnostics?.PostDiagnosticMessage(new("Cannot compare quantities with different units."));
            result = Default;
            return false;
        }
        result = new Quantity<Complex>(a.Scalar.Real >= b.Scalar.Real ? 1.0 : 0.0, Unit.UnitNone);
        return true;
    }

    /// <inheritdoc />
    public bool TryLessThan(Quantity<Complex> a, Quantity<Complex> b, IDiagnosticsHandler? diagnostics, out Quantity<Complex> result)
    {
        if (a.Unit != b.Unit)
        {
            // Cannot compare quantities with different units
            diagnostics?.PostDiagnosticMessage(new("Cannot compare quantities with different units."));
            result = Default;
            return false;
        }
        result = new Quantity<Complex>(a.Scalar.Real < b.Scalar.Real ? 1.0 : 0.0, Unit.UnitNone);
        return true;
    }

    /// <inheritdoc />
    public bool TryLessThanOrEqual(Quantity<Complex> a, Quantity<Complex> b, IDiagnosticsHandler? diagnostics, out Quantity<Complex> result)
    {
        if (a.Unit != b.Unit)
        {
            // Cannot compare quantities with different units
            diagnostics?.PostDiagnosticMessage(new("Cannot compare quantities with different units."));
            result = Default;
            return false;
        }
        result = new Quantity<Complex>(a.Scalar.Real <= b.Scalar.Real ? 1.0 : 0.0, Unit.UnitNone);
        return true;
    }

    /// <inheritdoc />
    public bool TryEquals(Quantity<Complex> a, Quantity<Complex> b, IDiagnosticsHandler? diagnostics, out Quantity<Complex> result)
    {
        if (a.Unit != b.Unit)
        {
            // Cannot compare quantities with different units
            diagnostics?.PostDiagnosticMessage(new("Cannot compare quantities with different units."));
            result = Default;
            return false;
        }
        result = new Quantity<Complex>(a.Scalar == b.Scalar ? 1.0 : 0.0, Unit.UnitNone);
        return true;
    }

    /// <inheritdoc />
    public bool TryNotEquals(Quantity<Complex> a, Quantity<Complex> b, IDiagnosticsHandler? diagnostics, out Quantity<Complex> result)
    {
        if (a.Unit != b.Unit)
        {
            // Cannot compare quantities with different units
            diagnostics?.PostDiagnosticMessage(new("Cannot compare quantities with different units."));
            result = Default;
            return false;
        }
        result = new Quantity<Complex>(a.Scalar != b.Scalar ? 1.0 : 0.0, Unit.UnitNone);
        return true;
    }

    /// <inheritdoc />
    public bool TryFormat(Quantity<Complex> value, string? format, IFormatProvider? formatProvider, out Quantity<string> result)
    {
        var sb = new StringBuilder();
        if (!value.Scalar.Real.Equals(0.0))
            sb.Append(value.Scalar.Real.ToString(format, formatProvider));
        if (!value.Scalar.Imaginary.Equals(0.0))
        {
            if (sb.Length > 0)
            {
                if (value.Scalar.Imaginary < 0)
                    sb.Append(" - ");
                else
                    sb.Append(" + ");
                sb.Append($"{Math.Abs(value.Scalar.Imaginary).ToString(format, formatProvider)}i");
            }
            else
                sb.Append($"{value.Scalar.Imaginary.ToString(format, formatProvider)}i");
        }
        if (sb.Length == 0)
            sb.Append(0.0.ToString(format, formatProvider));
        result = new(sb.ToString(), value.Unit);
        return true;
    }

    /// <inheritdoc />
    public bool TryIsTrue(Quantity<Complex> a, IDiagnosticsHandler? diagnostics, out bool result)
    {
        if (a.Scalar.Real == 0.0 && a.Scalar.Imaginary == 0.0)
            result = false;
        else
            result = true;
        return true;
    }

    /// <inheritdoc />
    public bool TryLogicalOr(Quantity<Complex> a, Quantity<Complex> b, IDiagnosticsHandler? diagnostics, out Quantity<Complex> result)
    {
        if (!TryIsTrue(a, diagnostics, out bool left) ||
            !TryIsTrue(b, diagnostics, out bool right))
        {
            result = Default;
            return false;
        }
        result = new(left || right ? 1.0 : 0.0, Unit.UnitNone);
        return true;
    }

    /// <inheritdoc />
    public bool TryLogicalAnd(Quantity<Complex> a, Quantity<Complex> b, IDiagnosticsHandler? diagnostics, out Quantity<Complex> result)
    {
        if (!TryIsTrue(a, diagnostics, out bool left) ||
            !TryIsTrue(b, diagnostics, out bool right))
        {
            result = Default;
            return false;
        }
        result = new(left && right ? 1.0 : 0.0, Unit.UnitNone);
        return true;
    }
}
