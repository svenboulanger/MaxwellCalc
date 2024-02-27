using MaxwellCalc.Parsers.Nodes;
using MaxwellCalc.Resolvers;
using MaxwellCalc.Units;
using System.Collections.Generic;

namespace MaxwellCalc.Workspaces
{
    /// <summary>
    /// Represents a workspace.
    /// </summary>
    public interface IWorkspace
    {
        /// <summary>
        /// Determines whether a string represents a unit.
        /// </summary>
        /// <param name="name">The unit name.</param>
        /// <returns>Returns <c>true</c> if the name represents a unit; otherwise, <c>false</c>.</returns>
        public bool IsUnit(string name);
    }

    /// <summary>
    /// Represents a workspace.
    /// </summary>
    public interface IWorkspace<T> : IWorkspace
    {
        /// <summary>
        /// Gets the variables in the workspace.
        /// </summary>
        public IVariableScope<T> Variables { get; }

        /// <summary>
        /// Tries to get a unit from the workspace.
        /// </summary>
        /// <param name="name">The name of the unit.</param>
        /// <param name="result">The result.</param>
        /// <returns>Returns <c>true</c> if the unit was found; otherwise, <c>false</c>.</returns>
        public bool TryGetUnit(string name, out Quantity<T> result);

        /// <summary>
        /// Tries to register a unit for the workspace.
        /// </summary>
        /// <param name="name">The name of the unit.</param>
        /// <param name="unit">The unit.</param>
        /// <returns>Returns <c>true</c> if the unit could be set; otherwise, <c>false</c>.</returns>
        public bool TryRegisterInputUnit(string name, T modifier, Unit unit);

        /// <summary>
        /// Tries to register a derived that can be used to format the output unit dimension.
        /// </summary>
        /// <param name="key">The base unit that this unit is derived from.</param>
        /// <param name="modifier">The modifier.</param>
        /// <param name="value">The resulting unit. Should be specified such that <paramref name="modifier"/> times <paramref name="value"/> is equal to <paramref name="input"/>.</param>
        /// <returns>Returns <c>true</c> if the unit could be set; otherwise, <c>false</c>.</returns>
        public bool TryRegisterDerivedUnit(Unit key, T modifier, Unit value);

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
        public bool TryRegisterFunction(string name, FunctionDelegate function);

        /// <summary>
        /// Tries to register a function for the given name and arguments.
        /// </summary>
        /// <param name="name">The name of the function.</param>
        /// <param name="arguments">The argument names.</param>
        /// <param name="expression">The expression.</param>
        /// <returns>Returns <c>true</c> if the user function was registered; otherwise, <c>false</c>.</returns>
        public bool TryRegisterUserFunction(string name, List<string> arguments, INode expression);

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
        /// Tries to resolve the naming of .
        /// </summary>
        /// <param name="quantity">The quantity.</param>
        /// <param name="result">The result.</param>
        /// <returns>Returns <c>true</c> if a naming was found; otherwise, <c>false</c>.</returns>
        public bool TryResolveNaming(Quantity<T> quantity, out Quantity<T> result);
    }
}
