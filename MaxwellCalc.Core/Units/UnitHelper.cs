using MaxwellCalc.Workspaces;
using System;
using System.Linq;

namespace MaxwellCalc.Units
{
    /// <summary>
    /// Helper methods for units.
    /// </summary>
    public static class UnitHelper
    {
        /// <summary>
        /// Registers a unit for both input and output, such that
        /// <paramref name="inputUnit"/> = <paramref name="modifier"/> * <paramref name="baseUnits"/>.
        /// </summary>
        /// <param name="workspace">The workspace.</param>
        /// <param name="inputUnit">The new input unit.</param>
        /// <param name="value">The value in base units.</param>
        public static bool TryRegisterInputOutputUnit<T>(
            this IWorkspace<T> workspace,
            string inputUnit, Quantity<string> value) where T : struct, IFormattable
        {
            if (!workspace.TryRegisterInputUnit(new(inputUnit, value)) ||
                !workspace.TryRegisterOutputUnit(new(new Unit((inputUnit, 1)), value)))
                return false;
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
            this IWorkspace<T> workspace,
            string name,
            Unit baseUnits,
            bool atto = false, bool femto = false, bool pico = false, bool nano = false, bool micro = false,
            bool milli = false, bool centi = false, bool kilo = false, bool mega = false, bool giga = false,
            bool tera = false, bool peta = false) where T : struct, IFormattable
        {
            if (atto)
            {
                if (!workspace.TryRegisterInputOutputUnit($"a{name}", new Quantity<string>("1e-18", baseUnits)))
                    return false;
            }
            if (femto)
            {
                if (!workspace.TryRegisterInputOutputUnit($"f{name}", new Quantity<string>("1e-15", baseUnits)))
                    return false;
            }
            if (pico)
            {
                if (!workspace.TryRegisterInputOutputUnit($"p{name}", new Quantity<string>("1e-12", baseUnits)))
                    return false;
            }
            if (nano)
            {
                if (!workspace.TryRegisterInputOutputUnit($"n{name}", new Quantity<string>("1e-9", baseUnits)))
                    return false;
            }
            if (micro)
            {
                if (!workspace.TryRegisterInputOutputUnit($"u{name}", new Quantity<string>("1e-6", baseUnits)))
                    return false;
            }
            if (milli)
            {
                if (!workspace.TryRegisterInputOutputUnit($"m{name}", new Quantity<string>("1e-3", baseUnits)))
                    return false;
            }
            if (centi)
            {
                if (!workspace.TryRegisterInputOutputUnit($"c{name}", new Quantity<string>("1e-2", baseUnits)))
                    return false;
            }
            if (!workspace.TryRegisterInputOutputUnit(name, new Quantity<string>("1", baseUnits)))
                return false;
            if (kilo)
            {
                if (!workspace.TryRegisterInputOutputUnit($"k{name}", new Quantity<string>("1e3", baseUnits)))
                    return false;
            }
            if (mega)
            {
                if (!workspace.TryRegisterInputOutputUnit($"M{name}", new Quantity<string>("1e6", baseUnits)))
                    return false;
            }
            if (giga)
            {
                if (!workspace.TryRegisterInputOutputUnit($"G{name}", new Quantity<string>("1e9", baseUnits)))
                    return false;
            }
            if (tera)
            {
                if (!workspace.TryRegisterInputOutputUnit($"T{name}", new Quantity<string>("1e12", baseUnits)))
                    return false;
            }
            if (peta)
            {
                if (!workspace.TryRegisterInputOutputUnit($"P{name}", new Quantity<string>("1e15", baseUnits)))
                    return false;
            }
            return true;
        }

        /// <summary>
        /// Registers a derived unit with modifiers in one of the dimensions in the base units.
        /// </summary>
        /// <param name="workspace">The workspace.</param>
        /// <param name="outputUnit">The output unit.</param>
        /// <param name="value">The output unit in base units.<paramref name="dimension"/>.</param>
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
                => new Unit(outputUnit.Dimension.Select(p => (p.Key == dimension ? $"{prefix}{p.Key}" : p.Key, p.Value)).ToArray());
            if (atto)
            {
                if (!workspace.TryRegisterOutputUnit(new(GetUnit("a"), new Quantity<string>("1e-18", baseUnits))))
                    return false;
            }
            if (femto)
            {
                if (!workspace.TryRegisterOutputUnit(new(GetUnit("f"), new Quantity<string>("1e-15", baseUnits))))
                    return false;
            }
            if (pico)
            {
                if (!workspace.TryRegisterOutputUnit(new(GetUnit("p"), new Quantity<string>("1e-12", baseUnits))))
                    return false;
            }
            if (nano)
            {
                if (!workspace.TryRegisterOutputUnit(new(GetUnit("n"), new Quantity<string>("1e-9", baseUnits))))
                    return false;
            }
            if (micro)
            {
                if (!workspace.TryRegisterOutputUnit(new(GetUnit("u"), new Quantity<string>("1e-6", baseUnits))))
                    return false;
            }
            if (milli)
            {
                if (!workspace.TryRegisterOutputUnit(new(GetUnit("m"), new Quantity<string>("1e-3", baseUnits))))
                    return false;
            }
            if (centi)
            {
                if (!workspace.TryRegisterOutputUnit(new(GetUnit("c"), new Quantity<string>("1e-2", baseUnits))))
                    return false;
            }
            if (!workspace.TryRegisterOutputUnit(new(outputUnit, new Quantity<string>("1", baseUnits))))
                return false;
            if (kilo)
            {
                if (!workspace.TryRegisterOutputUnit(new(GetUnit("k"), new Quantity<string>("1e3", baseUnits))))
                    return false;
            }
            if (mega)
            {
                if (!workspace.TryRegisterOutputUnit(new(GetUnit("M"), new Quantity<string>("1e6", baseUnits))))
                    return false;
            }
            if (giga)
            {
                if (!workspace.TryRegisterOutputUnit(new(GetUnit("G"), new Quantity<string>("1e9", baseUnits))))
                    return false;
            }
            if (tera)
            {
                if (!workspace.TryRegisterOutputUnit(new(GetUnit("T"), new Quantity<string>("1e12", baseUnits))))
                    return false;
            }
            if (peta)
            {
                if (!workspace.TryRegisterOutputUnit(new(GetUnit("P"), new Quantity<string>("1e15", baseUnits))))
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
                new(new Unit(("km", 1), ("hour", -1)),
                new("0.2777777777777777777777777", new((Unit.Meter, 1), (Unit.Second, -1)))));

