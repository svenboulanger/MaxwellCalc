using MaxwellCalc.Core.Parsers;
using MaxwellCalc.Core.Workspaces;
using System;
using System.Linq;

namespace MaxwellCalc.Core.Units;

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
        where T : IFormattable
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
        where T : IFormattable
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
        bool tera = false, bool peta = false) where T : IFormattable
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
        bool mega = false, bool giga = false, bool tera = false, bool peta = false,
        int power = 1) where T : IFormattable
    {
        if (outputUnit.Dimension is null || baseUnits.Dimension is null)
            throw new ArgumentException("Dimension cannot be null", nameof(outputUnit));

        if (power != 1)
            baseUnits = new Unit([.. baseUnits.Dimension.Select(p => (p.Key, p.Value * power))]);

        Unit GetUnit(string prefix)
            => new([.. outputUnit.Dimension.Select(p => (p.Key == dimension ? $"{prefix}{p.Key}" : p.Key, p.Value * power))]);
        if (atto)
        {
            if (!workspace.TryRegisterOutputUnit(GetUnit("a"), new Quantity<string>($"1e-{18 * power}", baseUnits)))
                return false;
        }
        if (femto)
        {
            if (!workspace.TryRegisterOutputUnit(GetUnit("f"), new Quantity<string>($"1e-{15 * power}", baseUnits)))
                return false;
        }
        if (pico)
        {
            if (!workspace.TryRegisterOutputUnit(GetUnit("p"), new Quantity<string>($"1e-{12 * power}", baseUnits)))
                return false;
        }
        if (nano)
        {
            if (!workspace.TryRegisterOutputUnit(GetUnit("n"), new Quantity<string>($"1e-{9 * power}", baseUnits)))
                return false;
        }
        if (micro)
        {
            if (!workspace.TryRegisterOutputUnit(GetUnit("u"), new Quantity<string>($"1e-{6 * power}", baseUnits)))
                return false;
        }
        if (milli)
        {
            if (!workspace.TryRegisterOutputUnit(GetUnit("m"), new Quantity<string>($"1e-{3 * power}", baseUnits)))
                return false;
        }
        if (centi)
        {
            if (!workspace.TryRegisterOutputUnit(GetUnit("c"), new Quantity<string>($"1e-{2 * power}", baseUnits)))
                return false;
        }
        if (!workspace.TryRegisterOutputUnit(outputUnit, new Quantity<string>("1", baseUnits)))
            return false;
        if (kilo)
        {
            if (!workspace.TryRegisterOutputUnit(GetUnit("k"), new Quantity<string>($"1e{3 * power}", baseUnits)))
                return false;
        }
        if (mega)
        {
            if (!workspace.TryRegisterOutputUnit(GetUnit("M"), new Quantity<string>($"1e{6 * power}", baseUnits)))
                return false;
        }
        if (giga)
        {
            if (!workspace.TryRegisterOutputUnit(GetUnit("G"), new Quantity<string>($"1e{9 * power}", baseUnits)))
                return false;
        }
        if (tera)
        {
            if (!workspace.TryRegisterOutputUnit(GetUnit("T"), new Quantity<string>($"1e{12 * power}", baseUnits)))
                return false;
        }
        if (peta)
        {
            if (!workspace.TryRegisterOutputUnit(GetUnit("P"), new Quantity<string>($"1e{15 * power}", baseUnits)))
                return false;
        }
        return true;
    }

    /// <summary>
    /// Registers common units.
    /// </summary>
    /// <param name="workspace">The workspace.</param>
    public static void RegisterCommonUnits<T>(this IWorkspace<T> workspace) where T : IFormattable
    {
        // Length
        workspace.TryRegisterModifierInputOutputUnits(Unit.Meter, Unit.UnitMeter,
            nano: true, micro: true, milli: true, centi: true, kilo: true);

        // Area
        workspace.TryRegisterModifierOutputUnits(Unit.UnitMeter, Unit.UnitMeter, Unit.Meter,
            nano: true, micro: true, milli: true, centi: true, kilo: true, power: 2);

        // Volume
        workspace.TryRegisterModifierOutputUnits(Unit.UnitMeter, Unit.UnitMeter, Unit.Meter,
            nano: true, micro: true, milli: true, centi: true, kilo: true, power: 3);

        // Speed
        workspace.TryRegisterModifierOutputUnits(
            new((Unit.Meter, 1), (Unit.Second, -1)),
            new((Unit.Meter, 1), (Unit.Second, -1)), Unit.Meter);
        workspace.TryRegisterOutputUnit(
            new Unit(("km", 1), ("hour", -1)),
            new("1/3.6", new((Unit.Meter, 1), (Unit.Second, -1))));

        // Speed squared (variance)
        workspace.TryRegisterModifierOutputUnits(
            new((Unit.Meter, 1), (Unit.Second, -1)),
            new((Unit.Meter, 1), (Unit.Second, -1)), Unit.Meter,
            power: 2);
        workspace.TryRegisterOutputUnit(
            new Unit(("km", 2), ("hour", -2)),
            new("1/12.96", new((Unit.Meter, 2), (Unit.Second, -2))));

        // Mass
        workspace.TryRegisterInputOutputUnit("ng", Unit.UnitKilogram, "1e-12");
        workspace.TryRegisterInputOutputUnit("ug", Unit.UnitKilogram, "1e-9");
        workspace.TryRegisterInputOutputUnit("mg", Unit.UnitKilogram, "1e-6");
        workspace.TryRegisterInputOutputUnit("g", Unit.UnitKilogram, "1e-3");
        workspace.TryRegisterInputOutputUnit("kg", Unit.UnitKilogram, "1");
        workspace.TryRegisterInputOutputUnit("ton", Unit.UnitKilogram, "1000");

        // Mass squared (variance)
        workspace.TryRegisterOutputUnit(new(("ng", 2)), new Quantity<string>("1e-24", new Unit((Unit.Kilogram, 2))));
        workspace.TryRegisterOutputUnit(new(("ug", 2)), new Quantity<string>("1e-18", new Unit((Unit.Kilogram, 2))));
        workspace.TryRegisterOutputUnit(new(("mg", 2)), new Quantity<string>("1e-12", new Unit((Unit.Kilogram, 2))));
        workspace.TryRegisterOutputUnit(new(("g", 2)), new Quantity<string>("1e-6", new Unit((Unit.Kilogram, 2))));
        workspace.TryRegisterOutputUnit(new(("kg", 2)), new Quantity<string>("1", new Unit((Unit.Kilogram, 2))));
        workspace.TryRegisterOutputUnit(new(("ton", 2)), new Quantity<string>("1000000", new Unit((Unit.Kilogram, 2))));

        // Time
        workspace.TryRegisterModifierInputOutputUnits(Unit.Second, Unit.UnitSeconds,
            femto: true, pico: true, nano: true, micro: true, milli: true);
        workspace.TryRegisterInputOutputUnit("min", Unit.UnitSeconds, "60");
        workspace.TryRegisterInputOutputUnit("hour", Unit.UnitSeconds, "3600");
        workspace.TryRegisterInputOutputUnit("day", Unit.UnitSeconds, "86400");

        // Time squared (variance)
        workspace.TryRegisterModifierOutputUnits(Unit.UnitSeconds, Unit.UnitSeconds, Unit.Second,
            femto: true, pico: true, nano: true, micro: true, milli: true, power: 2);
        workspace.TryRegisterOutputUnit(new(("min", 2)), new Quantity<string>("3600", new Unit((Unit.Second, 2))));
        workspace.TryRegisterOutputUnit(new(("hour", 2)), new Quantity<string>("12960000", new Unit((Unit.Second, 2))));
        workspace.TryRegisterOutputUnit(new(("day", 2)), new Quantity<string>("7465e6", new Unit((Unit.Second, 2))));

        // Ampere
        workspace.TryRegisterModifierInputOutputUnits(Unit.Ampere, Unit.UnitAmperes,
            pico: true, nano: true, micro: true, milli: true, kilo: true);

        // Ampere squared (variance)
        workspace.TryRegisterModifierOutputUnits(Unit.UnitAmperes, Unit.UnitAmperes, Unit.Ampere,
            pico: true, nano: true, micro: true, milli: true, kilo: true, power: 2);

        // Kelvin
        workspace.TryRegisterModifierInputOutputUnits(Unit.Kelvin, Unit.UnitKelvin,
            milli: true);

        // Kelvin squared (variance)
        workspace.TryRegisterModifierOutputUnits(Unit.UnitKelvin, Unit.UnitKelvin, Unit.Kelvin,
            milli: true, power: 2);

        // Candela
        workspace.TryRegisterModifierInputOutputUnits(Unit.Candela, Unit.UnitCandela);

        // Angle
        workspace.TryRegisterInputOutputUnit("rad", Unit.UnitRadian, "1");
    }

    /// <summary>
    /// Registers common units used by electrical/electronics engineers.
    /// </summary>
    /// <param name="workspace">The workspace.</param>
    public static void RegisterCommonElectronicsUnits<T>(IWorkspace<T> workspace) where T : IFormattable
    {
        // Coulomb
        workspace.TryRegisterModifierInputOutputUnits("C",
            new((Unit.Ampere, 1), (Unit.Second, 1)));

        // Coulomb squared (variance)
        workspace.TryRegisterModifierOutputUnits(new(("C", 1)),
            new((Unit.Ampere, 1), (Unit.Second, 1)), "C",
            power: 2);

        // Coulomb meter - electric dipole moment
        workspace.TryRegisterOutputUnit(
            new(("C", 1), (Unit.Meter, 1)), new Quantity<string>("1",
            new((Unit.Ampere, 1), (Unit.Second, 1), (Unit.Meter, 1))));

        // Coulomb meter squared (variance)
        workspace.TryRegisterOutputUnit(
            new(("C", 2), (Unit.Meter, 2)), new Quantity<string>("1",
            new((Unit.Ampere, 2), (Unit.Second, 2), (Unit.Meter, 2))));

        // Coulomb per meter - charge density
        workspace.TryRegisterOutputUnit(
            new(("C", 1), (Unit.Meter, -1)), new Quantity<string>("1",
            new((Unit.Ampere, 1), (Unit.Second, 1), (Unit.Meter, -1))));

        // (Coulomb per meter) squared (variance)
        workspace.TryRegisterOutputUnit(
            new(("C", 2), (Unit.Meter, -2)), new Quantity<string>("1",
            new((Unit.Ampere, 2), (Unit.Second, 2), (Unit.Meter, -2))));

        // Coulomb per square meter - charge density
        workspace.TryRegisterOutputUnit(
            new(("C", 1), (Unit.Meter, -2)), new Quantity<string>("1",
            new((Unit.Ampere, 1), (Unit.Second, 1), (Unit.Meter, -2))));

        // (Coulomb per square meter) squared (variance)
        workspace.TryRegisterOutputUnit(
            new(("C", 2), (Unit.Meter, -4)), new Quantity<string>("1",
            new((Unit.Ampere, 2), (Unit.Second, 2), (Unit.Meter, -4))));

        // Coulomb per cubic meter - charge density
        workspace.TryRegisterOutputUnit(
            new(("C", 1), (Unit.Meter, -3)), new Quantity<string>("1",
            new((Unit.Ampere, 1), (Unit.Second, 1), (Unit.Meter, -3))));

        // (Coulomb per cubic meter) squared (variance)
        workspace.TryRegisterOutputUnit(
            new(("C", 2), (Unit.Meter, -6)), new Quantity<string>("1",
            new((Unit.Ampere, 2), (Unit.Second, 2), (Unit.Meter, -6))));

        // Volts
        workspace.TryRegisterModifierInputOutputUnits("V",
            new Unit((Unit.Kilogram, 1), (Unit.Meter, 2), (Unit.Second, -3), (Unit.Ampere, -1)),
            nano: true, micro: true, milli: true, kilo: true, mega: true);

        // Volts squared (variance)
        workspace.TryRegisterModifierOutputUnits(new Unit(("V", 1)),
            new Unit((Unit.Kilogram, 1), (Unit.Meter, 2), (Unit.Second, -3), (Unit.Ampere, -1)), "V",
            nano: true, micro: true, milli: true, kilo: true, mega: true, power: 2);

        // Volts per meter - electric field
        workspace.TryRegisterModifierOutputUnits(new(("V", 1), (Unit.Meter, -1)),
            new Unit((Unit.Kilogram, 1), (Unit.Meter, 1), (Unit.Second, -3), (Unit.Ampere, -1)), "V");

        // (Volts per meter) squared (variance)
        workspace.TryRegisterModifierOutputUnits(new(("V", 1), (Unit.Meter, -1)),
            new Unit((Unit.Kilogram, 1), (Unit.Meter, 1), (Unit.Second, -3), (Unit.Ampere, -1)), "V",
            power: 2);

        // Volts per square meter - electric field gradient
        workspace.TryRegisterModifierOutputUnits(new(("V", 1), (Unit.Meter, -2)),
            new Unit((Unit.Kilogram, 1), (Unit.Second, -3), (Unit.Ampere, -1)), "V");

        // (Volts per square meter) squared (variance) - electric field gradient
        workspace.TryRegisterModifierOutputUnits(new(("V", 1), (Unit.Meter, -2)),
            new Unit((Unit.Kilogram, 1), (Unit.Second, -3), (Unit.Ampere, -1)), "V",
            power: 2);

        // Volts per second - slew rate
        workspace.TryRegisterModifierOutputUnits(new(("V", 1), (Unit.Second, -1)),
            new Unit((Unit.Kilogram, 1), (Unit.Meter, 2), (Unit.Second, -4), (Unit.Ampere, -1)), "V");

        // (Volts per second) squared (variance) - slew rate
        workspace.TryRegisterModifierOutputUnits(new(("V", 1), (Unit.Second, -1)),
            new Unit((Unit.Kilogram, 1), (Unit.Meter, 2), (Unit.Second, -4), (Unit.Ampere, -1)), "V",
            power: 2);

        // Watts
        workspace.TryRegisterModifierInputOutputUnits("W", 
            new((Unit.Kilogram, 1), (Unit.Meter, 2), (Unit.Second, -3)),
            pico: true, nano: true, micro: true, milli: true, kilo: true, mega: true, giga: true);

        // Watts squared (variance)
        workspace.TryRegisterModifierOutputUnits(new(("W", 1)),
            new((Unit.Kilogram, 1), (Unit.Meter, 2), (Unit.Second, -3)), "W",
            pico: true, nano: true, micro: true, milli: true, kilo: true, mega: true, giga: true, power: 2);

        // Joules
        workspace.TryRegisterModifierInputOutputUnits("J",
            new((Unit.Kilogram, 1), (Unit.Meter, 2), (Unit.Second, -2)),
            pico: true, nano: true, micro: true, milli: true, kilo: true, mega: true, giga: true);

        // Joules squared (variance)
        workspace.TryRegisterModifierOutputUnits(new(("J", 1)),
            new((Unit.Kilogram, 1), (Unit.Meter, 2), (Unit.Second, -2)), "J",
            pico: true, nano: true, micro: true, milli: true, kilo: true, mega: true, giga: true, power: 2);

        // Joules seconds (Planck constant)
        workspace.TryRegisterModifierOutputUnits(
            new(("J", 1), (Unit.Second, 1)),
            new((Unit.Kilogram, 1), (Unit.Meter, 2), (Unit.Second, -1)), "J");

        // Farad
        workspace.TryRegisterModifierInputOutputUnits("F",
            new((Unit.Kilogram, -1), (Unit.Meter, -2), (Unit.Second, 4), (Unit.Ampere, 2)),
            femto: true, pico: true, micro: true, milli: true);

        // Farad squared (variance)
        workspace.TryRegisterModifierOutputUnits(new(("F", 1)),
            new((Unit.Kilogram, -1), (Unit.Meter, -2), (Unit.Second, 4), (Unit.Ampere, 2)), "F",
            femto: true, pico: true, micro: true, milli: true, power: 2);

        // Farad per meter
        workspace.TryRegisterModifierOutputUnits(new(("F", 1), (Unit.Meter, -1)),
            new((Unit.Kilogram, -1), (Unit.Meter, -3), (Unit.Second, 4), (Unit.Ampere, 2)), "V");

        // (Farad per meter) squared (variance)
        workspace.TryRegisterModifierOutputUnits(new(("F", 1), (Unit.Meter, -1)),
            new((Unit.Kilogram, -1), (Unit.Meter, -3), (Unit.Second, 4), (Unit.Ampere, 2)), "V",
            power: 2);

        // Farad per square meter
        workspace.TryRegisterModifierOutputUnits(new(("F", 1), (Unit.Meter, -2)),
            new((Unit.Kilogram, -1), (Unit.Meter, -4), (Unit.Second, 4), (Unit.Ampere, 2)), "V");

        // (Farad per square meter) squared (variance)
        workspace.TryRegisterModifierOutputUnits(new(("F", 1), (Unit.Meter, -2)),
            new((Unit.Kilogram, -1), (Unit.Meter, -4), (Unit.Second, 4), (Unit.Ampere, 2)), "V",
            power: 2);

        // Henry
        workspace.TryRegisterModifierInputOutputUnits("H",
            new((Unit.Kilogram, 1), (Unit.Meter, 2), (Unit.Second, -2), (Unit.Ampere, -2)),
            nano: true, micro: true, milli: true);

        // Henry
        workspace.TryRegisterModifierOutputUnits(new(("H", 1)),
            new((Unit.Kilogram, 1), (Unit.Meter, 2), (Unit.Second, -2), (Unit.Ampere, -2)), "H",
            nano: true, micro: true, milli: true, power: 2);

        // Weber
        workspace.TryRegisterModifierInputOutputUnits("Wb",
            new((Unit.Kilogram, 1), (Unit.Meter, 2), (Unit.Second, -2), (Unit.Ampere, -1)));

        // Ohm
        workspace.TryRegisterModifierInputOutputUnits("Ohm",
            new((Unit.Kilogram, 1), (Unit.Meter, 2), (Unit.Second, -3), (Unit.Ampere, -2)),
            micro: true, milli: true, kilo: true, mega: true);

        // Ohm squared (variance)
        workspace.TryRegisterModifierOutputUnits(new(("Ohm", 1)),
            new((Unit.Kilogram, 1), (Unit.Meter, 2), (Unit.Second, -3), (Unit.Ampere, -2)), "Ohm",
            micro: true, milli: true, kilo: true, mega: true, power: 2);

        // Siemens
        workspace.TryRegisterModifierInputOutputUnits("S",
            new((Unit.Kilogram, -1), (Unit.Meter, -2), (Unit.Second, 3), (Unit.Ampere, 2)),
            micro: true, milli: true, kilo: true, mega: true);

        // Siemens squared
        workspace.TryRegisterModifierOutputUnits(new(("S", 1)),
            new((Unit.Kilogram, -1), (Unit.Meter, -2), (Unit.Second, 3), (Unit.Ampere, 2)), "S",
            micro: true, milli: true, kilo: true, mega: true, power: 2);

        // Hertz
        workspace.TryRegisterModifierInputOutputUnits("Hz", new((Unit.Second, -1)),
            milli: true, kilo: true, mega: true, giga: true, tera: true);

        // Hertz
        workspace.TryRegisterModifierOutputUnits(new(("Hz", 1)), new((Unit.Second, -1)), "Hz",
            milli: true, kilo: true, mega: true, giga: true, tera: true, power: 2);

        // Bits
        var b = new Unit(("bit", 1));
        workspace.TryRegisterInputOutputUnit("bit", b, "1");
        workspace.TryRegisterInputOutputUnit("B", b, "8");
        workspace.TryRegisterInputOutputUnit("kB", b, "8192");
        workspace.TryRegisterInputOutputUnit("MB", b, "8388608");
        workspace.TryRegisterInputOutputUnit("GB", b, "8589934592");
        workspace.TryRegisterInputOutputUnit("TB", b, "8796093022208");
        workspace.TryRegisterInputOutputUnit("PB", b, "9007199254740992");

        // Bits per second
        var bps = new Unit(("bit", 1), (Unit.Second, -1));
        workspace.TryRegisterInputOutputUnit("bps", bps, "1");
        workspace.TryRegisterInputOutputUnit("kbps", bps, "1000");
        workspace.TryRegisterInputOutputUnit("Mbps", bps, "1000000");
        workspace.TryRegisterInputOutputUnit("Gbps", bps, "1000000000");
        workspace.TryRegisterInputOutputUnit("Tbps", bps, "1000000000000");
        workspace.TryRegisterInputOutputUnit("Pbps", bps, "1000000000000000");
    }
}
