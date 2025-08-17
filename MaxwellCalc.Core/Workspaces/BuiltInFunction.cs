using System;

namespace MaxwellCalc.Workspaces;

/// <summary>
/// The description for a built-in function.
/// </summary>
/// <param name="Name">The name.</param>
/// <param name="MinimumArgumentCount">The minimum number of arguments.</param>
/// <param name="MaximumArgumentCount">The maximum number of arguments.</param>
/// <param name="Description">The description.</param>
/// <param name="Function">The function delegate.</param>
public record struct BuiltInFunction(string Name, int MinimumArgumentCount, int MaximumArgumentCount, string Description, Delegate Function)
{
}
