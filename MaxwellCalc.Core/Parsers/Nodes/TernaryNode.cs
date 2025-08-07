using MaxwellCalc.Domains;
using MaxwellCalc.Parsers.Nodes;
using MaxwellCalc.Units;
using MaxwellCalc.Workspaces;
using System;

namespace MaxwellCalc.Core.Parsers.Nodes
{
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

        /// <inheritdoc />
        public bool TryResolve<T>(IDomain<T> resolver, IWorkspace<T>? workspace, out Quantity<T> result) where T : struct, IFormattable
        {
            switch (Type)
            {
                case TernaryNodeTypes.Condition:
                    if (!A.TryResolve(resolver, workspace, out var condition) ||
                        !resolver.TryIsTrue(condition, workspace, out bool conditionResult))
                    {
                        result = resolver.Default;
                        return false;
                    }

                    if (conditionResult)
                    {
                        if (!B.TryResolve(resolver, workspace, out result))
                            return false;
                    }
                    else
                    {
                        if (!C.TryResolve(resolver, workspace, out result))
                            return false;
                    }
                    return true;

                default:
                    throw new NotImplementedException();
            }
        }
    }
}
