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
        /// Determines whether a string represents a unit.
        /// </summary>
        /// <param name="name">The unit name.</param>
        /// <returns>Returns <c>true</c> if the name represents a unit; otherwise, <c>false</c>.</returns>
        public bool IsUnit(string name);

        /// <summary>
        /// Tries to register a unit for the workspace.
        /// </summary>
        /// <param name="name">The name of the unit.</param>
        /// <param name="unit">The unit.</param>
        /// <returns>Returns <c>true</c> if the unit could be set; otherwise, <c>false</c>.</returns>
        public bool TryRegisterInputUnit(string name, Unit unit);

        /// <summary>
        /// Tries to register a unit that can be used for output.
        /// </summary>
        /// <param name="name">The name.</param>
        /// <param name="unit">The unit.</param>
        /// <returns>Returns <c>true</c> if the unit could be set; otherwise, <c>false</c>.</returns>
        public bool TryRegisterOutputUnit(string name, Unit unit);

        /// <summary>
        /// Determines whether a string represents a variable.
        /// </summary>
        /// <param name="name">The variable name.</param>
        /// <returns>Returns <c>true</c> if the name represents a variable; otherwise, <c>false</c>.</returns>
        public bool IsVariable(string name);
    }

    /// <summary>
    /// Represents a workspace.
    /// </summary>
    public interface IWorkspace<T> : IWorkspace
    {
        /// <summary>
        /// Tries to get a unit from the workspace.
        /// </summary>
        /// <param name="name">The name of the unit.</param>
        /// <param name="result">The result.</param>
        /// <returns>Returns <c>true</c> if the unit was found; otherwise, <c>false</c>.</returns>
        public bool TryGetUnit(string name, out Quantity<T> result);

        /// <summary>
        /// Tries to get a variable value from the workspace.
        /// </summary>
        /// <param name="name">The name of the variable.</param>
        /// <param name="result">The result.</param>
        /// <returns>Returns <c>true</c> if the variable was found; otherwise, <c>false</c>.</returns>
        public bool TryGetVariable(string name, out Quantity<T> result);

        /// <summary>
        /// Tries to update a variable value from the workspace.
        /// </summary>
        /// <param name="name">The name.</param>
        /// <param name="value">The value.</param>
        /// <returns>Returns <c>true</c> if the variable was updated; otherwise, <c>false</c>.</returns>
        public bool TrySetVariable(string name, Quantity<T> value);

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
        /// Tries to evaluate a function for the given arguments.
        /// </summary>
        /// <param name="name">The name.</param>
        /// <param name="arguments">The arguments.</param>
        /// <param name="result">The result.</param>
        /// <returns>Returns <c>true</c> if the function was evaluated; otherwise, <c>false</c>.</returns>
        public bool TryFunction(string name, IReadOnlyList<Quantity<T>> arguments, out Quantity<T> result);

        /// <summary>
        /// Tries to resolve the naming of .
        /// </summary>
        /// <param name="quantity">The quantity.</param>
        /// <param name="result">The result.</param>
        /// <returns>Returns <c>true</c> if a naming was found; otherwise, <c>false</c>.</returns>
        public bool TryResolveNaming(Quantity<T> quantity, out Quantity<T> result);
    }
}