            // Mass
            workspace.TryRegisterInputOutputUnit("ng", new Quantity<string>("1e-12", Unit.UnitKilogram));
            workspace.TryRegisterInputOutputUnit("ug", new Quantity<string>("1e-9", Unit.UnitKilogram));
            workspace.TryRegisterInputOutputUnit("mg", new Quantity<string>("1e-6", Unit.UnitKilogram));
            workspace.TryRegisterInputOutputUnit("g", new Quantity<string>("1e-3", Unit.UnitKilogram));
            workspace.TryRegisterInputOutputUnit("kg", new Quantity<string>("1", Unit.UnitKilogram));
            workspace.TryRegisterInputOutputUnit("ton", new Quantity<string>("1000", Unit.UnitKilogram));

            // Time
            workspace.TryRegisterModifierInputOutputUnits(Unit.Second, Unit.UnitSeconds,
                femto: true, pico: true, nano: true, micro: true);
            workspace.TryRegisterInputOutputUnit("min", new Quantity<string>("60", Unit.UnitSeconds));
            workspace.TryRegisterInputOutputUnit("hour", new Quantity<string>("3600", Unit.UnitSeconds));
            workspace.TryRegisterInputOutputUnit("day", new Quantity<string>("86400", Unit.UnitSeconds));

            // Ampere
            workspace.TryRegisterModifierInputOutputUnits(Unit.Ampere, Unit.UnitAmperes,
                pico: true, nano: true, micro: true, milli: true, kilo: true);

            // Kelvin
            workspace.TryRegisterModifierInputOutputUnits(Unit.Kelvin, Unit.UnitKelvin,
                milli: true);

            // Candela
            workspace.TryRegisterModifierInputOutputUnits(Unit.Candela, Unit.UnitCandela);

            // Angle
            workspace.TryRegisterInputUnit(new("rad", new Quantity<string>("1", Unit.UnitRadian)));
            workspace.TryRegisterInputUnit(new("deg", new Quantity<string>("17.4532925199432957692369076848861271344287188854173e-3", Unit.UnitRadian)));
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
                new(new(("C", 1), (Unit.Meter, 1)), new Quantity<string>("1",
                new((Unit.Ampere, 1), (Unit.Second, 1), (Unit.Meter, 1)))));

            // Coulomb per meter - charge density
            workspace.TryRegisterOutputUnit(
                new(new(("C", 1), (Unit.Meter, -1)), new Quantity<string>("1",
                new((Unit.Ampere, 1), (Unit.Second, 1), (Unit.Meter, -1)))));

            // Coulomb per square meter - charge density
            workspace.TryRegisterOutputUnit(
                new(new(("C", 1), (Unit.Meter, -2)), new Quantity<string>("1",
                new((Unit.Ampere, 1), (Unit.Second, 1), (Unit.Meter, -2)))));

            // Coulomb per cubic meter - charge density
            workspace.TryRegisterOutputUnit(
                new(new(("C", 1), (Unit.Meter, -3)), new Quantity<string>("1",
                new((Unit.Ampere, 1), (Unit.Second, 1), (Unit.Meter, -3)))));

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
            workspace.TryRegisterInputOutputUnit("bit", new Quantity<string>("1", b));
            workspace.TryRegisterInputOutputUnit("B", new Quantity<string>("8", b));
            workspace.TryRegisterInputOutputUnit("kB", new Quantity<string>("8192", b));
            workspace.TryRegisterInputOutputUnit("MB", new Quantity<string>("8388608", b));
            workspace.TryRegisterInputOutputUnit("GB", new Quantity<string>("8589934592", b));
            workspace.TryRegisterInputOutputUnit("TB", new Quantity<string>("8796093022208", b));
            workspace.TryRegisterInputOutputUnit("PB", new Quantity<string>("9007199254740992", b));

            // Bits per second
            var bps = new Unit(("bit", 1), (Unit.Second, -1));
            workspace.TryRegisterOutputUnit(new(bps, new Quantity<string>("1", bps)));
            workspace.TryRegisterOutputUnit(new(new(("kbps", 1)), new Quantity<string>("1000", bps)));
            workspace.TryRegisterOutputUnit(new(new(("Mbps", 1)), new Quantity<string>("1000000", bps)));
            workspace.TryRegisterOutputUnit(new(new(("Gbps", 1)), new Quantity<string>("1000000000", bps)));
            workspace.TryRegisterOutputUnit(new(new(("Tbps", 1)), new Quantity<string>("1000000000000", bps)));
        }
    }
}
