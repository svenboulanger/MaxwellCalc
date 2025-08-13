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
        [CalculatorName("gamma"), CalculatorDescription("The Euler–Mascheroni constant.")]
        public const double EulerGamma = Constants.EulerGamma;

        /// <summary>
        /// PI.
        /// </summary>
        [CalculatorDescription("Pi.")]
        public const double Pi = Math.PI;

        /// <summary>
        /// Euler's constant.
        /// </summary>
        [CalculatorDescription("Euler's constant.")]
        public const double E = Math.E;

        /// <summary>
        /// Light speed.
        /// </summary>
        [CalculatorName("c"), CalculatorDescription("Light speed.")]
        public static Quantity<double> LightSpeed = new(299792458.0, new Unit((Unit.Meter, 1), (Unit.Second, -1)));

        /// <summary>
        /// The elementary charge.
        /// </summary>
        [CalculatorName("q"), CalculatorDescription("The elementary charge.")]
        public static Quantity<double> ElementaryCharge = new(1.60217663e-19, new Unit((Unit.Ampere, 1), (Unit.Second, 1)));

        /// <summary>
        /// Permittivity of vacuum (Farad/meter).
        /// </summary>
        [CalculatorDescription("The permittivity of vacuum.")]
        public static Quantity<double> Eps0 = new(8.8541878128e-12, new Unit(
                    (Unit.Kilogram, -1),
                    (Unit.Meter, -3),
                    (Unit.Second, 4),
                    (Unit.Ampere, 2)));

        /// <summary>
        /// Permeability of vacuum (Newton Ampere^-2)
        /// </summary>
        [CalculatorDescription("The permeability of vacuum.")]
        public static Quantity<double> Mu0 = new(1.25663706212e-6, new Unit(
                (Unit.Kilogram, 1),
                (Unit.Meter, 1),
                (Unit.Second, -2),
                (Unit.Ampere, -2)));

        /// <summary>
        /// An electron-volt.
        /// </summary>
        [CalculatorName("eV"), CalculatorDescription("An electronvolt.")]
        public static Quantity<double> ElectronVolt = new(1.60217663e-19, new Unit(
                (Unit.Kilogram, 1),
                (Unit.Meter, 2),
                (Unit.Second, -2)));

        /// <summary>
        /// Planck's constant.
        /// </summary>
        [CalculatorName("h"), CalculatorDescription("Planck's constant.")]
        public static Quantity<double> Planck = new(6.6260693e-34, new Unit(
            (Unit.Kilogram, 1),
            (Unit.Meter, 2),
            (Unit.Second, -1)));

        /// <summary>
        /// Reduced Planck constant bar (J s)
        /// </summary>
        [CalculatorName("hbar"), CalculatorDescription("The reduced Planck's constant.")]
        public static Quantity<double> PlanckBar = new(6.6260693e-34 / Math.PI, new Unit(
            (Unit.Kilogram, 1),
            (Unit.Meter, 2),
            (Unit.Second, -1)));

        /// <summary>
        /// Boltzmann constant (J/K).
        /// </summary>
        [CalculatorName("k"), CalculatorDescription("Boltzmann's constant.")]
        public static Quantity<double> Boltzmann = new(1.3806505e-23, new Unit(
            (Unit.Kilogram, 1),
            (Unit.Meter, 2),
            (Unit.Second, -2),
            (Unit.Kelvin, -1)));

        /// <summary>
        /// The newtonian constant of gravitation.
        /// </summary>
        [CalculatorName("G"), CalculatorDescription("Newtonian constant of gravitation.")]
        public static Quantity<double> NewtonianGravityConstant = new(6.6743015e-11, new Unit(
            (Unit.Meter, 3),
            (Unit.Kilogram, -1),
            (Unit.Second, -2)));

        /// <summary>
        /// The mass of an electron.
        /// </summary>
        [CalculatorName("me"), CalculatorDescription("The mass of an electron.")]
        public static Quantity<double> ElectronMass = new(9.109383713928e-31, Unit.UnitKilogram);

        /// <summary>
        /// The mass of a muon.
        /// </summary>
        [CalculatorName("mmu"), CalculatorDescription("The mass of a muon.")]
        public static Quantity<double> MuonMass = new(1.88353162742e-28, Unit.UnitKilogram);

        /// <summary>
        /// The mass of a tau particle.
        /// </summary>
        [CalculatorName("mtau"), CalculatorDescription("The mass of a tau particle.")]
        public static Quantity<double> TauMass = new(3.1675421e10-27, Unit.UnitKilogram);

        /// <summary>
        /// The mass of a proton.
        /// </summary>
        [CalculatorName("mp"), CalculatorDescription("The mass of a proton.")]
        public static Quantity<double> ProtonMass = new(1.6726219259552e10-27, Unit.UnitKilogram);

        /// <summary>
        /// The mass of a neutron.
        /// </summary>
        [CalculatorName("mn"), CalculatorDescription("The mass of a neutron.")]
        public static Quantity<double> NeutronMass = new(1.6749275005685e10-27, Unit.UnitKilogram);

        /// <summary>
        /// The g-factor of an electron.
        /// </summary>
        [CalculatorName("ge"), CalculatorDescription("The g-factor of an electron.")]
        public const double ElectronGFactor = 2.0023193043609236;

        /// <summary>
        /// The g-factor of a muon.
        /// </summary>
        [CalculatorName("gm"), CalculatorDescription("The g-factor of a muon.")]
        public const double MuonGFactor = 2.0023318412382;

        /// <summary>
        /// The g-factor of a proton.
        /// </summary>
        [CalculatorName("gp"), CalculatorDescription("The g-factor of a proton.")]
        public const double ProtonGFactor = 5.585694689316;

        /// <summary>
        /// Avogadro's constant.
        /// </summary>
        [CalculatorName("NA"), CalculatorDescription("Avogadro's constant.")]
        public static Quantity<double> AvogadroConstant = new(6.02214076e23, new Unit((Unit.Mole, -1)));

        /// <summary>
        /// Computes the absolute value of a quantity.
        /// </summary>
        /// <param name="args">The arguments.</param>
        /// <param name="workspace">The workspace.</param>
        /// <param name="result">The result.</param>
        /// <returns>Returns <c>true</c> if the function was evaluated; otherwise, <c>false</c>.</returns>
        [CalculatorDescription("Calculates the absolute value of a real number. The result has the same units as the argument.")]
        public static bool Abs(IReadOnlyList<Quantity<double>> args, IWorkspace workspace, out Quantity<double> result)
        {
            if (args.Count != 1)
            {
                result = _invalid;
                workspace.PostDiagnosticMessage(new($"Expected a single argument for {nameof(Abs)}()."));
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
        [CalculatorDescription("Calculates the sine of a real number. The argument is expected to be in radians, or without units.")]
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
        [CalculatorDescription("Calculates the cosine of a real number. The argument is expected to be in radians, or without units.")]
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
        [CalculatorDescription("Calculates the tangent of a real number. The argument is expected to be in radians, or without units.")]
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
        [CalculatorDescription("Calculates the hyperbolic sine of a real number.")]
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
        [CalculatorDescription("Calculates the hyperbolic cosine of a real number. The argument is expected to have no units.")]
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
        [CalculatorDescription("Calculates the hyperbolic tangent of a real number. The argument is expected to have no units.")]
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
        [CalculatorDescription("Calculates the exponential of a real number. The argument is expected to have no units.")]
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
        [CalculatorDescription("Calculates the natural logarithm of a real number. The argument is expected to be positive and to have no units.")]
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
        [CalculatorDescription("Calculate the base-10 logarithm of a real number. The argument is expected to be positive and to have no units.")]
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
        [CalculatorDescription("Calculates the base-2 logarithm of a real number. The argument is expected to be positive and to have no units.")]
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
        [CalculatorDescription("Calculates the square root of a real number. The argument is expected to be positive and to have no units.")]
        public static bool Sqrt(IReadOnlyList<Quantity<double>> args, IWorkspace workspace, out Quantity<double> result)
        {
            if (args.Count != 1)
            {
                // Only one argument allowed
                result = _invalid;
                workspace.PostDiagnosticMessage(new($"Expected a single argument for {nameof(Sqrt)}()."));
                return false;
            }
            if (args[0].Scalar < 0.0)
            {
                result = _invalid;
                workspace.PostDiagnosticMessage(new($"Cannot calculate the square root of a negative number. If this was intended, please us a complex domain."));
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
        [CalculatorDescription("Calculates the arcsine of a real number. The argument is expected to be between -1 and 1, and to have no units.")]
        public static bool Asin(IReadOnlyList<Quantity<double>> args, IWorkspace workspace, out Quantity<double> result)
        {
            if (!args.SingleNonUnitArgument(workspace, nameof(Asin), out result))
                return false;
            if (args[0].Scalar < -1.0 || args[0].Scalar > 1.0)
            {
                result = _invalid;
                workspace.PostDiagnosticMessage(new($"The argument for {nameof(Asin)}() is out of range [-1, 1]."));
                return false;
            }
            result = new Quantity<double>(Math.Asin(args[0].Scalar), Unit.UnitNone);
            return true;
        }

        /// <summary>
        /// Computes the arccosine of a quantity.
        /// </summary>
        /// <param name="args">The arguments.</param>
        /// <param name="workspace">The workspace.</param>
        /// <param name="result">The result.</param>
        /// <returns>Returns <c>true</c> if the function was evaluated; otherwise, <c>false</c>.</returns>
        [CalculatorDescription("Calculates the arccosine of a real number. The argument is expected to be between -1 and 1, and to have no units.")]
        public static bool Acos(IReadOnlyList<Quantity<double>> args, IWorkspace workspace, out Quantity<double> result)
        {
            if (!args.SingleNonUnitArgument(workspace, nameof(Acos), out result))
                return false;
            if (args[0].Scalar < -1.0 || args[0].Scalar > 1.0)
            {
                result = _invalid;
                workspace.PostDiagnosticMessage(new("The argument for acos() is out of range [-1, 1]."));
                return false;
            }
            result = new Quantity<double>(Math.Acos(args[0].Scalar), Unit.UnitNone);
            return true;
        }

        /// <summary>
        /// Computes the arctangent of a quantity.
        /// </summary>
        /// <param name="args">The arguments.</param>
        /// <param name="workspace">The workspace.</param>
        /// <param name="result">The result.</param>
        /// <returns>Returns <c>true</c> if the function was evaluated; otherwise, <c>false</c>.</returns>
        [CalculatorDescription("Calculates the arctangent of a real number. The argument is expected to have no units.")]
        public static bool Atan(IReadOnlyList<Quantity<double>> args, IWorkspace workspace, out Quantity<double> result)
        {
            if (!args.SingleNonUnitArgument(workspace, nameof(Atan), out result))
                return false;
            result = new Quantity<double>(Math.Atan(args[0].Scalar), Unit.UnitNone);
            return true;
        }

        /// <summary>
        /// Computes the hyperbolic arcsine of a quantity.
        /// </summary>
        /// <param name="args">The arguments.</param>
        /// <param name="workspace">The workspace.</param>
        /// <param name="result">The result.</param>
        /// <returns>Returns <c>true</c> if the function was evaluated; otherwise, <c>false</c>.</returns>
        [CalculatorDescription("Calculates the hyperbolic arcsine of a real number. The argument is expected to have no units.")]
        public static bool Asinh(IReadOnlyList<Quantity<double>> args, IWorkspace workspace, out Quantity<double> result)
        {
            if (!args.SingleNonUnitArgument(workspace, nameof(Asinh), out result))
                return false;
            result = new Quantity<double>(Math.Asinh(args[0].Scalar), Unit.UnitNone);
            return true;
        }

        /// <summary>
        /// Computes the hyperbolic arccosine of a quantity.
        /// </summary>
        /// <param name="args">The arguments.</param>
        /// <param name="workspace">The workspace.</param>
        /// <param name="result">The result.</param>
        /// <returns>Returns <c>true</c> if the function was evaluated; otherwise, <c>false</c>.</returns>
        [CalculatorDescription("Calculates the hyperbolic arccosine of a real number. The argument is expected to have no units.")]
        public static bool Acosh(IReadOnlyList<Quantity<double>> args, IWorkspace workspace, out Quantity<double> result)
        {
            if (!args.SingleNonUnitArgument(workspace, nameof(Acosh), out result))
                return false;
            result = new Quantity<double>(Math.Acosh(args[0].Scalar), Unit.UnitNone);
            return true;
        }

        /// <summary>
        /// Computes the hyperbolic arctangent of a quantity.
        /// </summary>
        /// <param name="args">The arguments.</param>
        /// <param name="workspace">The workspace.</param>
        /// <param name="result">The result.</param>
        /// <returns>Returns <c>true</c> if the function was evaluated; otherwise, <c>false</c>.</returns>
        [CalculatorDescription("Calculates the hyperbolic arctangent of a real number. The argument is expected to have no units.")]
        public static bool Atanh(IReadOnlyList<Quantity<double>> args, IWorkspace workspace, out Quantity<double> result)
        {
            if (!args.SingleNonUnitArgument(workspace, nameof(Atanh), out result))
                return false;
            result = new Quantity<double>(Math.Atanh(args[0].Scalar), Unit.UnitNone);
            return true;
        }

        /// <summary>
        /// Computes the arcsine of a quantity.
        /// </summary>
        /// <param name="args">The arguments.</param>
        /// <param name="workspace">The workspace.</param>
        /// <param name="result">The result.</param>
        /// <returns>Returns <c>true</c> if the function was evaluated; otherwise, <c>false</c>.</returns>
        [CalculatorDescription("Calculates the full arctangent of a real number where the first argument is the Y-coordinate and the second is the X-coordinate. Both arguments need to have the same units.")]
        [MinArg(2), MaxArg(2)]
        public static bool Atan2(IReadOnlyList<Quantity<double>> args, IWorkspace workspace, out Quantity<double> result)
        {
            if (args.Count != 2)
            {
                // Only one argument allowed
                result = _invalid;
                workspace.PostDiagnosticMessage(new($"Expected a two arguments for {nameof(Atan2)}()."));
                return false;
            }
            if (args[0].Unit != args[1].Unit)
            {
                result = _invalid;
                workspace.PostDiagnosticMessage(new($"Expected the two arguments to have the same units for {nameof(Atan2)}()."));
                return false;
            }
            result = new Quantity<double>(Math.Atan2(args[0].Scalar, args[1].Scalar), Unit.UnitNone);
            return true;
        }

        /// <summary>
        /// Computes the maximum of a number of arguments.
        /// </summary>
        /// <param name="args">The arguments.</param>
        /// <param name="workspace">The workspace.</param>
        /// <param name="result">The result.</param>
        /// <returns>Returns <c>true</c> if the function was evaluated; otherwise, <c>false</c>.</returns>
        [CalculatorDescription("Calculates the maximum of a number of arguments. All arguments need to have the same units."), MinArg(1), MaxArg(int.MaxValue)]
        public static bool Max(IReadOnlyList<Quantity<double>> args, IWorkspace workspace, out Quantity<double> result)
        {
            if (args.Count < 1)
            {
                result = _invalid;
                workspace.PostDiagnosticMessage(new($"Expected at least 1 argument for {nameof(Max)}()."));
                return false;
            }

            double value = args[0].Scalar;
            var unit = args[0].Unit;
            for (int i = 1; i < args.Count; i++)
            {
                if (!unit.Equals(args[i].Unit))
                {
                    result = _invalid;
                    workspace.PostDiagnosticMessage(new($"The units of the arguments do not match for {nameof(Max)}()."));
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
        [CalculatorDescription("Calculates the minimum of a number of arguments. All arguments need to have the same units."), MinArg(1), MaxArg(int.MaxValue)]
        public static bool Min(IReadOnlyList<Quantity<double>> args, IWorkspace workspace, out Quantity<double> result)
        {
            if (args.Count < 1)
            {
                result = _invalid;
                workspace.PostDiagnosticMessage(new($"Expected at least 1 argument for {nameof(Min)}()."));
                return false;
            }

            double value = args[0].Scalar;
            var unit = args[0].Unit;
            for (int i = 1; i < args.Count; i++)
            {
                if (!unit.Equals(args[i].Unit))
                {
                    result = _invalid;
                    workspace.PostDiagnosticMessage(new($"The units of the arguments do not match for {nameof(Min)}()."));
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
        [CalculatorDescription("Rounds a number to some precision. The optional second argument should not have any units, and should be an integer. If the second argument for precision is not specified, a precision of 0 digits after the comma is assumed.")]
        [MinArg(1), MaxArg(2)]
        public static bool Round(IReadOnlyList<Quantity<double>> args, IWorkspace workspace, out Quantity<double> result)
        {
            if (args.Count != 1 && args.Count != 2)
            {
                result = _invalid;
                workspace.PostDiagnosticMessage(new($"Expected 1 or 2 arguments for {nameof(Round)}()"));
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
                    workspace.PostDiagnosticMessage(new($"Expected the second argument to not have units units for {nameof(Round)}()."));
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
        [CalculatorDescription("Calculates the factorial of a number. The argument is expected to be a positive integer, and to have no units. The argument is expected to have no units.")]
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
        [CalculatorDescription("Calculates the natural logarithm of the factorial of a number. The argument is expected to be a positive integer, and to have no units. The argument is expected to have no units.")]
        public static bool FactorialLn(IReadOnlyList<Quantity<double>> args, IWorkspace workspace, out Quantity<double> result)
        {
            if (args.Count != 1)
            {
                // Only one argument allowed
                result = _invalid;
                workspace.PostDiagnosticMessage(new($"Expected a single argument for {nameof(FactorialLn)}()."));
                return false;
            }
            var arg = args[0];
            if (arg.Unit != Unit.UnitNone)
            {
                // Cannot deal with units
                result = _invalid;
                workspace.PostDiagnosticMessage(new($"Expected an argument without units for {nameof(FactorialLn)}()."));
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
        [CalculatorName("B"), CalculatorDescription("Calculates the binomial of two numbers. The arguments are expected to be positive integers, and to not have any units.")]
        [MinArg(2), MaxArg(2)]
        public static bool Binomial(IReadOnlyList<Quantity<double>> args, IWorkspace workspace, out Quantity<double> result)
        {
            if (args.Count != 2)
            {
                // Only one argument allowed
                result = _invalid;
                workspace.PostDiagnosticMessage(new($"Expected two argument for {nameof(Binomial)}()."));
                return false;
            }
            if (args[0].Unit != Unit.UnitNone || args[1].Unit != Unit.UnitNone)
            {
                // Cannot deal with units
                result = _invalid;
                workspace.PostDiagnosticMessage(new($"Expected an arguments without units for {nameof(Binomial)}()."));
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
        [CalculatorName("BLn"), CalculatorDescription("Calculates the natural logarithm of the binomial of two numbers. The arguments are expected to be positive integers, and to not have any units.")]
        [MinArg(2), MaxArg(2)]
        public static bool BinomialLn(IReadOnlyList<Quantity<double>> args, IWorkspace workspace, out Quantity<double> result)
        {
            if (args.Count != 2)
            {
                // Only one argument allowed
                result = _invalid;
                workspace.PostDiagnosticMessage(new($"Expected two arguments for {nameof(BinomialLn)}()."));
                return false;
            }
            if (args[0].Unit != Unit.UnitNone || args[1].Unit != Unit.UnitNone)
            {
                // Cannot deal with units
                result = _invalid;
                workspace.PostDiagnosticMessage(new($"Expected an arguments without units for {nameof(BinomialLn)}()."));
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
        [CalculatorDescription("Calculates the multinomial of a number of arguments. The first is n, the others are the k's in the denominator. The arguments are expected to be positive integers, and to have no units.")]
        public static bool Multinomial(IReadOnlyList<Quantity<double>> args, IWorkspace workspace, out Quantity<double> result)
        {
            if (args.Count < 2)
            {
                // Only one argument allowed
                result = _invalid;
                workspace.PostDiagnosticMessage(new($"Expected at least two argument for {nameof(Multinomial)}()."));
                return false;
            }
            int n = 0;
            int[] k = new int[args.Count - 1];
            for (int i = 0; i < args.Count; i++)
            {
                if (args[0].Unit != Unit.UnitNone)
                {
                    result = _invalid;
                    workspace.PostDiagnosticMessage(new($"Expected all arguments without units for {nameof(Multinomial)}()."));
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
        /// <param name="workspace">The workspace.</param>
        /// <param name="result">The result.</param>
        /// <returns>Returns <c>true</c> if the function was evaluated; otherwise, <c>false</c>.</returns>
        [CalculatorName("Ei"), CalculatorDescription("Calculates the exponential integral Ei(x). The argument is expected to not have units.")]
        public static bool Expi(IReadOnlyList<Quantity<double>> args, IWorkspace workspace, out Quantity<double> result)
        {
            if (!SingleNonUnitArgument(args, workspace, "Ei", out result))
                return false;
            result = new Quantity<double>(ExponentialIntegralFunctions.ExpI(args[0].Scalar), Unit.UnitNone);
            return true;
        }

        /// <summary>
        /// Calculates the exp(-x) * Ei(x).
        /// </summary>
        /// <param name="args">The arguments.</param>
        /// <param name="workspace">The workspace.</param>
        /// <param name="result">The result.</param>
        /// <returns>Returns <c>true</c> if the function was evaluated; otherwise, <c>false</c>.</returns>
        [CalculatorName("expEi"), CalculatorDescription("Calculates exp(-x) * Ei(x). This function converges for very large arguments. The argument is expected to not have units.")]
        public static bool ExpiExp(IReadOnlyList<Quantity<double>> args, IWorkspace workspace, out Quantity<double> result)
        {
            if (!SingleNonUnitArgument(args, workspace, "expEi", out result))
                return false;
            result = new Quantity<double>(ExponentialIntegralFunctions.ExpExpI(args[0].Scalar), Unit.UnitNone);
            return true;
        }

        /// <summary>
        /// Calculates the exponential integral of a number.
        /// </summary>
        /// <param name="args">The arguments.</param>
        /// <param name="workspace">The result.</param>
        /// <param name="result">The result.</param>
        /// <returns>Returns <c>true</c> if the function was evaluated; otherwise, <c>false</c>.</returns>
        [CalculatorName("E1"), CalculatorDescription("Calculates the generalized exponential integral of a number with n=1. The argument is expected to not have units.")]
        public static bool Exp1(IReadOnlyList<Quantity<double>> args, IWorkspace workspace, out Quantity<double> result)
        {
            if (!args.SingleNonUnitArgument(workspace, "E1", out result))
                return false;
            result = new Quantity<double>(ExponentialIntegralFunctions.Exp1(args[0].Scalar), Unit.UnitNone);
            return true;
        }

        /// <summary>
        /// Calculates exp(x) * E1(x).
        /// </summary>
        /// <param name="args">The arguments.</param>
        /// <param name="workspace">The result.</param>
        /// <param name="result">The result.</param>
        /// <returns>Returns <c>true</c> if the function was evaluated; otherwise, <c>false</c>.</returns>
        [CalculatorName("expE1"), CalculatorDescription("Calculates exp(x) * E1(x). This function converges for very large arguments. The argument is expected to be positive, and to not have units.")]
        public static bool Exp1Exp(IReadOnlyList<Quantity<double>> args, IWorkspace workspace, out Quantity<double> result)
        {
            if (!args.SingleNonUnitArgument(workspace, "expE1", out result))
                return false;
            result = new Quantity<double>(ExponentialIntegralFunctions.ExpExp1(args[0].Scalar), Unit.UnitNone);
            return true;
        }

        /// <summary>
        /// Calculates the generalized exponential integral of a number.
        /// </summary>
        /// <param name="args">The arguments.</param>
        /// <param name="workspace">The workspace.</param>
        /// <param name="result">The result.</param>
        /// <returns>Returns <c>true</c> if the function was evaluated; otherwise, <c>false</c>.</returns>
        [CalculatorName("En"), CalculatorDescription("Calculates the generalized exponential integral of a number, with the second argument representing n. The arguments is expected to not have units.")]
        [MinArg(2), MaxArg(2)]
        public static bool Expn(IReadOnlyList<Quantity<double>> args, IWorkspace workspace, out Quantity<double> result)
        {
            if (args.Count != 2)
            {
                // Only one argument allowed
                result = _invalid;
                workspace.PostDiagnosticMessage(new($"Expected a single argument for En()."));
                return false;
            }
            if (args[0].Unit != Unit.UnitNone || args[1].Unit != Unit.UnitNone)
            {
                // Cannot deal with units
                result = _invalid;
                workspace.PostDiagnosticMessage(new($"Expected all arguments to not have units for En()."));
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
        [CalculatorName("Gamma"), CalculatorDescription("Calculates the Gamma function. The argument is expected to not have units.")]
        public static bool Gamma(IReadOnlyList<Quantity<double>> args, IWorkspace workspace, out Quantity<double> result)
        {
            if (!args.SingleNonUnitArgument(workspace, "Gamma", out result))
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
        [CalculatorName("GammaLn"), CalculatorDescription("Calculates the natural logarithm of the Gamma function. The argument is expected to not have units.")]
        public static bool GammaLn(IReadOnlyList<Quantity<double>> args, IWorkspace workspace, out Quantity<double> result)
        {
            if (!args.SingleNonUnitArgument(workspace, "GammaLn", out result))
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
                workspace.PostDiagnosticMessage(new($"Expected a single argument for {name}()."));
                return false;
            }
            var arg = args[0];
            if (arg.Unit != Unit.UnitNone && arg.Unit != unit)
            {
                // Cannot 
                result = _invalid;
                workspace.PostDiagnosticMessage(new($"Expected an argument without units, or in radians for {name}()."));
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
                workspace.PostDiagnosticMessage(new($"Expected a single argument for {name}()."));
                return false;
            }
            var arg = args[0];
            if (arg.Unit != Unit.UnitNone)
            {
                // Cannot deal with units
                result = _invalid;
                workspace.PostDiagnosticMessage(new($"Expected an argument without units for {name}()."));
                return false;
            }
            result = default;
            return true;
        }
    }
}
