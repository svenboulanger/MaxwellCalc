using MaxwellCalc.Parsers.Nodes;
using MaxwellCalc.Units;

namespace MaxwellCalc.Workspaces;

/// <summary>
/// A constant.
/// </summary>
public readonly record struct Constant
{
    /// <summary>
    /// Gets the value of the constant.
    /// </summary>
    public INode Value { get; }

    /// <summary>
    /// Gets the value of the description.
    /// </summary>
    public string? Description { get; }

    /// <param name="value">The value of the constant.</param>
    /// <param name="description">The description.</param>
    public Constant(INode value, string? description)
    {
        Value = value;
        Description = description;
    }
}
