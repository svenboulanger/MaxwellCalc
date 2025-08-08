using MaxwellCalc.Units;
using System;

namespace MaxwellCalc.Workspaces
{
    /// <summary>
    /// Event arguments for when an output unit changes.
    /// </summary>
    /// <param name="name">The name.</param>
    public class OutputUnitchangedEvent(Unit unit) : EventArgs
    {
        public Unit Name { get; } = unit;
    }
}
