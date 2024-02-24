using MaxwellCalc.Resolvers;
using MaxwellCalc.Workspaces;
using System;

namespace MaxwellCalc.Parsers.Nodes
{
    /// <summary>
    /// Creates a new <see cref="UnitNode"/>.
    /// </summary>
    /// <param name="content">The content as input.</param>
    public class UnitNode(ReadOnlyMemory<char> content) : INode
    {
        /// <inheritdoc />
        public ReadOnlyMemory<char> Content { get; } = content;

        /// <inheritdoc />
        public bool TryResolve<T>(IResolver<T> resolver, IWorkspace<T> workspace, out T result)
            => resolver.TryUnit(Content.ToString(), workspace, out result);
    }
}
