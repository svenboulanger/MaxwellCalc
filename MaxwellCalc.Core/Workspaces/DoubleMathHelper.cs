using MaxwellCalc.Core.Attributes;
using MaxwellCalc.Units;
using System;
using System.Collections.Generic;
using MaxwellCalc.Core.Workspaces.SpecialFunctions;

namespace MaxwellCalc.Workspaces
{
    /// <summary>
    /// Defines common functions for math involving real numbers.
    /// </summary>
    public static class DoubleMathHelper
    {
        private static readonly Quantity<double> _invalid = new(double.NaN, Unit.UnitNone);

        /// <summary>
        /// The Euler-Mascheroni constant.
        /// </summary>
        public const double EulerGamma = 0.57721566490153286060651209008240243104215933593;

        /// <summary>
        /// Registers common constants.
        /// </summary>
        /// <param name="workspace">The workspace.Variables.</param>
        public static void RegisterCommonConstants(IWorkspace<double> workspace)
        {
            // Pi, as expected
            workspace.Scope.TrySetVariable("pi", new Quantity<double>(Math.PI, Unit.UnitNone));

            // Euler number
            workspace.Scope.TrySetVariable("e", new Quantity<double>(Math.E, Unit.UnitNone));

            // Speed of light
            workspace.Scope.TrySetVariable("c", new Quantity<double>(299792458.0, new Unit((Unit.Meter, 1), (Unit.Second, -1))));

            // Euler constant
            workspace.Scope.TrySetVariable("euler", new Quantity<double>(Constants.EulerGamma, Unit.UnitNone));
        }

