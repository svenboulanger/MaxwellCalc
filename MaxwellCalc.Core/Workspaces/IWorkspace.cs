using MaxwellCalc.Domains;
using MaxwellCalc.Parsers.Nodes;
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
        /// Gets or sets a diagnostic message.
        /// </summary>
        public string? DiagnosticMessage { get; set; }

        /// <summary>
        /// Gets the variables defined in the workspace.
        /// </summary>
        public IEnumerable<Variable> Variables { get; }

        /// <summary>
        /// Gets the constants defined in the workspace.
        /// </summary>
        public IEnumerable<Variable> Constants { get; }

        /// <summary>
        /// Gets the input units defined in the workspace.
        /// </summary>
        public IEnumerable<InputUnit> InputUnits { get; }

        /// <summary>
        /// Gets the output units defined in the workspace.
        /// </summary>
        public IEnumerable<OutputUnit> OutputUnits { get; }

        /// <summary>
        /// Gets the user functions defined in the workspace.
        /// </summary>
        public IEnumerable<UserFunction> UserFunctions { get; }

        /// <summary>
        /// Gets the built-in functions registered to the workspace.
        /// </summary>
        public IEnumerable<BuiltInFunction> BuiltInFunctions { get; }

        /// <summary>
        /// Event that is called when a variable changes.
        /// </summary>
        public event EventHandler<VariableChangedEvent>? VariableChanged;

        /// <summary>
        /// Event that is called when a constant changes.
        /// </summary>
        public event EventHandler<VariableChangedEvent>? ConstantChanged;

        /// <summary>
        /// Event that is called when a function changes.
        /// </summary>
        public event EventHandler<FunctionChangedEvent>? FunctionChanged;

        /// <summary>
        /// Event that is called when a built-in function changes.
        /// </summary>
        public event EventHandler<FunctionChangedEvent>? BuiltInFunctionChanged;

        /// <summary>
        /// Event that is called when the input units change.
        /// </summary>
        public event EventHandler<InputUnitChangedEvent>? InputUnitChanged;

        /// <summary>
        /// Event that is called when the output units change.
        /// </summary>
        public event EventHandler<OutputUnitchangedEvent>? OutputUnitChanged;

        /// <summary>
        /// Determines whether a string represents a unit.
        /// </summary>
        /// <param name="name">The unit name.</param>
        /// <returns>Returns <c>true</c> if the name represents a unit; otherwise, <c>false</c>.</returns>
        public bool IsUnit(string name);

        /// <summary>
        /// Tries to register a unit for the workspace.
        /// </summary>
        /// <param name="inputUnit">The input unit.</param>
        /// <returns>Returns <c>true</c> if the unit could be set; otherwise, <c>false</c>.</returns>
        public bool TryRegisterInputUnit(InputUnit inputUnit);

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
        /// Tries to get an input unit information.
        /// </summary>
        /// <param name="name">The name.</param>
        /// <param name="unit">The unit.</param>
        /// <returns>Returns <c>true</c> if the unit was found; otherwise, <c>false</c>.</returns>
        public bool TryGetInputUnit(string name, out InputUnit unit);

        /// <summary>
        /// Tries to register an output unit. This unit can be used to format units of results.
        /// </summary>
        /// <param name="outputUnit">The output unit.</param>
        /// <returns>Returns <c>true</c> if the unit could be set; otherwise, <c>false</c>.</returns>
        public bool TryRegisterOutputUnit(OutputUnit outputUnit);

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
        /// <param name="outputUnits">The output units.</param>
        /// <param name="baseUnits">The base units.</param>
        /// <returns>Returns <c>true</c> if the unit was removed; otherwise, <c>false</c>.</returns>
        public bool TryRemoveOutputUnit(Unit outputUnits, Unit baseUnits);

        /// <summary>
        /// Tries to get an output unit information.
        /// </summary>
        /// <param name="name">The name.</param>
        /// <param name="unit">The unit.</param>
        /// <returns>Returns <c>true</c> if the unit was found; otherwise, <c>false</c>.</returns>
        public bool TryGetOutputUnit(Unit input, Unit output, out OutputUnit unit);

        /// <summary>
        /// Tries to register a function for the given name and arguments.
        /// </summary>
        /// <param name="userFunction">The user-defined function.</param>
        /// <returns>Returns <c>true</c> if the user function was registered; otherwise, <c>false</c>.</returns>
        public bool TryRegisterUserFunction(UserFunction userFunction);

        /// <summary>
        /// Tries to remove a user function with the given name and number of arguments.
        /// </summary>
        /// <param name="name">The name.</param>
        /// <param name="argumentCount">The argument count.</param>
        /// <returns>Returns <c>true</c> if the user function was removed; otherwise, <c>false</c>.</returns>
        public bool TryRemoveUserFunction(string name, int argumentCount);

        /// <summary>
        /// Tries to get a registered user function information.
        /// </summary>
        /// <param name="name">The name.</param>
        /// <param name="arguments">The function arguments.</param>
        /// <param name="function">The function information.</param>
        /// <returns>Returns <c>true</c> if the user function exists; otherwise, <c>false</c>.</returns>
        public bool TryGetUserFunction(string name, int arguments, out UserFunction function);

        /// <summary>
        /// Tries to get a registered built-in function information.
        /// </summary>
        /// <param name="name">The function name.</param>
        /// <param name="function">The function information.</param>
        /// <returns>Returns <c>true</c> if the built-in function exists; otherwise, <c>false</c>.</returns>
        public bool TryGetBuiltInFunction(string name, out BuiltInFunction function);

        /// <summary>
        /// Tries to set a variable on the workspace.
        /// </summary>
        /// <param name="variable">The variable.</param>
        /// <returns>Returns <c>true</c> if the variable was set; otherwise, <c>false</c>.</returns>
        public bool TrySetVariable(Variable variable);

        /// <summary>
        /// Tries to set a constant on the workspace.
        /// </summary>
        /// <remarks>A constant is different from a variable that it can be overwritten by variables, but not removed.</remarks>
        /// <param name="variable">The constant definition as a variable.</param>
        /// <returns>Returns <c>true</c> if the constant was set; otherwise, <c>false</c>.</returns>
        public bool TrySetConstant(Variable variable);

        /// <summary>
        /// Tries to remove a variable.
        /// </summary>
        /// <param name="name">The variable name.</param>
        /// <returns>Returns <c>true</c> if the variable was removed; otherwise, <c>false</c>.</returns>
        public bool TryRemoveVariable(string name);

        /// <summary>
        /// Tries to remove a constant.
        /// </summary>
        /// <param name="name">The name.</param>
        /// <returns>Returns <c>true</c> if the constant was removed; otherwise, <c>false</c>.</returns>
        public bool TryRemoveConstant(string name);

        /// <summary>
        /// Tries to get a variable from the workspace.
        /// </summary>
        /// <param name="name">The variable name.</param>
        /// <param name="value">The value of the variable.</param>
        /// <returns>Returns <c>true</c> if the variable was found; otherwise, <c>false</c>.</returns>
        public bool TryGetVariable(string name, out Quantity<string> value);

        /// <summary>
        /// Tries to get a variable from the workspace.
        /// </summary>
        /// <param name="name">The variable name.</param>
        /// <param name="value">The value of the variable.</param>
        /// <param name="description">The description.</param>
        /// <returns>Returns <c>true</c> if the variable was found; otherwise, <c>false</c>.</returns>
        public bool TryGetVariable(string name, out Quantity<string> value, out string? description);

        /// <summary>
        /// Tries to get a constant from the workspace.
        /// </summary>
        /// <param name="name">The constant name.</param>
        /// <param name="value">The constant value.</param>
        /// <param name="description">The description.</param>
        /// <returns>Returns <c>true</c> if the variable was found; otherwise, <c>false</c>.</returns>
        public bool TryGetConstant(string name, out Quantity<string> value, out string? description);

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
        /// <param name="resolveOutputUnits">If <c>true</c>, the units of the result</param>
        /// <param name="result">The formatted result.</param>
        /// <returns>Returns <c>true</c> if the node was resolved; otherwise, <c>false</c>.</returns>
        public bool TryResolveAndFormat(INode node, bool resolveOutputUnits, out Quantity<string> result);

        /// <summary>
        /// Resolves a node.
        /// </summary>
        /// <param name="node">The node.</param>
        /// <param name="format">The format.</param>
        /// <param name="formatProvider">The format provider.</param>
        /// <param name="result">The formatted result.</param>
        /// <returns>Returns <c>true</c> if the node was resolved; otherwise, <c>false</c>.</returns>
        public bool TryResolveAndFormat(INode node, string? format, IFormatProvider? formatProvider, bool resolveOutputUnits, out Quantity<string> result);

        /// <summary>
        /// Clears the workspace of input/output units, variables and user/built-in functions.
        /// </summary>
        public void Clear();
    }

    /// <summary>
    /// Represents a workspace.
    /// </summary>
    public interface IWorkspace<T> : IWorkspace where T : struct, IFormattable
    {
        /// <summary>
        /// Gets the resolver.
        /// </summary>
        public IDomain<T> Resolver { get; }

        /// <summary>
        /// Gets the constants scope in the workspace.
        /// </summary>
        public IVariableScope<T> ConstantsScope { get; }

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
        public delegate bool FunctionDelegate(IReadOnlyList<Quantity<T>> list, IWorkspace workspace, out Quantity<T> result);

        /// <summary>
        /// Tries to register a function under the given name.
        /// </summary>
        /// <param name="name">The name.</param>
        /// <param name="function">The function.</param>
        /// <param name="meta">The metadata for the function.</param>
        /// <returns>Returns <c>true</c> if the function was registered; otherwise, <c>false</c>.</returns>
        public bool TryRegisterBuiltInFunction(string name, FunctionDelegate function, BuiltInFunction meta);

        /// <summary>
        /// Tries to evaluate a function for the given arguments.
        /// </summary>
        /// <param name="name">The name.</param>
        /// <param name="arguments">The arguments.</param>
        /// <param name="resolver">The resolver.</param>
        /// <param name="result">The result.</param>
        /// <returns>Returns <c>true</c> if the function was evaluated; otherwise, <c>false</c>.</returns>
        public bool TryFunction(string name, IReadOnlyList<Quantity<T>> arguments, IDomain<T> resolver, out Quantity<T> result);

        /// <summary>
        /// Tries to remove a built-in function.
        /// </summary>
        /// <param name="name">The name.</param>
        /// <returns>Returns <c>true</c> if the function was removed; otherwise, <c>false</c>.</returns>
        public bool TryRemoveBuiltInFunction(string name);

        /// <summary>
        /// Tries to resolve the output units.
        /// </summary>
        /// <param name="quantity">The quantity.</param>
        /// <param name="result">The result.</param>
        /// <returns>Returns <c>true</c> if a naming was found; otherwise, <c>false</c>.</returns>
        public bool TryResolveOutputUnits(Quantity<T> quantity, out Quantity<T> result);
    }
}
