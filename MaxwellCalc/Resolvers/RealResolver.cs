using MaxwellCalc.Units;
using MaxwellCalc.Workspaces;
using System;
using System.Collections.Generic;

namespace MaxwellCalc.Resolvers
{
    /// <summary>
    /// A resolver for quantities with using doubles as scalars.
    /// </summary>
    public class RealResolver : IResolver<double>
    {
        /// <inheritdoc />
        public Quantity<double> Default { get; } = new Quantity<double>(1.0, Unit.UnitNone);

        /// <inheritdoc />
        public bool TryScalar(string scalar, IWorkspace<double> workspace, out Quantity<double> result)
        {
            // Parse the scalar
            if (!double.TryParse(scalar, System.Globalization.CultureInfo.InvariantCulture, out double dbl))
            {
                result = Default;
                return false;
            }
            result = new Quantity<double>(dbl, Unit.UnitNone);
            return true;
        }

        /// <inheritdoc />
        public bool TryUnit(string unit, IWorkspace<double> workspace, out Quantity<double> result)
            => workspace.TryGetUnit(unit, out result);

        /// <inheritdoc />
        public bool TryVariable(string variable, IWorkspace<double> workspace, out Quantity<double> result)
            => workspace.TryGetVariable(variable, out result);

        /// <inheritdoc />
        public bool TryPlus(Quantity<double> a, IWorkspace<double> workspace, out Quantity<double> result)
        {
            result = a;
            return true;
        }

        /// <inheritdoc />
        public bool TryMinus(Quantity<double> a, IWorkspace<double> workspace, out Quantity<double> result)
        {
            result = new Quantity<double>(-a.Scalar, a.Unit);
            return true;
        }

        public bool TryFactorial(Quantity<double> a, IWorkspace<double> workspace, out Quantity<double> result)
        {
            throw new NotImplementedException();
        }

        /// <inheritdoc />
        public bool TryAdd(Quantity<double> a, Quantity<double> b, IWorkspace<double> workspace, out Quantity<double> result)
        {
            // Check units
            if (a.Unit != b.Unit)
            {
                // Units should match!
                result = Default;
                return false;
            }

            result = new Quantity<double>(a.Scalar + b.Scalar, a.Unit);
            return true;
        }

        /// <inheritdoc />
        public bool TrySubtract(Quantity<double> a, Quantity<double> b, IWorkspace<double> workspace, out Quantity<double> result)
        {
            // Check units
            if (a.Unit != b.Unit)
            {
                // Units should match!
                result = Default;
                return false;
            }
            result = new Quantity<double>(a.Scalar - b.Scalar, a.Unit);
            return true;
        }

        /// <inheritdoc />
        public bool TryMultiply(Quantity<double> a, Quantity<double> b, IWorkspace<double> workspace, out Quantity<double> result)
        {
            result = new Quantity<double>(
                a.Scalar * b.Scalar,
                a.Unit * b.Unit);
            return true;
        }

        /// <inheritdoc />
        public bool TryDivide(Quantity<double> a, Quantity<double> b, IWorkspace<double> workspace, out Quantity<double> result)
        {
            result = new Quantity<double>(
                a.Scalar / b.Scalar,
                a.Unit / b.Unit);
            return true;
        }

        /// <inheritdoc />
        public bool TryModulo(Quantity<double> a, Quantity<double> b, IWorkspace<double> workspace, out Quantity<double> result)
        {
            result = new Quantity<double>(
                Math.IEEERemainder(a.Scalar, b.Scalar),
                a.Unit);
            return true;
        }

        /// <inheritdoc />
        public bool TryIntDivide(Quantity<double> a, Quantity<double> b, IWorkspace<double> workspace, out Quantity<double> result)
        {
            result = new Quantity<double>(
                Math.Truncate(a.Scalar / b.Scalar),
                a.Unit / b.Unit);
            return true;
        }

