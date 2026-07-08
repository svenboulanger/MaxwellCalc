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
        var unit = new Unit((inputUnit, 1));
        workspace.OutputUnits[new(unit, baseUnits)] = result.Scalar;

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
    /// <param name="category">An optional category name.</param>
    /// <returns>Returns <c>true</c> if the registration was successfull; otherwise, <c>false</c>.</returns>
    public static bool TryRegisterOutputUnit<T>(this IWorkspace<T> workspace, Unit outputUnit, Quantity<string> baseUnits, string? category = null)
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

        if (category is not null)
            workspace.UnitCategories[baseUnits.Unit] = category;

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
    /// <param name="baseUnit">The base unit.</param>
    /// <param name="category">An optional category.</param>
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
        this IWorkspace<T> workspace, string name, Unit baseUnit, string? category = null,
        bool atto = false, bool femto = false, bool pico = false, bool nano = false, bool micro = false,
        bool milli = false, bool centi = false, bool kilo = false, bool mega = false, bool giga = false,
        bool tera = false, bool peta = false) where T : IFormattable
    {
        if (category is not null)
            workspace.UnitCategories[baseUnit] = category;

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
    /// <param name="baseUnits">The base units.</param>
    /// <param name="power">The power multiplication factor (e.g., for squared units this would be 2).</param>
    /// <param name="category">An optional base unit category.</param>
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
    public static bool TryRegisterModifierOutputUnits<T>(
        this IWorkspace<T> workspace,
        Unit outputUnit, Unit baseUnits, string dimension, string? category = null,
        bool atto = false, bool femto = false, bool pico = false, bool nano = false,
        bool micro = false, bool milli = false, bool centi = false, bool kilo = false,
        bool mega = false, bool giga = false, bool tera = false, bool peta = false,
        int power = 1) where T : IFormattable
    {
        if (outputUnit.Dimension is null || baseUnits.Dimension is null)
            throw new ArgumentException("Dimension cannot be null", nameof(outputUnit));

        if (power != 1)
            baseUnits = new Unit([.. baseUnits.Dimension.Select(p => (p.Key, p.Value * power))]);

        if (category is not null)
            workspace.UnitCategories[baseUnits] = category;

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
        {
            var nUnit = GetUnit("");
            if (!workspace.TryRegisterOutputUnit(GetUnit(""), new Quantity<string>("1", baseUnits)))
                return false;
        }
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
        workspace.TryRegisterModifierInputOutputUnits(Unit.Meter, Unit.UnitMeter, "length",
            nano: true, micro: true, milli: true, centi: true, kilo: true);

        // Area
        workspace.TryRegisterModifierOutputUnits(Unit.UnitMeter, Unit.UnitMeter, Unit.Meter, "area",
            nano: true, micro: true, milli: true, centi: true, kilo: true, power: 2);

        // Volume
        workspace.TryRegisterModifierOutputUnits(Unit.UnitMeter, Unit.UnitMeter, Unit.Meter, "volume",
            nano: true, micro: true, milli: true, centi: true, kilo: true, power: 3);

        // Speed
        workspace.TryRegisterModifierOutputUnits(
            new((Unit.Meter, 1), (Unit.Second, -1)),
            new((Unit.Meter, 1), (Unit.Second, -1)), Unit.Meter, "velocity");
        workspace.TryRegisterOutputUnit(
            new Unit(("km", 1), ("hour", -1)),
            new("1/3.6", new((Unit.Meter, 1), (Unit.Second, -1))));

        // Speed squared (variance)
        workspace.TryRegisterModifierOutputUnits(
            new((Unit.Meter, 1), (Unit.Second, -1)),
            new((Unit.Meter, 1), (Unit.Second, -1)), Unit.Meter, "velocity squared",
            power: 2);
        workspace.TryRegisterOutputUnit(
            new Unit(("km", 2), ("hour", -2)),
            new("1/12.96", new((Unit.Meter, 2), (Unit.Second, -2))));

        // Mass
        workspace.UnitCategories[Unit.UnitKilogram] = "mass";
        workspace.TryRegisterInputOutputUnit("ng", Unit.UnitKilogram, "1e-12");
        workspace.TryRegisterInputOutputUnit("ug", Unit.UnitKilogram, "1e-9");
        workspace.TryRegisterInputOutputUnit("mg", Unit.UnitKilogram, "1e-6");
        workspace.TryRegisterInputOutputUnit("g", Unit.UnitKilogram, "1e-3");
        workspace.TryRegisterInputOutputUnit("kg", Unit.UnitKilogram, "1");
        workspace.TryRegisterInputOutputUnit("ton", Unit.UnitKilogram, "1000");

        // Mass squared (variance)
        workspace.UnitCategories[new Unit((Unit.Kilogram, 2))] = "mass squared";
        workspace.TryRegisterOutputUnit(new(("ng", 2)), new Quantity<string>("1e-24", new Unit((Unit.Kilogram, 2))));
        workspace.TryRegisterOutputUnit(new(("ug", 2)), new Quantity<string>("1e-18", new Unit((Unit.Kilogram, 2))));
        workspace.TryRegisterOutputUnit(new(("mg", 2)), new Quantity<string>("1e-12", new Unit((Unit.Kilogram, 2))));
        workspace.TryRegisterOutputUnit(new(("g", 2)), new Quantity<string>("1e-6", new Unit((Unit.Kilogram, 2))));
        workspace.TryRegisterOutputUnit(new(("kg", 2)), new Quantity<string>("1", new Unit((Unit.Kilogram, 2))));
        workspace.TryRegisterOutputUnit(new(("ton", 2)), new Quantity<string>("1000000", new Unit((Unit.Kilogram, 2))));

        // Time
        workspace.TryRegisterModifierInputOutputUnits(Unit.Second, Unit.UnitSeconds, "time",
            femto: true, pico: true, nano: true, micro: true, milli: true);
        workspace.TryRegisterInputOutputUnit("min", Unit.UnitSeconds, "60");
        workspace.TryRegisterInputOutputUnit("hour", Unit.UnitSeconds, "3600");
        workspace.TryRegisterInputOutputUnit("day", Unit.UnitSeconds, "86400");

        // Time squared (variance)
        workspace.TryRegisterModifierOutputUnits(Unit.UnitSeconds, Unit.UnitSeconds, Unit.Second, "time squared",
            femto: true, pico: true, nano: true, micro: true, milli: true, power: 2);
        workspace.TryRegisterOutputUnit(new(("min", 2)), new Quantity<string>("3600", new Unit((Unit.Second, 2))));
        workspace.TryRegisterOutputUnit(new(("hour", 2)), new Quantity<string>("12960000", new Unit((Unit.Second, 2))));
        workspace.TryRegisterOutputUnit(new(("day", 2)), new Quantity<string>("7465e6", new Unit((Unit.Second, 2))));

        // Ampere
        workspace.TryRegisterModifierInputOutputUnits(Unit.Ampere, Unit.UnitAmperes, "current",
            pico: true, nano: true, micro: true, milli: true, kilo: true);

        // Ampere squared (variance)
        workspace.TryRegisterModifierOutputUnits(Unit.UnitAmperes, Unit.UnitAmperes, Unit.Ampere, "current squared",
            pico: true, nano: true, micro: true, milli: true, kilo: true, power: 2);

        // Kelvin
        workspace.TryRegisterModifierInputOutputUnits(Unit.Kelvin, Unit.UnitKelvin, "temperature",
            milli: true);

        // Kelvin squared (variance)
        workspace.TryRegisterModifierOutputUnits(Unit.UnitKelvin, Unit.UnitKelvin, Unit.Kelvin, "temperature squared",
            milli: true, power: 2);

        // Candela
        workspace.TryRegisterModifierInputOutputUnits(Unit.Candela, Unit.UnitCandela, "luminous intensity");

        // Candela squared
        workspace.TryRegisterModifierOutputUnits(Unit.UnitCandela, Unit.UnitCandela, "luminous intensity squared",
            power: 2);

        // Angle
        workspace.UnitCategories[Unit.UnitRadian] = "angle";
        workspace.TryRegisterInputOutputUnit("rad", Unit.UnitRadian, "1");

        workspace.TryRegisterOutputUnit(new Unit((Unit.Radian, 2)), new Quantity<string>("1", new((Unit.Radian, 2))), "angle squared");
    }

    /// <summary>
    /// Registers common units used by electrical/electronics engineers.
    /// </summary>
    /// <param name="workspace">The workspace.</param>
    public static void RegisterCommonElectronicsUnits<T>(IWorkspace<T> workspace) where T : IFormattable
    {
        Unit bu, u;

        // Coulomb
        bu = new((Unit.Ampere, 1), (Unit.Second, 1));
        workspace.TryRegisterModifierInputOutputUnits("C", bu, "charge");

        // Coulomb squared (variance)
        workspace.TryRegisterModifierOutputUnits(new(("C", 1)), bu, "C", "charge squared",
            power: 2);

        // Coulomb meter - electric dipole moment
        u = new(("C", 1), (Unit.Meter, 1));
        bu = new((Unit.Ampere, 1), (Unit.Second, 1), (Unit.Meter, 1));
        workspace.TryRegisterOutputUnit(u, new("1", bu), "electric dipole moment");
        workspace.TryRegisterOutputUnit(Unit.Pow(u, 2), new("1", Unit.Pow(bu, 2)), "electric dipole moment squared");

        // Coulomb per meter - charge density
        u = new(("C", 1), (Unit.Meter, -1));
        bu = new((Unit.Ampere, 1), (Unit.Second, 1), (Unit.Meter, -1));
        workspace.TryRegisterOutputUnit(u, new("1", bu), "1D charge density");
        workspace.TryRegisterOutputUnit(Unit.Pow(u, 2), new("1", Unit.Pow(bu, 2)), "1D charge density squared");

        // Coulomb per square meter - charge density
        u = new(("C", 1), (Unit.Meter, -2));
        bu = new((Unit.Ampere, 1), (Unit.Second, 1), (Unit.Meter, -2));
        workspace.TryRegisterOutputUnit(u, new("1", bu), "2D charge density");
        workspace.TryRegisterOutputUnit(Unit.Pow(u, 2), new("1", Unit.Pow(bu, 2)), "2D charge density squared");

        // Coulomb per cubic meter - charge density
        u = new(("C", 1), (Unit.Meter, -3));
        bu = new((Unit.Ampere, 1), (Unit.Second, 1), (Unit.Meter, -3)); 
        workspace.TryRegisterOutputUnit(u, new("1", bu), "3D charge density");
        workspace.TryRegisterOutputUnit(Unit.Pow(u, 2), new("1", Unit.Pow(bu, 2)), "3D charge density squared");

        // Volts
        u = new(("V", 1));
        bu = new Unit((Unit.Kilogram, 1), (Unit.Meter, 2), (Unit.Second, -3), (Unit.Ampere, -1));
        workspace.TryRegisterModifierInputOutputUnits("V", bu, "voltage",
            nano: true, micro: true, milli: true, kilo: true, mega: true);
        workspace.TryRegisterModifierOutputUnits(u, bu, "V", "voltage squared",
            nano: true, micro: true, milli: true, kilo: true, mega: true, power: 2);

        // Volts per meter - electric field
        u = new(("V", 1), (Unit.Meter, -1));
        bu = new Unit((Unit.Kilogram, 1), (Unit.Meter, 1), (Unit.Second, -3), (Unit.Ampere, -1));
        workspace.TryRegisterModifierOutputUnits(u, bu, "V", "electric field");
        workspace.TryRegisterModifierOutputUnits(u, bu, "V", "electric field squared",
            power: 2);

        // Volts per square meter - electric field gradient
        u = new(("V", 1), (Unit.Meter, -2));
        bu = new Unit((Unit.Kilogram, 1), (Unit.Second, -3), (Unit.Ampere, -1));
        workspace.TryRegisterModifierOutputUnits(u, bu, "V", "electric field gradient");
        workspace.TryRegisterModifierOutputUnits(u, bu, "V", "electric field gradient squared",
            power: 2);

        // Volts per second - slew rate
        u = new(("V", 1), (Unit.Second, -1));
        workspace.TryRegisterModifierOutputUnits(u, bu, "V", "slew rate");
        workspace.TryRegisterModifierOutputUnits(u, bu, "V", "slew rate squared",
            power: 2);

        // Watts
        u = new(("W", 1));
        bu = new((Unit.Kilogram, 1), (Unit.Meter, 2), (Unit.Second, -3));
        workspace.TryRegisterModifierInputOutputUnits("W", bu, "power",
            pico: true, nano: true, micro: true, milli: true, kilo: true, mega: true, giga: true);
        workspace.TryRegisterModifierOutputUnits(u, bu, "W", "power squared",
            pico: true, nano: true, micro: true, milli: true, kilo: true, mega: true, giga: true, power: 2);

        // Joules
        u = new(("J", 1));
        bu = new((Unit.Kilogram, 1), (Unit.Meter, 2), (Unit.Second, -2));
        workspace.TryRegisterModifierInputOutputUnits("J", bu, "energy",
            pico: true, nano: true, micro: true, milli: true, kilo: true, mega: true, giga: true);
        workspace.TryRegisterModifierOutputUnits(u, bu, "J", "energy squared",
            pico: true, nano: true, micro: true, milli: true, kilo: true, mega: true, giga: true, power: 2);

        // Joules seconds (Planck constant)
        u = new(("J", 1), (Unit.Second, 1));
        bu = new((Unit.Kilogram, 1), (Unit.Meter, 2), (Unit.Second, -1));
        workspace.TryRegisterModifierOutputUnits(u, bu, "J", "Planck constant units");

        // Farad
        u = new(("F", 1));
        bu = new((Unit.Kilogram, -1), (Unit.Meter, -2), (Unit.Second, 4), (Unit.Ampere, 2));
        workspace.TryRegisterModifierInputOutputUnits("F", bu, "capacitance",
            femto: true, pico: true, micro: true, milli: true);
        workspace.TryRegisterModifierOutputUnits(u, bu, "F", "capacitance squared",
            femto: true, pico: true, micro: true, milli: true, power: 2);

        // Farad per meter
        u = new(("F", 1), (Unit.Meter, -1));
        bu = new((Unit.Kilogram, -1), (Unit.Meter, -3), (Unit.Second, 4), (Unit.Ampere, 2));
        workspace.TryRegisterModifierOutputUnits(u, bu, "F", "1D capacitance density");
        workspace.TryRegisterOutputUnit(new(("fF", 1), ("um", -1)), new("1e-9", bu));
        workspace.TryRegisterModifierOutputUnits(u, bu, "F", "1D capacitance density squared",
            power: 2);
        workspace.TryRegisterOutputUnit(new(("fF", 2), ("um", -2)), new("1e-18", Unit.Pow(bu, 2)));

        // Farad per square meter
        u = new(("F", 1), (Unit.Meter, -2));
        bu = new((Unit.Kilogram, -1), (Unit.Meter, -4), (Unit.Second, 4), (Unit.Ampere, 2));
        workspace.TryRegisterModifierOutputUnits(u, bu, "F", "2D capacitance density");
        workspace.TryRegisterOutputUnit(new(("fF", 1), ("um", -2)), new("1e-3", bu));
        workspace.TryRegisterModifierOutputUnits(u, bu, "F", "2D capacitance density squared",
            power: 2);
        workspace.TryRegisterOutputUnit(new(("fF", 2), ("um", -4)), new("1e-6", Unit.Pow(bu, 2)));

        // Henry
        u = new(("H", 1));
        bu = new((Unit.Kilogram, 1), (Unit.Meter, 2), (Unit.Second, -2), (Unit.Ampere, -2));
        workspace.TryRegisterModifierInputOutputUnits("H", bu, "inductance",
            nano: true, micro: true, milli: true);
        workspace.TryRegisterModifierOutputUnits(u, bu, "H", "inductance squared",
            nano: true, micro: true, milli: true, power: 2);

        // Weber
        u = new(("Wb", 1));
        bu = new((Unit.Kilogram, 1), (Unit.Meter, 2), (Unit.Second, -2), (Unit.Ampere, -1));
        workspace.TryRegisterModifierInputOutputUnits("Wb", bu, "magnetic flux");
        workspace.TryRegisterModifierOutputUnits(u, bu, "Wb", "magnetic flux squared",
            power: 2);

        // Ohm
        u = new(("Ohm", 1));
        bu = new((Unit.Kilogram, 1), (Unit.Meter, 2), (Unit.Second, -3), (Unit.Ampere, -2));
        workspace.TryRegisterModifierInputOutputUnits("Ohm", bu, "resistance",
            micro: true, milli: true, kilo: true, mega: true);
        workspace.TryRegisterModifierOutputUnits(u, bu, "Ohm", "resistance squared",
            micro: true, milli: true, kilo: true, mega: true, power: 2);

        // Siemens
        u = new(("S", 1));
        bu = new((Unit.Kilogram, -1), (Unit.Meter, -2), (Unit.Second, 3), (Unit.Ampere, 2));
        workspace.TryRegisterModifierInputOutputUnits("S", bu, "conductance",
            micro: true, milli: true, kilo: true, mega: true);
        workspace.TryRegisterModifierOutputUnits(u, bu, "S", "conductance squared",
            micro: true, milli: true, kilo: true, mega: true, power: 2);

        // Hertz
        u = new(("Hz", 1));
        bu = new((Unit.Second, -1));
        workspace.TryRegisterModifierInputOutputUnits("Hz", bu, "frequency",
            milli: true, kilo: true, mega: true, giga: true, tera: true);
        workspace.TryRegisterModifierOutputUnits(u, bu, "Hz", "frequency squared",
            milli: true, kilo: true, mega: true, giga: true, tera: true, power: 2);

        // Current density
        bu = new Unit((Unit.Ampere, 1), (Unit.Meter, -2));
        workspace.TryRegisterOutputUnit(new(("pA", 1), ("cm", -2)), new("1e-8", bu), "current density");
        workspace.TryRegisterOutputUnit(new(("nA", 1), ("cm", -2)), new("1e-5", bu));
        workspace.TryRegisterOutputUnit(new(("uA", 1), ("cm", -2)), new("1e-2", bu));

        // Bits
        bu = new Unit(("bit", 1));
        workspace.UnitCategories[bu] = "data";
        workspace.TryRegisterInputOutputUnit("bit", bu, "1");
        workspace.TryRegisterInputOutputUnit("B", bu, "8");
        workspace.TryRegisterInputOutputUnit("kB", bu, "8192");
        workspace.TryRegisterInputOutputUnit("MB", bu, "8388608");
        workspace.TryRegisterInputOutputUnit("GB", bu, "8589934592");
        workspace.TryRegisterInputOutputUnit("TB", bu, "8796093022208");
        workspace.TryRegisterInputOutputUnit("PB", bu, "9007199254740992");

        // Bits per second
        bu = new Unit(("bit", 1), (Unit.Second, -1));
        workspace.UnitCategories[bu] = "data rate";
        workspace.TryRegisterInputOutputUnit("bps", bu, "1");
        workspace.TryRegisterInputOutputUnit("kbps", bu, "1000");
        workspace.TryRegisterInputOutputUnit("Mbps", bu, "1000000");
        workspace.TryRegisterInputOutputUnit("Gbps", bu, "1000000000");
        workspace.TryRegisterInputOutputUnit("Tbps", bu, "1000000000000");
        workspace.TryRegisterInputOutputUnit("Pbps", bu, "1000000000000000");
    }
}
