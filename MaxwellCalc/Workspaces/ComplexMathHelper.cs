using MaxwellCalc.Units;
using System;
using System.Collections.Generic;
using System.Numerics;

namespace MaxwellCalc.Workspaces
{
    public static class ComplexMathHelper
    {
        private static readonly Quantity<Complex> _invalid = new(Complex.NaN, Unit.UnitNone);

        /// <summary>
        /// Registers the functions in the class to a workspace.
        /// </summary>
        /// <param name="workspace">The workspace.</param>
        public static void RegisterFunctions(IWorkspace<Complex> workspace)
        {
            workspace.TryRegisterBuiltInFunction("abs", Abs);
            workspace.TryRegisterBuiltInFunction("sin", Sin);
            workspace.TryRegisterBuiltInFunction("cos", Cos);
            workspace.TryRegisterBuiltInFunction("tan", Tan);
            workspace.TryRegisterBuiltInFunction("sinh", Sinh);
            workspace.TryRegisterBuiltInFunction("cosh", Cosh);
            workspace.TryRegisterBuiltInFunction("tanh", Tanh);
            workspace.TryRegisterBuiltInFunction("ln", Ln);
            workspace.TryRegisterBuiltInFunction("log10", Log10);
            workspace.TryRegisterBuiltInFunction("log2", Log2);
            workspace.TryRegisterBuiltInFunction("exp", Exp);
            workspace.TryRegisterBuiltInFunction("sqrt", Sqrt);
        }

        /// <summary>
        /// Computes the absolute value of a quantity.
        /// </summary>
        /// <param name="args">The arguments.</param>
        /// <param name="workspace">The workspace.</param>
        /// <param name="result">The result.</param>
        /// <returns>Returns <c>true</c> if the function was evaluated; otherwise, <c>false</c>.</returns>
        public static bool Abs(IReadOnlyList<Quantity<Complex>> args, IWorkspace workspace, out Quantity<Complex> result)
        {
            if (args.Count != 1)
            {
                result = _invalid;
                return false;
            }
            result = new Quantity<Complex>(Complex.Abs(args[0].Scalar), args[0].Unit);
            return true;
        }

        /// <summary>
        /// Computes the sine of a quantity.
        /// </summary>
        /// <param name="args">The arguments.</param>
        /// <param name="workspace">The workspace.</param>
        /// <param name="result">The result.</param>
        /// <returns>Returns <c>true</c> if the function was evaluated; otherwise, <c>false</c>.</returns>
        public static bool Sin(IReadOnlyList<Quantity<Complex>> args, IWorkspace workspace, out Quantity<Complex> result)
        {
            if (args.Count != 1)
            {
                // Only one argument allowed
                result = _invalid;
                return false;
            }
            var arg = args[0];
            if (arg.Unit != Unit.UnitNone && arg.Unit != Unit.UnitRadian)
            {
                // Cannot 
                result = _invalid;
                return false;
            }
            result = new Quantity<Complex>(Complex.Sin(args[0].Scalar), Unit.UnitNone);
            return true;
        }

        /// <summary>
        /// Computes the cosine of a quantity.
        /// </summary>
        /// <param name="args">The arguments.</param>
        /// <param name="workspace">The workspace.</param>
        /// <param name="result">The result.</param>
        /// <returns>Returns <c>true</c> if the function was evaluated; otherwise, <c>false</c>.</returns>
        public static bool Cos(IReadOnlyList<Quantity<Complex>> args, IWorkspace workspace, out Quantity<Complex> result)
        {
            if (args.Count != 1)
            {
                // Only one argument allowed
                result = _invalid;
                return false;
            }
            var arg = args[0];
            if (arg.Unit != Unit.UnitNone && arg.Unit != Unit.UnitRadian)
            {
                // Cannot 
                result = _invalid;
                return false;
            }
            result = new Quantity<Complex>(Complex.Cos(args[0].Scalar), Unit.UnitNone);
            return true;
        }

