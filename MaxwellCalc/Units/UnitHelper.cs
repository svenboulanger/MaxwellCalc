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
        public static void RegisterSIUnits(IWorkspace workspace)
        {
            workspace.TryRegisterInputUnit("meter", new Unit(1.0, BaseUnit.UnitMeter));
            workspace.TryRegisterInputUnit("kilogram", new Unit(1.0, BaseUnit.UnitKilogram));
            workspace.TryRegisterInputUnit("second", new Unit(1.0, BaseUnit.UnitSeconds));
            workspace.TryRegisterInputUnit("mol", new Unit(1.0, BaseUnit.UnitMole));
            workspace.TryRegisterInputUnit("ampere", new Unit(1.0, BaseUnit.UnitAmperes));
            workspace.TryRegisterInputUnit("kelvin", new Unit(1.0, BaseUnit.UnitKelvin));
            workspace.TryRegisterInputUnit("candela", new Unit(1.0, BaseUnit.UnitCandela));
            workspace.TryRegisterInputUnit("radian", new Unit(1.0, BaseUnit.UnitRadian));
            workspace.TryRegisterInputUnit("degree", new Unit(Math.PI / 180.0, BaseUnit.UnitRadian));
        }

        /// <summary>
        /// Register shorthand SI units.
        /// </summary>
        /// <param name="workspace">The workspace.</param>
        public static void RegisterShortSIUnits(IWorkspace workspace)
        {
            // Length
            RegisterModifierUnits(workspace, "m", new Unit(1.0, BaseUnit.UnitMeter));
            workspace.TryRegisterInputUnit("cm", new Unit(1e-2, BaseUnit.UnitMeter));

            // Mass
            workspace.TryRegisterInputUnit("ng", new Unit(1e-12, BaseUnit.UnitKilogram));
            workspace.TryRegisterInputUnit("ug", new Unit(1e-9, BaseUnit.UnitKilogram));
            workspace.TryRegisterInputUnit("mg", new Unit(1e-6, BaseUnit.UnitKilogram));
            workspace.TryRegisterInputUnit("g", new Unit(1e-3, BaseUnit.UnitKilogram));
            workspace.TryRegisterInputUnit("kg", new Unit(1.0, BaseUnit.UnitKilogram));
            workspace.TryRegisterInputUnit("ton", new Unit(1e3, BaseUnit.UnitKilogram));

            // Time
            RegisterModifierUnits(workspace, "s", new Unit(1.0, BaseUnit.UnitSeconds),
                atto: false, kilo: false, mega: false, giga: false, tera: false, peta: false);
            workspace.TryRegisterInputUnit("min", new Unit(60.0, BaseUnit.UnitSeconds));
            workspace.TryRegisterDerivedUnit(BaseUnit.UnitSeconds, new Unit(1.0 / 60.0, new BaseUnit(("min", 1))));
            workspace.TryRegisterInputUnit("hour", new Unit(3600.0, BaseUnit.UnitSeconds));
            workspace.TryRegisterDerivedUnit(BaseUnit.UnitSeconds, new Unit(1.0 / 3600.0, new BaseUnit(("hour", 1))));
            workspace.TryRegisterInputUnit("day", new Unit(24.0 * 3600.0, BaseUnit.UnitSeconds));
            workspace.TryRegisterDerivedUnit(BaseUnit.UnitSeconds, new Unit(1.0 / 24.0 / 3600.0, new BaseUnit(("day", 1))));

            // Ampere
            RegisterModifierUnits(workspace, "A", new Unit(1.0, BaseUnit.UnitAmperes));

            // Kelvin
            workspace.TryRegisterInputUnit("K", new Unit(1.0, BaseUnit.UnitKelvin));

            // Candela
            workspace.TryRegisterInputUnit("cd", new Unit(1.0, BaseUnit.UnitCandela));

            // Angle
            workspace.TryRegisterInputUnit("rad", new Unit(1.0, BaseUnit.UnitRadian));
            workspace.TryRegisterInputUnit("deg", new Unit(Math.PI / 180.0, BaseUnit.UnitRadian));
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
        public static void RegisterModifierUnits(IWorkspace workspace,
            string name,
            Unit unit,
            bool includeOutputs = true,
            bool atto = true,
            bool femto = true,
            bool pico = true,
            bool nano = true,
            bool micro = true,
            bool milli = true,
            bool kilo = true,
            bool mega = true,
            bool giga = true,
            bool tera = true,
            bool peta = true)
        {
            void Add(string n, Unit u)
            {
                workspace.TryRegisterInputUnit(n, u);
                if (includeOutputs)
                    workspace.TryRegisterDerivedUnit(u.BaseUnits, new Unit(1.0 / u.Modifier, new BaseUnit((n, 1))));
            }

            if (atto)
                Add($"a{name}", new Unit(1e-18 * unit.Modifier, unit.BaseUnits));
            if (femto)
                Add($"f{name}", new Unit(1e-15 * unit.Modifier, unit.BaseUnits));
            if (pico)
                Add($"p{name}", new Unit(1e-12 * unit.Modifier, unit.BaseUnits));
            if (nano)
                Add($"n{name}", new Unit(1e-9 * unit.Modifier, unit.BaseUnits));
            if (micro)
                Add($"u{name}", new Unit(1e-6 * unit.Modifier, unit.BaseUnits));
            if (milli)
                Add($"m{name}", new Unit(1e-3 * unit.Modifier, unit.BaseUnits));
            workspace.TryRegisterInputUnit(name, unit);
            workspace.TryRegisterDerivedUnit(unit.BaseUnits, new Unit(1.0, new BaseUnit((name, 1))));
            if (kilo)
                Add($"k{name}", new Unit(1e3 * unit.Modifier, unit.BaseUnits));
            if (mega)
                Add($"M{name}", new Unit(1e6 * unit.Modifier, unit.BaseUnits));
            if (giga)
                Add($"G{name}", new Unit(1e9 * unit.Modifier, unit.BaseUnits));
            if (tera)
                Add($"T{name}", new Unit(1e12 * unit.Modifier, unit.BaseUnits));
            if (peta)
                Add($"P{name}", new Unit(1e15 * unit.Modifier, unit.BaseUnits));
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
        public static void RegisterModifierDerivedUnits(IWorkspace workspace,
            BaseUnit key, Unit value, string dimension,
            bool atto = true,
            bool femto = true,
            bool pico = true,
            bool nano = true,
            bool micro = true,
            bool milli = true,
            bool kilo = true,
            bool mega = true,
            bool giga = true,
            bool tera = true,
            bool peta = true)
        {
            var f = value.BaseUnits.Dimension[dimension];
            double power = (double)f.Numerator / f.Denominator;
            void Add(string prefix, double modifier)
            {
                // Create a modified unit for this modifier
                var u = new Unit(value.Modifier * Math.Pow(modifier, -power), new BaseUnit(
                    value.BaseUnits.Dimension.Select(p => (p.Key == dimension ? $"{prefix}{p.Key}" : p.Key, p.Value)).ToArray()));
                workspace.TryRegisterDerivedUnit(key, u);
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
            workspace.TryRegisterDerivedUnit(key, value);
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
        /// Registers common units used by electrical/electronics engineers.
        /// </summary>
        /// <param name="workspace">The workspace.</param>
        public static void RegisterCommonElectricalUnits(IWorkspace workspace)
        {
            // Volts
            RegisterModifierUnits(workspace, "V",
                new Unit(1.0, new BaseUnit(
                    (BaseUnit.Kilogram, 1),
                    (BaseUnit.Meter, 2),
                    (BaseUnit.Second, -3),
                    (BaseUnit.Ampere, -1))));

            // Electric field
            RegisterModifierDerivedUnits(workspace,
                new BaseUnit(
                    (BaseUnit.Kilogram, 1),
                    (BaseUnit.Meter, 1),
                    (BaseUnit.Second, -3),
                    (BaseUnit.Ampere, -1)),
                new Unit(1.0, new BaseUnit(("V", 1), (BaseUnit.Meter, -1))), "V");
            
            // Ampere
            RegisterModifierUnits(workspace, "A",
                new Unit(1.0, BaseUnit.UnitAmperes));
            
            // Watts
            RegisterModifierUnits(workspace, "W",
                new Unit(1.0, new BaseUnit(
                    (BaseUnit.Kilogram, 1),
                    (BaseUnit.Meter, 2),
                    (BaseUnit.Second, -3))));
            
            // Joules
            RegisterModifierUnits(workspace, "J",
                new Unit(1.0, new BaseUnit(
                    (BaseUnit.Kilogram, 1),
                    (BaseUnit.Meter, 2),
                    (BaseUnit.Second, -2))));
            
            // Farad
            RegisterModifierUnits(workspace, "F",
                new Unit(1.0, new BaseUnit(
                    (BaseUnit.Kilogram, -1),
                    (BaseUnit.Meter, -2),
                    (BaseUnit.Second, 4),
                    (BaseUnit.Ampere, 2))));
            
            // Henry
            RegisterModifierUnits(workspace, "H",
                new Unit(1.0, new BaseUnit(
                    (BaseUnit.Kilogram, 1),
                    (BaseUnit.Meter, 2),
                    (BaseUnit.Second, -2),
                    (BaseUnit.Ampere, -2))));
            
            // Ohm = A-2 kg m2 s-3
            RegisterModifierUnits(workspace, "ohm",
                new Unit(1.0, new BaseUnit(
                    (BaseUnit.Kilogram, 1),
                    (BaseUnit.Meter, 2),
                    (BaseUnit.Second, -3),
                    (BaseUnit.Ampere, -2))));
        }
    }
}
