using MaxwellCalc.Units;

namespace MaxwellCalc.Workspaces;

/// <summary>
/// A variable information.
/// </summary>
/// <param name="Description">The description.</param>
/// <param name="Value">The value.</param>
public record struct Variable<T>(Quantity<T> Value, string? Description)
{
}
