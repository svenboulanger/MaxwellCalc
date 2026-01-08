using System;

namespace MaxwellCalc.Core.Parsers.Nodes;

/// <summary>
/// Creates a new <see cref="UnitNode"/>.
/// </summary>
/// <param name="content">The content as input.</param>
public class UnitNode(ReadOnlyMemory<char> content) : INode
{
    /// <inheritdoc />
    public ReadOnlyMemory<char> Content { get; } = content;
}
