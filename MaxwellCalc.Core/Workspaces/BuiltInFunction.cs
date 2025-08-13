using System;

namespace MaxwellCalc.Workspaces;

/// <summary>
/// The description for a built-in function.
/// </summary>
public readonly record struct BuiltInFunction
{
    /// <summary>
    /// Gets the name of the function.
    /// </summary>
    public string Name { get; }

    /// <summary>
    /// Gets the minimum argument count.
    /// </summary>
    public int MinimumArgumentCount { get; }

    /// <summary>
    /// Gets the maximum argument count.
    /// </summary>
    public int MaximumArgumentCount { get; }

    /// <summary>
    /// Gets the description.
    /// </summary>
    public string Description { get; }

    /// <summary>
    /// Gets the function.
    /// </summary>
    public Delegate Function { get; }

    /// <summary>
    /// Creates a new <see cref="BuiltInFunction"/>.
    /// </summary>
    /// <param name="name">The name.</param>
    /// <param name="minArgs">The minimum number of arguments.</param>
    /// <param name="maxArgs">The maximum number of arguments.</param>
    /// <param name="description">The description.</param>
    public BuiltInFunction(string name, int minArgs, int maxArgs, string description, Delegate function)
    {
        Name = name;
        MinimumArgumentCount = minArgs;
        MaximumArgumentCount = maxArgs;
        Description = description;
        Function = function;
    }
}
