using System;

namespace MaxwellCalc.Core.Parsers.Nodes;

/// <summary>
/// Represents a node in the abstract syntax tree.
/// </summary>
public interface INode
{
    /// <summary>
    /// Gets the node content as it was at the input.
    /// </summary>
    public ReadOnlyMemory<char> Content { get; }
}