        /// <summary>
        /// Computes the tangent of a quantity.
        /// </summary>
        /// <param name="args">The arguments.</param>
        /// <param name="workspace">The workspace.</param>
        /// <param name="result">The result.</param>
        /// <returns>Returns <c>true</c> if the function was evaluated; otherwise, <c>false</c>.</returns>
        public static bool Tan(IReadOnlyList<Quantity<Complex>> args, IWorkspace workspace, out Quantity<Complex> result)
        {
            if (args.Count != 1)
            {
                // Only one argument allowed
                result = _invalid;
                return false;
            }
            var arg = args[0];
            if (arg.Unit != Unit.UnitNone && arg.Unit != Unit.UnitRadian)
            {
                // Cannot 
                result = _invalid;
                return false;
            }
            result = new Quantity<Complex>(Complex.Tan(args[0].Scalar), Unit.UnitNone);
            return true;
        }

        /// <summary>
        /// Computes the hyperbolic sine of a quantity.
        /// </summary>
        /// <param name="args">The arguments.</param>
        /// <param name="workspace">The workspace.</param>
        /// <param name="result">The result.</param>
        /// <returns>Returns <c>true</c> if the function was evaluated; otherwise, <c>false</c>.</returns>
        public static bool Sinh(IReadOnlyList<Quantity<Complex>> args, IWorkspace workspace, out Quantity<Complex> result)
        {
            if (args.Count != 1)
            {
                // Only one argument allowed
                result = _invalid;
                return false;
            }
            var arg = args[0];
            if (arg.Unit != Unit.UnitNone && arg.Unit != Unit.UnitRadian)
            {
                // Cannot 
                result = _invalid;
                return false;
            }
            result = new Quantity<Complex>(Complex.Sinh(args[0].Scalar), Unit.UnitNone);
            return true;
        }

        /// <summary>
        /// Computes the hyperbolic cosine of a quantity.
        /// </summary>
        /// <param name="args">The arguments.</param>
        /// <param name="workspace">The workspace.</param>
        /// <param name="result">The result.</param>
        /// <returns>Returns <c>true</c> if the function was evaluated; otherwise, <c>false</c>.</returns>
        public static bool Cosh(IReadOnlyList<Quantity<Complex>> args, IWorkspace workspace, out Quantity<Complex> result)
        {
            if (args.Count != 1)
            {
                // Only one argument allowed
                result = _invalid;
                return false;
            }
            var arg = args[0];
            if (arg.Unit != Unit.UnitNone && arg.Unit != Unit.UnitRadian)
            {
                // Cannot 
                result = _invalid;
                return false;
            }
            result = new Quantity<Complex>(Complex.Cosh(args[0].Scalar), Unit.UnitNone);
            return true;
        }

        /// <summary>
        /// Computes the hyperbolic tangent of a quantity.
        /// </summary>
        /// <param name="args">The arguments.</param>
        /// <param name="workspace">The workspace.</param>
        /// <param name="result">The result.</param>
        /// <returns>Returns <c>true</c> if the function was evaluated; otherwise, <c>false</c>.</returns>
        public static bool Tanh(IReadOnlyList<Quantity<Complex>> args, IWorkspace workspace, out Quantity<Complex> result)
        {
            if (args.Count != 1)
            {
                // Only one argument allowed
                result = _invalid;
                return false;
            }
            var arg = args[0];
            if (arg.Unit != Unit.UnitNone && arg.Unit != Unit.UnitRadian)
            {
                // Cannot 
                result = _invalid;
                return false;
            }
            result = new Quantity<Complex>(Complex.Tanh(args[0].Scalar), Unit.UnitNone);
            return true;
        }

