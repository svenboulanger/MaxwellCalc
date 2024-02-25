using MaxwellCalc.Units;
using System;
using System.Collections.Generic;

namespace MaxwellCalc.Workspaces
{
    /// <summary>
    /// Defines common functions for math involving real numbers.
    /// </summary>
    public static class RealMathHelper
    {
        private static readonly Quantity<double> _invalid = new(double.NaN, Unit.Scalar);

        /// <summary>
        /// Registers the functions in the class to a workspace.
        /// </summary>
        /// <param name="workspace">The workspace.</param>
        public static void RegisterFunctions(IWorkspace<double> workspace)
        {
            workspace.TryRegisterFunction("abs", Abs);
            workspace.TryRegisterFunction("sin", Sin);
            workspace.TryRegisterFunction("cos", Cos);
            workspace.TryRegisterFunction("tan", Tan);
            workspace.TryRegisterFunction("sinh", Sinh);
            workspace.TryRegisterFunction("cosh", Cosh);
            workspace.TryRegisterFunction("tanh", Tanh);
            workspace.TryRegisterFunction("ln", Ln);
            workspace.TryRegisterFunction("log10", Log10);
            workspace.TryRegisterFunction("log2", Log2);
            workspace.TryRegisterFunction("exp", Exp);
            workspace.TryRegisterFunction("sqrt", Sqrt);
        }

        /// <summary>
        /// Computes the absolute value of a quantity.
        /// </summary>
        /// <param name="args">The arguments.</param>
        /// <param name="workspace">The workspace.</param>
        /// <param name="result">The result.</param>
        /// <returns>Returns <c>true</c> if the function was evaluated; otherwise, <c>false</c>.</returns>
        public static bool Abs(IReadOnlyList<Quantity<double>> args, IWorkspace workspace, out Quantity<double> result)
        {
            if (args.Count != 1)
            {
                result = _invalid;
                return false;
            }
            result = new Quantity<double>(Math.Abs(args[0].Scalar * args[0].Unit.Modifier), new Unit(1.0, args[0].Unit.BaseUnits));
            return true;
        }

        /// <summary>
        /// Computes the sine of a quantity.
        /// </summary>
        /// <param name="args">The arguments.</param>
        /// <param name="workspace">The workspace.</param>
        /// <param name="result">The result.</param>
        /// <returns>Returns <c>true</c> if the function was evaluated; otherwise, <c>false</c>.</returns>
        public static bool Sin(IReadOnlyList<Quantity<double>> args, IWorkspace workspace, out Quantity<double> result)
        {
            if (args.Count != 1)
            {
                // Only one argument allowed
                result = _invalid;
                return false;
            }
            var arg = args[0];
            if (arg.Unit.BaseUnits != BaseUnit.UnitNone && arg.Unit.BaseUnits != BaseUnit.UnitRadian)
            {
                // Cannot 
                result = _invalid;
                return false;
            }
            result = new Quantity<double>(Math.Sin(args[0].Scalar * args[0].Unit.Modifier), Unit.Scalar);
            return true;
        }

        /// <summary>
        /// Computes the cosine of a quantity.
        /// </summary>
        /// <param name="args">The arguments.</param>
        /// <param name="workspace">The workspace.</param>
        /// <param name="result">The result.</param>
        /// <returns>Returns <c>true</c> if the function was evaluated; otherwise, <c>false</c>.</returns>
        public static bool Cos(IReadOnlyList<Quantity<double>> args, IWorkspace workspace, out Quantity<double> result)
        {
            if (args.Count != 1)
            {
                // Only one argument allowed
                result = _invalid;
                return false;
            }
            var arg = args[0];
            if (arg.Unit.BaseUnits != BaseUnit.UnitNone && arg.Unit.BaseUnits != BaseUnit.UnitRadian)
            {
                // Cannot 
                result = _invalid;
                return false;
            }
            result = new Quantity<double>(Math.Cos(args[0].Scalar * args[0].Unit.Modifier), Unit.Scalar);
            return true;
        }

