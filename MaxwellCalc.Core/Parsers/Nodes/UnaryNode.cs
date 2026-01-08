using System;

namespace MaxwellCalc.Core.Parsers.Nodes;

/// <summary>
/// Creates a new <see cref="UnaryNode"/>.
/// </summary>
/// <param name="type">The operator type.</param>
/// <param name="argument">The argument.</param>
/// <param name="content">The content as input.</param>
public class UnaryNode(UnaryOperatorTypes type, INode argument, ReadOnlyMemory<char> content) : INode
{
    /// <inheritdoc />
    public ReadOnlyMemory<char> Content { get; } = content;

    /// <summary>
    /// Gets the operator type.
    /// </summary>
    public UnaryOperatorTypes Type { get; } = type;

    /// <summary>
    /// Gets the argument.
    /// </summary>
    public INode Argument { get; } = argument;
}
