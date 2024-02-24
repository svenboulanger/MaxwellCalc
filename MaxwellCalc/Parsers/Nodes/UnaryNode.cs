using System;
using MaxwellCalc.Resolvers;
using MaxwellCalc.Workspaces;

namespace MaxwellCalc.Parsers.Nodes
{
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

        /// <inheritdoc />
        public bool TryResolve<T>(IResolver<T> resolver, IWorkspace<T> workspace, out T result)
        {
            if (!Argument.TryResolve(resolver, workspace, out var arg))
            {
                result = resolver.Default;
                return false;
            }

            return Type switch
            {
                UnaryOperatorTypes.Plus => resolver.TryPlus(arg, workspace, out result),
                UnaryOperatorTypes.Minus => resolver.TryMinus(arg, workspace, out result),
                UnaryOperatorTypes.Factorial => resolver.TryFactorial(arg, workspace, out result),
                _ => throw new NotImplementedException(),
            };
        }
    }
}
