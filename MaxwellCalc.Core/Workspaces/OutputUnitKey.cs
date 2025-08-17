using MaxwellCalc.Core.Units;

namespace MaxwellCalc.Core.Workspaces;

/// <summary>
/// A structure that represents an output unit.
/// </summary>
/// <param name="OutputUnit">The output unit.</param>
/// <param name="BaseUnit">The base unit that the output unit is a derivation of.</param>
public record struct OutputUnitKey(Unit OutputUnit, Unit BaseUnit)
{
}
