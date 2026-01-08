namespace MaxwellCalc.Core.Parsers.Nodes;

/// <summary>
/// The possible unary operator types.
/// </summary>
public enum UnaryOperatorTypes
{
    /// <summary>
    /// Unary plus (+a).
    /// </summary>
    Plus,

    /// <summary>
    /// Unary minus (-a).
    /// </summary>
    Minus,

    /// <summary>
    /// Factorial (a!).
    /// </summary>
    Factorial,

    /// <summary>
    /// Remove units operator ('a).
    /// </summary>
    RemoveUnits,
}
