using System;

namespace MaxwellCalc.Core.Parsers.Nodes
{
    /// <summary>
    /// A binary node.
    /// </summary>
    /// <param name="type">The operator type.</param>
    /// <param name="left">The left argument.</param>
    /// <param name="right">The right argument.</param>
    /// <param name="content">The content as input.</param>
    public class BinaryNode(BinaryOperatorTypes type, INode left, INode right, ReadOnlyMemory<char> content) : INode
    {
        /// <inheritdoc />
        public ReadOnlyMemory<char> Content { get; } = content;

        /// <summary>
        /// Gets the operator type.
        /// </summary>
        public BinaryOperatorTypes Type { get; } = type;

        /// <summary>
        /// Gets the left argument.
        /// </summary>
        public INode Left { get; } = left;

        /// <summary>
        /// Gets the right argument.
        /// </summary>
        public INode Right { get; } = right;
    }
}
