using MaxwellCalc.Core.Units;
using MaxwellCalc.Core.Workspaces;
using System;
using System.Text.Json.Serialization;

namespace MaxwellCalc.Core.Domains
{
    /// <summary>
    /// A resolver for quantities with using doubles as scalars.
    /// </summary>
    public class DoubleDomain : IDomain<double>
    {
        /// <inheritdoc />
        public Quantity<double> Default { get; } = new Quantity<double>(0.0, Unit.UnitNone);

        /// <inheritdoc />
        public double One => 1.0;

        /// <inheritdoc />
        public JsonConverter<double> Converter { get; } = new DoubleJsonConverter();

        /// <inheritdoc />
        public bool TryScalar(string scalar, IDiagnosticsHandler? diagnostics, out Quantity<double> result)
        {
            // Parse the scalar
            if (!double.TryParse(scalar, System.Globalization.NumberStyles.Float, System.Globalization.CultureInfo.InvariantCulture, out double dbl))
            {
                diagnostics?.PostDiagnosticMessage(new($"Could not evaluate the scalar '{scalar}'."));
                result = Default;
                return false;
            }
            result = new Quantity<double>(dbl, Unit.UnitNone);
            return true;
        }

        /// <inheritdoc />
        public bool TryFactor(Quantity<double> a, Quantity<double> unit, out double factor)
        {
            factor = Math.Abs(a.Scalar * unit.Scalar);
            return true;
        }

        /// <inheritdoc />
        public bool TryPlus(Quantity<double> a, IDiagnosticsHandler? diagnostics, out Quantity<double> result)
        {
            result = a;
            return true;
        }

        /// <inheritdoc />
        public bool TryMinus(Quantity<double> a, IDiagnosticsHandler? diagnostics, out Quantity<double> result)
        {
            result = new Quantity<double>(-a.Scalar, a.Unit);
            return true;
        }

        /// <inheritdoc />
        public bool TryFactorial(Quantity<double> a, IDiagnosticsHandler? diagnostics, out Quantity<double> result)
        {
            if (diagnostics is not null)
                return DoubleMathHelper.Factorial([a], diagnostics, out result);
            throw new NotImplementedException();
        }

        /// <inheritdoc />
        public bool TryAdd(Quantity<double> a, Quantity<double> b, IDiagnosticsHandler? diagnostics, out Quantity<double> result)
        {
            // Check units
            if (a.Unit != b.Unit)
            {
                // Units should match!
                diagnostics?.PostDiagnosticMessage(new("Units do not match for addition."));
                result = Default;
                return false;
            }

            result = new Quantity<double>(a.Scalar + b.Scalar, a.Unit);
            return true;
        }

        /// <inheritdoc />
        public bool TrySubtract(Quantity<double> a, Quantity<double> b, IDiagnosticsHandler? diagnostics, out Quantity<double> result)
        {
            // Check units
            if (a.Unit != b.Unit)
            {
                // Units should match!
                diagnostics?.PostDiagnosticMessage(new("Units do not match for subtraction."));
                result = Default;
                return false;
            }
            result = new Quantity<double>(a.Scalar - b.Scalar, a.Unit);
            return true;
        }

        /// <inheritdoc />
        public bool TryMultiply(Quantity<double> a, Quantity<double> b, IDiagnosticsHandler? diagnostics, out Quantity<double> result)
        {
            result = new Quantity<double>(
                a.Scalar * b.Scalar,
                a.Unit * b.Unit);
            return true;
        }

        /// <inheritdoc />
        public bool TryDivide(Quantity<double> a, Quantity<double> b, IDiagnosticsHandler? diagnostics, out Quantity<double> result)
        {
            result = new Quantity<double>(
                a.Scalar / b.Scalar,
                a.Unit / b.Unit);
            return true;
        }

        /// <inheritdoc />
        public bool TryInvert(Quantity<double> a, IDiagnosticsHandler? diagnostics, out Quantity<double> result)
        {
            result = new(1.0 / a.Scalar, Unit.Inv(a.Unit));
            return true;
        }

