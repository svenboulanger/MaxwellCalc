using MaxwellCalc.Resolvers;
using MaxwellCalc.Workspaces;
using System;

namespace MaxwellCalc.Parsers.Nodes
{
    /// <summary>
    /// Creates a new <see cref="ScalarNode"/>.
    /// </summary>
    /// <param name="content">The content node.</param>
    public class ScalarNode(ReadOnlyMemory<char> content) : INode
    {
        /// <inheritdoc />
        public ReadOnlyMemory<char> Content { get; } = content;

        /// <inheritdoc />
        public bool TryResolve<T>(IResolver<T> resolver, IWorkspace<T> workspace, out T result)
            => resolver.TryScalar(Content.ToString(), workspace, out result);
    }
}
