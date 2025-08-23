using MaxwellCalc.Core.Dictionaries;
using MaxwellCalc.Core.Domains;
using MaxwellCalc.Core.Parsers.Nodes;
using MaxwellCalc.Core.Units;
using MaxwellCalc.Core.Workspaces.Variables;
using System;
using System.Collections.Generic;

namespace MaxwellCalc.Core.Workspaces;

/// <summary>
/// Represents a workspace.
/// </summary>
public interface IWorkspace : IDiagnosticsHandler
{
    /// <summary>
    /// Gets the scalar type for the workspace. The workspace should also implement the <see cref="IWorkspace{T}"/> interface
    /// with this type being the scalar type.
    /// </summary>
    public Type ScalarType { get; }

    /// <summary>
    /// Gets or sets whether the workspace allows units to be used in expressions.
    /// </summary>
    public bool AllowUnits { get; set; }

    /// <summary>
    /// Gets or sets whether the workspace allows variables to be used in expressions.
    /// </summary>
    public bool AllowVariables { get; set; }

    /// <summary>
    /// Gets or sets whether the workspace allows user functions to be used in expressions.
    /// </summary>
    public bool AllowUserFunctions { get; set; }

    /// <summary>
    /// Gets or sets whether the workspace allows built-in functions to be used in expressions.
    /// </summary>
    public bool AllowBuiltInFunctions { get; set; }

    /// <summary>
    /// Gets or sets whether units should be resolved using input units.
    /// </summary>
    public bool ResolveInputUnits { get; set; }

    /// <summary>
    /// Gets or sets whether units should be converted back to possible output units.
    /// </summary>
    public bool ResolveOutputUnits { get; set; }

    /// <summary>
    /// Gets the variables defined in the workspace. The key represents the name
    /// of the variable.
    /// </summary>
    public IVariableScope Variables { get; }

    /// <summary>
    /// Gets the constants defined in the workspace. The key represents the name
    /// of the constant.
    /// </summary>
    public IVariableScope Constants { get; }

    /// <summary>
    /// Gets the input units.
    /// </summary>
    public IReadOnlyObservableDictionary<string, Quantity<string>> InputUnits { get; }

    /// <summary>
    /// Gets the output units.
    /// </summary>
    public IReadOnlyObservableDictionary<OutputUnitKey, string> OutputUnits { get; }

    /// <summary>
    /// Gets the user functions defined in the workspace. The key represents the name
    /// and argument count of the user function.
    /// </summary>
    public IObservableDictionary<UserFunctionKey, UserFunction> UserFunctions { get; }

    /// <summary>
    /// Gets the built-in functions registered to the workspace. The key represents the
    /// name of the built-in function.
    /// </summary>
    public IObservableDictionary<string, BuiltInFunction> BuiltInFunctions { get; }

    /// <summary>
    /// Tries to resolve a node and formats it.
    /// </summary>
    /// <param name="node">The node.</param>
    /// <param name="result">The formatted result.</param>
    /// <returns>Returns <c>true</c> if the node was resolved; otherwise, <c>false</c>.</returns>
    public bool TryResolveAndFormat(INode node, out Quantity<string> result);

    /// <summary>
    /// Tries to resolve a node and formats it.
    /// </summary>
    /// <param name="node">The node.</param>
    /// <param name="format">The format.</param>
    /// <param name="formatProvider">The format provider.</param>
    /// <param name="result">The formatted result.</param>
    /// <returns>Returns <c>true</c> if the node was resolved; otherwise, <c>false</c>.</returns>
    public bool TryResolveAndFormat(INode node, string? format, IFormatProvider? formatProvider, out Quantity<string> result);

    /// <summary>
    /// Tries to remove an input unit from the workspace.
    /// </summary>
    /// <param name="key">The input unit name.</param>
    /// <returns>Returns <c>true</c> if the input unit was removed; otherwise, <c>false</c>.</returns>
    public bool TryRemoveInputUnit(string key);

    /// <summary>
    /// Tries to assign an input unit to the workspace.
    /// </summary>
    /// <param name="key">The name of the input unit.</param>
    /// <param name="node">The node representing the expression on how to calculate the input unit.</param>
    /// <returns>Returns <c>true</c> if the input unit was assigned; otherwise, <c>false</c>.</returns>
    public bool TryAssignInputUnit(string key, INode node);

    /// <summary>
    /// Tries to remove an output unit from the workspace.
    /// </summary>
    /// <param name="key">The output unit pair.</param>
    /// <returns>Returns <c>true</c> if the output unit was removed; otherwise, <c>false</c>.</returns>
    public bool TryRemoveOutputUnit(OutputUnitKey key);

    /// <summary>
    /// Tries to assign an output unit to the workspace.
    /// </summary>
    /// <param name="outputUnits">The unit representing the output unit.</param>
    /// <param name="node">The value of the output unit, which will be resolved to base units. It is allowed to specify this argument using existing input units.</param>
    /// <returns>Returns <c>true</c> if the output unit was assigned; otherwise, <c>false</c>.</returns>
    public bool TryAssignOutputUnit(INode outputUnits, INode node);
}

/// <summary>
/// Represents a workspace.
/// </summary>
public interface IWorkspace<T> : IWorkspace where T : IFormattable
{
    /// <summary>
    /// Gets the resolver.
    /// </summary>
    public IDomain<T> Resolver { get; }

    /// <summary>
    /// Gets the current scope.
    /// </summary>
    public IVariableScope<T> Scope { get; }

    /// <summary>
    /// Gets the input units defined in the workspace. The key represents the
    /// unit that should be mapped to base units.
    /// </summary>
    public new IObservableDictionary<string, Quantity<T>> InputUnits { get; }

    /// <summary>
    /// Gets the output units defined in the workspace. The key represents a set of
    /// base units and output units.
    /// </summary>
    public new IObservableDictionary<OutputUnitKey, T> OutputUnits { get; }

    /// <summary>
    /// Tries to resolve a node to its result.
    /// </summary>
    /// <param name="node">The node.</param>
    /// <param name="result">The result.</param>
    /// <returns>Returns <c>true</c> if the node could be resolved; otherwise, <c>false</c>.</returns>
    public bool TryResolve(INode node, out Quantity<T> result);

    /// <summary>
    /// Tries to resolve the output units.
    /// </summary>
    /// <param name="quantity">The quantity.</param>
    /// <param name="result">The result.</param>
    /// <returns>Returns <c>true</c> if a naming was found; otherwise, <c>false</c>.</returns>
    public bool TryResolveOutputUnits(Quantity<T> quantity, out Quantity<T> result);

    /// <summary>
    /// A delegate for built-in functions.
    /// </summary>
    /// <param name="list">The list of arguments.</param>
    /// <param name="workspace">The workspace.</param>
    /// <param name="result">The result.</param>
    /// <returns>Returns <c>true</c> if the function was evaluated correctly; otherwise, <c>false</c>.</returns>
    public delegate bool BuiltInFunctionDelegate(IReadOnlyList<Quantity<T>> list, IDiagnosticsHandler? workspace, out Quantity<T> result);
}
