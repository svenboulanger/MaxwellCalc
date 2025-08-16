using MaxwellCalc.Core.Workspaces;
using MaxwellCalc.Units;
using System;

namespace MaxwellCalc.Workspaces;

/// <summary>
/// Represents a scope of variables.
/// </summary>
public interface IVariableScope
{
    /// <summary>
    /// Gets the variables that are currently in the scope.
    /// </summary>
    public IReadonlyObservableDictionary<string, Variable<string>> Local { get; }

    /// <summary>
    /// Tries to remove a local variable.
    /// </summary>
    /// <param name="name">The name.</param>
    /// <returns>Returns <c>true</c> if a variable by this name was removed; otherwise, <c>false</c>.</returns>
    public bool TryRemoveVariable(string name);
}

/// <summary>
/// Represents a variable scope.
/// </summary>
public interface IVariableScope<T> : IVariableScope
{
    /// <summary>
    /// Gets a dictionary with the locally defined variables.
    /// </summary>
    public new IObservableDictionary<string, Variable<T>> Local { get; }

    /// <summary>
    /// Tries to get a variable value from the workspace.
    /// </summary>
    /// <param name="name">The name of the variable.</param>
    /// <param name="result">The result.</param>
    /// <returns>Returns <c>true</c> if the variable was found; otherwise, <c>false</c>.</returns>
    public bool TryGetComputedVariable(string name, out Quantity<T> result);
}