        /// <summary>
        /// Computes the tangent of a quantity.
        /// </summary>
        /// <param name="args">The arguments.</param>
        /// <param name="workspace">The workspace.</param>
        /// <param name="result">The result.</param>
        /// <returns>Returns <c>true</c> if the function was evaluated; otherwise, <c>false</c>.</returns>
        public static bool Tan(IReadOnlyList<Quantity<double>> args, IWorkspace workspace, out Quantity<double> result)
        {
            if (args.Count != 1)
            {
                // Only one argument allowed
                result = _invalid;
                return false;
            }
            var arg = args[0];
            if (arg.Unit.BaseUnits != BaseUnit.UnitNone && arg.Unit.BaseUnits != BaseUnit.UnitRadian)
            {
                // Cannot 
                result = _invalid;
                return false;
            }
            result = new Quantity<double>(Math.Tan(args[0].Scalar * args[0].Unit.Modifier), Unit.Scalar);
            return true;
        }

        /// <summary>
        /// Computes the hyperbolic sine of a quantity.
        /// </summary>
        /// <param name="args">The arguments.</param>
        /// <param name="workspace">The workspace.</param>
        /// <param name="result">The result.</param>
        /// <returns>Returns <c>true</c> if the function was evaluated; otherwise, <c>false</c>.</returns>
        public static bool Sinh(IReadOnlyList<Quantity<double>> args, IWorkspace workspace, out Quantity<double> result)
        {
            if (args.Count != 1)
            {
                // Only one argument allowed
                result = _invalid;
                return false;
            }
            var arg = args[0];
            if (arg.Unit.BaseUnits != BaseUnit.UnitNone && arg.Unit.BaseUnits != BaseUnit.UnitRadian)
            {
                // Cannot 
                result = _invalid;
                return false;
            }
            result = new Quantity<double>(Math.Sinh(args[0].Scalar * args[0].Unit.Modifier), Unit.Scalar);
            return true;
        }

        /// <summary>
        /// Computes the hyperbolic cosine of a quantity.
        /// </summary>
        /// <param name="args">The arguments.</param>
        /// <param name="workspace">The workspace.</param>
        /// <param name="result">The result.</param>
        /// <returns>Returns <c>true</c> if the function was evaluated; otherwise, <c>false</c>.</returns>
        public static bool Cosh(IReadOnlyList<Quantity<double>> args, IWorkspace workspace, out Quantity<double> result)
        {
            if (args.Count != 1)
            {
                // Only one argument allowed
                result = _invalid;
                return false;
            }
            var arg = args[0];
            if (arg.Unit.BaseUnits != BaseUnit.UnitNone && arg.Unit.BaseUnits != BaseUnit.UnitRadian)
            {
                // Cannot 
                result = _invalid;
                return false;
            }
            result = new Quantity<double>(Math.Cosh(args[0].Scalar * args[0].Unit.Modifier), Unit.Scalar);
            return true;
        }

        /// <summary>
        /// Computes the hyperbolic tangent of a quantity.
        /// </summary>
        /// <param name="args">The arguments.</param>
        /// <param name="workspace">The workspace.</param>
        /// <param name="result">The result.</param>
        /// <returns>Returns <c>true</c> if the function was evaluated; otherwise, <c>false</c>.</returns>
        public static bool Tanh(IReadOnlyList<Quantity<double>> args, IWorkspace workspace, out Quantity<double> result)
        {
            if (args.Count != 1)
            {
                // Only one argument allowed
                result = _invalid;
                return false;
            }
            var arg = args[0];
            if (arg.Unit.BaseUnits != BaseUnit.UnitNone && arg.Unit.BaseUnits != BaseUnit.UnitRadian)
            {
                // Cannot 
                result = _invalid;
                return false;
            }
            result = new Quantity<double>(Math.Tanh(args[0].Scalar * args[0].Unit.Modifier), Unit.Scalar);
            return true;
        }

