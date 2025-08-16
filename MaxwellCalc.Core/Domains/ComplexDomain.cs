using MaxwellCalc.Units;
using MaxwellCalc.Workspaces;
using System;
using System.Collections.Generic;
using System.Numerics;
using System.Text;
using System.Text.Json;

namespace MaxwellCalc.Domains
{
    /// <summary>
    /// Describes the complex domain for doubles.
    /// </summary>
    public class ComplexDomain : IDomain<Complex>
    {
        /// <inheritdoc />
        public Quantity<Complex> Default { get; } = new Quantity<Complex>(0.0, Unit.UnitNone);

        /// <inheritdoc />
        public bool TryScalar(string scalar, IWorkspace<Complex>? workspace, out Quantity<Complex> result)
        {
            // Parse the scalar
            if (!double.TryParse(scalar, System.Globalization.NumberStyles.Float, System.Globalization.CultureInfo.InvariantCulture, out double dbl))
            {
                workspace?.PostDiagnosticMessage(new($"Could not evaluate the scalar '{scalar}'."));
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
        public bool TryUnit(string unit, IWorkspace<Complex>? workspace, out Quantity<Complex> result)
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

            result = new Quantity<Complex>(1.0, new Unit((unit, 1)));
            return true;
        }

        /// <inheritdoc />
        public bool TryVariable(string variable, IWorkspace<Complex>? workspace, out Quantity<Complex> result)
        {
            if (workspace is not null)
            {
                // We're not checking here on AllowVariables because it might be user function parameters...
                if (workspace.Scope.TryGetComputedVariable(variable, out result))
                    return true;
                if (variable == "i" || variable == "j")
                {
                    result = new Quantity<Complex>(new Complex(0.0, 1.0), Unit.UnitNone);
                    return true;
                }
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
        public bool TryPlus(Quantity<Complex> a, IWorkspace<Complex>? workspace, out Quantity<Complex> result)
        {
            result = a;
            return true;
        }

        /// <inheritdoc />
        public bool TryMinus(Quantity<Complex> a, IWorkspace<Complex>? workspace, out Quantity<Complex> result)
        {
            result = new Quantity<Complex>(-a.Scalar, a.Unit);
            return true;
        }

        /// <inheritdoc />
        public bool TryRemoveUnits(Quantity<Complex> a, IWorkspace<Complex>? workspace, out Quantity<Complex> result)
        {
            result = new Quantity<Complex>(a.Scalar, Unit.UnitNone);
            return true;
        }

        /// <inheritdoc />
        public bool TryFactorial(Quantity<Complex> a, IWorkspace<Complex>? workspace, out Quantity<Complex> result)
        {
            if (workspace is not null)
                return ComplexMathHelper.Factorial([a], workspace, out result);
            throw new NotImplementedException();
        }

        /// <inheritdoc />
        public bool TryAdd(Quantity<Complex> a, Quantity<Complex> b, IWorkspace<Complex>? workspace, out Quantity<Complex> result)
        {
            // Check units
            if (a.Unit != b.Unit)
            {
                // Units should match!
                workspace?.PostDiagnosticMessage(new("Units do not match for addition."));
                result = Default;
                return false;
            }
            result = new Quantity<Complex>(a.Scalar + b.Scalar, a.Unit);
            return true;
        }

        public bool TrySubtract(Quantity<Complex> a, Quantity<Complex> b, IWorkspace<Complex>? workspace, out Quantity<Complex> result)
        {
            // Check units
            if (a.Unit != b.Unit)
            {
                // Units should match!
                workspace?.PostDiagnosticMessage(new("Units do not match for subtraction."));
                result = Default;
                return false;
            }
            result = new Quantity<Complex>(a.Scalar - b.Scalar, a.Unit);
            return true;
        }

        /// <inheritdoc />
        public bool TryMultiply(Quantity<Complex> a, Quantity<Complex> b, IWorkspace<Complex>? workspace, out Quantity<Complex> result)
        {
            result = new Quantity<Complex>(a.Scalar * b.Scalar, a.Unit * b.Unit);
            return true;
        }

        /// <inheritdoc />
        public bool TryDivide(Quantity<Complex> a, Quantity<Complex> b, IWorkspace<Complex>? workspace, out Quantity<Complex> result)
        {
            result = new Quantity<Complex>(a.Scalar / b.Scalar, a.Unit / b.Unit);
            return true;
        }

        /// <inheritdoc />
        public bool TryInvert(Quantity<Complex> a, IWorkspace<Complex>? workspace, out Quantity<Complex> result)
        {
            result = new Quantity<Complex>(1.0 / a.Scalar, Unit.Inv(a.Unit));
            return true;
        }

        /// <inheritdoc />
        public bool TryModulo(Quantity<Complex> a, Quantity<Complex> b, IWorkspace<Complex>? workspace, out Quantity<Complex> result)
        {
            result = new Quantity<Complex>(
                Math.IEEERemainder(a.Scalar.Real, b.Scalar.Real),
                a.Unit);
            return true;
        }

        /// <inheritdoc />
        public bool TryIntDivide(Quantity<Complex> a, Quantity<Complex> b, IWorkspace<Complex>? workspace, out Quantity<Complex> result)
        {
            result = new Quantity<Complex>(
                Math.Truncate(a.Scalar.Real / b.Scalar.Real),
                a.Unit / b.Unit);
            return true;
        }

        /// <inheritdoc />
        public bool TryExp(Quantity<Complex> a, Quantity<Complex> b, IWorkspace<Complex>? workspace, out Quantity<Complex> result)
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
                    workspace?.PostDiagnosticMessage(new("Cannot raise units to a complex power."));
                    result = Default;
                    return false;
                }

                if (!Fraction.TryConvert(b.Scalar.Real, out var fraction))
                {
                    // Could not convert to a fraction
                    workspace?.PostDiagnosticMessage(new("Cannot raise units to a power that is too complex."));
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
        public bool TryInUnit(Quantity<Complex> a, Quantity<Complex> b, ReadOnlyMemory<char> unit, IWorkspace<Complex>? workspace, out Quantity<Complex> result)
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
        public bool TryBitwiseOr(Quantity<Complex> a, Quantity<Complex> b, IWorkspace<Complex>? workspace, out Quantity<Complex> result)
        {
            // First convert to integer
            if (a.Unit != Unit.UnitNone || b.Unit != Unit.UnitNone)
            {
                // Don't know what to do here
                workspace?.PostDiagnosticMessage(new("Cannot take a bitwise OR of quantities with units."));
                result = Default;
                return false;
            }
            var la = (long)Math.Truncate(a.Scalar.Real);
            var lb = (long)Math.Truncate(b.Scalar.Real);
            result = new Quantity<Complex>(la | lb, Unit.UnitNone);
            return true;
        }

        /// <inheritdoc />
        public bool TryBitwiseAnd(Quantity<Complex> a, Quantity<Complex> b, IWorkspace<Complex>? workspace, out Quantity<Complex> result)
        {
            // First convert to integer
            if (a.Unit != Unit.UnitNone || b.Unit != Unit.UnitNone)
            {
                // Don't know what to do here
                workspace?.PostDiagnosticMessage(new("Cannot take a bitwise AND of quantities with units."));
                result = Default;
                return false;
            }
            var la = (long)Math.Truncate(a.Scalar.Real);
            var lb = (long)Math.Truncate(b.Scalar.Real);
            result = new Quantity<Complex>(la & lb, Unit.UnitNone);
            return true;
        }

        /// <inheritdoc />
        public bool TryLeftShift(Quantity<Complex> a, Quantity<Complex> b, IWorkspace<Complex>? workspace, out Quantity<Complex> result)
        {
            // First convert to integer
            if (a.Unit != Unit.UnitNone || b.Unit != Unit.UnitNone)
            {
                // Don't know what to do here
                workspace?.PostDiagnosticMessage(new("Cannot take shift quantities with units."));
                result = Default;
                return false;
            }
            var la = (long)Math.Truncate(a.Scalar.Real);
            var lb = (int)Math.Truncate(b.Scalar.Real);
            result = new Quantity<Complex>(la << lb, Unit.UnitNone);
            return true;
        }

        public bool TryRightShift(Quantity<Complex> a, Quantity<Complex> b, IWorkspace<Complex>? workspace, out Quantity<Complex> result)
        {
            // First convert to integer
            if (a.Unit != Unit.UnitNone || b.Unit != Unit.UnitNone)
            {
                // Don't know what to do here
                workspace?.PostDiagnosticMessage(new("Cannot take shift quantities with units."));
                result = Default;
                return false;
            }
            var la = (long)Math.Truncate(a.Scalar.Real);
            var lb = (int)Math.Truncate(b.Scalar.Real);
            result = new Quantity<Complex>(la >> lb, Unit.UnitNone);
            return true;
        }

        /// <inheritdoc />
        public bool TryGreaterThan(Quantity<Complex> a, Quantity<Complex> b, IWorkspace<Complex>? workspace, out Quantity<Complex> result)
        {
            if (a.Unit != b.Unit)
            {
                // Cannot compare quantities with different units
                workspace?.PostDiagnosticMessage(new("Cannot compare quantities with different units."));
                result = Default;
                return false;
            }
            result = new Quantity<Complex>(a.Scalar.Real > b.Scalar.Real ? 1.0 : 0.0, Unit.UnitNone);
            return true;
        }

        /// <inheritdoc />
        public bool TryGreaterThanOrEqual(Quantity<Complex> a, Quantity<Complex> b, IWorkspace<Complex>? workspace, out Quantity<Complex> result)
        {
            if (a.Unit != b.Unit)
            {
                // Cannot compare quantities with different units
                workspace?.PostDiagnosticMessage(new("Cannot compare quantities with different units."));
                result = Default;
                return false;
            }
            result = new Quantity<Complex>(a.Scalar.Real >= b.Scalar.Real ? 1.0 : 0.0, Unit.UnitNone);
            return true;
        }

        /// <inheritdoc />
        public bool TryLessThan(Quantity<Complex> a, Quantity<Complex> b, IWorkspace<Complex>? workspace, out Quantity<Complex> result)
        {
            if (a.Unit != b.Unit)
            {
                // Cannot compare quantities with different units
                workspace?.PostDiagnosticMessage(new("Cannot compare quantities with different units."));
                result = Default;
                return false;
            }
            result = new Quantity<Complex>(a.Scalar.Real < b.Scalar.Real ? 1.0 : 0.0, Unit.UnitNone);
            return true;
        }

        /// <inheritdoc />
        public bool TryLessThanOrEqual(Quantity<Complex> a, Quantity<Complex> b, IWorkspace<Complex>? workspace, out Quantity<Complex> result)
        {
            if (a.Unit != b.Unit)
            {
                // Cannot compare quantities with different units
                workspace?.PostDiagnosticMessage(new("Cannot compare quantities with different units."));
                result = Default;
                return false;
            }
            result = new Quantity<Complex>(a.Scalar.Real <= b.Scalar.Real ? 1.0 : 0.0, Unit.UnitNone);
            return true;
        }

        /// <inheritdoc />
        public bool TryEquals(Quantity<Complex> a, Quantity<Complex> b, IWorkspace<Complex>? workspace, out Quantity<Complex> result)
        {
            if (a.Unit != b.Unit)
            {
                // Cannot compare quantities with different units
                workspace?.PostDiagnosticMessage(new("Cannot compare quantities with different units."));
                result = Default;
                return false;
            }
            result = new Quantity<Complex>(a.Scalar == b.Scalar ? 1.0 : 0.0, Unit.UnitNone);
            return true;
        }

        /// <inheritdoc />
        public bool TryNotEquals(Quantity<Complex> a, Quantity<Complex> b, IWorkspace<Complex>? workspace, out Quantity<Complex> result)
        {
            if (a.Unit != b.Unit)
            {
                // Cannot compare quantities with different units
                workspace?.PostDiagnosticMessage(new("Cannot compare quantities with different units."));
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
        public bool TryIsTrue(Quantity<Complex> a, IWorkspace<Complex>? workspace, out bool result)
        {
            if (a.Scalar.Real == 0.0 && a.Scalar.Imaginary == 0.0)
                result = false;
            else
                result = true;
            return true;
        }

        /// <inheritdoc />
        public bool TryLogicalOr(Quantity<Complex> a, Quantity<Complex> b, IWorkspace<Complex>? workspace, out Quantity<Complex> result)
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
        public bool TryLogicalAnd(Quantity<Complex> a, Quantity<Complex> b, IWorkspace<Complex>? workspace, out Quantity<Complex> result)
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

        /// <inheritdoc />
        public void ToJSON(Complex value, Utf8JsonWriter writer, JsonWriterOptions options)
        {
            if (value.Imaginary.Equals(0.0))
            {
                writer.WriteNumberValue(value.Real);
            }
            else
            {
                writer.WriteStartArray();
                writer.WriteNumberValue(value.Real);
                writer.WriteNumberValue(value.Imaginary);
                writer.WriteEndArray();
            }
        }

        /// <inheritdoc />
        public Complex FromJSON(ref Utf8JsonReader reader, JsonReaderOptions options)
        {
            switch (reader.TokenType)
            {
                case JsonTokenType.Number:
                    return reader.GetDouble();

                case JsonTokenType.StartArray:
                    reader.Read();
                    if (reader.TokenType != JsonTokenType.Number)
                        throw new JsonException("Expected a number for the real part of a complex number.");
                    double real = reader.GetDouble();
                    reader.Read();
                    if (reader.TokenType != JsonTokenType.Number)
                        throw new JsonException("Expected a number for the imaginary part of a complex number.");
                    double imaginary = reader.GetDouble();
                    reader.Read();
                    if (reader.TokenType != JsonTokenType.EndArray)
                        throw new JsonException("Expected only 2 numbers for a complex number.");
                    return new Complex(real, imaginary);

                default:
                    throw new JsonException("Expected a number or a 2-number array for a complex number.");
            }
        }
    }
}
