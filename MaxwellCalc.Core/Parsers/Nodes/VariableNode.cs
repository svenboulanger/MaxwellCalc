using System;

namespace MaxwellCalc.Core.Parsers.Nodes
{
    /// <summary>
    /// Creates a new <see cref="VariableNode"/>.
    /// </summary>
    /// <param name="content">The content as input.</param>
    public class VariableNode(ReadOnlyMemory<char> content) : INode
    {
        /// <inheritdoc />
        public ReadOnlyMemory<char> Content { get; } = content;
    }
}
