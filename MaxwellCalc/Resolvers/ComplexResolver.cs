using MaxwellCalc.Units;
using MaxwellCalc.Workspaces;
using System;
using System.Collections.Generic;
using System.Numerics;

namespace MaxwellCalc.Resolvers
{
    public class ComplexResolver : IResolver<Complex>
    {
        /// <inheritdoc />
        public Quantity<Complex> Default { get; } = new Quantity<Complex>(0.0, Unit.UnitNone);

        /// <inheritdoc />
        public string Error { get; private set; } = string.Empty;

        /// <inheritdoc />
        public bool TryScalar(string scalar, IWorkspace<Complex> workspace, out Quantity<Complex> result)
        {
            // Parse the scalar
            if (!double.TryParse(scalar, System.Globalization.CultureInfo.InvariantCulture, out double dbl))
            {
                Error = $"Could not evaluate the scalar '{scalar}'.";
                result = Default;
                return false;
            }
            result = new Quantity<Complex>(dbl, Unit.UnitNone);
            return true;
        }

        /// <inheritdoc />
        public bool TryUnit(string unit, IWorkspace<Complex> workspace, out Quantity<Complex> result)
        {
            if (workspace.TryGetUnit(unit, out result))
                return true;
            Error = $"Could not recognize unit '{unit}'.";
            return false;
        }

        /// <inheritdoc />
        public bool TryVariable(string variable, IWorkspace<Complex> workspace, out Quantity<Complex> result)
        {
            if (workspace.TryGetVariable(variable, out result))
                return true;
            if (variable == "i" || variable == "j")
            {
                result = new Quantity<Complex>(new Complex(0.0, 1.0), Unit.UnitNone);
                return true;
            }
            Error = $"Could not find a variable with the name '{variable}'.";
            return false;
        }

        /// <inheritdoc />
        public bool TryPlus(Quantity<Complex> a, IWorkspace<Complex> workspace, out Quantity<Complex> result)
        {
            result = a;
            return true;
        }

        /// <inheritdoc />
        public bool TryMinus(Quantity<Complex> a, IWorkspace<Complex> workspace, out Quantity<Complex> result)
        {
            result = new Quantity<Complex>(-a.Scalar, a.Unit);
            return true;
        }

        public bool TryFactorial(Quantity<Complex> a, IWorkspace<Complex> workspace, out Quantity<Complex> result)
        {
            throw new NotImplementedException();
        }

        /// <inheritdoc />
        public bool TryAdd(Quantity<Complex> a, Quantity<Complex> b, IWorkspace<Complex> workspace, out Quantity<Complex> result)
        {
            // Check units
            if (a.Unit != b.Unit)
            {
                // Units should match!
                Error = "Units do not match for addition.";
                result = Default;
                return false;
            }
            result = new Quantity<Complex>(a.Scalar + b.Scalar, a.Unit);
            return true;
        }

        public bool TrySubtract(Quantity<Complex> a, Quantity<Complex> b, IWorkspace<Complex> workspace, out Quantity<Complex> result)
        {
            // Check units
            if (a.Unit != b.Unit)
            {
                // Units should match!
                Error = "Units do not match for subtraction.";
                result = Default;
                return false;
            }
            result = new Quantity<Complex>(a.Scalar - b.Scalar, a.Unit);
            return true;
        }

        public bool TryMultiply(Quantity<Complex> a, Quantity<Complex> b, IWorkspace<Complex> workspace, out Quantity<Complex> result)
        {
            result = new Quantity<Complex>(a.Scalar * b.Scalar, a.Unit * b.Unit);
            return true;
        }

        public bool TryDivide(Quantity<Complex> a, Quantity<Complex> b, IWorkspace<Complex> workspace, out Quantity<Complex> result)
        {
            result = new Quantity<Complex>(a.Scalar / b.Scalar, a.Unit / b.Unit);
            return true;
        }

        /// <inheritdoc />
        public bool TryModulo(Quantity<Complex> a, Quantity<Complex> b, IWorkspace<Complex> workspace, out Quantity<Complex> result)
        {
            result = new Quantity<Complex>(
                Math.IEEERemainder(a.Scalar.Real, b.Scalar.Real),
                a.Unit);
            return true;
        }

