using MaxwellCalc.Core.Attributes;
using MaxwellCalc.Core.Units;
using MaxwellCalc.Core.Workspaces.SpecialFunctions;
using System;
using System.Collections.Generic;
using System.Numerics;

namespace MaxwellCalc.Core.Workspaces
{
    /// <summary>
    /// Methods for complex math.
    /// </summary>
    public static class ComplexMathHelper
    {
        private static readonly Quantity<Complex> _invalid = new(new Complex(double.NaN, double.NaN), Unit.UnitNone);

        /// <summary>
        /// The Euler-Mascheroni constant.
        /// </summary>
        [CalculatorName("gamma"), CalculatorDescription("The Euler–Mascheroni constant.")]
        public static Complex EulerGamma = Constants.EulerGamma;

        /// <summary>
        /// PI.
        /// </summary>
        [CalculatorDescription("Pi.")]
        public static Complex Pi = Math.PI;

        /// <summary>
        /// Euler's constant.
        /// </summary>
        [CalculatorDescription("Euler's constant.")]
        public static Complex E = Math.E;

        /// <summary>
        /// Light speed.
        /// </summary>
        [CalculatorName("c"), CalculatorDescription("Light speed.")]
        public static Quantity<Complex> LightSpeed = new(299792458.0, new Unit((Unit.Meter, 1), (Unit.Second, -1)));

        /// <summary>
        /// The elementary charge.
        /// </summary>
        [CalculatorName("q"), CalculatorDescription("The elementary charge.")]
        public static Quantity<Complex> ElementaryCharge = new(1.60217663e-19, new Unit((Unit.Ampere, 1), (Unit.Second, 1)));

        /// <summary>
        /// Permittivity of vacuum (Farad/meter).
        /// </summary>
        [CalculatorDescription("The permittivity of vacuum.")]
        public static Quantity<Complex> Eps0 = new(8.8541878128e-12, new Unit(
                    (Unit.Kilogram, -1),
                    (Unit.Meter, -3),
                    (Unit.Second, 4),
                    (Unit.Ampere, 2)));

        /// <summary>
        /// Permeability of vacuum (Newton Ampere^-2)
        /// </summary>
        [CalculatorDescription("The permeability of vacuum.")]
        public static Quantity<Complex> Mu0 = new(1.25663706212e-6, new Unit(
                (Unit.Kilogram, 1),
                (Unit.Meter, 1),
                (Unit.Second, -2),
                (Unit.Ampere, -2)));

        /// <summary>
        /// An electron-volt.
        /// </summary>
        [CalculatorName("eV"), CalculatorDescription("An electronvolt.")]
        public static Quantity<Complex> ElectronVolt = new(1.60217663e-19, new Unit(
                (Unit.Kilogram, 1),
                (Unit.Meter, 2),
                (Unit.Second, -2)));

        /// <summary>
        /// Planck's constant.
        /// </summary>
        [CalculatorName("h"), CalculatorDescription("Planck's constant.")]
        public static Quantity<Complex> Planck = new(6.6260693e-34, new Unit(
            (Unit.Kilogram, 1),
            (Unit.Meter, 2),
            (Unit.Second, -1)));

        /// <summary>
        /// Reduced Planck constant bar (J s)
        /// </summary>
        [CalculatorName("hbar"), CalculatorDescription("The reduced Planck's constant.")]
        public static Quantity<Complex> PlanckBar = new(6.6260693e-34 / Math.PI, new Unit(
            (Unit.Kilogram, 1),
            (Unit.Meter, 2),
            (Unit.Second, -1)));

        /// <summary>
        /// Boltzmann constant (J/K).
        /// </summary>
        [CalculatorName("k"), CalculatorDescription("Boltzmann's constant.")]
        public static Quantity<Complex> Boltzmann = new(1.3806505e-23, new Unit(
            (Unit.Kilogram, 1),
            (Unit.Meter, 2),
            (Unit.Second, -2),
            (Unit.Kelvin, -1)));

