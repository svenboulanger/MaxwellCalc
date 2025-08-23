using MaxwellCalc.Core.Domains;
using MaxwellCalc.Core.Units;
using MaxwellCalc.Core.Workspaces;
using System;
using System.Collections.Generic;

namespace MaxwellCalc.Core.Parsers.Nodes
{
    /// <summary>
    /// Creates a new <see cref="FunctionNode"/>.
    /// </summary>
    /// <param name="name">The name.</param>
    /// <param name="arguments">The arguments.</param>
    /// <param name="content">The content as input.</param>
    public class FunctionNode(string name, List<INode>? arguments, ReadOnlyMemory<char> content) : INode
    {
        /// <inheritdoc />
        public ReadOnlyMemory<char> Content { get; } = content;

        /// <summary>
        /// Gets the name of the function.
        /// </summary>
        public string Name { get; } = name;

        /// <summary>
        /// Gets the arguments of the function.
        /// </summary>
        public IReadOnlyList<INode> Arguments { get; } = arguments is not null ? arguments.AsReadOnly() : Array.Empty<INode>();
    }
}
