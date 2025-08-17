using MaxwellCalc.Core.Parsers;
using MaxwellCalc.Core.Workspaces;
using System;
using System.Linq;

namespace MaxwellCalc.Core.Units
{
    /// <summary>
    /// Helper methods for units.
    /// </summary>
    public static class UnitHelper
    {
        /// <summary>
        /// Registers a unit for both input and output, such that
        /// <paramref name="inputUnit"/> = <paramref name="value"/> * <paramref name="baseUnits"/>.
        /// </summary>
        /// <param name="workspace">The workspace.</param>
        /// <param name="inputUnit">The new input unit.</param>
        /// <param name="value">The value in base units to get the input unit.</param>
        public static bool TryRegisterInputOutputUnit<T>(this IWorkspace<T> workspace, string inputUnit, Unit baseUnits, string value)
            where T : struct, IFormattable
        {
            var oldState = workspace.Restrict(false, false, true, false, false);

            // Parse
            var lexer = new Lexer(value);
            var node = Parser.Parse(lexer, workspace);
            if (node is null)
                return false;
            if (!workspace.TryResolve(node, out var result))
                return false;

            // Set the input and output unit
            workspace.InputUnits[inputUnit] = new(result.Scalar, baseUnits);
            workspace.OutputUnits[new(new Unit((inputUnit, 1)), baseUnits)] = result.Scalar;

            // Reset
            workspace.Restore(oldState);
            return true;
        }

        /// <summary>
        /// Registers an output unit on a workspace.
        /// </summary>
        /// <typeparam name="T">The base type.</typeparam>
        /// <param name="workspace">The workspace.</param>
        /// <param name="outputUnit">The output unit.</param>
        /// <param name="baseUnits">The same quantity in base units.</param>
        /// <returns>Returns <c>true</c> if the registration was successfull; otherwise, <c>false</c>.</returns>
        public static bool TryRegisterOutputUnit<T>(this IWorkspace<T> workspace, Unit outputUnit, Quantity<string> baseUnits)
            where T : struct, IFormattable
        {
            bool oldAllowUnits = workspace.AllowUnits;
            bool oldAllowVariables = workspace.AllowVariables;
            bool oldAllowUserFunctions = workspace.AllowUserFunctions;
            bool oldAllowBuiltInFunctions = workspace.AllowBuiltInFunctions;
            workspace.AllowUnits = false;
            workspace.AllowVariables = false;
            workspace.AllowUserFunctions = false;
            workspace.AllowBuiltInFunctions = false;

            // Parse
            var lexer = new Lexer(baseUnits.Scalar);
            var node = Parser.Parse(lexer, workspace);
            if (node is null)
                return false;
            if (!workspace.TryResolve(node, out var result))
                return false;

            // Set the output unit
            var key = new OutputUnitKey(outputUnit, baseUnits.Unit);
            workspace.OutputUnits[key] = result.Scalar;

            // Reset
            workspace.AllowUnits = oldAllowUnits;
            workspace.AllowVariables = oldAllowVariables;
            workspace.AllowUserFunctions = oldAllowUserFunctions;
            workspace.AllowBuiltInFunctions = oldAllowBuiltInFunctions;
            return true;
        }