        /// <summary>
        /// The newtonian constant of gravitation.
        /// </summary>
        [CalculatorName("G"), CalculatorDescription("Newtonian constant of gravitation.")]
        public static Quantity<Complex> NewtonianGravityConstant = new(6.6743015e-11, new Unit(
            (Unit.Meter, 3),
            (Unit.Kilogram, -1),
            (Unit.Second, -2)));

        /// <summary>
        /// The mass of an electron.
        /// </summary>
        [CalculatorName("me"), CalculatorDescription("The mass of an electron.")]
        public static Quantity<Complex> ElectronMass = new(9.109383713928e-31, Unit.UnitKilogram);

        /// <summary>
        /// The mass of a muon.
        /// </summary>
        [CalculatorName("mmu"), CalculatorDescription("The mass of a muon.")]
        public static Quantity<Complex> MuonMass = new(1.88353162742e-28, Unit.UnitKilogram);

        /// <summary>
        /// The mass of a tau particle.
        /// </summary>
        [CalculatorName("mtau"), CalculatorDescription("The mass of a tau particle.")]
        public static Quantity<Complex> TauMass = new(3.1675421e-27, Unit.UnitKilogram);

        /// <summary>
        /// The mass of a proton.
        /// </summary>
        [CalculatorName("mp"), CalculatorDescription("The mass of a proton.")]
        public static Quantity<Complex> ProtonMass = new(1.6726219259552e-27, Unit.UnitKilogram);

        /// <summary>
        /// The mass of a neutron.
        /// </summary>
        [CalculatorName("mn"), CalculatorDescription("The mass of a neutron.")]
        public static Quantity<Complex> NeutronMass = new(1.6749275005685e-27, Unit.UnitKilogram);

        /// <summary>
        /// The g-factor of an electron.
        /// </summary>
        [CalculatorName("ge"), CalculatorDescription("The g-factor of an electron.")]
        public static Complex ElectronGFactor = 2.0023193043609236;

        /// <summary>
        /// The g-factor of a muon.
        /// </summary>
        [CalculatorName("gm"), CalculatorDescription("The g-factor of a muon.")]
        public static Complex MuonGFactor = 2.0023318412382;

        /// <summary>
        /// The g-factor of a proton.
        /// </summary>
        [CalculatorName("gp"), CalculatorDescription("The g-factor of a proton.")]
        public static Complex ProtonGFactor = 5.585694689316;

        /// <summary>
        /// Avogadro's constant.
        /// </summary>
        [CalculatorName("NA"), CalculatorDescription("Avogadro's constant.")]
        public static Quantity<Complex> AvogadroConstant = new(6.02214076e23, new Unit((Unit.Mole, -1)));

