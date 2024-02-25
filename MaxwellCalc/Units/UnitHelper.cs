using MaxwellCalc.Workspaces;
using System;

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
            workspace.TryRegisterInputUnit("meter", new Unit(1.0, BaseUnit.UnitMeter, "m"));
            workspace.TryRegisterInputUnit("kilogram", new Unit(1.0, BaseUnit.UnitKilogram, "kg"));
            workspace.TryRegisterInputUnit("second", new Unit(1.0, BaseUnit.UnitSeconds, "s"));
            workspace.TryRegisterInputUnit("mol", new Unit(1.0, BaseUnit.UnitMole, "mol"));
            workspace.TryRegisterInputUnit("ampere", new Unit(1.0, BaseUnit.UnitAmperes, "A"));
            workspace.TryRegisterInputUnit("kelvin", new Unit(1.0, BaseUnit.UnitKelvin, "K"));
            workspace.TryRegisterInputUnit("candela", new Unit(1.0, BaseUnit.UnitCandela, "cd"));
        }

        /// <summary>
        /// Register shorthand SI units.
        /// </summary>
        /// <param name="workspace">The workspace.</param>
        public static void RegisterShortSIUnits(IWorkspace workspace)
        {
            workspace.TryRegisterInputUnit("m", new Unit(1.0, BaseUnit.UnitMeter, "m"));
            workspace.TryRegisterInputUnit("kg", new Unit(1.0, BaseUnit.UnitKilogram, "kg"));
            workspace.TryRegisterInputUnit("s", new Unit(1.0, BaseUnit.UnitSeconds, "s"));
            workspace.TryRegisterInputUnit("A", new Unit(1.0, BaseUnit.UnitAmperes, "A"));
            workspace.TryRegisterInputUnit("K", new Unit(1.0, BaseUnit.UnitKelvin, "K"));
            workspace.TryRegisterInputUnit("cd", new Unit(1.0, BaseUnit.UnitCandela, "cd"));
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
            string name = unit.Name ?? throw new ArgumentNullException(nameof(unit));
            void Add(Unit u)
            {
                string n = u.Name ?? string.Empty;
                workspace.TryRegisterInputUnit(n, u);
                if (includeOutputs)
                    workspace.TryRegisterOutputUnit(n, u);
            }

            if (atto)
                Add(new Unit(1e-18 * unit.Modifier, unit.SIUnits, $"a{name}"));
            if (femto)
                Add(new Unit(1e-15 * unit.Modifier, unit.SIUnits, $"f{name}"));
            if (pico)
                Add(new Unit(1e-12 * unit.Modifier, unit.SIUnits, $"p{name}"));
            if (nano)
                Add(new Unit(1e-9 * unit.Modifier, unit.SIUnits, $"n{name}"));
            if (micro)
                Add(new Unit(1e-6 * unit.Modifier, unit.SIUnits, $"u{name}"));
            if (milli)
                Add(new Unit(1e-3 * unit.Modifier, unit.SIUnits, $"m{name}"));
            workspace.TryRegisterInputUnit(name, unit);
            workspace.TryRegisterOutputUnit(name, unit);
            if (kilo)
                Add(new Unit(1e3 * unit.Modifier, unit.SIUnits, $"k{name}"));
            if (mega)
                Add(new Unit(1e6 * unit.Modifier, unit.SIUnits, $"M{name}"));
            if (giga)
                Add(new Unit(1e9 * unit.Modifier, unit.SIUnits, $"G{name}"));
            if (tera)
                Add(new Unit(1e12 * unit.Modifier, unit.SIUnits, $"T{name}"));
            if (peta)
                Add(new Unit(1e15 * unit.Modifier, unit.SIUnits, $"P{name}"));
        }

        /// <summary>
        /// Registers common units used by electrical/electronics engineers.
        /// </summary>
        /// <param name="workspace">The workspace.</param>
        public static void RegisterCommonElectricalUnits(IWorkspace workspace)
        {
            // Volts
            RegisterModifierUnits(workspace,
                new Unit(1.0, new BaseUnit(
                    (BaseUnit.Kilogram, 1),
                    (BaseUnit.Meter, 2),
                    (BaseUnit.Second, -3),
                    (BaseUnit.Ampere, -1)), "V"));
            
            // Ampere
            RegisterModifierUnits(workspace,
                new Unit(1.0, BaseUnit.UnitAmperes, "A"));
            
            // Watts
            RegisterModifierUnits(workspace,
                new Unit(1.0, new BaseUnit(
                    (BaseUnit.Kilogram, 1),
                    (BaseUnit.Meter, 2),
                    (BaseUnit.Second, -3)), "W"));
            
            // Joules
            RegisterModifierUnits(workspace,
                new Unit(1.0, new BaseUnit(
                    (BaseUnit.Kilogram, 1),
                    (BaseUnit.Meter, 2),
                    (BaseUnit.Second, -2)), "J"));
            
            // Farad
            RegisterModifierUnits(workspace,
                new Unit(1.0, new BaseUnit(
                    (BaseUnit.Kilogram, -1),
                    (BaseUnit.Meter, -2),
                    (BaseUnit.Second, 4),
                    (BaseUnit.Ampere, 2)), "F"));
            
            // Henry
            RegisterModifierUnits(workspace,
                new Unit(1.0, new BaseUnit(
                    (BaseUnit.Kilogram, 1),
                    (BaseUnit.Meter, 2),
                    (BaseUnit.Second, -2),
                    (BaseUnit.Ampere, -2)), "H"));
            
            // Ohm = A-2 kg m2 s-3
            RegisterModifierUnits(workspace,
                new Unit(1.0, new BaseUnit(
                    (BaseUnit.Kilogram, 1),
                    (BaseUnit.Meter, 2),
                    (BaseUnit.Second, -3),
                    (BaseUnit.Ampere, -2)), "ohm"));
        }
    }
}
