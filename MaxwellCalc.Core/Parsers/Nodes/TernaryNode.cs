using MaxwellCalc.Core.Domains;
using MaxwellCalc.Core.Units;
using MaxwellCalc.Core.Workspaces;
using System;

namespace MaxwellCalc.Core.Parsers.Nodes;

/// <summary>
/// A ternary node.
/// </summary>
/// <param name="type">The operator type.</param>
/// <param name="a">The first argument.</param>
/// <param name="b">The second argument.</param>
/// <param name="c">The third argument.</param>
/// <param name="content">The content as input.</param>
public class TernaryNode(TernaryNodeTypes type, INode a, INode b, INode c, ReadOnlyMemory<char> content) : INode
{
    /// <inheritdoc />
    public ReadOnlyMemory<char> Content { get; } = content;

    /// <summary>
    /// Gets the operator type.
    /// </summary>
    public TernaryNodeTypes Type { get; } = type;

    /// <summary>
    /// Gets the first argument.
    /// </summary>
    public INode A { get; } = a;

    /// <summary>
    /// Gets the second argument.
    /// </summary>
    public INode B { get; } = b;

    /// <summary>
    /// Gets the third argument.
    /// </summary>
    public INode C { get; } = c;
}