        /// <summary>
        /// Computes the absolute value of a quantity.
        /// </summary>
        /// <param name="args">The arguments.</param>
        /// <param name="workspace">The workspace.</param>
        /// <param name="result">The result.</param>
        /// <returns>Returns <c>true</c> if the function was evaluated; otherwise, <c>false</c>.</returns>
        [CalculatorDescription("Calculates the magnitude of a complex number. The units are the same as the argument.")]
        public static bool Abs(IReadOnlyList<Quantity<Complex>> args, IWorkspace workspace, out Quantity<Complex> result)
        {
            if (args.Count != 1)
            {
                result = _invalid;
                workspace.PostDiagnosticMessage(new("Expected single argument for abs()."));
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
        [CalculatorDescription("Calculates the argument (polar angle) of a complex number. The result has units in radians.")]
        public static bool Arg(IReadOnlyList<Quantity<Complex>> args, IWorkspace workspace, out Quantity<Complex> result)
        {
            if (args.Count != 1)
            {
                result = _invalid;
                workspace.PostDiagnosticMessage(new("Expected single argument for arg()."));
                return false;
            }
            result = new Quantity<Complex>(Math.Atan2(args[0].Scalar.Imaginary, args[0].Scalar.Real), Unit.UnitRadian);
            return true;
        }

        /// <summary>
        /// Computes the real part.
        /// </summary>
        /// <param name="args">The arguments.</param>
        /// <param name="workspace">The workspace.</param>
        /// <param name="result">The result.</param>
        /// <returns>Returns <c>true</c> if the function was evaluated; otherwise, <c>false</c>.</returns>
        [CalculatorDescription("Calculates the real part of a complex number.")]
        public static bool Re(IReadOnlyList<Quantity<Complex>> args, IWorkspace workspace, out Quantity<Complex> result)
        {
            if (args.Count != 1)
            {
                result = _invalid;
                workspace.PostDiagnosticMessage(new($"Expected single argument for {nameof(Re)}."));
                return false;
            }
            result = new Quantity<Complex>(args[0].Scalar.Real, args[0].Unit);
            return true;
        }

        /// <summary>
        /// Computes the real part.
        /// </summary>
        /// <param name="args">The arguments.</param>
        /// <param name="workspace">The workspace.</param>
        /// <param name="result">The result.</param>
        /// <returns>Returns <c>true</c> if the function was evaluated; otherwise, <c>false</c>.</returns>
        [CalculatorDescription("Calculates the imaginary part of a complex number.")]
        public static bool Im(IReadOnlyList<Quantity<Complex>> args, IWorkspace workspace, out Quantity<Complex> result)
        {
            if (args.Count != 1)
            {
                result = _invalid;
                workspace.PostDiagnosticMessage(new($"Expected single argument for {nameof(Im)}."));
                return false;
            }
            result = new Quantity<Complex>(args[0].Scalar.Imaginary, args[0].Unit);
            return true;
        }

        /// <summary>
        /// Computes the sine of a quantity.
        /// </summary>
        /// <param name="args">The arguments.</param>
        /// <param name="workspace">The workspace.</param>
        /// <param name="result">The result.</param>
        /// <returns>Returns <c>true</c> if the function was evaluated; otherwise, <c>false</c>.</returns>
        [CalculatorDescription("Calculates the sine of a complex number.")]
        public static bool Sin(IReadOnlyList<Quantity<Complex>> args, IWorkspace workspace, out Quantity<Complex> result)
        {
            if (!args.SingleGivenUnitArgument(Unit.UnitRadian, workspace, nameof(Sin), out result))
                return false;
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
        [CalculatorDescription("Calculates the cosine of a complex number.")]
        public static bool Cos(IReadOnlyList<Quantity<Complex>> args, IWorkspace workspace, out Quantity<Complex> result)
        {
            if (!args.SingleGivenUnitArgument(Unit.UnitRadian, workspace, nameof(Sin), out result))
                return false;
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
        [CalculatorDescription("Calculates the tangent of a complex number.")]
        public static bool Tan(IReadOnlyList<Quantity<Complex>> args, IWorkspace workspace, out Quantity<Complex> result)
        {
            if (!args.SingleGivenUnitArgument(Unit.UnitRadian, workspace, nameof(Sin), out result))
                return false;
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
        [CalculatorDescription("Calculates the hyperbolic sine of a complex number.")]
        public static bool Sinh(IReadOnlyList<Quantity<Complex>> args, IWorkspace workspace, out Quantity<Complex> result)
        {
            if (!args.SingleNonUnitArgument(workspace, nameof(Sinh), out result))
                return false;
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
        [CalculatorDescription("Calculates the hyperbolic cosine of a complex number.")]
        public static bool Cosh(IReadOnlyList<Quantity<Complex>> args, IWorkspace workspace, out Quantity<Complex> result)
        {
            if (!args.SingleNonUnitArgument(workspace, nameof(Sinh), out result))
                return false;
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
        [CalculatorDescription("Calculates the hyperbolic tangent of a complex number.")]
        public static bool Tanh(IReadOnlyList<Quantity<Complex>> args, IWorkspace workspace, out Quantity<Complex> result)
        {
            if (!args.SingleNonUnitArgument(workspace, nameof(Sinh), out result))
                return false;
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
        [CalculatorDescription("Calculates the exponent of a complex number.")]
        public static bool Exp(IReadOnlyList<Quantity<Complex>> args, IWorkspace workspace, out Quantity<Complex> result)
        {
            if (!args.SingleNonUnitArgument(workspace, nameof(Sinh), out result))
                return false;
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
        [CalculatorDescription("Calculates the natural logarithm of a complex number.")]
        public static bool Ln(IReadOnlyList<Quantity<Complex>> args, IWorkspace workspace, out Quantity<Complex> result)
        {
            if (!args.SingleNonUnitArgument(workspace, nameof(Sinh), out result))
                return false;
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
        [CalculatorDescription("Calculates the base-10 logarithm of a complex number.")]
        public static bool Log10(IReadOnlyList<Quantity<Complex>> args, IWorkspace workspace, out Quantity<Complex> result)
        {
            if (!args.SingleNonUnitArgument(workspace, nameof(Sinh), out result))
                return false;
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
        [CalculatorDescription("Calculates the base-2 logarithm of a complex number.")]
        public static bool Log2(IReadOnlyList<Quantity<Complex>> args, IWorkspace workspace, out Quantity<Complex> result)
        {
            if (!args.SingleNonUnitArgument(workspace, nameof(Sinh), out result))
                return false;
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
        [CalculatorDescription("Gets the sqrt of a complex number.")]
        public static bool Sqrt(IReadOnlyList<Quantity<Complex>> args, IWorkspace workspace, out Quantity<Complex> result)
        {
            if (args.Count != 1)
            {
                // Only one argument allowed
                result = _invalid;
                workspace.PostDiagnosticMessage(new("Expected single argument for sqrt()."));
                return false;
            }
            var arg = args[0];
            result = new Quantity<Complex>(
                Complex.Sqrt(args[0].Scalar),
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
        [CalculatorDescription("Calculates the arcsine of a real number.")]
        public static bool Asin(IReadOnlyList<Quantity<Complex>> args, IWorkspace workspace, out Quantity<Complex> result)
            => EvaluateAsReal(args, DoubleMathHelper.Asin, workspace, nameof(Asin), out result);

        /// <summary>
        /// Computes the arccosine of a quantity.
        /// </summary>
        /// <param name="args">The arguments.</param>
        /// <param name="workspace">The workspace.</param>
        /// <param name="result">The result.</param>
        /// <returns>Returns <c>true</c> if the function was evaluated; otherwise, <c>false</c>.</returns>
        [CalculatorDescription("Calculates the arccosine of a real number.")]
        public static bool Acos(IReadOnlyList<Quantity<Complex>> args, IWorkspace workspace, out Quantity<Complex> result)
            => EvaluateAsReal(args, DoubleMathHelper.Acos, workspace, nameof(Acos), out result);

        /// <summary>
        /// Computes the arctangent of a quantity.
        /// </summary>
        /// <param name="args">The arguments.</param>
        /// <param name="workspace">The workspace.</param>
        /// <param name="result">The result.</param>
        /// <returns>Returns <c>true</c> if the function was evaluated; otherwise, <c>false</c>.</returns>
        [CalculatorDescription("Calculates the arctangent of a real number.")]
        public static bool Atan(IReadOnlyList<Quantity<Complex>> args, IWorkspace workspace, out Quantity<Complex> result)
            => EvaluateAsReal(args, DoubleMathHelper.Atan, workspace, nameof(Atan), out result);


        /// <summary>
        /// Computes the hyperbolic arcsine of a quantity.
        /// </summary>
        /// <param name="args">The arguments.</param>
        /// <param name="workspace">The workspace.</param>
        /// <param name="result">The result.</param>
        /// <returns>Returns <c>true</c> if the function was evaluated; otherwise, <c>false</c>.</returns>
        [CalculatorDescription("Calculates the hyperbolic arcsine of a real number.")]
        public static bool Asinh(IReadOnlyList<Quantity<Complex>> args, IWorkspace workspace, out Quantity<Complex> result)
            => EvaluateAsReal(args, DoubleMathHelper.Asinh, workspace, nameof(Asinh), out result);

        /// <summary>
        /// Computes the hyperbolic arccosine of a quantity.
        /// </summary>
        /// <param name="args">The arguments.</param>
        /// <param name="workspace">The workspace.</param>
        /// <param name="result">The result.</param>
        /// <returns>Returns <c>true</c> if the function was evaluated; otherwise, <c>false</c>.</returns>
        [CalculatorDescription("Calculates the hyperbolic arccosine of a real number.")]
        public static bool Acosh(IReadOnlyList<Quantity<Complex>> args, IWorkspace workspace, out Quantity<Complex> result)
            => EvaluateAsReal(args, DoubleMathHelper.Acosh, workspace, nameof(Acosh), out result);

        /// <summary>
        /// Computes the hyperbolic arctangent of a quantity.
        /// </summary>
        /// <param name="args">The arguments.</param>
        /// <param name="workspace">The workspace.</param>
        /// <param name="result">The result.</param>
        /// <returns>Returns <c>true</c> if the function was evaluated; otherwise, <c>false</c>.</returns>
        [CalculatorDescription("Calculates the hyperbolic arctangent of a real number.")]
        public static bool Atanh(IReadOnlyList<Quantity<Complex>> args, IWorkspace workspace, out Quantity<Complex> result)
            => EvaluateAsReal(args, DoubleMathHelper.Atanh, workspace, nameof(Atanh), out result);

        /// <summary>
        /// Computes the arcsine of a quantity.
        /// </summary>
        /// <param name="args">The arguments.</param>
        /// <param name="workspace">The workspace.</param>
        /// <param name="result">The result.</param>
        /// <returns>Returns <c>true</c> if the function was evaluated; otherwise, <c>false</c>.</returns>
        [CalculatorDescription("Calculates the full arctangent of a real number where the first argument is the Y-coordinate and the second is the X-coordinate. Both arguments need to have the same units.")]
        [MinArg(2), MaxArg(2)]
        public static bool Atan2(IReadOnlyList<Quantity<Complex>> args, IWorkspace workspace, out Quantity<Complex> result)
            => EvaluateAsReal(args, DoubleMathHelper.Atan2, workspace, nameof(Atan2), out result);


        /// <summary>
        /// Computes the maximum of a number of arguments.
        /// </summary>
        /// <param name="args">The arguments.</param>
        /// <param name="workspace">The workspace.</param>
        /// <param name="result">The result.</param>
        /// <returns>Returns <c>true</c> if the function was evaluated; otherwise, <c>false</c>.</returns>
        [CalculatorDescription("Calculates the maximum of a number of arguments. All arguments need to have the same units."), MinArg(1), MaxArg(int.MaxValue)]
        public static bool Max(IReadOnlyList<Quantity<Complex>> args, IWorkspace workspace, out Quantity<Complex> result)
            => EvaluateAsReal(args, DoubleMathHelper.Max, workspace, nameof(Max), out result);

        /// <summary>
        /// Computes the minimum of a number of arguments.
        /// </summary>
        /// <param name="args">The arguments.</param>
        /// <param name="workspace">The workspace.</param>
        /// <param name="result">The result.</param>
        /// <returns>Returns <c>true</c> if the function was evaluated; otherwise, <c>false</c>.</returns>
        [CalculatorDescription("Calculates the minimum of a number of arguments. All arguments need to have the same units."), MinArg(1), MaxArg(int.MaxValue)]
        public static bool Min(IReadOnlyList<Quantity<Complex>> args, IWorkspace workspace, out Quantity<Complex> result)
            => EvaluateAsReal(args, DoubleMathHelper.Min, workspace, nameof(Min), out result);

        /// <summary>
        /// Rounds a number.
        /// </summary>
        /// <param name="args">The arguments.</param>
        /// <param name="workspace">The workspace.</param>
        /// <param name="result">The result.</param>
        /// <returns>Returns <c>true</c> if the function was evaluated; otherwise, <c>false</c>.</returns>
        [CalculatorDescription("Rounds a number to some precision. If the second argument for precision is not specified, a precision of 0 digits after the comma is assumed.")]
        [MinArg(1), MaxArg(2)]
        public static bool Round(IReadOnlyList<Quantity<Complex>> args, IWorkspace workspace, out Quantity<Complex> result)
        {
            if (args.Count != 1 && args.Count != 2)
            {
                result = _invalid;
                workspace.PostDiagnosticMessage(new($"Expected 1 or 2 arguments for {nameof(Round)}()"));
                return false;
            }

            if (args.Count == 1)
                result = new Quantity<Complex>(
                    new Complex(
                        Math.Round(args[0].Scalar.Real, MidpointRounding.AwayFromZero),
                        Math.Round(args[0].Scalar.Imaginary, MidpointRounding.AwayFromZero)),
                    args[0].Unit);
            else
            {
                if (args[1].Unit != Unit.UnitNone)
                {
                    // Cannot deal with units
                    result = _invalid;
                    workspace.PostDiagnosticMessage(new($"Expected the second argument to not have units units for {nameof(Round)}()."));
                    return false;
                }
                if (args[1].Scalar.Imaginary != 0.0)
                {
                    result = _invalid;
                    workspace.PostDiagnosticMessage(new($"Expected the second argument to be real."));
                    return false;
                }
                int digits = (int)args[1].Scalar.Real;
                result = new Quantity<Complex>(
                    new Complex(
                        Math.Round(args[0].Scalar.Real, digits, MidpointRounding.AwayFromZero),
                        Math.Round(args[0].Scalar.Imaginary, digits, MidpointRounding.AwayFromZero)),
                    args[0].Unit);
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
        [CalculatorDescription("Calculates the factorial of a number. If the number is real, it is converted to an integer. The argument is expected to have no units.")]
        public static bool Factorial(IReadOnlyList<Quantity<Complex>> args, IWorkspace workspace, out Quantity<Complex> result)
            => EvaluateAsReal(args, DoubleMathHelper.Factorial, workspace, nameof(Factorial), out result);

        /// <summary>
        /// Calculates the natural logarithm of a number.
        /// </summary>
        /// <param name="args">The arguments.</param>
        /// <param name="workspace">The workspace.</param>
        /// <param name="result">The result.</param>
        /// <returns>Returns <c>true</c> if the function was evaluated; otherwise, <c>false</c>.</returns>
        [CalculatorDescription("Calculates the natural logarithm of the factorial of a number. If the number is real, it is converted to an integer. The argument is expected to have no units.")]
        public static bool FactorialLn(IReadOnlyList<Quantity<Complex>> args, IWorkspace workspace, out Quantity<Complex> result)
            => EvaluateAsReal(args, DoubleMathHelper.FactorialLn, workspace, nameof(FactorialLn), out result);

        /// <summary>
        /// Calculates the binomial of two numbers.
        /// </summary>
        /// <param name="args">The arguments.</param>
        /// <param name="workspace">The workspace.</param>
        /// <param name="result">The result.</param>
        /// <returns>Returns <c>true</c> if the function was evaluated; otherwise, <c>false</c>.</returns>
        [CalculatorName("B"), CalculatorDescription("Calculates the binomial of two numbers. If the numbers are real, they are converted to an integer. The arguments are expected to have no units.")]
        [MinArg(2), MaxArg(2)]
        public static bool Binomial(IReadOnlyList<Quantity<Complex>> args, IWorkspace workspace, out Quantity<Complex> result)
            => EvaluateAsReal(args, DoubleMathHelper.Binomial, workspace, nameof(Binomial), out result);

        /// <summary>
        /// Calculates the natural logarithm of two numbers.
        /// </summary>
        /// <param name="args">The arguments.</param>
        /// <param name="workspace">The workspace.</param>
        /// <param name="result">The result.</param>
        /// <returns>Returns <c>true</c> if the function was evaluated; otherwise, <c>false</c>.</returns>
        [CalculatorName("BLn"), CalculatorDescription("Calculates the natural logarithm of the binomial of two numbers. If the numbers are real, they are converted to an integer. The arguments are expected to have no units.")]
        [MinArg(2), MaxArg(2)]
        public static bool BinomialLn(IReadOnlyList<Quantity<Complex>> args, IWorkspace workspace, out Quantity<Complex> result)
            => EvaluateAsReal(args, DoubleMathHelper.BinomialLn, workspace, nameof(BinomialLn), out result);

        /// <summary>
        /// Calculates the multinomial of multiple numbers.
        /// </summary>
        /// <param name="args">The arguments.</param>
        /// <param name="workspace">The result.</param>
        /// <param name="result">The result.</param>
        /// <returns>Returns <c>true</c> if the function was evaluated; otherwise, <c>false</c>.</returns>
        [CalculatorDescription("Calculates the multinomial of a number of arguments. The first is n, the others are the k's in the denominator. The arguments are expected to have no units.")]
        public static bool Multinomial(IReadOnlyList<Quantity<Complex>> args, IWorkspace workspace, out Quantity<Complex> result)
            => EvaluateAsReal(args, DoubleMathHelper.Multinomial, workspace, nameof(Multinomial), out result);

        /// <summary>
        /// Calculates the exponential integral of a number.
        /// </summary>
        /// <param name="args">The arguments.</param>
        /// <param name="workspace">The result.</param>
        /// <param name="result">The result.</param>
        /// <returns>Returns <c>true</c> if the function was evaluated; otherwise, <c>false</c>.</returns>
        [CalculatorName("Ei"), CalculatorDescription("Calculates the exponential integral of a number. The argument is expected to not have units.")]
        public static bool Expi(IReadOnlyList<Quantity<Complex>> args, IWorkspace workspace, out Quantity<Complex> result)
        {
            if (!args.SingleNonUnitArgument(workspace, "Ei", out result))
                return false;
            result = new Quantity<Complex>(ExponentialIntegralFunctions.ExpI(args[0].Scalar), Unit.UnitNone);
            return true;
        }

        /// <summary>
        /// Calculates the exp(-x) * Ei(x).
        /// </summary>
        /// <param name="args">The arguments.</param>
        /// <param name="workspace">The workspace.</param>
        /// <param name="result">The result.</param>
        /// <returns>Returns <c>true</c> if the function was evaluated; otherwise, <c>false</c>.</returns>
        [CalculatorName("expEi"), CalculatorDescription("Calculates the multiplication exp(-x) * Ei(x). The argument is expected to not have units.")]
        public static bool ExpiExp(IReadOnlyList<Quantity<Complex>> args, IWorkspace workspace, out Quantity<Complex> result)
        {
            if (!args.SingleNonUnitArgument(workspace, "expEi", out result))
                return false;
            result = new Quantity<Complex>(ExponentialIntegralFunctions.ExpExpI(args[0].Scalar), Unit.UnitNone);
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
        public static bool Exp1(IReadOnlyList<Quantity<Complex>> args, IWorkspace workspace, out Quantity<Complex> result)
        {
            if (!args.SingleNonUnitArgument(workspace, "E1", out result))
                return false;
            result = new Quantity<Complex>(ExponentialIntegralFunctions.Exp1(args[0].Scalar), Unit.UnitNone);
            return true;
        }

        /// <summary>
        /// Calculates exp(x) * E1(x).
        /// </summary>
        /// <param name="args">The arguments.</param>
        /// <param name="workspace">The result.</param>
        /// <param name="result">The result.</param>
        /// <returns>Returns <c>true</c> if the function was evaluated; otherwise, <c>false</c>.</returns>
        [CalculatorName("expE1"), CalculatorDescription("Calculates the generalized exponential integral of a number with n=1. The argument is expected to not have units.")]
        public static bool Exp1Exp(IReadOnlyList<Quantity<Complex>> args, IWorkspace workspace, out Quantity<Complex> result)
        {
            if (!args.SingleNonUnitArgument(workspace, "expE1", out result))
                return false;
            result = new Quantity<Complex>(ExponentialIntegralFunctions.ExpExp1(args[0].Scalar), Unit.UnitNone);
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
        public static bool Expn(IReadOnlyList<Quantity<Complex>> args, IWorkspace workspace, out Quantity<Complex> result)
            => EvaluateAsReal(args, DoubleMathHelper.Expn, workspace, "En", out result);

        /// <summary>
        /// Calculates the Gamma function.
        /// </summary>
        /// <param name="args">The arguments.</param>
        /// <param name="workspace">The workspace.</param>
        /// <param name="result">The result.</param>
        /// <returns>Returns <c>true</c> if the function was evaluated; otherwise, <c>false</c>.</returns>
        [CalculatorName("Gamma"), CalculatorDescription("Calculates the Gamma function. The argument is expected to not have units.")]
        public static bool Gamma(IReadOnlyList<Quantity<Complex>> args, IWorkspace workspace, out Quantity<Complex> result)
            => EvaluateAsReal(args, DoubleMathHelper.Gamma, workspace, "Gamma", out result);

        /// <summary>
        /// Calculates the natural logarithm of the Gamma function.
        /// </summary>
        /// <param name="args">The arguments.</param>
        /// <param name="workspace">The workspace.</param>
        /// <param name="result">The result.</param>
        /// <returns>Returns <c>true</c> if the function was evaluated; otherwise, <c>false</c>.</returns>
        [CalculatorName("GammaLn"), CalculatorDescription("Calculates the natural logarithm of the Gamma function. The argument is expected to not have units.")]
        public static bool GammaLn(IReadOnlyList<Quantity<Complex>> args, IWorkspace workspace, out Quantity<Complex> result)
            => EvaluateAsReal(args, DoubleMathHelper.GammaLn, workspace, "GammaLn", out result);

        private static bool SingleGivenUnitArgument(this IReadOnlyList<Quantity<Complex>> args, Unit unit, IWorkspace workspace, string name, out Quantity<Complex> result)
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

        private static bool SingleNonUnitArgument(this IReadOnlyList<Quantity<Complex>> args, IWorkspace workspace, string name, out Quantity<Complex> result)
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

        private static bool EvaluateAsReal(IReadOnlyList<Quantity<Complex>> args, IWorkspace<double>.BuiltInFunctionDelegate function, IWorkspace workspace, string name, out Quantity<Complex> result)
        {
            var realArgs = new Quantity<double>[args.Count];
            for (int i = 0; i < args.Count; i++)
            {
                if (args[i].Scalar.Imaginary != 0.0)
                {
                    result = _invalid;
                    workspace.PostDiagnosticMessage(new($"Expected real arguments for {name}()."));
                    return false;
                }
                realArgs[i] = new Quantity<double>(args[i].Scalar.Real, args[i].Unit);
            }
            
            if (!function(realArgs, workspace, out var realResult))
            {
                result = _invalid;
                return false;
            }

            result = new Quantity<Complex>(realResult.Scalar, realResult.Unit);
            return true;
        }
    }
}
