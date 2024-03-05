using MaxwellCalc.Parsers.Nodes;
using MaxwellCalc.Resolvers;
using MaxwellCalc.Units;
using System;
using System.Collections.Generic;

namespace MaxwellCalc.Workspaces
{
    /// <summary>
    /// Represents a workspace.
    /// </summary>
    public interface IWorkspace
    {
        /// <summary>
        /// Gets or sets an error message.
        /// </summary>
        public string? ErrorMessage { get; set; }

        /// <summary>
        /// Gets the variables defined in the workspace.
        /// </summary>
        public IEnumerable<(string, Quantity<string>)> Variables { get; }

        /// <summary>
        /// Gets the input units defined in the workspace.
        /// </summary>
        public IEnumerable<(string, Quantity<string>)> InputUnits { get; }

        /// <summary>
        /// Gets the output units defined in the workspace.
        /// </summary>
        public IEnumerable<(Unit, Quantity<string>)> OutputUnits { get; }

        /// <summary>
        /// Gets the user functions defined in the workspace.
        /// </summary>
        public IEnumerable<(string Name, string[] parameters, INode body)> UserFunctions { get; }

        /// <summary>
        /// Determines whether a string represents a unit.
        /// </summary>
        /// <param name="name">The unit name.</param>
        /// <returns>Returns <c>true</c> if the name represents a unit; otherwise, <c>false</c>.</returns>
        public bool IsUnit(string name);

        /// <summary>
        /// Tries to register a unit for the workspace.
        /// </summary>
        /// <param name="name">The name of the unit.</param>
        /// <param name="quantity">The quantity.</param>
        /// <returns>Returns <c>true</c> if the unit could be set; otherwise, <c>false</c>.</returns>
        public bool TryRegisterInputUnit(string name, Quantity<string> quantity);

        /// <summary>
        /// Tries to register a unit for the workspace.
        /// </summary>
        /// <param name="name">The name.</param>
        /// <param name="node">The node.</param>
        /// <returns>Returns <c>true</c> if the unit could be resolved and set; otherwise, <c>false</c>.</returns>
        public bool TryRegisterInputUnit(string name, INode node);

        /// <summary>
        /// Tries to remove a unit from the workspace that can be used as an input.
        /// </summary>
        /// <param name="name">The name of the unit.</param>
        /// <returns>Returns <c>true</c> if the unit was removed; otherwise, <c>false</c>.</returns>
        public bool TryRemoveInputUnit(string name);

        /// <summary>
        /// Tries to register an output unit. This unit can be used to format units of results.
        /// </summary>
        /// <param name="outputUnits">The unit that can be used for output.</param>
        /// <param name="quantity">The equivalent quantity in base units.</param>
        /// <returns>Returns <c>true</c> if the unit could be set; otherwise, <c>false</c>.</returns>
        public bool TryRegisterOutputUnit(Unit outputUnits, Quantity<string> quantity);

        /// <summary>
        /// Tries to register an output unit. This unit can be used to format units of results.
        /// </summary>
        /// <param name="outputUnits">The unit that can be used for output.</param>
        /// <param name="value">The equivalent quantity in base units.</param>
        /// <returns>Returns <c>true</c> if the unit could be set; otherwise, <c>false</c>.</returns>
        public bool TryRegisterOutputUnit(INode outputUnits, INode value);

        /// <summary>
        /// Tries to remove an output unit.
        /// </summary>
        /// <param name="outputUnit">The output units.</param>
        /// <param name="baseUnits">The base units.</param>
        /// <returns>Returns <c>true</c> if the unit was removed; otherwise, <c>false</c>.</returns>
        public bool TryRemoveOutputUnit(Unit outputUnits, Unit baseUnits);

        /// <summary>
        /// Tries to register a function for the given name and arguments.
        /// </summary>
        /// <param name="name">The name of the function.</param>
        /// <param name="arguments">The argument names.</param>
        /// <param name="expression">The expression.</param>
        /// <returns>Returns <c>true</c> if the user function was registered; otherwise, <c>false</c>.</returns>
        public bool TryRegisterUserFunction(string name, List<string> arguments, INode expression);