        /// <summary>
        /// Registers constants that are common for electrical applications.
        /// </summary>
        /// <param name="workspace">The workspace.Variables.</param>
        public static void RegisterCommonElectronicsConstants(IWorkspace<double> workspace)
        {
            // Elementary charge (Coulomb)
            workspace.Scope.TrySetVariable("q", new Quantity<double>(1.60217663e-19, new Unit((Unit.Ampere, 1), (Unit.Second, 1))));

            // Permittivity of vacuum (Farad/meter)
            workspace.Scope.TrySetVariable("eps0", new Quantity<double>(8.8541878128e-12, new Unit(
                    (Unit.Kilogram, -1),
                    (Unit.Meter, -3),
                    (Unit.Second, 4),
                    (Unit.Ampere, 2))));

            // Permeability of vacuum (Newton Ampere^-2)
            workspace.Scope.TrySetVariable("mu0", new Quantity<double>(1.25663706212e-6, new Unit(
                (Unit.Kilogram, 1),
                (Unit.Meter, 1),
                (Unit.Second, -2),
                (Unit.Ampere, -2))));

            // Electron-volt (eV)
            workspace.Scope.TrySetVariable("eV", new Quantity<double>(1.60217663e-19, new Unit(
                (Unit.Kilogram, 1),
                (Unit.Meter, 2),
                (Unit.Second, -2))));

            // Planck constant (J s)
            workspace.Scope.TrySetVariable("h", new Quantity<double>(6.6260693e-34, new Unit(
                (Unit.Kilogram, 1),
                (Unit.Meter, 2),
                (Unit.Second, -1))));

            // Reduced Planck constant bar (J s)
            workspace.Scope.TrySetVariable("hbar", new Quantity<double>(6.6260693e-34 / Math.PI, new Unit(
                (Unit.Kilogram, 1),
                (Unit.Meter, 2),
                (Unit.Second, -1))));

            // Boltzmann constant (J/K)
            workspace.Scope.TrySetVariable("k", new Quantity<double>(1.3806505e-23, new Unit(
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
        [FunctionDescription("Takes the absolute value of a number. The result has the same units as the argument.")]
        public static bool Abs(IReadOnlyList<Quantity<double>> args, IWorkspace workspace, out Quantity<double> result)
        {
            if (args.Count != 1)
            {
                result = _invalid;
                workspace.DiagnosticMessage = $"Expected a single argument for {nameof(Abs)}().";
                return false;
            }
            result = new Quantity<double>(Math.Abs(args[0].Scalar), args[0].Unit);
            return true;
        }

        /// <summary>
        /// Computes the sine of a quantity.
        /// </summary>
        /// <param name="args">The arguments.</param>
        /// <param name="workspace">The workspace.</param>
        /// <param name="result">The result.</param>
        /// <returns>Returns <c>true</c> if the function was evaluated; otherwise, <c>false</c>.</returns>
        [FunctionDescription("Calculates the sine of a real number.")]
        public static bool Sin(IReadOnlyList<Quantity<double>> args, IWorkspace workspace, out Quantity<double> result)
        {
            if (!args.SingleGivenUnitArgument(Unit.UnitRadian, workspace, nameof(Sin), out result))
                return false;
            result = new Quantity<double>(Math.Sin(args[0].Scalar), Unit.UnitNone);
            return true;
        }

        /// <summary>
        /// Computes the cosine of a quantity.
        /// </summary>
        /// <param name="args">The arguments.</param>
        /// <param name="workspace">The workspace.</param>
        /// <param name="result">The result.</param>
        /// <returns>Returns <c>true</c> if the function was evaluated; otherwise, <c>false</c>.</returns>
        [FunctionDescription("Calculates the cosine of a real number.")]
        public static bool Cos(IReadOnlyList<Quantity<double>> args, IWorkspace workspace, out Quantity<double> result)
        {
            if (!args.SingleGivenUnitArgument(Unit.UnitRadian, workspace, nameof(Cos), out result))
                return false;
            result = new Quantity<double>(Math.Cos(args[0].Scalar), Unit.UnitNone);
            return true;
        }

        /// <summary>
        /// Computes the tangent of a quantity.
        /// </summary>
        /// <param name="args">The arguments.</param>
        /// <param name="workspace">The workspace.</param>
        /// <param name="result">The result.</param>
        /// <returns>Returns <c>true</c> if the function was evaluated; otherwise, <c>false</c>.</returns>
        [FunctionDescription("Calculates the tangent of a real number.")]
        public static bool Tan(IReadOnlyList<Quantity<double>> args, IWorkspace workspace, out Quantity<double> result)
        {
            if (!args.SingleGivenUnitArgument(Unit.UnitRadian, workspace, nameof(Tan), out result))
                return false;
            result = new Quantity<double>(Math.Tan(args[0].Scalar), Unit.UnitNone);
            return true;
        }

        /// <summary>
        /// Computes the hyperbolic sine of a quantity.
        /// </summary>
        /// <param name="args">The arguments.</param>
        /// <param name="workspace">The workspace.</param>
        /// <param name="result">The result.</param>
        /// <returns>Returns <c>true</c> if the function was evaluated; otherwise, <c>false</c>.</returns>
        [FunctionDescription("Calculates the hyperbolic sine of a real number.")]
        public static bool Sinh(IReadOnlyList<Quantity<double>> args, IWorkspace workspace, out Quantity<double> result)
        {
            if (!args.SingleNonUnitArgument(workspace, nameof(Sinh), out result))
                return false;
            result = new Quantity<double>(Math.Sinh(args[0].Scalar), Unit.UnitNone);
            return true;
        }

        /// <summary>
        /// Computes the hyperbolic cosine of a quantity.
        /// </summary>
        /// <param name="args">The arguments.</param>
        /// <param name="workspace">The workspace.</param>
        /// <param name="result">The result.</param>
        /// <returns>Returns <c>true</c> if the function was evaluated; otherwise, <c>false</c>.</returns>
        [FunctionDescription("Calculates the hyperbolic cosine of a real number.")]
        public static bool Cosh(IReadOnlyList<Quantity<double>> args, IWorkspace workspace, out Quantity<double> result)
        {
            if (!args.SingleNonUnitArgument(workspace, nameof(Cosh), out result))
                return false;
            result = new Quantity<double>(Math.Cosh(args[0].Scalar), Unit.UnitNone);
            return true;
        }

        /// <summary>
        /// Computes the hyperbolic tangent of a quantity.
        /// </summary>
        /// <param name="args">The arguments.</param>
        /// <param name="workspace">The workspace.</param>
        /// <param name="result">The result.</param>
        /// <returns>Returns <c>true</c> if the function was evaluated; otherwise, <c>false</c>.</returns>
        [FunctionDescription("Calculates the hyperbolic tangent of a real number.")]
        public static bool Tanh(IReadOnlyList<Quantity<double>> args, IWorkspace workspace, out Quantity<double> result)
        {
            if (!args.SingleNonUnitArgument(workspace, nameof(Tanh), out result))
                return false;
            result = new Quantity<double>(Math.Tanh(args[0].Scalar), Unit.UnitNone);
            return true;
        }

        /// <summary>
        /// Computes the exponential of a quantity.
        /// </summary>
        /// <param name="args">The arguments.</param>
        /// <param name="workspace">The workspace.</param>
        /// <param name="result">The result.</param>
        /// <returns>Returns <c>true</c> if the function was evaluated; otherwise, <c>false</c>.</returns>
        [FunctionDescription("Calculates the exponential of a real number.")]
        public static bool Exp(IReadOnlyList<Quantity<double>> args, IWorkspace workspace, out Quantity<double> result)
        {
            if (!args.SingleNonUnitArgument(workspace, nameof(Exp), out result))
                return false;
            result = new Quantity<double>(Math.Exp(args[0].Scalar), Unit.UnitNone);
            return true;
        }

        /// <summary>
        /// Computes the natural logarithm of a quantity.
        /// </summary>
        /// <param name="args">The arguments.</param>
        /// <param name="workspace">The workspace.</param>
        /// <param name="result">The result.</param>
        /// <returns>Returns <c>true</c> if the function was evaluated; otherwise, <c>false</c>.</returns>
        [FunctionDescription("Calculates the natural logarithm of a real number.")]
        public static bool Ln(IReadOnlyList<Quantity<double>> args, IWorkspace workspace, out Quantity<double> result)
        {
            if (!args.SingleNonUnitArgument(workspace, nameof(Ln), out result))
                return false;
            result = new Quantity<double>(Math.Log(args[0].Scalar), Unit.UnitNone);
            return true;
        }

        /// <summary>
        /// Computes the base-10 logarithm of a quantity.
        /// </summary>
        /// <param name="args">The arguments.</param>
        /// <param name="workspace">The workspace.</param>
        /// <param name="result">The result.</param>
        /// <returns>Returns <c>true</c> if the function was evaluated; otherwise, <c>false</c>.</returns>
        [FunctionDescription("Calculate the base-10 logarithm of a real number.")]
        public static bool Log10(IReadOnlyList<Quantity<double>> args, IWorkspace workspace, out Quantity<double> result)
        {
            if (!args.SingleNonUnitArgument(workspace, nameof(Log10), out result))
                return false;
            result = new Quantity<double>(Math.Log10(args[0].Scalar), Unit.UnitNone);
            return true;
        }

        /// <summary>
        /// Computes the base-2 logarithm of a quantity.
        /// </summary>
        /// <param name="args">The arguments.</param>
        /// <param name="workspace">The workspace.</param>
        /// <param name="result">The result.</param>
        /// <returns>Returns <c>true</c> if the function was evaluated; otherwise, <c>false</c>.</returns>
        [FunctionDescription("Calculates the base-2 logarithm of a real number.")]
        public static bool Log2(IReadOnlyList<Quantity<double>> args, IWorkspace workspace, out Quantity<double> result)
        {
            if (!args.SingleNonUnitArgument(workspace, nameof(Log2), out result))
                return false;
            result = new Quantity<double>(Math.Log(args[0].Scalar) / Math.Log(2), Unit.UnitNone);
            return true;
        }

        /// <summary>
        /// Computes the square root of a quantity.
        /// </summary>
        /// <param name="args">The arguments.</param>
        /// <param name="workspace">The workspace.</param>
        /// <param name="result">The result.</param>
        /// <returns>Returns <c>true</c> if the function was evaluated; otherwise, <c>false</c>.</returns>
        [FunctionDescription("Calculates the square root of a real number.")]
        public static bool Sqrt(IReadOnlyList<Quantity<double>> args, IWorkspace workspace, out Quantity<double> result)
        {
            if (args.Count != 1)
            {
                // Only one argument allowed
                result = _invalid;
                workspace.DiagnosticMessage = $"Expected a single argument for {nameof(Sqrt)}().";
                return false;
            }
            if (args[0].Scalar < 0.0)
            {
                result = _invalid;
                workspace.DiagnosticMessage = $"Cannot calculate the square root of a negative number. If this was intended, please us a complex domain.";
                return false;
            }
            var arg = args[0];
            result = new Quantity<double>(
                Math.Sqrt(args[0].Scalar),
                Unit.Pow(arg.Unit, new Fraction(1, 2)));
            return true;
        }

        /// <summary>
        /// Computes the arcsine of a quantity.
        /// </summary>
        /// <param name="args">The arguments.</param>
        /// <param name="workspace">The workspace.</param>
        /// <param name="result">The result.</param>
        /// <returns>Returns <c>true</c> if the function was evaluated; otherwise, <c>false</c>.</returns>
        [FunctionDescription("Calculates the arcsine of a real number. The result is in radians.")]
        public static bool Asin(IReadOnlyList<Quantity<double>> args, IWorkspace workspace, out Quantity<double> result)
        {
            if (!args.SingleNonUnitArgument(workspace, nameof(Asin), out result))
                return false;
            if (args[0].Scalar < -1.0 || args[0].Scalar > 1.0)
            {
                result = _invalid;
                workspace.DiagnosticMessage = $"The argument for {nameof(Asin)}() is out of range [-1, 1].";
                return false;
            }
            result = new Quantity<double>(Math.Asin(args[0].Scalar), Unit.UnitRadian);
            return true;
        }
        /// <summary>
        /// Computes the arccosine of a quantity.
        /// </summary>
        /// <param name="args">The arguments.</param>
        /// <param name="workspace">The workspace.</param>
        /// <param name="result">The result.</param>
        /// <returns>Returns <c>true</c> if the function was evaluated; otherwise, <c>false</c>.</returns>
        [FunctionDescription("Calculates the arccosine of a real number. The result is in radians.")]
        public static bool Acos(IReadOnlyList<Quantity<double>> args, IWorkspace workspace, out Quantity<double> result)
        {
            if (!args.SingleNonUnitArgument(workspace, nameof(Acos), out result))
                return false;
            if (args[0].Scalar < -1.0 || args[0].Scalar > 1.0)
            {
                result = _invalid;
                workspace.DiagnosticMessage = "The argument for acos() is out of range [-1, 1].";
                return false;
            }
            result = new Quantity<double>(Math.Acos(args[0].Scalar), Unit.UnitRadian);
            return true;
        }

        /// <summary>
        /// Computes the arctangent of a quantity.
        /// </summary>
        /// <param name="args">The arguments.</param>
        /// <param name="workspace">The workspace.</param>
        /// <param name="result">The result.</param>
        /// <returns>Returns <c>true</c> if the function was evaluated; otherwise, <c>false</c>.</returns>
        [FunctionDescription("Calculates the arctangent of a real number. The result is in radians.")]
        public static bool Atan(IReadOnlyList<Quantity<double>> args, IWorkspace workspace, out Quantity<double> result)
        {
            if (!args.SingleNonUnitArgument(workspace, nameof(Atan), out result))
                return false;
            result = new Quantity<double>(Math.Atan(args[0].Scalar), Unit.UnitRadian);
            return true;
        }

        /// <summary>
        /// Computes the hyperbolic arcsine of a quantity.
        /// </summary>
        /// <param name="args">The arguments.</param>
        /// <param name="workspace">The workspace.</param>
        /// <param name="result">The result.</param>
        /// <returns>Returns <c>true</c> if the function was evaluated; otherwise, <c>false</c>.</returns>
        [FunctionDescription("Calculates the hyperbolic arcsine of a real number. The result is in radians.")]
        public static bool Asinh(IReadOnlyList<Quantity<double>> args, IWorkspace workspace, out Quantity<double> result)
        {
            if (!args.SingleNonUnitArgument(workspace, nameof(Asinh), out result))
                return false;
            result = new Quantity<double>(Math.Asinh(args[0].Scalar), Unit.UnitRadian);
            return true;
        }

        /// <summary>
        /// Computes the hyperbolic arccosine of a quantity.
        /// </summary>
        /// <param name="args">The arguments.</param>
        /// <param name="workspace">The workspace.</param>
        /// <param name="result">The result.</param>
        /// <returns>Returns <c>true</c> if the function was evaluated; otherwise, <c>false</c>.</returns>
        [FunctionDescription("Calculates the hyperbolic arccosine of a real number. The result is in radians.")]
        public static bool Acosh(IReadOnlyList<Quantity<double>> args, IWorkspace workspace, out Quantity<double> result)
        {
            if (!args.SingleNonUnitArgument(workspace, nameof(Acosh), out result))
                return false;
            result = new Quantity<double>(Math.Acosh(args[0].Scalar), Unit.UnitRadian);
            return true;
        }

        /// <summary>
        /// Computes the hyperbolic arctangent of a quantity.
        /// </summary>
        /// <param name="args">The arguments.</param>
        /// <param name="workspace">The workspace.</param>
        /// <param name="result">The result.</param>
        /// <returns>Returns <c>true</c> if the function was evaluated; otherwise, <c>false</c>.</returns>
        [FunctionDescription("Calculates the hyperbolic arctangent of a real number. The result is in radians.")]
        public static bool Atanh(IReadOnlyList<Quantity<double>> args, IWorkspace workspace, out Quantity<double> result)
        {
            if (!args.SingleNonUnitArgument(workspace, nameof(Atanh), out result))
                return false;
            result = new Quantity<double>(Math.Atanh(args[0].Scalar), Unit.UnitRadian);
            return true;
        }

        /// <summary>
        /// Computes the arcsine of a quantity.
        /// </summary>
        /// <param name="args">The arguments.</param>
        /// <param name="workspace">The workspace.</param>
        /// <param name="result">The result.</param>
        /// <returns>Returns <c>true</c> if the function was evaluated; otherwise, <c>false</c>.</returns>
        [FunctionDescription("Calculates the full arctangent of a real number where the first argument is the Y-coordinate and the second is the X-coordinate. Both arguments need to have the same units. The result is in radians.")]
        [MinArg(2), MaxArg(2)]
        public static bool Atan2(IReadOnlyList<Quantity<double>> args, IWorkspace workspace, out Quantity<double> result)
        {
            if (args.Count != 2)
            {
                // Only one argument allowed
                result = _invalid;
                workspace.DiagnosticMessage = $"Expected a two arguments for {nameof(Atan2)}().";
                return false;
            }
            if (args[0].Unit != args[1].Unit)
            {
                result = _invalid;
                workspace.DiagnosticMessage = $"Expected the two arguments to have the same units for {nameof(Atan2)}().";
                return false;
            }
            result = new Quantity<double>(Math.Atan2(args[0].Scalar, args[1].Scalar), Unit.UnitRadian);
            return true;
        }

        /// <summary>
        /// Computes the maximum of a number of arguments.
        /// </summary>
        /// <param name="args">The arguments.</param>
        /// <param name="workspace">The workspace.</param>
        /// <param name="result">The result.</param>
        /// <returns>Returns <c>true</c> if the function was evaluated; otherwise, <c>false</c>.</returns>
        [FunctionDescription("Calculates the maximum of a number of arguments. All arguments need to have the same units."), MinArg(1), MaxArg(int.MaxValue)]
        public static bool Max(IReadOnlyList<Quantity<double>> args, IWorkspace workspace, out Quantity<double> result)
        {
            if (args.Count < 1)
            {
                result = _invalid;
                workspace.DiagnosticMessage = $"Expected at least 1 argument for {nameof(Max)}().";
                return false;
            }

            double value = args[0].Scalar;
            var unit = args[0].Unit;
            for (int i = 1; i < args.Count; i++)
            {
                if (!unit.Equals(args[i].Unit))
                {
                    result = _invalid;
                    workspace.DiagnosticMessage = $"The units of the arguments do not match for {nameof(Max)}().";
                    return false;
                }
                value = Math.Max(value, args[i].Scalar);
            }

            // Finish up
            result = new Quantity<double>(value, unit);
            return true;
        }

        /// <summary>
        /// Computes the minimum of a number of arguments.
        /// </summary>
        /// <param name="args">The arguments.</param>
        /// <param name="workspace">The workspace.</param>
        /// <param name="result">The result.</param>
        /// <returns>Returns <c>true</c> if the function was evaluated; otherwise, <c>false</c>.</returns>
        [FunctionDescription("Calculates the minimum of a number of arguments. All arguments need to have the same units."), MinArg(1), MaxArg(int.MaxValue)]
        public static bool Min(IReadOnlyList<Quantity<double>> args, IWorkspace workspace, out Quantity<double> result)
        {
            if (args.Count < 1)
            {
                result = _invalid;
                workspace.DiagnosticMessage = $"Expected at least 1 argument for {nameof(Min)}().";
                return false;
            }

            double value = args[0].Scalar;
            var unit = args[0].Unit;
            for (int i = 1; i < args.Count; i++)
            {
                if (!unit.Equals(args[i].Unit))
                {
                    result = _invalid;
                    workspace.DiagnosticMessage = $"The units of the arguments do not match for {nameof(Min)}().";
                    return false;
                }
                value = Math.Min(value, args[i].Scalar);
            }

            // Finish up
            result = new Quantity<double>(value, unit);
            return true;
        }

        /// <summary>
        /// Rounds a number.
        /// </summary>
        /// <param name="args">The arguments.</param>
        /// <param name="workspace">The workspace.</param>
        /// <param name="result">The result.</param>
        /// <returns>Returns <c>true</c> if the function was evaluated; otherwise, <c>false</c>.</returns>
        [FunctionDescription("Rounds a number to some precision. If the second argument for precision is not specified, a precision of 0 digits after the comma is assumed.")]
        [MinArg(1), MaxArg(2)]
        public static bool Round(IReadOnlyList<Quantity<double>> args, IWorkspace workspace, out Quantity<double> result)
        {
            if (args.Count != 1 && args.Count != 2)
            {
                result = _invalid;
                workspace.DiagnosticMessage = $"Expected 1 or 2 arguments for {nameof(Round)}()";
                return false;
            }

            if (args.Count == 1)
                result = new Quantity<double>(Math.Round(args[0].Scalar, MidpointRounding.AwayFromZero), args[0].Unit);
            else
            {
                if (args[1].Unit != Unit.UnitNone)
                {
                    // Cannot deal with units
                    result = _invalid;
                    workspace.DiagnosticMessage = $"Expected the second argument to not have units units for {nameof(Round)}().";
                    return false;
                }

                int digits = (int)args[1].Scalar;
                result = new Quantity<double>(Math.Round(args[0].Scalar, digits, MidpointRounding.AwayFromZero), args[0].Unit);
            }
            return true;
        }

        /// <summary>
        /// Calculates the factorial of a number.
        /// </summary>
        /// <param name="args">The arguments.</param>
        /// <param name="workspace">The workspace.</param>
        /// <param name="result">The result.</param>
        /// <returns>Returns <c>true</c> if the function was evaluated; otherwise, <c>false</c>.</returns>
        [FunctionDescription("Calculates the factorial of a number. If the number is real, it is converted to an integer. The argument is expected to have no units.")]
        public static bool Factorial(IReadOnlyList<Quantity<double>> args, IWorkspace workspace, out Quantity<double> result)
        {
            if (!args.SingleNonUnitArgument(workspace, nameof(Factorial), out result))
                return false;
            int value = (int)args[0].Scalar;
            result = new Quantity<double>(FactorialFunctions.Factorial(value), Unit.UnitNone);
            return true;
        }

        /// <summary>
        /// Calculates the natural logarithm of a number.
        /// </summary>
        /// <param name="args">The arguments.</param>
        /// <param name="workspace">The workspace.</param>
        /// <param name="result">The result.</param>
        /// <returns>Returns <c>true</c> if the function was evaluated; otherwise, <c>false</c>.</returns>
        [FunctionDescription("Calculates the natural logarithm of the factorial of a number. If the number is real, it is converted to an integer. The argument is expected to have no units.")]
        public static bool FactorialLn(IReadOnlyList<Quantity<double>> args, IWorkspace workspace, out Quantity<double> result)
        {
            if (args.Count != 1)
            {
                // Only one argument allowed
                result = _invalid;
                workspace.DiagnosticMessage = $"Expected a single argument for {nameof(FactorialLn)}().";
                return false;
            }
            var arg = args[0];
            if (arg.Unit != Unit.UnitNone)
            {
                // Cannot deal with units
                result = _invalid;
                workspace.DiagnosticMessage = $"Expected an argument without units for {nameof(FactorialLn)}().";
                return false;
            }
            int value = (int)arg.Scalar;
            result = new Quantity<double>(FactorialFunctions.FactorialLn(value), Unit.UnitNone);
            return true;
        }

        /// <summary>
        /// Calculates the binomial of two numbers.
        /// </summary>
        /// <param name="args">The arguments.</param>
        /// <param name="workspace">The workspace.</param>
        /// <param name="result">The result.</param>
        /// <returns>Returns <c>true</c> if the function was evaluated; otherwise, <c>false</c>.</returns>
        [FunctionDescription("Calculates the binomial of two numbers. If the numbers are real, they are converted to an integer. The arguments are expected to have no units.")]
        [MinArg(2), MaxArg(2)]
        public static bool Binomial(IReadOnlyList<Quantity<double>> args, IWorkspace workspace, out Quantity<double> result)
        {
            if (args.Count != 2)
            {
                // Only one argument allowed
                result = _invalid;
                workspace.DiagnosticMessage = $"Expected two argument for {nameof(Binomial)}().";
                return false;
            }
            if (args[0].Unit != Unit.UnitNone || args[1].Unit != Unit.UnitNone)
            {
                // Cannot deal with units
                result = _invalid;
                workspace.DiagnosticMessage = $"Expected an arguments without units for {nameof(Binomial)}().";
                return false;
            }
            var a = (int)args[0].Scalar;
            var b = (int)args[1].Scalar;
            result = new Quantity<double>(FactorialFunctions.Binomial(a, b), Unit.UnitNone);
            return true;
        }

        /// <summary>
        /// Calculates the natural logarithm of two numbers.
        /// </summary>
        /// <param name="args">The arguments.</param>
        /// <param name="workspace">The workspace.</param>
        /// <param name="result">The result.</param>
        /// <returns>Returns <c>true</c> if the function was evaluated; otherwise, <c>false</c>.</returns>
        [FunctionDescription("Calculates the natural logarithm of the binomial of two numbers. If the numbers are real, they are converted to an integer. The arguments are expected to have no units.")]
        [MinArg(2), MaxArg(2)]
        public static bool BinomialLn(IReadOnlyList<Quantity<double>> args, IWorkspace workspace, out Quantity<double> result)
        {
            if (args.Count != 2)
            {
                // Only one argument allowed
                result = _invalid;
                workspace.DiagnosticMessage = $"Expected two arguments for {nameof(BinomialLn)}().";
                return false;
            }
            if (args[0].Unit != Unit.UnitNone || args[1].Unit != Unit.UnitNone)
            {
                // Cannot deal with units
                result = _invalid;
                workspace.DiagnosticMessage = $"Expected an arguments without units for {nameof(BinomialLn)}().";
                return false;
            }
            var a = (int)args[0].Scalar;
            var b = (int)args[1].Scalar;
            result = new Quantity<double>(FactorialFunctions.BinomialLn(a, b), Unit.UnitNone);
            return true;
        }

        /// <summary>
        /// Calculates the multinomial of multiple numbers.
        /// </summary>
        /// <param name="args">The arguments.</param>
        /// <param name="workspace">The result.</param>
        /// <param name="result">The result.</param>
        /// <returns>Returns <c>true</c> if the function was evaluated; otherwise, <c>false</c>.</returns>
        [FunctionDescription("Calculates the multinomial of a number of arguments. The first is n, the others are the k's in the denominator. The arguments are expected to have no units.")]
        public static bool Multinomial(IReadOnlyList<Quantity<double>> args, IWorkspace workspace, out Quantity<double> result)
        {
            if (args.Count < 2)
            {
                // Only one argument allowed
                result = _invalid;
                workspace.DiagnosticMessage = $"Expected at least two argument for {nameof(Multinomial)}().";
                return false;
            }
            int n = 0;
            int[] k = new int[args.Count - 1];
            for (int i = 0; i < args.Count; i++)
            {
                if (args[0].Unit != Unit.UnitNone)
                {
                    result = _invalid;
                    workspace.DiagnosticMessage = $"Expected all arguments without units for {nameof(Multinomial)}().";
                    return false;
                }
                if (i == 0)
                    n = (int)args[i].Scalar;
                else
                    k[i - 1] = (int)args[i].Scalar;
            }
            result = new Quantity<double>(FactorialFunctions.Multinomial(n, k), Unit.UnitNone);
            return true;
        }

        /// <summary>
        /// Calculates the exponential integral of a number.
        /// </summary>
        /// <param name="args">The arguments.</param>
        /// <param name="workspace">The result.</param>
        /// <param name="result">The result.</param>
        /// <returns>Returns <c>true</c> if the function was evaluated; otherwise, <c>false</c>.</returns>
        [FunctionDescription("Calculates the exponential integral of a number. The argument is expected to not have units.")]
        public static bool Expi(IReadOnlyList<Quantity<double>> args, IWorkspace workspace, out Quantity<double> result)
        {
            if (args.Count != 1)
            {
                // Only one argument allowed
                result = _invalid;
                workspace.DiagnosticMessage = $"Expected a single argument for {nameof(Expi)}().";
                return false;
            }
            var arg = args[0];
            if (arg.Unit != Unit.UnitNone)
            {
                // Cannot deal with units
                result = _invalid;
                workspace.DiagnosticMessage = $"Expected an argument without units for {nameof(Expi)}().";
                return false;
            }
            result = new Quantity<double>(ExponentialIntegralFunctions.ExpI(arg.Scalar), Unit.UnitNone);
            return true;
        }

        /// <summary>
        /// Calculates the exponential integral of a number.
        /// </summary>
        /// <param name="args">The arguments.</param>
        /// <param name="workspace">The result.</param>
        /// <param name="result">The result.</param>
        /// <returns>Returns <c>true</c> if the function was evaluated; otherwise, <c>false</c>.</returns>
        [FunctionDescription("Calculates the generalized exponential integral of a number with n=1. The argument is expected to not have units.")]
        public static bool Exp1(IReadOnlyList<Quantity<double>> args, IWorkspace workspace, out Quantity<double> result)
        {
            if (!args.SingleNonUnitArgument(workspace, nameof(Exp1), out result))
                return false;
            int value = (int)args[0].Scalar;
            result = new Quantity<double>(ExponentialIntegralFunctions.Exp1(value), Unit.UnitNone);
            return true;
        }

        /// <summary>
        /// Calculates the generalized exponential integral of a number.
        /// </summary>
        /// <param name="args">The arguments.</param>
        /// <param name="workspace">The workspace.</param>
        /// <param name="result">The result.</param>
        /// <returns>Returns <c>true</c> if the function was evaluated; otherwise, <c>false</c>.</returns>
        [FunctionDescription("Calculates the generalized exponential integral of a number, with the second argument representing n. The arguments is expected to not have units.")]
        [MinArg(2), MaxArg(2)]
        public static bool Expn(IReadOnlyList<Quantity<double>> args, IWorkspace workspace, out Quantity<double> result)
        {
            if (args.Count != 2)
            {
                // Only one argument allowed
                result = _invalid;
                workspace.DiagnosticMessage = $"Expected a single argument for {nameof(Expn)}().";
                return false;
            }
            if (args[0].Unit != Unit.UnitNone || args[1].Unit != Unit.UnitNone)
            {
                // Cannot deal with units
                result = _invalid;
                workspace.DiagnosticMessage = $"Expected all arguments to not have units for {nameof(Expn)}().";
                return false;
            }
            int n = (int)args[1].Scalar;
            result = new Quantity<double>(ExponentialIntegralFunctions.ExpN(n, args[0].Scalar), Unit.UnitNone);
            return true;
        }

        /// <summary>
        /// Calculates the Gamma function.
        /// </summary>
        /// <param name="args">The arguments.</param>
        /// <param name="workspace">The workspace.</param>
        /// <param name="result">The result.</param>
        /// <returns>Returns <c>true</c> if the function was evaluated; otherwise, <c>false</c>.</returns>
        [FunctionDescription("Calculates the Gamma function. The argument is expected to not have units.")]
        public static bool Gamma(IReadOnlyList<Quantity<double>> args, IWorkspace workspace, out Quantity<double> result)
        {
            if (!args.SingleNonUnitArgument(workspace, nameof(Gamma), out result))
                return false;
            result = new Quantity<double>(GammaFunctions.Gamma(args[0].Scalar), Unit.UnitNone);
            return true;
        }

        /// <summary>
        /// Calculates the natural logarithm of the Gamma function.
        /// </summary>
        /// <param name="args">The arguments.</param>
        /// <param name="workspace">The workspace.</param>
        /// <param name="result">The result.</param>
        /// <returns>Returns <c>true</c> if the function was evaluated; otherwise, <c>false</c>.</returns>
        [FunctionDescription("Calculates the natural logarithm of the Gamma function. The argument is expected to not have units.")]
        public static bool GammaLn(IReadOnlyList<Quantity<double>> args, IWorkspace workspace, out Quantity<double> result)
        {
            if (!args.SingleNonUnitArgument(workspace, nameof(GammaLn), out result))
                return false;
            result = new Quantity<double>(GammaFunctions.GammaLn(args[0].Scalar), Unit.UnitNone);
            return true;
        }

        private static bool SingleGivenUnitArgument(this IReadOnlyList<Quantity<double>> args, Unit unit, IWorkspace workspace, string name, out Quantity<double> result)
        {
            if (args.Count != 1)
            {
                // Only one argument allowed
                result = _invalid;
                workspace.DiagnosticMessage = $"Expected a single argument for {name}().";
                return false;
            }
            var arg = args[0];
            if (arg.Unit != Unit.UnitNone && arg.Unit != unit)
            {
                // Cannot 
                result = _invalid;
                workspace.DiagnosticMessage = $"Expected an argument without units, or in radians for {name}().";
                return false;
            }
            result = default;
            return true;
        }

        private static bool SingleNonUnitArgument(this IReadOnlyList<Quantity<double>> args, IWorkspace workspace, string name, out Quantity<double> result)
        {
            if (args.Count != 1)
            {
                // Only one argument allowed
                result = _invalid;
                workspace.DiagnosticMessage = $"Expected a single argument for {name}().";
                return false;
            }
            var arg = args[0];
            if (arg.Unit != Unit.UnitNone)
            {
                // Cannot deal with units
                result = _invalid;
                workspace.DiagnosticMessage = $"Expected an argument without units for {name}().";
                return false;
            }
            result = default;
            return true;
        }
    }
}