        /// <summary>
        /// Computes the exponential of a quantity.
        /// </summary>
        /// <param name="args">The arguments.</param>
        /// <param name="workspace">The workspace.</param>
        /// <param name="result">The result.</param>
        /// <returns>Returns <c>true</c> if the function was evaluated; otherwise, <c>false</c>.</returns>
        public static bool Exp(IReadOnlyList<Quantity<double>> args, IWorkspace workspace, out Quantity<double> result)
        {
            if (args.Count != 1)
            {
                // Only one argument allowed
                result = _invalid;
                return false;
            }
            var arg = args[0];
            if (arg.Unit.BaseUnits != BaseUnit.UnitNone)
            {
                // Cannot deal with units
                result = _invalid;
                return false;
            }
            result = new Quantity<double>(Math.Exp(args[0].Scalar * args[0].Unit.Modifier), Unit.Scalar);
            return true;
        }

        /// <summary>
        /// Computes the natural logarithm of a quantity.
        /// </summary>
        /// <param name="args">The arguments.</param>
        /// <param name="workspace">The workspace.</param>
        /// <param name="result">The result.</param>
        /// <returns>Returns <c>true</c> if the function was evaluated; otherwise, <c>false</c>.</returns>
        public static bool Ln(IReadOnlyList<Quantity<double>> args, IWorkspace workspace, out Quantity<double> result)
        {
            if (args.Count != 1)
            {
                // Only one argument allowed
                result = _invalid;
                return false;
            }
            var arg = args[0];
            if (arg.Unit.BaseUnits != BaseUnit.UnitNone)
            {
                // Cannot deal with units
                result = _invalid;
                return false;
            }
            result = new Quantity<double>(Math.Log(args[0].Scalar * args[0].Unit.Modifier), Unit.Scalar);
            return true;
        }

        /// <summary>
        /// Computes the base-10 logarithm of a quantity.
        /// </summary>
        /// <param name="args">The arguments.</param>
        /// <param name="workspace">The workspace.</param>
        /// <param name="result">The result.</param>
        /// <returns>Returns <c>true</c> if the function was evaluated; otherwise, <c>false</c>.</returns>
        public static bool Log10(IReadOnlyList<Quantity<double>> args, IWorkspace workspace, out Quantity<double> result)
        {
            if (args.Count != 1)
            {
                // Only one argument allowed
                result = _invalid;
                return false;
            }
            var arg = args[0];
            if (arg.Unit.BaseUnits != BaseUnit.UnitNone)
            {
                // Cannot deal with units
                result = _invalid;
                return false;
            }
            result = new Quantity<double>(Math.Log10(args[0].Scalar * args[0].Unit.Modifier), Unit.Scalar);
            return true;
        }

        /// <summary>
        /// Computes the base-2 logarithm of a quantity.
        /// </summary>
        /// <param name="args">The arguments.</param>
        /// <param name="workspace">The workspace.</param>
        /// <param name="result">The result.</param>
        /// <returns>Returns <c>true</c> if the function was evaluated; otherwise, <c>false</c>.</returns>
        public static bool Log2(IReadOnlyList<Quantity<double>> args, IWorkspace workspace, out Quantity<double> result)
        {
            if (args.Count != 1)
            {
                // Only one argument allowed
                result = _invalid;
                return false;
            }
            var arg = args[0];
            if (arg.Unit.BaseUnits != BaseUnit.UnitNone)
            {
                // Cannot deal with units
                result = _invalid;
                return false;
            }
            result = new Quantity<double>(Math.Log2(args[0].Scalar * args[0].Unit.Modifier), Unit.Scalar);
            return true;
        }

        /// <summary>
        /// Computes the square root of a quantity.
        /// </summary>
        /// <param name="args">The arguments.</param>
        /// <param name="workspace">The workspace.</param>
        /// <param name="result">The result.</param>
        /// <returns>Returns <c>true</c> if the function was evaluated; otherwise, <c>false</c>.</returns>
        public static bool Sqrt(IReadOnlyList<Quantity<double>> args, IWorkspace workspace, out Quantity<double> result)
        {
            if (args.Count != 1)
            {
                // Only one argument allowed
                result = _invalid;
                return false;
            }
            var arg = args[0];
            result = new Quantity<double>(
                Math.Sqrt(args[0].Scalar * args[0].Unit.Modifier),
                new Unit(1.0, BaseUnit.Pow(arg.Unit.BaseUnits, new Fraction(1, 2))));
            return true;
        }
    }
}