        /// <summary>
        /// Computes the exponential of a quantity.
        /// </summary>
        /// <param name="args">The arguments.</param>
        /// <param name="workspace">The workspace.</param>
        /// <param name="result">The result.</param>
        /// <returns>Returns <c>true</c> if the function was evaluated; otherwise, <c>false</c>.</returns>
        public static bool Exp(IReadOnlyList<Quantity<Complex>> args, IWorkspace workspace, out Quantity<Complex> result)
        {
            if (args.Count != 1)
            {
                // Only one argument allowed
                result = _invalid;
                return false;
            }
            var arg = args[0];
            if (arg.Unit != Unit.UnitNone)
            {
                // Cannot deal with units
                result = _invalid;
                return false;
            }
            result = new Quantity<Complex>(Complex.Exp(args[0].Scalar), Unit.UnitNone);
            return true;
        }

        /// <summary>
        /// Computes the natural logarithm of a quantity.
        /// </summary>
        /// <param name="args">The arguments.</param>
        /// <param name="workspace">The workspace.</param>
        /// <param name="result">The result.</param>
        /// <returns>Returns <c>true</c> if the function was evaluated; otherwise, <c>false</c>.</returns>
        public static bool Ln(IReadOnlyList<Quantity<Complex>> args, IWorkspace workspace, out Quantity<Complex> result)
        {
            if (args.Count != 1)
            {
                // Only one argument allowed
                result = _invalid;
                return false;
            }
            var arg = args[0];
            if (arg.Unit != Unit.UnitNone)
            {
                // Cannot deal with units
                result = _invalid;
                return false;
            }
            result = new Quantity<Complex>(Complex.Log(args[0].Scalar), Unit.UnitNone);
            return true;
        }

        /// <summary>
        /// Computes the base-10 logarithm of a quantity.
        /// </summary>
        /// <param name="args">The arguments.</param>
        /// <param name="workspace">The workspace.</param>
        /// <param name="result">The result.</param>
        /// <returns>Returns <c>true</c> if the function was evaluated; otherwise, <c>false</c>.</returns>
        public static bool Log10(IReadOnlyList<Quantity<Complex>> args, IWorkspace workspace, out Quantity<Complex> result)
        {
            if (args.Count != 1)
            {
                // Only one argument allowed
                result = _invalid;
                return false;
            }
            var arg = args[0];
            if (arg.Unit != Unit.UnitNone)
            {
                // Cannot deal with units
                result = _invalid;
                return false;
            }
            result = new Quantity<Complex>(Complex.Log10(args[0].Scalar), Unit.UnitNone);
            return true;
        }

        /// <summary>
        /// Computes the base-2 logarithm of a quantity.
        /// </summary>
        /// <param name="args">The arguments.</param>
        /// <param name="workspace">The workspace.</param>
        /// <param name="result">The result.</param>
        /// <returns>Returns <c>true</c> if the function was evaluated; otherwise, <c>false</c>.</returns>
        public static bool Log2(IReadOnlyList<Quantity<Complex>> args, IWorkspace workspace, out Quantity<Complex> result)
        {
            if (args.Count != 1)
            {
                // Only one argument allowed
                result = _invalid;
                return false;
            }
            var arg = args[0];
            if (arg.Unit != Unit.UnitNone)
            {
                // Cannot deal with units
                result = _invalid;
                return false;
            }
            result = new Quantity<Complex>(Complex.Log(args[0].Scalar) / Math.Log(2.0), Unit.UnitNone);
            return true;
        }

        /// <summary>
        /// Computes the square root of a quantity.
        /// </summary>
        /// <param name="args">The arguments.</param>
        /// <param name="workspace">The workspace.</param>
        /// <param name="result">The result.</param>
        /// <returns>Returns <c>true</c> if the function was evaluated; otherwise, <c>false</c>.</returns>
        public static bool Sqrt(IReadOnlyList<Quantity<Complex>> args, IWorkspace workspace, out Quantity<Complex> result)
        {
            if (args.Count != 1)
            {
                // Only one argument allowed
                result = _invalid;
                return false;
            }
            var arg = args[0];
            result = new Quantity<Complex>(
                Complex.Sqrt(args[0].Scalar),
                Unit.Pow(arg.Unit, new Fraction(1, 2)));
            return true;
        }
    }
}
