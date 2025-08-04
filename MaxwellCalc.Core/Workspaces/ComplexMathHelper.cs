using MaxwellCalc.Core.Attributes;
using MaxwellCalc.Units;
using System;
using System.Collections.Generic;
using System.Numerics;

namespace MaxwellCalc.Workspaces
{
    /// <summary>
    /// Methods for complex math.
    /// </summary>
    public static class ComplexMathHelper
    {
        private static readonly Quantity<Complex> _invalid = new(new Complex(double.NaN, double.NaN), Unit.UnitNone);

        /// <summary>
        /// Registers common constants.
        /// </summary>
        /// <param name="workspace">The workspace.Variables.</param>
        public static void RegisterCommonConstants(IWorkspace<Complex> workspace)
        {
            // Pi, as expected
            workspace.Scope.TrySetVariable("pi", new Quantity<Complex>(Math.PI, Unit.UnitNone));

            // Euler number
            workspace.Scope.TrySetVariable("e", new Quantity<Complex>(Math.E, Unit.UnitNone));

            // Speed of light
            workspace.Scope.TrySetVariable("c", new Quantity<Complex>(299792458.0, new Unit((Unit.Meter, 1), (Unit.Second, -1))));
        }

        /// <summary>
        /// Registers constants that are common for electrical applications.
        /// </summary>
        /// <param name="workspace">The workspace.Variables.</param>
        public static void RegisterCommonElectronicsConstants(IWorkspace<Complex> workspace)
        {
            // Elementary charge (Coulomb)
            workspace.Scope.TrySetVariable("q", new Quantity<Complex>(1.60217663e-19, new Unit((Unit.Ampere, 1), (Unit.Second, 1))));

            // Permittivity of vacuum (Farad/meter)
            workspace.Scope.TrySetVariable("eps0", new Quantity<Complex>(8.8541878128e-12, new Unit(
                    (Unit.Kilogram, -1),
                    (Unit.Meter, -3),
                    (Unit.Second, 4),
                    (Unit.Ampere, 2))));

            // Permeability of vacuum (Newton Ampere^-2)
            workspace.Scope.TrySetVariable("mu0", new Quantity<Complex>(1.25663706212e-6, new Unit(
                (Unit.Kilogram, 1),
                (Unit.Meter, 1),
                (Unit.Second, -2),
                (Unit.Ampere, -2))));

            // Electron-volt (eV)
            workspace.Scope.TrySetVariable("eV", new Quantity<Complex>(1.60217663e-19, new Unit(
                (Unit.Kilogram, 1),
                (Unit.Meter, 2),
                (Unit.Second, -2))));

            // Planck constant (J s)
            workspace.Scope.TrySetVariable("h", new Quantity<Complex>(6.6260693e-34, new Unit(
                (Unit.Kilogram, 1),
                (Unit.Meter, 2),
                (Unit.Second, -1))));

            // Reduced Planck constant bar (J s)
            workspace.Scope.TrySetVariable("hbar", new Quantity<Complex>(6.6260693e-34 / Math.PI, new Unit(
                (Unit.Kilogram, 1),
                (Unit.Meter, 2),
                (Unit.Second, -1))));

            // Boltzmann constant (J/K)
            workspace.Scope.TrySetVariable("k", new Quantity<Complex>(1.3806505e-23, new Unit(
                (Unit.Kilogram, 1),
                (Unit.Meter, 2),
                (Unit.Second, -2),
                (Unit.Kelvin, -1))));
        }

        /// <summary>
        /// Computes the absolute value of a quantity.
        /// </summary>
        /// <param name="args">The arguments.</param>
        /// <param name="workspace">The workspace.</param>
        /// <param name="result">The result.</param>
        /// <returns>Returns <c>true</c> if the function was evaluated; otherwise, <c>false</c>.</returns>
        [FunctionDescription("Gets the magnitude of a complex number. The units are the same as the argument.")]
        public static bool Abs(IReadOnlyList<Quantity<Complex>> args, IWorkspace workspace, out Quantity<Complex> result)
        {
            if (args.Count != 1)
            {
                result = _invalid;
                workspace.DiagnosticMessage = "Expected single argument for abs().";
                return false;
            }
            result = new Quantity<Complex>(Complex.Abs(args[0].Scalar), args[0].Unit);
            return true;
        }

        /// <summary>
        /// Computes the argument.
        /// </summary>
        /// <param name="args">The arguments.</param>
        /// <param name="workspace">The workspace.</param>
        /// <param name="result">The result.</param>
        /// <returns>Returns <c>true</c> if the function was evaluated; otherwise, <c>false</c>.</returns>
        [FunctionDescription("Gets the argument (polar angle) of a complex number. The units are in radians.")]
        public static bool Arg(IReadOnlyList<Quantity<Complex>> args, IWorkspace workspace, out Quantity<Complex> result)
        {
            if (args.Count != 1)
            {
                result = _invalid;
                workspace.DiagnosticMessage = "Expected single argument for arg().";
                return false;
            }
            result = new Quantity<Complex>(Math.Atan2(args[0].Scalar.Imaginary, args[0].Scalar.Real), Unit.UnitRadian);
            return true;
        }