        /// <inheritdoc />
        public bool TryExp(Quantity<double> a, Quantity<double> b, IWorkspace<double> workspace, out Quantity<double> result)
        {
            // Exponentiation can only happen with numbers that don't have units
            if (b.Unit != Unit.UnitNone)
            {
                // Cannot use exponent with units
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
        public bool TryInUnit(Quantity<double> a, Quantity<double> b, ReadOnlyMemory<char> content, IWorkspace<double> workspace, out Quantity<double> result)
        {
            if (a.Unit != b.Unit)
            {
                // Should be same units in order to compute
                result = Default;
                return false;
            }

            // Don't do anything, formatting is done at the top level
            result = a;
            return true;
        }

        /// <inheritdoc />
        public bool TryBitwiseOr(Quantity<double> a, Quantity<double> b, IWorkspace<double> workspace, out Quantity<double> result)
        {
            // First convert to integer
            if (a.Unit != Unit.UnitNone || b.Unit != Unit.UnitNone)
            {
                // Don't know what to do here
                result = Default;
                return false;
            }
            var la = (long)Math.Truncate(a.Scalar);
            var lb = (long)Math.Truncate(b.Scalar);
            result = new Quantity<double>(la | lb, Unit.UnitNone);
            return true;
        }

        /// <inheritdoc />
        public bool TryBitwiseAnd(Quantity<double> a, Quantity<double> b, IWorkspace<double> workspace, out Quantity<double> result)
        {
            // First convert to integer
            if (a.Unit != Unit.UnitNone || b.Unit != Unit.UnitNone)
            {
                // Don't know what to do here
                result = Default;
                return false;
            }
            var la = (long)Math.Truncate(a.Scalar );
            var lb = (long)Math.Truncate(b.Scalar);
            result = new Quantity<double>(la & lb, Unit.UnitNone);
            return true;
        }

        /// <inheritdoc />
        public bool TryLeftShift(Quantity<double> a, Quantity<double> b, IWorkspace<double> workspace, out Quantity<double> result)
        {
            // First convert to integer
            if (a.Unit != Unit.UnitNone || b.Unit != Unit.UnitNone)
            {
                // Don't know what to do here
                result = Default;
                return false;
            }
            var la = (long)Math.Truncate(a.Scalar);
            var lb = (int)Math.Truncate(b.Scalar);
            result = new Quantity<double>(la << lb, Unit.UnitNone);
            return true;
        }

        /// <inheritdoc />
        public bool TryRightShift(Quantity<double> a, Quantity<double> b, IWorkspace<double> workspace, out Quantity<double> result)
        {
            // First convert to integer
            if (a.Unit != Unit.UnitNone || b.Unit != Unit.UnitNone)
            {
                // Don't know what to do here
                result = Default;
                return false;
            }
            var la = (long)Math.Truncate(a.Scalar);
            var lb = (int)Math.Truncate(b.Scalar);
            result = new Quantity<double>(la >> lb, Unit.UnitNone);
            return true;
        }

        /// <inheritdoc />
        public bool TryGreaterThan(Quantity<double> a, Quantity<double> b, IWorkspace<double> workspace, out Quantity<double> result)
        {
            if (a.Unit != b.Unit)
            {
                // Cannot compare quantities with different units
                result = Default;
                return false;
            }
            result = new Quantity<double>(a.Scalar > b.Scalar ? 1.0 : 0.0, Unit.UnitNone);
            return true;
        }

        /// <inheritdoc />
        public bool TryGreaterThanOrEqual(Quantity<double> a, Quantity<double> b, IWorkspace<double> workspace, out Quantity<double> result)
        {
            if (a.Unit!= b.Unit)
            {
                // Cannot compare quantities with different units
                result = Default;
                return false;
            }
            result = new Quantity<double>(a.Scalar >= b.Scalar ? 1.0 : 0.0, Unit.UnitNone);
            return true;
        }

        /// <inheritdoc />
        public bool TryLessThan(Quantity<double> a, Quantity<double> b, IWorkspace<double> workspace, out Quantity<double> result)
        {
            if (a.Unit != b.Unit)
            {
                // Cannot compare quantities with different units
                result = Default;
                return false;
            }
            result = new Quantity<double>(a.Scalar < b.Scalar ? 1.0 : 0.0, Unit.UnitNone);
            return true;
        }

        /// <inheritdoc />
        public bool TryLessThanOrEqual(Quantity<double> a, Quantity<double> b, IWorkspace<double> workspace, out Quantity<double> result)
        {
            if (a.Unit != b.Unit)
            {
                // Cannot compare quantities with different units
                result = Default;
                return false;
            }
            result = new Quantity<double>(a.Scalar <= b.Scalar ? 1.0 : 0.0, Unit.UnitNone);
            return true;
        }

        /// <inheritdoc />
        public bool TryEquals(Quantity<double> a, Quantity<double> b, IWorkspace<double> workspace, out Quantity<double> result)
        {
            if (a.Unit != b.Unit)
            {
                // Cannot compare quantities with different units
                result = Default;
                return false;
            }
            result = new Quantity<double>(a.Scalar == b.Scalar ? 1.0 : 0.0, Unit.UnitNone);
            return true;
        }

        /// <inheritdoc />
        public bool TryNotEquals(Quantity<double> a, Quantity<double> b, IWorkspace<double> workspace, out Quantity<double> result)
        {
            if (a.Unit != b.Unit)
            {
                // Cannot compare quantities with different units
                result = Default;
                return false;
            }
            result = new Quantity<double>(a.Scalar != b.Scalar ? 1.0 : 0.0, Unit.UnitNone);
            return true;
        }

        /// <inheritdoc />
        public bool TryAssign(string name, Quantity<double> b, IWorkspace<double> workspace, out Quantity<double> result)
        {
            if (!workspace.TrySetVariable(name, b))
            {
                result = Default;
                return false;
            }
            result = b;
            return true;
        }

        /// <inheritdoc />
        public bool TryFunction(string name, IReadOnlyList<Quantity<double>> arguments, IWorkspace<double> workspace, out Quantity<double> result)
            => workspace.TryFunction(name, arguments, out result);
    }
}
