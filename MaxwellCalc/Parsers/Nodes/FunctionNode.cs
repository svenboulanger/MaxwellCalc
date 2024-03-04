using MaxwellCalc.Resolvers;
using MaxwellCalc.Units;
using MaxwellCalc.Workspaces;
using System;
using System.Collections.Generic;

namespace MaxwellCalc.Parsers.Nodes
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

        /// <inheritdoc />
        public bool TryResolve<T>(IResolver<T> resolver, IWorkspace<T>? workspace, out Quantity<T> result) where T : struct, IFormattable
        {
            // Evaluate the arguments
            var args = new List<Quantity<T>>(Arguments.Count);
            for (int i = 0; i < Arguments.Count; i++)
            {
                if (!Arguments[i].TryResolve(resolver, workspace, out var arg))
                {
                    result = resolver.Default;
                    return false;
                }
                args.Add(arg);
            }
            return resolver.TryFunction(Name, args, workspace, out result);
        }
    }
}