        /// <summary>
        /// Computes the sine of a quantity.
        /// </summary>
        /// <param name="args">The arguments.</param>
        /// <param name="workspace">The workspace.</param>
        /// <param name="result">The result.</param>
        /// <returns>Returns <c>true</c> if the function was evaluated; otherwise, <c>false</c>.</returns>
        [FunctionDescription("Gets the sine of a complex number.")]
        public static bool Sin(IReadOnlyList<Quantity<Complex>> args, IWorkspace workspace, out Quantity<Complex> result)
        {
            if (args.Count != 1)
            {
                // Only one argument allowed
                result = _invalid;
                workspace.DiagnosticMessage = "Expected single argument for sin().";
                return false;
            }
            var arg = args[0];
            if (arg.Unit != Unit.UnitNone && arg.Unit != Unit.UnitRadian)
            {
                // Cannot 
                result = _invalid;
                workspace.DiagnosticMessage = "Expected an argument without unit or in radian.";
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
        [FunctionDescription("Gets the cosine of a complex number.")]
        public static bool Cos(IReadOnlyList<Quantity<Complex>> args, IWorkspace workspace, out Quantity<Complex> result)
        {
            if (args.Count != 1)
            {
                // Only one argument allowed
                result = _invalid;
                workspace.DiagnosticMessage = "Expected single argument for cos().";
                return false;
            }
            var arg = args[0];
            if (arg.Unit != Unit.UnitNone && arg.Unit != Unit.UnitRadian)
            {
                // Cannot 
                result = _invalid;
                workspace.DiagnosticMessage = "Expected an argument without unit or in radian.";
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
        [FunctionDescription("Gets the tangent of a complex number.")]
        public static bool Tan(IReadOnlyList<Quantity<Complex>> args, IWorkspace workspace, out Quantity<Complex> result)
        {
            if (args.Count != 1)
            {
                // Only one argument allowed
                result = _invalid;
                workspace.DiagnosticMessage = "Expected single argument for tan().";
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
        [FunctionDescription("Gets the hyperbolic sine of a complex number.")]
        public static bool Sinh(IReadOnlyList<Quantity<Complex>> args, IWorkspace workspace, out Quantity<Complex> result)
        {
            if (args.Count != 1)
            {
                // Only one argument allowed
                result = _invalid;
                workspace.DiagnosticMessage = "Expected single argument for sinh().";
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
        [FunctionDescription("Gets the hyperbolic cosine of a complex number.")]
        public static bool Cosh(IReadOnlyList<Quantity<Complex>> args, IWorkspace workspace, out Quantity<Complex> result)
        {
            if (args.Count != 1)
            {
                // Only one argument allowed
                result = _invalid;
                workspace.DiagnosticMessage = "Expected single argument for cosh().";
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
        [FunctionDescription("Gets the hyperbolic tangent of a complex number.")]
        public static bool Tanh(IReadOnlyList<Quantity<Complex>> args, IWorkspace workspace, out Quantity<Complex> result)
        {
            if (args.Count != 1)
            {
                // Only one argument allowed
                result = _invalid;
                workspace.DiagnosticMessage = "Expected single argument for tanh().";
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
        [FunctionDescription("Gets the exponent of a complex number.")]
        public static bool Exp(IReadOnlyList<Quantity<Complex>> args, IWorkspace workspace, out Quantity<Complex> result)
        {
            if (args.Count != 1)
            {
                // Only one argument allowed
                result = _invalid;
                workspace.DiagnosticMessage = "Expected single argument for exp().";
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
        [FunctionDescription("Gets the natural logarithm of a complex number.")]
        public static bool Ln(IReadOnlyList<Quantity<Complex>> args, IWorkspace workspace, out Quantity<Complex> result)
        {
            if (args.Count != 1)
            {
                // Only one argument allowed
                result = _invalid;
                workspace.DiagnosticMessage = "Expected single argument for ln().";
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
        [FunctionDescription("Gets the base-10 logarithm of a complex number.")]
        public static bool Log10(IReadOnlyList<Quantity<Complex>> args, IWorkspace workspace, out Quantity<Complex> result)
        {
            if (args.Count != 1)
            {
                // Only one argument allowed
                result = _invalid;
                workspace.DiagnosticMessage = "Expected single argument for log10().";
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
        [FunctionDescription("Gets the base-2 logarithm of a complex number.")]
        public static bool Log2(IReadOnlyList<Quantity<Complex>> args, IWorkspace workspace, out Quantity<Complex> result)
        {
            if (args.Count != 1)
            {
                // Only one argument allowed
                result = _invalid;
                workspace.DiagnosticMessage = "Expected single argument for log2().";
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
        [FunctionDescription("Gets the sqrt of a complex number.")]
        public static bool Sqrt(IReadOnlyList<Quantity<Complex>> args, IWorkspace workspace, out Quantity<Complex> result)
        {
            if (args.Count != 1)
            {
                // Only one argument allowed
                result = _invalid;
                workspace.DiagnosticMessage = "Expected single argument for sqrt().";
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
