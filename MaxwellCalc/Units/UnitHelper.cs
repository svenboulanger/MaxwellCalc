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
            workspace.TryRegisterInputUnit(name, new Quantity<double>(1.0 / modifier, key));
            workspace.TryRegisterOutputUnit(key, new Quantity<double>(modifier, value));
        }

        /// <summary>
        /// Registers a unit with modifiers for both input and output.
        /// </summary>
        /// <param name="workspace">The workspace.</param>
        /// <param name="name">The name of the unit.</param>
        /// <param name="modifier">The modifier.</param>
        /// <param name="unit">The unit using base units.</param>
        /// <param name="includeOutputs">If <c>true</c>, the modifier units are also added for output units.</param>
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
        public static void RegisterModifierInputOutputUnits(IWorkspace<double> workspace,
            string name,
            Quantity<double> baseUnits,
            bool includeOutputs = true,
            bool atto = false, bool femto = false, bool pico = false, bool nano = false, bool micro = false,
            bool milli = false, bool centi = false, bool kilo = false, bool mega = false, bool giga = false,
            bool tera = false, bool peta = false)
        {
            double modifier = baseUnits.Scalar;
            var unit = baseUnits.Unit;
            void Add(string n, double m, Unit u)
            {
                workspace.TryRegisterInputUnit(n, new Quantity<double>(m, u));
                if (includeOutputs)
                    workspace.TryRegisterOutputUnit(u, new Quantity<double>(1.0 / m, new Unit((n, 1))));
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
            workspace.TryRegisterInputUnit(name, new Quantity<double>(modifier, unit));
            workspace.TryRegisterOutputUnit(unit, new Quantity<double>(1.0 / modifier, new Unit((name, 1))));
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
        public static void RegisterModifierOutputUnits(IWorkspace<double> workspace,
            Unit key, double modifier, Unit value, string dimension,
            bool atto = false, bool femto = false, bool pico = false, bool nano = false,
            bool micro = false, bool milli = false, bool centi = false, bool kilo = false,
            bool mega = false, bool giga = false, bool tera = false, bool peta = false)
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
                workspace.TryRegisterOutputUnit(key, new Quantity<double>(nm, nu));
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
            workspace.TryRegisterOutputUnit(key, new Quantity<double>(modifier, value));
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
            RegisterModifierInputOutputUnits(workspace, "m", new(1.0, Unit.UnitMeter),
                nano: true, micro: true, milli: true, centi: true, kilo: true);

            // Speed
            RegisterModifierOutputUnits(workspace,
                new Unit((Unit.Meter, 1), (Unit.Second, -1)), 1.0, new Unit((Unit.Meter, 1), (Unit.Second, -1)), Unit.Meter);
            workspace.TryRegisterOutputUnit(new Unit((Unit.Meter, 1), (Unit.Second, -1)), new Quantity<double>(1.0 / 1000.0 * 3600.0, new Unit(("km", 1), ("hour", -1))));

            // Mass
            workspace.TryRegisterInputUnit("ng", new Quantity<double>(1e-12, Unit.UnitKilogram));
            workspace.TryRegisterInputUnit("ug", new Quantity<double>(1e-9, Unit.UnitKilogram));
            workspace.TryRegisterInputUnit("mg", new Quantity<double>(1e-6, Unit.UnitKilogram));
            workspace.TryRegisterInputUnit("g", new Quantity<double>(1e-3, Unit.UnitKilogram));
            workspace.TryRegisterInputUnit("kg", new Quantity<double>(1.0, Unit.UnitKilogram));
            workspace.TryRegisterInputUnit("ton", new Quantity<double>(1e3, Unit.UnitKilogram));

            // Time
            RegisterModifierInputOutputUnits(workspace, "s", new(1.0, Unit.UnitSeconds),
                femto: true, pico: true, nano: true, micro: true);
            workspace.TryRegisterInputUnit("min", new Quantity<double>(60.0, Unit.UnitSeconds));
            workspace.TryRegisterOutputUnit(Unit.UnitSeconds, new Quantity<double>(1.0 / 60.0, new Unit(("min", 1))));
            workspace.TryRegisterInputUnit("hour", new Quantity<double>(3600.0, Unit.UnitSeconds));
            workspace.TryRegisterOutputUnit(Unit.UnitSeconds, new Quantity<double>(1.0 / 3600.0, new Unit(("hour", 1))));
            workspace.TryRegisterInputUnit("day", new Quantity<double>(24.0 * 3600.0, Unit.UnitSeconds));
            workspace.TryRegisterOutputUnit(Unit.UnitSeconds, new Quantity<double>(1.0 / 24.0 / 3600.0, new Unit(("day", 1))));

            // Ampere
            RegisterModifierInputOutputUnits(workspace, "A", new(1.0, Unit.UnitAmperes),
                pico: true, nano: true, milli: true, kilo: true);

            // Kelvin
            RegisterModifierInputOutputUnits(workspace, "K", new(1.0, Unit.UnitKelvin),
                milli: true);

            // Candela
            workspace.TryRegisterInputUnit("cd", new Quantity<double>(1.0, Unit.UnitCandela));

            // Angle
            workspace.TryRegisterInputUnit("rad", new Quantity<double>(1.0, Unit.UnitRadian));
            workspace.TryRegisterInputUnit("deg", new Quantity<double>(Math.PI / 180.0, Unit.UnitRadian));
        }

        /// <summary>
        /// Registers common units used by electrical/electronics engineers.
        /// </summary>
        /// <param name="workspace">The workspace.</param>
        public static void RegisterCommonElectronicsUnits(IWorkspace<double> workspace)
        {
            // Coulomb
            RegisterModifierInputOutputUnits(workspace, "C",
                new(1.0, new Unit((Unit.Ampere, 1), (Unit.Second, 1))));

            // Coulomb meter - electric dipole moment
            workspace.TryRegisterOutputUnit(new Unit(
                (Unit.Ampere, 1), (Unit.Second, 1), (Unit.Meter, 1)),
                new Quantity<double>(1.0, new Unit(("C", 1), (Unit.Meter, 1))));

            // Coulomb per meter - charge density
            workspace.TryRegisterOutputUnit(new Unit((Unit.Ampere, 1), (Unit.Second, 1), (Unit.Meter, -1)),
                new Quantity<double>(1.0, new Unit(("C", 1), (Unit.Meter, -1))));

            // Coulomb per square meter - charge density
            workspace.TryRegisterOutputUnit(new Unit((Unit.Ampere, 1), (Unit.Second, 1), (Unit.Meter, -2)),
                new Quantity<double>(1.0, new Unit(("C", 1), (Unit.Meter, -2))));

            // Coulomb per cubic meter - charge density
            workspace.TryRegisterOutputUnit(new Unit((Unit.Ampere, 1), (Unit.Second, 1), (Unit.Meter, -3)),
                new Quantity<double>(1.0, new Unit(("C", 1), (Unit.Meter, -3))));

            // Volts
            RegisterModifierInputOutputUnits(workspace, "V",
                new(1.0, new Unit(
                    (Unit.Kilogram, 1),
                    (Unit.Meter, 2),
                    (Unit.Second, -3),
                    (Unit.Ampere, -1))),
                nano: true, micro: true, milli: true, kilo: true, mega: true);

            // Volts per meter - electric field
            RegisterModifierOutputUnits(workspace,
                new Unit(
                    (Unit.Kilogram, 1),
                    (Unit.Meter, 1),
                    (Unit.Second, -3),
                    (Unit.Ampere, -1)),
                1.0, new Unit(("V", 1), (Unit.Meter, -1)), "V");

            // Volts per square meter - electric field gradient
            workspace.TryRegisterOutputUnit(new Unit(
                    (Unit.Kilogram, 1),
                    (Unit.Meter, 1),
                    (Unit.Second, -3),
                    (Unit.Ampere, -1)), 
                    new Quantity<double>(1.0, new Unit(("V", 1), (Unit.Meter, -2))));
            
            // Ampere
            RegisterModifierInputOutputUnits(workspace, "A",
                new(1.0, Unit.UnitAmperes));
            
            // Watts
            RegisterModifierInputOutputUnits(workspace, "W",
                new(1.0, new Unit(
                    (Unit.Kilogram, 1),
                    (Unit.Meter, 2),
                    (Unit.Second, -3))));
            
            // Joules
            RegisterModifierInputOutputUnits(workspace, "J",
                new(1.0, new Unit(
                    (Unit.Kilogram, 1),
                    (Unit.Meter, 2),
                    (Unit.Second, -2))));

            // Joules seconds (Planck constant)
            workspace.TryRegisterOutputUnit(
                new Unit(
                    (Unit.Kilogram, 1),
                    (Unit.Meter, 2),
                    (Unit.Second, -1)), 
                new Quantity<double>(1.0, new Unit(("J", 1), (Unit.Second, 1))));

            // Farad
            RegisterModifierInputOutputUnits(workspace, "F",
                new(1.0, new Unit(
                    (Unit.Kilogram, -1),
                    (Unit.Meter, -2),
                    (Unit.Second, 4),
                    (Unit.Ampere, 2))));

            // Farad per meter
            RegisterModifierOutputUnits(workspace,
                new Unit(
                    (Unit.Kilogram, -1),
                    (Unit.Meter, -3),
                    (Unit.Second, 4),
                    (Unit.Ampere, 2)), 1.0, new Unit(("F", 1), (Unit.Meter, -1)), Unit.Meter, centi: true);

            // Farad per square meter
            RegisterModifierOutputUnits(workspace,
                new Unit(
                    (Unit.Kilogram, -1),
                    (Unit.Meter, -4),
                    (Unit.Second, 4),
                    (Unit.Ampere, 2)), 1.0, new Unit(("F", 1), (Unit.Meter, -2)), Unit.Meter, centi: true);

            // Henry
            RegisterModifierInputOutputUnits(workspace, "H",
                new(1.0, new Unit(
                    (Unit.Kilogram, 1),
                    (Unit.Meter, 2),
                    (Unit.Second, -2),
                    (Unit.Ampere, -2))));

            // Weber
            RegisterModifierOutputUnits(workspace, new Unit(
                    (Unit.Kilogram, 1),
                    (Unit.Meter, 2),
                    (Unit.Second, -2),
                    (Unit.Ampere, -1)),
                    1.0, new Unit(("Wb", 1)), "Wb");
            
            // Ohm = A-2 kg m2 s-3
            RegisterModifierInputOutputUnits(workspace, "ohm",
                new(1.0, new Unit(
                    (Unit.Kilogram, 1),
                    (Unit.Meter, 2),
                    (Unit.Second, -3),
                    (Unit.Ampere, -2))),
                micro: true, milli: true, kilo: true, mega: true);

            // Siemens
            RegisterModifierInputOutputUnits(workspace, "S",
                new(1.0, new Unit(
                    (Unit.Kilogram, -1),
                    (Unit.Meter, -2),
                    (Unit.Second, 3),
                    (Unit.Ampere, 2))),
                micro: true, milli: true, kilo: true, mega: true);

            // Hertz
            RegisterModifierInputOutputUnits(workspace, "Hz",
                new(1.0, new Unit((Unit.Second, -1))),
                milli: true, kilo: true, mega: true, giga: true, tera: true);

            // Bits
            var b = new Unit(("bit", 1));
            double m = 1024.0;
            workspace.TryRegisterInputUnit("b", new Quantity<double>(1.0, b));
            workspace.TryRegisterInputUnit("B", new Quantity<double>(8.0, b));
            workspace.TryRegisterInputUnit("kB", new Quantity<double>(8.0 * m, b));
            workspace.TryRegisterInputUnit("MB", new Quantity<double>(8.0 * m * m, b));
            workspace.TryRegisterInputUnit("GB", new Quantity<double>(8.0 * m * m * m, b));
            workspace.TryRegisterInputUnit("TB", new Quantity<double>(8.0 * m * m * m * m, b));
            workspace.TryRegisterInputUnit("PB", new Quantity<double>(8.0 * m * m * m * m * m, b));
            workspace.TryRegisterOutputUnit(b, new Quantity<double>(1.0 / 8.0, new Unit(("B", 1))));
            workspace.TryRegisterOutputUnit(b, new Quantity<double>(1.0 / 8.0 / m, new Unit(("kB", 1))));
            workspace.TryRegisterOutputUnit(b, new Quantity<double>(1.0 / 8.0 / m / m, new Unit(("MB", 1))));
            workspace.TryRegisterOutputUnit(b, new Quantity<double>(1.0 / 8.0 / m / m / m, new Unit(("GB", 1))));
            workspace.TryRegisterOutputUnit(b, new Quantity<double>(1.0 / 8.0 / m / m / m / m, new Unit(("TB", 1))));
            workspace.TryRegisterOutputUnit(b, new Quantity<double>(1.0 / 8.0 / m / m / m / m / m, new Unit(("PB", 1))));

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
