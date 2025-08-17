using MaxwellCalc.Core.Domains;
using MaxwellCalc.Core.Units;
using MaxwellCalc.Core.Workspaces;
using System;

namespace MaxwellCalc.Core.Parsers.Nodes
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
        public bool TryResolve<T>(IDomain<T> resolver, IWorkspace<T>? workspace, out Quantity<T> result) where T : struct, IFormattable
            => resolver.TryScalar(Content.ToString(), workspace, out result);
    }
}
