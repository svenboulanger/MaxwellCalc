using MaxwellCalc.Resolvers;
using MaxwellCalc.Workspaces;
using System;

namespace MaxwellCalc.Parsers.Nodes
{
    /// <summary>
    /// Represents a node in the abstract syntax tree.
    /// </summary>
    public interface INode
    {
        /// <summary>
        /// Gets the node content as it was at the input.
        /// </summary>
        public ReadOnlyMemory<char> Content { get; }

        /// <summary>
        /// Resolves the node using an <see cref="IResolver{T}"/>.
        /// </summary>
        /// <typeparam name="T">The result type.</typeparam>
        /// <param name="resolver">The resolver.</param>
        /// <param name="workspace">The diagnostics message handler.</param>
        /// <returns>Returns the resolved quantity.</returns>
        public bool TryResolve<T>(IResolver<T> resolver, IWorkspace<T> workspace, out T result);
    }
}
