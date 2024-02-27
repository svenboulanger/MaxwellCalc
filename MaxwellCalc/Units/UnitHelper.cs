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
        /// Register SI units.
        /// </summary>
        /// <param name="workspace">The workspace.</param>
        public static void RegisterSIUnits(IWorkspace<double> workspace)
        {
            workspace.TryRegisterInputUnit("meter", 1.0, Unit.UnitMeter);
            workspace.TryRegisterInputUnit("kilogram", 1.0, Unit.UnitKilogram);
            workspace.TryRegisterInputUnit("second", 1.0, Unit.UnitSeconds);
            workspace.TryRegisterInputUnit("mol", 1.0, Unit.UnitMole);
            workspace.TryRegisterInputUnit("ampere", 1.0, Unit.UnitAmperes);
            workspace.TryRegisterInputUnit("kelvin", 1.0, Unit.UnitKelvin);
            workspace.TryRegisterInputUnit("candela", 1.0, Unit.UnitCandela);
            workspace.TryRegisterInputUnit("radian", 1.0, Unit.UnitRadian);
            workspace.TryRegisterInputUnit("degree", Math.PI / 180.0, Unit.UnitRadian);
        }

        /// <summary>
        /// Registers a unit for both input and output, such that
        /// <paramref name="key"/> = <paramref name="modifier"/> * <paramref name="value"/>.
        /// </summary>
        /// <param name="workspace">The workspace.</param>
        /// <param name="key">The base unit that is used behind the scenes.</param>
        /// <param name="modifier">The modifier.</param>
        /// <param name="value">The unit that can be used for formatting.</param>
        public static void RegisterInputOutputUnit(IWorkspace<double> workspace,
            Unit key, double modifier, Unit value)
        {
            if (value.Dimension.Count != 1)
                throw new ArgumentException("Expected a unit with one dimension", nameof(value));
            string name = value.Dimension.First().Key;
            workspace.TryRegisterInputUnit(name, 1.0 / modifier, key);
            workspace.TryRegisterDerivedUnit(key, modifier, value);
        }

        /// <summary>
        /// Registers units with modifiers.
        /// </summary>
        /// <param name="workspace">The workspace.</param>
        /// <param name="unit">The unit.</param>
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
        public static void RegisterModifierUnits(IWorkspace<double> workspace,
            string name,
            double modifier,
            Unit unit,
            bool includeOutputs = true,
            bool atto = true,
            bool femto = true,
            bool pico = true,
            bool nano = true,
            bool micro = true,
            bool milli = true,
            bool centi = false,
            bool kilo = true,
            bool mega = true,
            bool giga = true,
            bool tera = true,
            bool peta = false)
        {
            void Add(string n, double m, Unit u)
            {
                workspace.TryRegisterInputUnit(n, m, u);
                if (includeOutputs)
                    workspace.TryRegisterDerivedUnit(u, 1.0 / m, new Unit((n, 1)));
            }

            if (atto)
                Add($"a{name}", 1e-18 * modifier, unit);
            if (femto)
                Add($"f{name}", 1e-15 * modifier, unit);
            if (pico)
                Add($"p{name}", 1e-12 * modifier, unit);
            if (nano)
                Add($"n{name}", 1e-9 * modifier, unit);
            if (micro)
                Add($"u{name}", 1e-6 * modifier, unit);
            if (milli)
                Add($"m{name}", 1e-3 * modifier, unit);
            if (centi)
                Add($"c{name}", 1e-2 * modifier, unit);
            workspace.TryRegisterInputUnit(name, modifier, unit);
            workspace.TryRegisterDerivedUnit(unit, 1.0 / modifier, new Unit((name, 1)));
            if (kilo)
                Add($"k{name}", 1e3 * modifier, unit);
            if (mega)
                Add($"M{name}", 1e6 * modifier, unit);
            if (giga)
                Add($"G{name}", 1e9 * modifier, unit);
            if (tera)
                Add($"T{name}", 1e12 * modifier, unit);
            if (peta)
                Add($"P{name}", 1e15 * modifier, unit);
        }

        /// <summary>
        /// Registers a derived unit with modifiers in one of the units.
        /// </summary>
        /// <param name="workspace">The workspace.</param>
        /// <param name="key">The key base unit.</param>
        /// <param name="value">The value, of which one dimension is <paramref name="dimension"/>.</param>
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
        public static void RegisterModifierDerivedUnits(IWorkspace<double> workspace,
            Unit key, double modifier, Unit value, string dimension,
            bool atto = true,
            bool femto = true,
            bool pico = true,
            bool nano = true,
            bool micro = true,
            bool milli = true,
            bool centi = false,
            bool kilo = true,
            bool mega = true,
            bool giga = true,
            bool tera = true,
            bool peta = false)
        {
            if (value.Dimension is null)
                throw new ArgumentException("Dimension cannot be null", nameof(value));
            var f = value.Dimension[dimension];
            double power = (double)f.Numerator / f.Denominator;
            void Add(string prefix, double m)
            {
                // Create a modified unit for this modifier
                double nm = modifier * Math.Pow(m, -power);
                var nu = new Unit(value.Dimension.Select(p => (p.Key == dimension ? $"{prefix}{p.Key}" : p.Key, p.Value)).ToArray());
                workspace.TryRegisterDerivedUnit(key, nm, nu);
            }

            if (atto)
                Add("a", 1e-18);
            if (femto)
                Add("f", 1e-15);
            if (pico)
                Add("p", 1e-12);
            if (nano)
                Add("n", 1e-9);
            if (micro)
                Add("u", 1e-6);
            if (milli)
                Add("m", 1e-3);
            if (centi)
                Add("c", 1e-2);
            workspace.TryRegisterDerivedUnit(key, modifier, value);
            if (kilo)
                Add("k", 1e3);
            if (mega)
                Add("M", 1e6);
            if (giga)
                Add("G", 1e9);
            if (tera)
                Add("T", 1e12);
            if (peta)
                Add("P", 1e15);
        }

        /// <summary>
        /// Registers common units.
        /// </summary>
        /// <param name="workspace">The workspace.</param>
        public static void RegisterCommonUnits(IWorkspace<double> workspace)
        {
            // Length
            RegisterModifierUnits(workspace, "m", 1.0, Unit.UnitMeter,
                centi: true, mega: false, giga: false, peta: false);

            // Speed
            RegisterModifierDerivedUnits(workspace,
                new Unit((Unit.Meter, 1), (Unit.Second, -1)), 1.0, new Unit((Unit.Meter, 1), (Unit.Second, -1)), Unit.Meter,
                centi: true, mega: false, giga: false, peta: false);

            // Mass
            workspace.TryRegisterInputUnit("ng", 1e-12, Unit.UnitKilogram);
            workspace.TryRegisterInputUnit("ug", 1e-9, Unit.UnitKilogram);
            workspace.TryRegisterInputUnit("mg", 1e-6, Unit.UnitKilogram);
            workspace.TryRegisterInputUnit("g", 1e-3, Unit.UnitKilogram);
            workspace.TryRegisterInputUnit("kg", 1.0, Unit.UnitKilogram);
            workspace.TryRegisterInputUnit("ton", 1e3, Unit.UnitKilogram);

            // Time
            RegisterModifierUnits(workspace, "s", 1.0, Unit.UnitSeconds,
                atto: false, kilo: false, mega: false, giga: false, tera: false, peta: false);
            workspace.TryRegisterInputUnit("min", 60.0, Unit.UnitSeconds);
            workspace.TryRegisterDerivedUnit(Unit.UnitSeconds, 1.0 / 60.0, new Unit(("min", 1)));
            workspace.TryRegisterInputUnit("hour", 3600.0, Unit.UnitSeconds);
            workspace.TryRegisterDerivedUnit(Unit.UnitSeconds, 1.0 / 3600.0, new Unit(("hour", 1)));
            workspace.TryRegisterInputUnit("day", 24.0 * 3600.0, Unit.UnitSeconds);
            workspace.TryRegisterDerivedUnit(Unit.UnitSeconds, 1.0 / 24.0 / 3600.0, new Unit(("day", 1)));

            // Ampere
            RegisterModifierUnits(workspace, "A", 1.0, Unit.UnitAmperes);

            // Kelvin
            workspace.TryRegisterInputUnit("mK", 1e-3, Unit.UnitKelvin);
            workspace.TryRegisterInputUnit("K", 1.0, Unit.UnitKelvin);

            // Candela
            workspace.TryRegisterInputUnit("cd", 1.0, Unit.UnitCandela);

            // Angle
            workspace.TryRegisterInputUnit("rad", 1.0, Unit.UnitRadian);
            workspace.TryRegisterInputUnit("deg", Math.PI / 180.0, Unit.UnitRadian);
        }

        /// <summary>
        /// Registers common units used by electrical/electronics engineers.
        /// </summary>
        /// <param name="workspace">The workspace.</param>
        public static void RegisterCommonElectronicsUnits(IWorkspace<double> workspace)
        {
            // Coulomb
            RegisterModifierUnits(workspace, "C",
                1.0, new Unit((Unit.Ampere, 1), (Unit.Second, 1)));

            // Coulomb meter - electric dipole moment
            workspace.TryRegisterDerivedUnit(new Unit(
                (Unit.Ampere, 1), (Unit.Second, 1), (Unit.Meter, 1)),
                1.0, new Unit(("C", 1), (Unit.Meter, 1)));

            // Coulomb per meter - charge density
            workspace.TryRegisterDerivedUnit(new Unit((Unit.Ampere, 1), (Unit.Second, 1), (Unit.Meter, -1)), 1.0, new Unit(("C", 1), (Unit.Meter, -1)));

            // Coulomb per square meter - charge density
            workspace.TryRegisterDerivedUnit(new Unit((Unit.Ampere, 1), (Unit.Second, 1), (Unit.Meter, -2)), 1.0, new Unit(("C", 1), (Unit.Meter, -2)));

            // Coulomb per cubic meter - charge density
            workspace.TryRegisterDerivedUnit(new Unit((Unit.Ampere, 1), (Unit.Second, 1), (Unit.Meter, -3)), 1.0, new Unit(("C", 1), (Unit.Meter, -3)));

            // Volts
            RegisterModifierUnits(workspace, "V",
                1.0, new Unit(
                    (Unit.Kilogram, 1),
                    (Unit.Meter, 2),
                    (Unit.Second, -3),
                    (Unit.Ampere, -1)));

            // Volts per meter - electric field
            RegisterModifierDerivedUnits(workspace,
                new Unit(
                    (Unit.Kilogram, 1),
                    (Unit.Meter, 1),
                    (Unit.Second, -3),
                    (Unit.Ampere, -1)),
                1.0, new Unit(("V", 1), (Unit.Meter, -1)), "V");

            // Volts per square meter - electric field gradient
            workspace.TryRegisterDerivedUnit(new Unit(
                    (Unit.Kilogram, 1),
                    (Unit.Meter, 1),
                    (Unit.Second, -3),
                    (Unit.Ampere, -1)), 1.0, new Unit(("V", 1), (Unit.Meter, -2)));
            
            // Ampere
            RegisterModifierUnits(workspace, "A",
                1.0, Unit.UnitAmperes);
            
            // Watts
            RegisterModifierUnits(workspace, "W",
                1.0, new Unit(
                    (Unit.Kilogram, 1),
                    (Unit.Meter, 2),
                    (Unit.Second, -3)));
            
            // Joules
            RegisterModifierUnits(workspace, "J",
                1.0, new Unit(
                    (Unit.Kilogram, 1),
                    (Unit.Meter, 2),
                    (Unit.Second, -2)));

            // Joules seconds (Planck constant)
            workspace.TryRegisterDerivedUnit(
                new Unit(
                    (Unit.Kilogram, 1),
                    (Unit.Meter, 2),
                    (Unit.Second, -1)), 1.0, new Unit(("J", 1), (Unit.Second, 1)));

            // Farad
            RegisterModifierUnits(workspace, "F",
                1.0, new Unit(
                    (Unit.Kilogram, -1),
                    (Unit.Meter, -2),
                    (Unit.Second, 4),
                    (Unit.Ampere, 2)));

            // Farad per meter
            RegisterModifierDerivedUnits(workspace,
                new Unit(
                    (Unit.Kilogram, -1),
                    (Unit.Meter, -3),
                    (Unit.Second, 4),
                    (Unit.Ampere, 2)), 1.0, new Unit(("F", 1), (Unit.Meter, -1)), Unit.Meter, centi: true);

            // Farad per square meter
            RegisterModifierDerivedUnits(workspace,
                new Unit(
                    (Unit.Kilogram, -1),
                    (Unit.Meter, -4),
                    (Unit.Second, 4),
                    (Unit.Ampere, 2)), 1.0, new Unit(("F", 1), (Unit.Meter, -2)), Unit.Meter, centi: true);

            // Henry
            RegisterModifierUnits(workspace, "H",
                1.0, new Unit(
                    (Unit.Kilogram, 1),
                    (Unit.Meter, 2),
                    (Unit.Second, -2),
                    (Unit.Ampere, -2)));

            // Weber
            RegisterModifierDerivedUnits(workspace, new Unit(
                    (Unit.Kilogram, 1),
                    (Unit.Meter, 2),
                    (Unit.Second, -2),
                    (Unit.Ampere, -1)),
                    1.0, new Unit(("Wb", 1)), "Wb");
            
            // Ohm = A-2 kg m2 s-3
            RegisterModifierUnits(workspace, "ohm",
                1.0, new Unit(
                    (Unit.Kilogram, 1),
                    (Unit.Meter, 2),
                    (Unit.Second, -3),
                    (Unit.Ampere, -2)));

            // Siemens
            RegisterModifierUnits(workspace, "S",
                1.0, new Unit(
                    (Unit.Kilogram, -1),
                    (Unit.Meter, -2),
                    (Unit.Second, 3),
                    (Unit.Ampere, 2)));

            // Hertz
            RegisterModifierUnits(workspace, "Hz",
                1.0, new Unit((Unit.Second, -1)));

            // Bits
            var b = new Unit(("bit", 1));
            double m = 1024.0;
            workspace.TryRegisterInputUnit("b", 1.0, b);
            workspace.TryRegisterInputUnit("B", 8.0, b);
            workspace.TryRegisterInputUnit("kB", 8.0 * m, b);
            workspace.TryRegisterInputUnit("MB", 8.0 * m * m, b);
            workspace.TryRegisterInputUnit("GB", 8.0 * m * m * m, b);
            workspace.TryRegisterInputUnit("TB", 8.0 * m * m * m * m, b);
            workspace.TryRegisterInputUnit("PB", 8.0 * m * m * m * m * m, b);
            workspace.TryRegisterDerivedUnit(b, 1.0 / 8.0, new Unit(("B", 1)));
            workspace.TryRegisterDerivedUnit(b, 1.0 / 8.0 / m, new Unit(("kB", 1)));
            workspace.TryRegisterDerivedUnit(b, 1.0 / 8.0 / m / m, new Unit(("MB", 1)));
            workspace.TryRegisterDerivedUnit(b, 1.0 / 8.0 / m / m / m, new Unit(("GB", 1)));
            workspace.TryRegisterDerivedUnit(b, 1.0 / 8.0 / m / m / m / m, new Unit(("TB", 1)));
            workspace.TryRegisterDerivedUnit(b, 1.0 / 8.0 / m / m / m / m / m, new Unit(("PB", 1)));

            // Bits per second
            var bps = new Unit(("bit", 1), (Unit.Second, -1));
            RegisterInputOutputUnit(workspace, bps, 1, new Unit(("bps", 1)));
            RegisterInputOutputUnit(workspace, bps, 1.0 / m, new Unit(("kbps", 1)));
            RegisterInputOutputUnit(workspace, bps, 1.0 / m / m, new Unit(("Mbps", 1)));
            RegisterInputOutputUnit(workspace, bps, 1.0 / m / m / m, new Unit(("Gbps", 1)));
            RegisterInputOutputUnit(workspace, bps, 1.0 / m / m / m / m, new Unit(("Tbps", 1)));
        }
    }
}
