using MaxwellCalc.Core.Workspaces;
using MaxwellCalc.Parsers.Nodes;
using MaxwellCalc.Units;

namespace MaxwellCalc.Workspaces;

/// <summary>
/// Represents a scope of variables.
/// </summary>
public interface IVariableScope
{
    /// <summary>
    /// Gets the dictionary for the given variables.
    /// </summary>
    public IObservableDictionary<string, INode> Variables { get; }

    /// <summary>
    /// Tries to add a description to a variable.
    /// </summary>
    /// <param name="name">The name.</param>
    /// <param name="description">The description.</param>
    /// <returns>Returns <c>true</c> if the description was set; otherwise, <c>false</c>.</returns>
    public bool TrySetDescription(string name, string? description);

    /// <summary>
    /// Tries to get a description for the given variable.
    /// </summary>
    /// <param name="name">The name.</param>
    /// <param name="description">The description.</param>
    /// <returns>Returns <c>true</c> if the description was found; otherwise, <c>false</c>.</returns>
    public bool TryGetDescription(string name, out string? description);
}

/// <summary>
/// Represents a variable scope.
/// </summary>
public interface IVariableScope<T> : IVariableScope
{
    /// <summary>
    /// Tries to get a variable value from the workspace.
    /// </summary>
    /// <param name="name">The name of the variable.</param>
    /// <param name="result">The result.</param>
    /// <returns>Returns <c>true</c> if the variable was found; otherwise, <c>false</c>.</returns>
    public bool TryGetComputedVariable(string name, out Quantity<T> result);
}
