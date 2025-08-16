using MaxwellCalc.Core.Workspaces;
using MaxwellCalc.Parsers.Nodes;
using MaxwellCalc.Units;
using System;
using System.Collections.Generic;

namespace MaxwellCalc.Workspaces;

/// <summary>
/// An implementation for a variable scope.
/// </summary>
/// <typeparam name="T">The base type.</typeparam>
public class VariableScope<T> : IVariableScope<T>
    where T : struct, IFormattable
{
    private readonly IVariableScope<T>? _parent;
    private readonly IWorkspace<T> _workspace;
    private readonly IReadOnlyObservableDictionary<string, Variable<string>> _mapped;

    /// <inheritdoc />
    public IEnumerable<string> VariableNames => Local.Keys;

    /// <inheritdoc />
    public IObservableDictionary<string, Variable<T>> Local { get; } = new ObservableDictionary<string, Variable<T>>();

    IReadOnlyObservableDictionary<string, Variable<string>> IVariableScope.Local => _mapped;

    /// <summary>
    /// Creates a new <see cref="VariableScope{T}"/>.
    /// </summary>
    public VariableScope(IWorkspace<T> workspace)
    {
        _parent = null;
        _workspace = workspace;
        _mapped = new MappedObservableDictionary<string, Variable<string>, Variable<T>>(Local, Format);
    }

    /// <summary>
    /// Creates a <see cref="VariableScope{T}"/>
    /// </summary>
    /// <param name="parent">The parent scope.</param>
    public VariableScope(IWorkspace<T> workspace, IVariableScope<T> parent)
    {
        _parent = parent;
        _workspace = workspace;
        _mapped = new MappedObservableDictionary<string, Variable<string>, Variable<T>>(Local, Format);
    }

    private Variable<string> Format(Variable<T> quantity)
    {
        if (_workspace.Resolver.TryFormat(quantity.Value, "g", System.Globalization.CultureInfo.InvariantCulture, out var value))
            return new(value, quantity.Description);
        return default;
    }

    /// <inheritdoc />
    bool IVariableScope<T>.TryGetComputedVariable(string name, out Quantity<T> result)
    {
        if (Local.TryGetValue(name, out var r))
        {
            result = default;
            return true;
        }

        // If we couldn't figure it out, ask the parent scope
        if (_parent is not null)
            return _parent.TryGetComputedVariable(name, out result);
        result = default;
        return false;
    }

    /// <inheritdoc />
    public bool TryAssignVariable(string name, INode node, string? description)
    {
        if (_workspace.TryResolve(node, out var result))
        {
            Local[name] = new(result, description);
            return true;
        }
        return false;
    }

    /// <inheritdoc />
    public bool TryRemoveVariable(string name)
        => Local.Remove(name);
}