        /// <summary>
        /// Registers a unit with modifiers for both input and output.
        /// </summary>
        /// <param name="workspace">The workspace.</param>
        /// <param name="name">The name of the unit.</param>
        /// <param name="atto">If <c>true</c>, the "atto" modifier is added.</param>
        /// <param name="femto">If <c>true</c>, the "femto" modifier is added.</param>
        /// <param name="pico">If <c>true</c>, the "pico" modifier is added.</param>
        /// <param name="nano">If <c>true</c>, the "nano" modifier is added.</param>
        /// <param name="micro">If <c>true</c>, the "micro" modifier is added.</param>
        /// <param name="milli">If <c>true</c>, the "milli" modifier is added.</param>
        /// <param name="centi">If <c>true</c>, the "centi" modifier is added.</param>
        /// <param name="kilo">If <c>true</c>, the "kilo" modifier is added.</param>
        /// <param name="mega">If <c>true</c>, the "mega" modifier is added.</param>
        /// <param name="giga">If <c>true</c>, the "giga" modifier is added.</param>
        /// <param name="tera">If <c>true</c>, the "tera" modifier is added.</param>
        /// <param name="peta">If <c>true</c>, the "peta" modifier is added.</param>
        public static bool TryRegisterModifierInputOutputUnits<T>(
            this IWorkspace<T> workspace, string name, Unit baseUnit,
            bool atto = false, bool femto = false, bool pico = false, bool nano = false, bool micro = false,
            bool milli = false, bool centi = false, bool kilo = false, bool mega = false, bool giga = false,
            bool tera = false, bool peta = false) where T : struct, IFormattable
        {
            if (atto)
            {
                if (!workspace.TryRegisterInputOutputUnit($"a{name}", baseUnit, $"1e-18"))
                    return false;
            }
            if (femto)
            {
                if (!workspace.TryRegisterInputOutputUnit($"f{name}", baseUnit, "1e-15"))
                    return false;
            }
            if (pico)
            {
                if (!workspace.TryRegisterInputOutputUnit($"p{name}", baseUnit, "1e-12"))
                    return false;
            }
            if (nano)
            {
                if (!workspace.TryRegisterInputOutputUnit($"n{name}", baseUnit, "1e-9"))
                    return false;
            }
            if (micro)
            {
                if (!workspace.TryRegisterInputOutputUnit($"u{name}", baseUnit, "1e-6"))
                    return false;
            }
            if (milli)
            {
                if (!workspace.TryRegisterInputOutputUnit($"m{name}", baseUnit, "1e-3"))
                    return false;
            }
            if (centi)
            {
                if (!workspace.TryRegisterInputOutputUnit($"c{name}", baseUnit, "1e-2"))
                    return false;
            }
            if (!workspace.TryRegisterInputOutputUnit(name, baseUnit, "1"))
                return false;
            if (kilo)
            {
                if (!workspace.TryRegisterInputOutputUnit($"k{name}", baseUnit, "1e3"))
                    return false;
            }
            if (mega)
            {
                if (!workspace.TryRegisterInputOutputUnit($"M{name}", baseUnit, "1e6"))
                    return false;
            }
            if (giga)
            {
                if (!workspace.TryRegisterInputOutputUnit($"G{name}", baseUnit, "1e9"))
                    return false;
            }
            if (tera)
            {
                if (!workspace.TryRegisterInputOutputUnit($"T{name}", baseUnit, "1e12"))
                    return false;
            }
            if (peta)
            {
                if (!workspace.TryRegisterInputOutputUnit($"P{name}", baseUnit, "1e15"))
                    return false;
            }
            return true;
        }

