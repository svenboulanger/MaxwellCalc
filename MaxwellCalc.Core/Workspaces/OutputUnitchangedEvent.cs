using MaxwellCalc.Core.Units;
using System;

namespace MaxwellCalc.Core.Workspaces;

/// <summary>
/// Event arguments for when an output unit changes.
/// </summary>
/// <param name="name">The name.</param>
public class OutputUnitchangedEvent(Unit outputUnit, Unit baseUnit) : EventArgs
{
    /// <summary>
    /// Gets the base units.
    /// </summary>
    public Unit BaseUnit { get; } = baseUnit;

    /// <summary>
    /// Gets the output units.
    /// </summary>
    public Unit OutputUnit { get; } = outputUnit;
}