        /// <summary>
        /// Tries to remove a user function with the given name and number of arguments.
        /// </summary>
        /// <param name="name">The name.</param>
        /// <param name="argumentCount">The argument count.</param>
        /// <returns>Returns <c>true</c> if the user function was removed; otherwise, <c>false</c>.</returns>
        public bool TryRemoveUserFunction(string name, int argumentCount);

        /// <summary>
        /// Resolves a node.
        /// </summary>
        /// <param name="node">The node.</param>
        /// <param name="result">The formatted result.</param>
        /// <returns>Returns <c>true</c> if the node was resolved; otherwise, <c>false</c>.</returns>
        public bool TryResolveAndFormat(INode node, out Quantity<string> result);

        /// <summary>
        /// Resolves a node.
        /// </summary>
        /// <param name="node">The node.</param>
        /// <param name="format">The format.</param>
        /// <param name="formatProvider">The format provider.</param>
        /// <param name="result">The formatted result.</param>
        /// <returns>Returns <c>true</c> if the node was resolved; otherwise, <c>false</c>.</returns>
        public bool TryResolveAndFormat(INode node, string? format, IFormatProvider? formatProvider, out Quantity<string> result);
    }

    /// <summary>
    /// Represents a workspace.
    /// </summary>
    public interface IWorkspace<T> : IWorkspace where T : struct, IFormattable
    {
        /// <summary>
        /// Gets the resolver.
        /// </summary>
        public IResolver<T> Resolver { get; }

        /// <summary>
        /// Gets the variables in the workspace.
        /// </summary>
        public IVariableScope<T> Scope { get; }

        /// <summary>
        /// Tries to get a unit from the workspace.
        /// </summary>
        /// <param name="name">The name of the unit.</param>
        /// <param name="result">The result.</param>
        /// <returns>Returns <c>true</c> if the unit was found; otherwise, <c>false</c>.</returns>
        public bool TryGetUnit(string name, out Quantity<T> result);

        /// <summary>
        /// A delegate for functions.
        /// </summary>
        /// <param name="list">The list of arguments.</param>
        /// <param name="workspace">The workspace.</param>
        /// <param name="result">The result.</param>
        /// <returns>Returns <c>true</c> if the function was evaluated correctly; otherwise, <c>false</c>.</returns>
        public delegate bool FunctionDelegate(IReadOnlyList<Quantity<T>> list, IWorkspace<T> workspace, out Quantity<T> result);

        /// <summary>
        /// Tries to register a function under the given name.
        /// </summary>
        /// <param name="name">The name.</param>
        /// <param name="function">The function.</param>
        /// <returns>Returns <c>true</c> if the function was registered; otherwise, <c>false</c>.</returns>
        public bool TryRegisterBuiltInFunction(string name, FunctionDelegate function);

        /// <summary>
        /// Tries to evaluate a function for the given arguments.
        /// </summary>
        /// <param name="name">The name.</param>
        /// <param name="arguments">The arguments.</param>
        /// <param name="resolver">The resolver.</param>
        /// <param name="result">The result.</param>
        /// <returns>Returns <c>true</c> if the function was evaluated; otherwise, <c>false</c>.</returns>
        public bool TryFunction(string name, IReadOnlyList<Quantity<T>> arguments, IResolver<T> resolver, out Quantity<T> result);

        /// <summary>
        /// Tries to resolve the output units.
        /// </summary>
        /// <param name="quantity">The quantity.</param>
        /// <param name="result">The result.</param>
        /// <returns>Returns <c>true</c> if a naming was found; otherwise, <c>false</c>.</returns>
        public bool TryResolveOutputUnits(Quantity<T> quantity, out Quantity<T> result);

        /// <summary>
        /// Tries to format a quantity.
        /// </summary>
        /// <param name="quantity">The quantity.</param>
        /// <param name="format">The format.</param>
        /// <param name="formatProvider">The format provider.</param>
        /// <param name="formatted">The formatted quantity.</param>
        /// <returns>Returns <c>true</c> if the quantity was formatted; otherwise, <c>false</c>.</returns>
        public bool TryFormat(Quantity<T> quantity, string? format, IFormatProvider? formatProvider, out Quantity<string> formatted);
    }
}