        /// <summary>
        /// Registers a derived unit with modifiers in one of the dimensions in the base units.
        /// </summary>
        /// <param name="workspace">The workspace.</param>
        /// <param name="outputUnit">The output unit.</param>
        /// <param name="dimension">The dimension that will receive the modifier.</param>
        /// <param name="atto">If <c>true</c>, the "atto" modifier is added.</param>
        /// <param name="femto">If <c>true</c>, the "femto" modifier is added.</param>
        /// <param name="pico">If <c>true</c>, the "pico" modifier is added.</param>
        /// <param name="nano">If <c>true</c>, the "nano" modifier is added.</param>
        /// <param name="micro">If <c>true</c>, the "micro" modifier is added.</param>
        /// <param name="milli">If <c>true</c>, the "milli" modifier is added.</param>
        /// <param name="kilo">If <c>true</c>, the "kilo" modifier is added.</param>
        /// <param name="mega">If <c>true</c>, the "mega" modifier is added.</param>
        /// <param name="giga">If <c>true</c>, the "giga" modifier is added.</param>
        /// <param name="tera">If <c>true</c>, the "tera" modifier is added.</param>
        /// <param name="peta">If <c>true</c>, the "peta" modifier is added.</param>
        public static bool TryRegisterModifierOutputUnits<T>(
            this IWorkspace<T> workspace,
            Unit outputUnit, Unit baseUnits, string dimension, string modifier = "1",
            bool atto = false, bool femto = false, bool pico = false, bool nano = false,
            bool micro = false, bool milli = false, bool centi = false, bool kilo = false,
            bool mega = false, bool giga = false, bool tera = false, bool peta = false) where T : struct, IFormattable
        {
            if (outputUnit.Dimension is null || baseUnits.Dimension is null)
                throw new ArgumentException("Dimension cannot be null", nameof(outputUnit));

            Unit GetUnit(string prefix)
                => new([.. outputUnit.Dimension.Select(p => (p.Key == dimension ? $"{prefix}{p.Key}" : p.Key, p.Value))]);
            if (atto)
            {
                if (!workspace.TryRegisterOutputUnit(GetUnit("a"), new Quantity<string>("1e-18", baseUnits)))
                    return false;
            }
            if (femto)
            {
                if (!workspace.TryRegisterOutputUnit(GetUnit("f"), new Quantity<string>("1e-15", baseUnits)))
                    return false;
            }
            if (pico)
            {
                if (!workspace.TryRegisterOutputUnit(GetUnit("p"), new Quantity<string>("1e-12", baseUnits)))
                    return false;
            }
            if (nano)
            {
                if (!workspace.TryRegisterOutputUnit(GetUnit("n"), new Quantity<string>("1e-9", baseUnits)))
                    return false;
            }
            if (micro)
            {
                if (!workspace.TryRegisterOutputUnit(GetUnit("u"), new Quantity<string>("1e-6", baseUnits)))
                    return false;
            }
            if (milli)
            {
                if (!workspace.TryRegisterOutputUnit(GetUnit("m"), new Quantity<string>("1e-3", baseUnits)))
                    return false;
            }
            if (centi)
            {
                if (!workspace.TryRegisterOutputUnit(GetUnit("c"), new Quantity<string>("1e-2", baseUnits)))
                    return false;
            }
            if (!workspace.TryRegisterOutputUnit(outputUnit, new Quantity<string>("1", baseUnits)))
                return false;
            if (kilo)
            {
                if (!workspace.TryRegisterOutputUnit(GetUnit("k"), new Quantity<string>("1e3", baseUnits)))
                    return false;
            }
            if (mega)
            {
                if (!workspace.TryRegisterOutputUnit(GetUnit("M"), new Quantity<string>("1e6", baseUnits)))
                    return false;
            }
            if (giga)
            {
                if (!workspace.TryRegisterOutputUnit(GetUnit("G"), new Quantity<string>("1e9", baseUnits)))
                    return false;
            }
            if (tera)
            {
                if (!workspace.TryRegisterOutputUnit(GetUnit("T"), new Quantity<string>("1e12", baseUnits)))
                    return false;
            }
            if (peta)
            {
                if (!workspace.TryRegisterOutputUnit(GetUnit("P"), new Quantity<string>("1e15", baseUnits)))
                    return false;
            }
            return true;
        }

        /// <summary>
        /// Registers common units.
        /// </summary>
        /// <param name="workspace">The workspace.</param>
        public static void RegisterCommonUnits<T>(this IWorkspace<T> workspace) where T : struct, IFormattable
        {
            // Length
            workspace.TryRegisterModifierInputOutputUnits(Unit.Meter, Unit.UnitMeter,
                nano: true, micro: true, milli: true, centi: true, kilo: true);

            // Speed
            workspace.TryRegisterModifierOutputUnits(
                new((Unit.Meter, 1), (Unit.Second, -1)),
                new((Unit.Meter, 1), (Unit.Second, -1)), Unit.Meter);
            workspace.TryRegisterOutputUnit(
                new Unit(("km", 1), ("hour", -1)),
                new("1/3.6", new((Unit.Meter, 1), (Unit.Second, -1))));

            // Mass
            workspace.TryRegisterInputOutputUnit("ng", Unit.UnitKilogram, "1e-12");
            workspace.TryRegisterInputOutputUnit("ug", Unit.UnitKilogram, "1e-9");
            workspace.TryRegisterInputOutputUnit("mg", Unit.UnitKilogram, "1e-6");
            workspace.TryRegisterInputOutputUnit("g", Unit.UnitKilogram, "1e-3");
            workspace.TryRegisterInputOutputUnit("kg", Unit.UnitKilogram, "1");
            workspace.TryRegisterInputOutputUnit("ton", Unit.UnitKilogram, "1000");

            // Time
            workspace.TryRegisterModifierInputOutputUnits(Unit.Second, Unit.UnitSeconds,
                femto: true, pico: true, nano: true, micro: true);
            workspace.TryRegisterInputOutputUnit("min", Unit.UnitSeconds, "60");
            workspace.TryRegisterInputOutputUnit("hour", Unit.UnitSeconds, "3600");
            workspace.TryRegisterInputOutputUnit("day", Unit.UnitSeconds, "86400");

            // Ampere
            workspace.TryRegisterModifierInputOutputUnits(Unit.Ampere, Unit.UnitAmperes,
                pico: true, nano: true, micro: true, milli: true, kilo: true);

            // Kelvin
            workspace.TryRegisterModifierInputOutputUnits(Unit.Kelvin, Unit.UnitKelvin,
                milli: true);

            // Candela
            workspace.TryRegisterModifierInputOutputUnits(Unit.Candela, Unit.UnitCandela);

            // Angle
            workspace.TryRegisterInputOutputUnit("rad", Unit.UnitRadian, "1");
        }

