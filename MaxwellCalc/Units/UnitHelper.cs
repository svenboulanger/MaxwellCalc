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
            if (atto)
            {
                workspace.TryRegisterInputUnit($"a{name}", new Unit(1e-18 * unit.Modifier, unit.SIUnits, $"a{name}"));
            }
            if (femto)
            {
                workspace.TryRegisterInputUnit($"f{name}", new Unit(1e-15 * unit.Modifier, unit.SIUnits, $"f{name}"));
            }
            if (pico)
            {
                workspace.TryRegisterInputUnit($"f{name}", new Unit(1e-12 * unit.Modifier, unit.SIUnits, $"p{name}"));
            }
            if (nano)
            {
                workspace.TryRegisterInputUnit($"n{name}", new Unit(1e-9 * unit.Modifier, unit.SIUnits, $"n{name}"));
            }
            if (micro)
            {
                workspace.TryRegisterInputUnit($"u{name}", new Unit(1e-6 * unit.Modifier, unit.SIUnits, $"u{name}"));
            }
            if (milli)
            {
                workspace.TryRegisterInputUnit($"m{name}", new Unit(1e-3 * unit.Modifier, unit.SIUnits, $"m{name}"));
            }
            workspace.TryRegisterInputUnit(name, unit);
            if (kilo)
            {
                workspace.TryRegisterInputUnit($"k{name}", new Unit(1e3 * unit.Modifier, unit.SIUnits, $"k{name}"));
            }
            if (mega)
            {
                workspace.TryRegisterInputUnit($"M{name}", new Unit(1e6 * unit.Modifier, unit.SIUnits, $"M{name}"));
            }
            if (giga)
            {
                workspace.TryRegisterInputUnit($"G{name}", new Unit(1e9 * unit.Modifier, unit.SIUnits, $"G{name}"));
            }
            if (tera)
            {
                workspace.TryRegisterInputUnit($"T{name}", new Unit(1e12 * unit.Modifier, unit.SIUnits, $"T{name}"));
            }
            if (peta)
            {
                workspace.TryRegisterInputUnit($"P{name}", new Unit(1e15 * unit.Modifier, unit.SIUnits, $"P{name}"));
            }
        }

        /// <summary>
        /// Registers common units.
        /// </summary>
        /// <param name="workspace">The workspace.</param>
        public static void RegisterCommonUnits(IWorkspace workspace)
        {
            Unit unit;

            // Volts
            RegisterModifierUnits(workspace,
                unit = new Unit(1.0, new BaseUnit(
                    (BaseUnit.Kilogram, 1),
                    (BaseUnit.Meter, 2),
                    (BaseUnit.Second, -3),
                    (BaseUnit.Ampere, -1)), "V"));
            workspace.TryRegisterOutputUnit("V", unit);

            // Ampere
            RegisterModifierUnits(workspace,
                unit = new Unit(1.0, BaseUnit.UnitAmperes, "A"));
            workspace.TryRegisterOutputUnit("A", unit);

            // Watts
            RegisterModifierUnits(workspace,
                unit = new Unit(1.0, new BaseUnit(
                    (BaseUnit.Kilogram, 1),
                    (BaseUnit.Meter, 2),
                    (BaseUnit.Second, -3)), "W"));
            workspace.TryRegisterOutputUnit("W", unit);

            // Joules
            RegisterModifierUnits(workspace,
                unit = new Unit(1.0, new BaseUnit(
                    (BaseUnit.Kilogram, 1),
                    (BaseUnit.Meter, 2),
                    (BaseUnit.Second, -2)), "J"));
            workspace.TryRegisterOutputUnit("J", unit);

            // Farad
            RegisterModifierUnits(workspace,
                unit = new Unit(1.0, new BaseUnit(
                    (BaseUnit.Kilogram, -1),
                    (BaseUnit.Meter, -2),
                    (BaseUnit.Second, 4),
                    (BaseUnit.Ampere, 2)), "F"));
            workspace.TryRegisterOutputUnit("F", unit);

            // Henry
            RegisterModifierUnits(workspace,
                unit = new Unit(1.0, new BaseUnit(
                    (BaseUnit.Kilogram, 1),
                    (BaseUnit.Meter, 2),
                    (BaseUnit.Second, -2),
                    (BaseUnit.Ampere, -2)), "H"));
            workspace.TryRegisterOutputUnit("H", unit);
        }
    }
}
