using MaxwellCalc.Core.Workspaces;
using MaxwellCalc.Domains;
using MaxwellCalc.Parsers.Nodes;
using MaxwellCalc.Units;
using System;
using System.Collections.Generic;

namespace MaxwellCalc.Workspaces;

/// <summary>
/// Represents a workspace.
/// </summary>
public interface IWorkspace
{
    /// <summary>
    /// An event that is called when a diagnostic message is posted.
    /// </summary>
    public event EventHandler<DiagnosticMessagePostedEventArgs>? DiagnosticMessagePosted;

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
    /// Gets the input units defined in the workspace. The key represents the
    /// unit that should be mapped to base units.
    /// </summary>
    public IObservableDictionary<string, Quantity<INode>> InputUnits { get; }

    /// <summary>
    /// Gets the output units defined in the workspace. The key represents a set of
    /// base units and output units.
    /// </summary>
    public IObservableDictionary<OutputUnitKey, INode> OutputUnits { get; }

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
    /// A method that can be called to post a diagnostic message.
    /// </summary>
    /// <param name="args">The event arguments.</param>
    public void PostDiagnosticMessage(DiagnosticMessagePostedEventArgs args);
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
    /// Gets the current scope.
    /// </summary>
    public IVariableScope<T> Scope { get; }

    /// <summary>
    /// Tries to resolve a node to its result.
    /// </summary>
    /// <param name="node">The node.</param>
    /// <param name="result">The result.</param>
    /// <returns>Returns <c>true</c> if the node could be resolved; otherwise, <c>false</c>.</returns>
    public bool TryResolve(INode node, out Quantity<T> result);

    /// <summary>
    /// Tries to evaluate a function for the given arguments.
    /// </summary>
    /// <param name="name">The name.</param>
    /// <param name="arguments">The arguments.</param>
    /// <param name="result">The result.</param>
    /// <returns>Returns <c>true</c> if the function was evaluated; otherwise, <c>false</c>.</returns>
    public bool TryCalculateFunction(string name, IReadOnlyList<INode> arguments, out Quantity<T> result);

    /// <summary>
    /// Tries to resolve the output units.
    /// </summary>
    /// <param name="quantity">The quantity.</param>
    /// <param name="result">The result.</param>
    /// <returns>Returns <c>true</c> if a naming was found; otherwise, <c>false</c>.</returns>
    public bool TryResolveOutputUnits(Quantity<T> quantity, out Quantity<T> result);

    /// <summary>
    /// Tries to get a unit from a name.
    /// </summary>
    /// <param name="name">The name.</param>
    /// <param name="quantity">The quantity.</param>
    /// <returns>Returns <c>true</c> if the unit could be found; otherwise, <c>false</c>.</returns>
    public bool TryGetUnit(string name, out Quantity<T> quantity);

    /// <summary>
    /// A delegate for built-in functions.
    /// </summary>
    /// <param name="list">The list of arguments.</param>
    /// <param name="workspace">The workspace.</param>
    /// <param name="result">The result.</param>
    /// <returns>Returns <c>true</c> if the function was evaluated correctly; otherwise, <c>false</c>.</returns>
    public delegate bool BuiltInFunctionDelegate(IReadOnlyList<Quantity<T>> list, IWorkspace workspace, out Quantity<T> result);
}
