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
        public JsonConverter<double> Converter { get; } = new DoubleJsonConverter();

        /// <inheritdoc />
        public bool TryScalar(string scalar, IWorkspace<double>? workspace, out Quantity<double> result)
        {
            // Parse the scalar
            if (!double.TryParse(scalar, System.Globalization.NumberStyles.Float, System.Globalization.CultureInfo.InvariantCulture, out double dbl))
            {
                workspace?.PostDiagnosticMessage(new($"Could not evaluate the scalar '{scalar}'."));
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
        public bool TryUnit(string unit, IWorkspace<double>? workspace, out Quantity<double> result)
        {
            if (workspace is not null)
            {
                if (!workspace.AllowUnits)
                {
                    workspace.PostDiagnosticMessage(new("Units are not allowed"));
                    result = default;
                    return false;
                }

                if (workspace.ResolveInputUnits)
                {
                    if (workspace.InputUnits.TryGetValue(unit, out result))
                        return true;
                    workspace.PostDiagnosticMessage(new($"Could not recognize unit '{unit}'."));
                    return false;
                }
            }

            result = new Quantity<double>(1.0, new Unit((unit, 1)));
            return true;
        }

        /// <inheritdoc />
        public bool TryVariable(string variable, IWorkspace<double>? workspace, out Quantity<double> result)
        {
            if (workspace is not null)
            {
                // We're not checking here on AllowVariables because it might be user function parameters...
                if (workspace.Scope.TryGetComputedVariable(variable, out result))
                    return true;
                workspace.PostDiagnosticMessage(new($"Could not find a variable with the name '{variable}'."));
                return false;
            }
            else
            {
                // workspace.ErrorMessage = "Variables are not supported.";
                result = Default;
                return false;
            }
        }

        /// <inheritdoc />
        public bool TryPlus(Quantity<double> a, IWorkspace<double>? workspace, out Quantity<double> result)
        {
            result = a;
            return true;
        }

        /// <inheritdoc />
        public bool TryMinus(Quantity<double> a, IWorkspace<double>? workspace, out Quantity<double> result)
        {
            result = new Quantity<double>(-a.Scalar, a.Unit);
            return true;
        }

        /// <inheritdoc />
        public bool TryRemoveUnits(Quantity<double> a, IWorkspace<double>? workspace, out Quantity<double> result)
        {
            result = new Quantity<double>(a.Scalar, Unit.UnitNone);
            return true;
        }

        /// <inheritdoc />
        public bool TryFactorial(Quantity<double> a, IWorkspace<double>? workspace, out Quantity<double> result)
        {
            if (workspace is not null)
                return DoubleMathHelper.Factorial([a], workspace, out result);
            throw new NotImplementedException();
        }

        /// <inheritdoc />
        public bool TryAdd(Quantity<double> a, Quantity<double> b, IWorkspace<double>? workspace, out Quantity<double> result)
        {
            // Check units
            if (a.Unit != b.Unit)
            {
                // Units should match!
                workspace?.PostDiagnosticMessage(new("Units do not match for addition."));
                result = Default;
                return false;
            }

            result = new Quantity<double>(a.Scalar + b.Scalar, a.Unit);
            return true;
        }

        /// <inheritdoc />
        public bool TrySubtract(Quantity<double> a, Quantity<double> b, IWorkspace<double>? workspace, out Quantity<double> result)
        {
            // Check units
            if (a.Unit != b.Unit)
            {
                // Units should match!
                workspace?.PostDiagnosticMessage(new("Units do not match for subtraction."));
                result = Default;
                return false;
            }
            result = new Quantity<double>(a.Scalar - b.Scalar, a.Unit);
            return true;
        }

        /// <inheritdoc />
        public bool TryMultiply(Quantity<double> a, Quantity<double> b, IWorkspace<double>? workspace, out Quantity<double> result)
        {
            result = new Quantity<double>(
                a.Scalar * b.Scalar,
                a.Unit * b.Unit);
            return true;
        }

        /// <inheritdoc />
        public bool TryDivide(Quantity<double> a, Quantity<double> b, IWorkspace<double>? workspace, out Quantity<double> result)
        {
            result = new Quantity<double>(
                a.Scalar / b.Scalar,
                a.Unit / b.Unit);
            return true;
        }

        /// <inheritdoc />
        public bool TryInvert(Quantity<double> a, IWorkspace<double>? workspace, out Quantity<double> result)
        {
            result = new(1.0 / a.Scalar, Unit.Inv(a.Unit));
            return true;
        }

        /// <inheritdoc />
        public bool TryModulo(Quantity<double> a, Quantity<double> b, IWorkspace<double>? workspace, out Quantity<double> result)
        {
            result = new Quantity<double>(
                Math.IEEERemainder(a.Scalar, b.Scalar),
                a.Unit);
            return true;
        }

        /// <inheritdoc />
        public bool TryIntDivide(Quantity<double> a, Quantity<double> b, IWorkspace<double>? workspace, out Quantity<double> result)
        {
            result = new Quantity<double>(
                Math.Truncate(a.Scalar / b.Scalar),
                a.Unit / b.Unit);
            return true;
        }

        /// <inheritdoc />
        public bool TryExp(Quantity<double> a, Quantity<double> b, IWorkspace<double>? workspace, out Quantity<double> result)
        {
            // Exponentiation can only happen with numbers that don't have units
            if (b.Unit != Unit.UnitNone)
            {
                // Cannot use exponent with units
                workspace?.PostDiagnosticMessage(new("Cannot raise to a power where the exponent contains units."));
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
                    workspace?.PostDiagnosticMessage(new("Cannot raise units to a power that is too complex."));
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
        public bool TryInUnit(Quantity<double> a, Quantity<double> b, ReadOnlyMemory<char> content, IWorkspace<double>? workspace, out Quantity<double> result)
        {
            if (a.Unit != b.Unit)
            {
                // Should be same units in order to compute
                workspace?.PostDiagnosticMessage(new("The units do not match."));
                result = Default;
                return false;
            }

            // Don't do anything, formatting is done at the top level
            result = a;
            return true;
        }

        /// <inheritdoc />
        public bool TryBitwiseOr(Quantity<double> a, Quantity<double> b, IWorkspace<double>? workspace, out Quantity<double> result)
        {
            // First convert to integer
            if (a.Unit != Unit.UnitNone || b.Unit != Unit.UnitNone)
            {
                // Don't know what to do here
                workspace?.PostDiagnosticMessage(new("Cannot take a bitwise OR of quantities with units."));
                result = Default;
                return false;
            }
            var la = (long)Math.Truncate(a.Scalar);
            var lb = (long)Math.Truncate(b.Scalar);
            result = new Quantity<double>(la | lb, Unit.UnitNone);
            return true;
        }

        /// <inheritdoc />
        public bool TryBitwiseAnd(Quantity<double> a, Quantity<double> b, IWorkspace<double>? workspace, out Quantity<double> result)
        {
            // First convert to integer
            if (a.Unit != Unit.UnitNone || b.Unit != Unit.UnitNone)
            {
                // Don't know what to do here
                workspace?.PostDiagnosticMessage(new("Cannot take a bitwise AND of quantities with units."));
                result = Default;
                return false;
            }
            var la = (long)Math.Truncate(a.Scalar);
            var lb = (long)Math.Truncate(b.Scalar);
            result = new Quantity<double>(la & lb, Unit.UnitNone);
            return true;
        }

        /// <inheritdoc />
        public bool TryLeftShift(Quantity<double> a, Quantity<double> b, IWorkspace<double>? workspace, out Quantity<double> result)
        {
            // First convert to integer
            if (a.Unit != Unit.UnitNone || b.Unit != Unit.UnitNone)
            {
                // Don't know what to do here
                workspace?.PostDiagnosticMessage(new("Cannot take shift quantities with units."));
                result = Default;
                return false;
            }
            var la = (long)Math.Truncate(a.Scalar);
            var lb = (int)Math.Truncate(b.Scalar);
            result = new Quantity<double>(la << lb, Unit.UnitNone);
            return true;
        }

        /// <inheritdoc />
        public bool TryRightShift(Quantity<double> a, Quantity<double> b, IWorkspace<double>? workspace, out Quantity<double> result)
        {
            // First convert to integer
            if (a.Unit != Unit.UnitNone || b.Unit != Unit.UnitNone)
            {
                // Don't know what to do here
                workspace?.PostDiagnosticMessage(new("Cannot take shift quantities with units."));
                result = Default;
                return false;
            }
            var la = (long)Math.Truncate(a.Scalar);
            var lb = (int)Math.Truncate(b.Scalar);
            result = new Quantity<double>(la >> lb, Unit.UnitNone);
            return true;
        }

        /// <inheritdoc />
        public bool TryGreaterThan(Quantity<double> a, Quantity<double> b, IWorkspace<double>? workspace, out Quantity<double> result)
        {
            if (a.Unit != b.Unit)
            {
                // Cannot compare quantities with different units
                workspace?.PostDiagnosticMessage(new("Cannot compare quantities with different units."));
                result = Default;
                return false;
            }
            result = new Quantity<double>(a.Scalar > b.Scalar ? 1.0 : 0.0, Unit.UnitNone);
            return true;
        }

        /// <inheritdoc />
        public bool TryGreaterThanOrEqual(Quantity<double> a, Quantity<double> b, IWorkspace<double>? workspace, out Quantity<double> result)
        {
            if (a.Unit != b.Unit)
            {
                // Cannot compare quantities with different units
                workspace?.PostDiagnosticMessage(new("Cannot compare quantities with different units."));
                result = Default;
                return false;
            }
            result = new Quantity<double>(a.Scalar >= b.Scalar ? 1.0 : 0.0, Unit.UnitNone);
            return true;
        }

        /// <inheritdoc />
        public bool TryLessThan(Quantity<double> a, Quantity<double> b, IWorkspace<double>? workspace, out Quantity<double> result)
        {
            if (a.Unit != b.Unit)
            {
                // Cannot compare quantities with different units
                workspace?.PostDiagnosticMessage(new("Cannot compare quantities with different units."));
                result = Default;
                return false;
            }
            result = new Quantity<double>(a.Scalar < b.Scalar ? 1.0 : 0.0, Unit.UnitNone);
            return true;
        }

        /// <inheritdoc />
        public bool TryLessThanOrEqual(Quantity<double> a, Quantity<double> b, IWorkspace<double>? workspace, out Quantity<double> result)
        {
            if (a.Unit != b.Unit)
            {
                // Cannot compare quantities with different units
                workspace?.PostDiagnosticMessage(new("Cannot compare quantities with different units."));
                result = Default;
                return false;
            }
            result = new Quantity<double>(a.Scalar <= b.Scalar ? 1.0 : 0.0, Unit.UnitNone);
            return true;
        }

        /// <inheritdoc />
        public bool TryEquals(Quantity<double> a, Quantity<double> b, IWorkspace<double>? workspace, out Quantity<double> result)
        {
            if (a.Unit != b.Unit)
            {
                // Cannot compare quantities with different units
                workspace?.PostDiagnosticMessage(new("Cannot compare quantities with different units."));
                result = Default;
                return false;
            }
            result = new Quantity<double>(a.Scalar == b.Scalar ? 1.0 : 0.0, Unit.UnitNone);
            return true;
        }

        /// <inheritdoc />
        public bool TryNotEquals(Quantity<double> a, Quantity<double> b, IWorkspace<double>? workspace, out Quantity<double> result)
        {
            if (a.Unit != b.Unit)
            {
                // Cannot compare quantities with different units
                workspace?.PostDiagnosticMessage(new("Cannot compare quantities with different units."));
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
        public bool TryIsTrue(Quantity<double> a, IWorkspace<double>? workspace, out bool result)
        {
            if (a.Scalar == 0.0)
                result = false;
            else
                result = true;
            return true;
        }

        /// <inheritdoc />
        public bool TryLogicalOr(Quantity<double> a, Quantity<double> b, IWorkspace<double>? workspace, out Quantity<double> result)
        {
            if (!TryIsTrue(a, workspace, out bool left) ||
                !TryIsTrue(b, workspace, out bool right))
            {
                result = Default;
                return false;
            }
            result = new(left || right ? 1.0 : 0.0, Unit.UnitNone);
            return true;
        }

        /// <inheritdoc />
        public bool TryLogicalAnd(Quantity<double> a, Quantity<double> b, IWorkspace<double>? workspace, out Quantity<double> result)
        {
            if (!TryIsTrue(a, workspace, out bool left) ||
                !TryIsTrue(b, workspace, out bool right))
            {
                result = Default;
                return false;
            }
            result = new(left && right ? 1.0 : 0.0, Unit.UnitNone);
            return true;
        }
    }
}