        /// <inheritdoc />
        public bool TryModulo(Quantity<double> a, Quantity<double> b, IDiagnosticsHandler? diagnostics, out Quantity<double> result)
        {
            result = new Quantity<double>(
                Math.IEEERemainder(a.Scalar, b.Scalar),
                a.Unit);
            return true;
        }

        /// <inheritdoc />
        public bool TryIntDivide(Quantity<double> a, Quantity<double> b, IDiagnosticsHandler? diagnostics, out Quantity<double> result)
        {
            result = new Quantity<double>(
                Math.Truncate(a.Scalar / b.Scalar),
                a.Unit / b.Unit);
            return true;
        }

        /// <inheritdoc />
        public bool TryPow(Quantity<double> a, Quantity<double> b, IDiagnosticsHandler? diagnostics, out Quantity<double> result)
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
                result = new Quantity<double>(
                    Math.Pow(a.Scalar, b.Scalar),
                    a.Unit);
            }
            else
            {
                // We will be raising units to the power, so let's try to convert the exponent to a fraction
                if (!Fraction.TryConvert(b.Scalar, out var fraction))
                {
                    // Could not convert to a fraction
                    diagnostics?.PostDiagnosticMessage(new("Cannot raise units to a power that is too complex."));
                    result = Default;
                    return false;
                }

                result = new Quantity<double>(
                    Math.Pow(a.Scalar, b.Scalar),
                    Unit.Pow(a.Unit, fraction));
            }
            return true;
        }

        /// <inheritdoc />
        public bool TryBitwiseOr(Quantity<double> a, Quantity<double> b, IDiagnosticsHandler? diagnostics, out Quantity<double> result)
        {
            // First convert to integer
            if (a.Unit != Unit.UnitNone || b.Unit != Unit.UnitNone)
            {
                // Don't know what to do here
                diagnostics?.PostDiagnosticMessage(new("Cannot take a bitwise OR of quantities with units."));
                result = Default;
                return false;
            }
            var la = (long)Math.Truncate(a.Scalar);
            var lb = (long)Math.Truncate(b.Scalar);
            result = new Quantity<double>(la | lb, Unit.UnitNone);
            return true;
        }

        /// <inheritdoc />
        public bool TryBitwiseAnd(Quantity<double> a, Quantity<double> b, IDiagnosticsHandler? diagnostics, out Quantity<double> result)
        {
            // First convert to integer
            if (a.Unit != Unit.UnitNone || b.Unit != Unit.UnitNone)
            {
                // Don't know what to do here
                diagnostics?.PostDiagnosticMessage(new("Cannot take a bitwise AND of quantities with units."));
                result = Default;
                return false;
            }
            var la = (long)Math.Truncate(a.Scalar);
            var lb = (long)Math.Truncate(b.Scalar);
            result = new Quantity<double>(la & lb, Unit.UnitNone);
            return true;
        }

        /// <inheritdoc />
        public bool TryLeftShift(Quantity<double> a, Quantity<double> b, IDiagnosticsHandler? diagnostics, out Quantity<double> result)
        {
            // First convert to integer
            if (a.Unit != Unit.UnitNone || b.Unit != Unit.UnitNone)
            {
                // Don't know what to do here
                diagnostics?.PostDiagnosticMessage(new("Cannot take shift quantities with units."));
                result = Default;
                return false;
            }
            var la = (long)Math.Truncate(a.Scalar);
            var lb = (int)Math.Truncate(b.Scalar);
            result = new Quantity<double>(la << lb, Unit.UnitNone);
            return true;
        }

        /// <inheritdoc />
        public bool TryRightShift(Quantity<double> a, Quantity<double> b, IDiagnosticsHandler? diagnostics, out Quantity<double> result)
        {
            // First convert to integer
            if (a.Unit != Unit.UnitNone || b.Unit != Unit.UnitNone)
            {
                // Don't know what to do here
                diagnostics?.PostDiagnosticMessage(new("Cannot take shift quantities with units."));
                result = Default;
                return false;
            }
            var la = (long)Math.Truncate(a.Scalar);
            var lb = (int)Math.Truncate(b.Scalar);
            result = new Quantity<double>(la >> lb, Unit.UnitNone);
            return true;
        }

        /// <inheritdoc />
        public bool TryGreaterThan(Quantity<double> a, Quantity<double> b, IDiagnosticsHandler? diagnostics, out Quantity<double> result)
        {
            if (a.Unit != b.Unit)
            {
                // Cannot compare quantities with different units
                diagnostics?.PostDiagnosticMessage(new("Cannot compare quantities with different units."));
                result = Default;
                return false;
            }
            result = new Quantity<double>(a.Scalar > b.Scalar ? 1.0 : 0.0, Unit.UnitNone);
            return true;
        }

        /// <inheritdoc />
        public bool TryGreaterThanOrEqual(Quantity<double> a, Quantity<double> b, IDiagnosticsHandler? diagnostics, out Quantity<double> result)
        {
            if (a.Unit != b.Unit)
            {
                // Cannot compare quantities with different units
                diagnostics?.PostDiagnosticMessage(new("Cannot compare quantities with different units."));
                result = Default;
                return false;
            }
            result = new Quantity<double>(a.Scalar >= b.Scalar ? 1.0 : 0.0, Unit.UnitNone);
            return true;
        }

        /// <inheritdoc />
        public bool TryLessThan(Quantity<double> a, Quantity<double> b, IDiagnosticsHandler? diagnostics, out Quantity<double> result)
        {
            if (a.Unit != b.Unit)
            {
                // Cannot compare quantities with different units
                diagnostics?.PostDiagnosticMessage(new("Cannot compare quantities with different units."));
                result = Default;
                return false;
            }
            result = new Quantity<double>(a.Scalar < b.Scalar ? 1.0 : 0.0, Unit.UnitNone);
            return true;
        }

        /// <inheritdoc />
        public bool TryLessThanOrEqual(Quantity<double> a, Quantity<double> b, IDiagnosticsHandler? diagnostics, out Quantity<double> result)
        {
            if (a.Unit != b.Unit)
            {
                // Cannot compare quantities with different units
                diagnostics?.PostDiagnosticMessage(new("Cannot compare quantities with different units."));
                result = Default;
                return false;
            }
            result = new Quantity<double>(a.Scalar <= b.Scalar ? 1.0 : 0.0, Unit.UnitNone);
            return true;
        }

        /// <inheritdoc />
        public bool TryEquals(Quantity<double> a, Quantity<double> b, IDiagnosticsHandler? diagnostics, out Quantity<double> result)
        {
            if (a.Unit != b.Unit)
            {
                // Cannot compare quantities with different units
                diagnostics?.PostDiagnosticMessage(new("Cannot compare quantities with different units."));
                result = Default;
                return false;
            }
            result = new Quantity<double>(a.Scalar == b.Scalar ? 1.0 : 0.0, Unit.UnitNone);
            return true;
        }

        /// <inheritdoc />
        public bool TryNotEquals(Quantity<double> a, Quantity<double> b, IDiagnosticsHandler? diagnostics, out Quantity<double> result)
        {
            if (a.Unit != b.Unit)
            {
                // Cannot compare quantities with different units
                diagnostics?.PostDiagnosticMessage(new("Cannot compare quantities with different units."));
                result = Default;
                return false;
            }
            result = new Quantity<double>(a.Scalar != b.Scalar ? 1.0 : 0.0, Unit.UnitNone);
            return true;
        }

        /// <inheritdoc />
        public bool TryFormat(Quantity<double> value, string? format, IFormatProvider? formatProvider, out Quantity<string> result)
        {
            result = new(value.Scalar.ToString(format, formatProvider) ?? string.Empty, value.Unit);
            return true;
        }

        /// <inheritdoc />
        public bool TryIsTrue(Quantity<double> a, IDiagnosticsHandler? diagnostics, out bool result)
        {
            if (a.Scalar == 0.0)
                result = false;
            else
                result = true;
            return true;
        }

        /// <inheritdoc />
        public bool TryLogicalOr(Quantity<double> a, Quantity<double> b, IDiagnosticsHandler? diagnostics, out Quantity<double> result)
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
        public bool TryLogicalAnd(Quantity<double> a, Quantity<double> b, IDiagnosticsHandler? diagnostics, out Quantity<double> result)
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
}