        /// <summary>
        /// Registers common units used by electrical/electronics engineers.
        /// </summary>
        /// <param name="workspace">The workspace.</param>
        public static void RegisterCommonElectronicsUnits<T>(IWorkspace<T> workspace) where T : struct, IFormattable
        {
            // Coulomb
            workspace.TryRegisterModifierInputOutputUnits("C",
                new((Unit.Ampere, 1), (Unit.Second, 1)));

            // Coulomb meter - electric dipole moment
            workspace.TryRegisterOutputUnit(
                new(("C", 1), (Unit.Meter, 1)), new Quantity<string>("1",
                new((Unit.Ampere, 1), (Unit.Second, 1), (Unit.Meter, 1))));

            // Coulomb per meter - charge density
            workspace.TryRegisterOutputUnit(
                new(("C", 1), (Unit.Meter, -1)), new Quantity<string>("1",
                new((Unit.Ampere, 1), (Unit.Second, 1), (Unit.Meter, -1))));

            // Coulomb per square meter - charge density
            workspace.TryRegisterOutputUnit(
                new(("C", 1), (Unit.Meter, -2)), new Quantity<string>("1",
                new((Unit.Ampere, 1), (Unit.Second, 1), (Unit.Meter, -2))));

            // Coulomb per cubic meter - charge density
            workspace.TryRegisterOutputUnit(
                new(("C", 1), (Unit.Meter, -3)), new Quantity<string>("1",
                new((Unit.Ampere, 1), (Unit.Second, 1), (Unit.Meter, -3))));

            // Volts
            workspace.TryRegisterModifierInputOutputUnits("V",
                new Unit((Unit.Kilogram, 1), (Unit.Meter, 2), (Unit.Second, -3), (Unit.Ampere, -1)),
                nano: true, micro: true, milli: true, kilo: true, mega: true);

            // Volts per meter - electric field
            workspace.TryRegisterModifierOutputUnits(
                new(("V", 1), (Unit.Meter, -1)),
                new Unit((Unit.Kilogram, 1), (Unit.Meter, 1), (Unit.Second, -3), (Unit.Ampere, -1)), "V");

            // Volts per square meter - electric field gradient
            workspace.TryRegisterModifierOutputUnits(
                new(("V", 1), (Unit.Meter, -2)),
                new Unit((Unit.Kilogram, 1), (Unit.Second, -3), (Unit.Ampere, -1)), "V");

            // Volts per second - slew rate
            workspace.TryRegisterModifierOutputUnits(
                new(("V", 1), (Unit.Second, -1)),
                new Unit((Unit.Kilogram, 1), (Unit.Meter, 2), (Unit.Second, -4), (Unit.Ampere, -1)), "V");
            
            // Watts
            workspace.TryRegisterModifierInputOutputUnits("W", 
                new((Unit.Kilogram, 1), (Unit.Meter, 2), (Unit.Second, -3)),
                pico: true, nano: true, micro: true, milli: true, kilo: true, mega: true, giga: true);
            
            // Joules
            workspace.TryRegisterModifierInputOutputUnits("J",
                new((Unit.Kilogram, 1), (Unit.Meter, 2), (Unit.Second, -2)),
                pico: true, nano: true, micro: true, milli: true, kilo: true, mega: true, giga: true);

            // Joules seconds (Planck constant)
            workspace.TryRegisterModifierOutputUnits(
                new(("J", 1), (Unit.Second, 1)),
                new((Unit.Kilogram, 1), (Unit.Meter, 2), (Unit.Second, -1)), "J");

            // Farad
            workspace.TryRegisterModifierInputOutputUnits("F",
                new((Unit.Kilogram, -1), (Unit.Meter, -2), (Unit.Second, 4), (Unit.Ampere, 2)),
                femto: true, pico: true, micro: true, milli: true);

            // Farad per meter
            workspace.TryRegisterModifierOutputUnits(
                new(("F", 1), (Unit.Meter, -1)),
                new((Unit.Kilogram, -1), (Unit.Meter, -3), (Unit.Second, 4), (Unit.Ampere, 2)), "V");

            // Farad per square meter
            workspace.TryRegisterModifierOutputUnits(
                new(("F", 1), (Unit.Meter, -2)),
                new((Unit.Kilogram, -1), (Unit.Meter, -4), (Unit.Second, 4), (Unit.Ampere, 2)), "V");

            // Henry
            workspace.TryRegisterModifierInputOutputUnits("H",
                new((Unit.Kilogram, 1), (Unit.Meter, 2), (Unit.Second, -2), (Unit.Ampere, -2)),
                nano: true, micro: true, milli: true);

            // Weber
            workspace.TryRegisterModifierInputOutputUnits("Wb",
                new((Unit.Kilogram, 1), (Unit.Meter, 2), (Unit.Second, -2), (Unit.Ampere, -1)));
            
            // Ohm
            workspace.TryRegisterModifierInputOutputUnits("Ohm",
                new((Unit.Kilogram, 1), (Unit.Meter, 2), (Unit.Second, -3), (Unit.Ampere, -2)),
                micro: true, milli: true, kilo: true, mega: true);

            // Siemens
            workspace.TryRegisterModifierInputOutputUnits("S",
                new((Unit.Kilogram, -1), (Unit.Meter, -2), (Unit.Second, 3), (Unit.Ampere, 2)),
                micro: true, milli: true, kilo: true, mega: true);

            // Hertz
            workspace.TryRegisterModifierInputOutputUnits("Hz",
                new((Unit.Second, -1)),
                milli: true, kilo: true, mega: true, giga: true, tera: true);

            // Bits
            var b = new Unit(("bit", 1));
            workspace.TryRegisterInputOutputUnit("bit", new Unit(("bit", 1)), "1");
            workspace.TryRegisterInputOutputUnit("B", new Unit(("bit", 1)), "8");
            workspace.TryRegisterInputOutputUnit("kB", new Unit(("bit", 1)), "8192");
            workspace.TryRegisterInputOutputUnit("MB", new Unit(("bit", 1)), "8388608");
            workspace.TryRegisterInputOutputUnit("GB", new Unit(("bit", 1)), "8589934592");
            workspace.TryRegisterInputOutputUnit("TB", new Unit(("bit", 1)), "8796093022208");
            workspace.TryRegisterInputOutputUnit("PB", new Unit(("bit", 1)), "9007199254740992");

            // Bits per second
            var bps = new Unit(("bit", 1), (Unit.Second, -1));
            workspace.TryRegisterOutputUnit(bps, new Quantity<string>("1", bps));
            workspace.TryRegisterOutputUnit(new(("kbps", 1)), new Quantity<string>("1000", bps));
            workspace.TryRegisterOutputUnit(new(("Mbps", 1)), new Quantity<string>("1000000", bps));
            workspace.TryRegisterOutputUnit(new(("Gbps", 1)), new Quantity<string>("1000000000", bps));
            workspace.TryRegisterOutputUnit(new(("Tbps", 1)), new Quantity<string>("1000000000000", bps));
        }
    }
}