        /// <inheritdoc />
        public bool TryIntDivide(Quantity<Complex> a, Quantity<Complex> b, IWorkspace<Complex> workspace, out Quantity<Complex> result)
        {
            result = new Quantity<Complex>(
                Math.Truncate(a.Scalar.Real / b.Scalar.Real),
                a.Unit / b.Unit);
            return true;
        }

        /// <inheritdoc />
        public bool TryExp(Quantity<Complex> a, Quantity<Complex> b, IWorkspace<Complex> workspace, out Quantity<Complex> result)
        {
            // Exponentiation can only happen with numbers that don't have units
            if (b.Unit != Unit.UnitNone)
            {
                // Cannot use exponent with units
                Error = "Cannot raise to a power where the exponent contains units.";
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
                    Error = "Cannot raise units to a complex power.";
                    result = Default;
                    return false;
                }

                if (!Fraction.TryConvert(b.Scalar.Real, out var fraction))
                {
                    // Could not convert to a fraction
                    Error = "Cannot raise units to a power that is too complex.";
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
        public bool TryInUnit(Quantity<Complex> a, Quantity<Complex> b, ReadOnlyMemory<char> unit, IWorkspace<Complex> workspace, out Quantity<Complex> result)
        {
            if (a.Unit != b.Unit)
            {
                // Should be same units in order to compute
                Error = "The units do not match.";
                result = Default;
                return false;
            }

            // Don't do anything, formatting is done at the top level
            result = a;
            return true;
        }

        /// <inheritdoc />
        public bool TryBitwiseOr(Quantity<Complex> a, Quantity<Complex> b, IWorkspace<Complex> workspace, out Quantity<Complex> result)
        {
            // First convert to integer
            if (a.Unit != Unit.UnitNone || b.Unit != Unit.UnitNone)
            {
                // Don't know what to do here
                Error = "Cannot take a bitwise OR of quantities with units.";
                result = Default;
                return false;
            }
            var la = (long)Math.Truncate(a.Scalar.Real);
            var lb = (long)Math.Truncate(b.Scalar.Real);
            result = new Quantity<Complex>(la | lb, Unit.UnitNone);
            return true;
        }

        /// <inheritdoc />
        public bool TryBitwiseAnd(Quantity<Complex> a, Quantity<Complex> b, IWorkspace<Complex> workspace, out Quantity<Complex> result)
        {
            // First convert to integer
            if (a.Unit != Unit.UnitNone || b.Unit != Unit.UnitNone)
            {
                // Don't know what to do here
                Error = "Cannot take a bitwise AND of quantities with units.";
                result = Default;
                return false;
            }
            var la = (long)Math.Truncate(a.Scalar.Real);
            var lb = (long)Math.Truncate(b.Scalar.Real);
            result = new Quantity<Complex>(la & lb, Unit.UnitNone);
            return true;
        }

        /// <inheritdoc />
        public bool TryLeftShift(Quantity<Complex> a, Quantity<Complex> b, IWorkspace<Complex> workspace, out Quantity<Complex> result)
        {
            // First convert to integer
            if (a.Unit != Unit.UnitNone || b.Unit != Unit.UnitNone)
            {
                // Don't know what to do here
                Error = "Cannot take shift quantities with units.";
                result = Default;
                return false;
            }
            var la = (long)Math.Truncate(a.Scalar.Real);
            var lb = (int)Math.Truncate(b.Scalar.Real);
            result = new Quantity<Complex>(la << lb, Unit.UnitNone);
            return true;
        }

        public bool TryRightShift(Quantity<Complex> a, Quantity<Complex> b, IWorkspace<Complex> workspace, out Quantity<Complex> result)
        {
            // First convert to integer
            if (a.Unit != Unit.UnitNone || b.Unit != Unit.UnitNone)
            {
                // Don't know what to do here
                Error = "Cannot take shift quantities with units.";
                result = Default;
                return false;
            }
            var la = (long)Math.Truncate(a.Scalar.Real);
            var lb = (int)Math.Truncate(b.Scalar.Real);
            result = new Quantity<Complex>(la >> lb, Unit.UnitNone);
            return true;
        }

        /// <inheritdoc />
        public bool TryGreaterThan(Quantity<Complex> a, Quantity<Complex> b, IWorkspace<Complex> workspace, out Quantity<Complex> result)
        {
            if (a.Unit != b.Unit)
            {
                // Cannot compare quantities with different units
                Error = "Cannot compare quantities with different units.";
                result = Default;
                return false;
            }
            result = new Quantity<Complex>(a.Scalar.Real > b.Scalar.Real ? 1.0 : 0.0, Unit.UnitNone);
            return true;
        }

        /// <inheritdoc />
        public bool TryGreaterThanOrEqual(Quantity<Complex> a, Quantity<Complex> b, IWorkspace<Complex> workspace, out Quantity<Complex> result)
        {
            if (a.Unit != b.Unit)
            {
                // Cannot compare quantities with different units
                Error = "Cannot compare quantities with different units.";
                result = Default;
                return false;
            }
            result = new Quantity<Complex>(a.Scalar.Real >= b.Scalar.Real ? 1.0 : 0.0, Unit.UnitNone);
            return true;
        }

        /// <inheritdoc />
        public bool TryLessThan(Quantity<Complex> a, Quantity<Complex> b, IWorkspace<Complex> workspace, out Quantity<Complex> result)
        {
            if (a.Unit != b.Unit)
            {
                // Cannot compare quantities with different units
                Error = "Cannot compare quantities with different units.";
                result = Default;
                return false;
            }
            result = new Quantity<Complex>(a.Scalar.Real < b.Scalar.Real ? 1.0 : 0.0, Unit.UnitNone);
            return true;
        }

        /// <inheritdoc />
        public bool TryLessThanOrEqual(Quantity<Complex> a, Quantity<Complex> b, IWorkspace<Complex> workspace, out Quantity<Complex> result)
        {
            if (a.Unit != b.Unit)
            {
                // Cannot compare quantities with different units
                Error = "Cannot compare quantities with different units.";
                result = Default;
                return false;
            }
            result = new Quantity<Complex>(a.Scalar.Real <= b.Scalar.Real ? 1.0 : 0.0, Unit.UnitNone);
            return true;
        }

        /// <inheritdoc />
        public bool TryEquals(Quantity<Complex> a, Quantity<Complex> b, IWorkspace<Complex> workspace, out Quantity<Complex> result)
        {
            if (a.Unit != b.Unit)
            {
                // Cannot compare quantities with different units
                Error = "Cannot compare quantities with different units.";
                result = Default;
                return false;
            }
            result = new Quantity<Complex>(a.Scalar == b.Scalar ? 1.0 : 0.0, Unit.UnitNone);
            return true;
        }

        /// <inheritdoc />
        public bool TryNotEquals(Quantity<Complex> a, Quantity<Complex> b, IWorkspace<Complex> workspace, out Quantity<Complex> result)
        {
            if (a.Unit != b.Unit)
            {
                // Cannot compare quantities with different units
                Error = "Cannot compare quantities with different units.";
                result = Default;
                return false;
            }
            result = new Quantity<Complex>(a.Scalar != b.Scalar ? 1.0 : 0.0, Unit.UnitNone);
            return true;
        }

        /// <inheritdoc />
        public bool TryAssign(string name, Quantity<Complex> b, IWorkspace<Complex> workspace, out Quantity<Complex> result)
        {
            if (workspace.TrySetVariable(name, b))
            {
                result = b;
                return true;
            }

            Error = $"Could not assign to '{name}'.";
            result = Default;
            return false;
        }

        /// <inheritdoc />
        public bool TryFunction(string name, IReadOnlyList<Quantity<Complex>> arguments, IWorkspace<Complex> workspace, out Quantity<Complex> result)
        {
            if (workspace.TryFunction(name, arguments, out result))
                return true;
            Error = $"Cannot find function '{name}' for the given arguments.";
            return false;
        }
    }
}
