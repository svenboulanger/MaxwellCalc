using MaxwellCalc.Core.Units;
using System.Collections.Generic;

namespace MaxwellCalc.Notebook.ViewModels.Overlay;

/// <summary>
/// Names the physical quantity a base unit represents (Length, Mass, Time, …), for grouping the output
/// units in the command palette. Falls back to the base unit's own string for combinations not in the
/// known map, so every output unit still lands in a labelled group.
/// </summary>
internal static class PhysicalQuantity
{
    // Keyed by the canonical base-unit string (Unit.ToString(), dimensions sorted by symbol).
    private static readonly Dictionary<string, string> Names = new()
    {
        [""] = "Dimensionless",
        ["m"] = "Length",
        ["m^2"] = "Area",
        ["m^3"] = "Volume",
        ["kg"] = "Mass",
        ["s"] = "Time",
        ["A"] = "Current",
        ["K"] = "Temperature",
        ["mol"] = "Amount of substance",
        ["cd"] = "Luminous intensity",
        ["rad"] = "Angle",
        ["s^-1"] = "Frequency",
        ["m s^-1"] = "Speed",
        ["m s^-2"] = "Acceleration",
        ["kg m s^-2"] = "Force",
        ["kg m^-1 s^-2"] = "Pressure",
        ["kg m^2 s^-2"] = "Energy",
        ["kg m^2 s^-3"] = "Power",
        ["A s"] = "Charge",
        ["kg m^2 s^-3 A^-1"] = "Voltage",
        ["kg m^2 s^-3 A^-2"] = "Resistance",
        ["kg^-1 m^-2 s^3 A^2"] = "Conductance",
        ["kg^-1 m^-2 s^4 A^2"] = "Capacitance",
        ["kg m^2 s^-2 A^-2"] = "Inductance",
        ["kg m^2 s^-2 A^-1"] = "Magnetic flux",
        ["kg s^-2 A^-1"] = "Magnetic flux density",
    };

    /// <summary>Gets a display label for the physical quantity of the given base unit.</summary>
    /// <param name="baseUnit">The base unit.</param>
    /// <returns>Returns a friendly label, or the base unit's string when unknown.</returns>
    public static string Label(Unit baseUnit)
    {
        string key = baseUnit.ToString();
        if (Names.TryGetValue(key, out var name))
            return name;
        return key.Length == 0 ? "Dimensionless" : key;
    }
}
