using MaxwellCalc.Core.Workspaces;
using MaxwellCalc.Domains;
using MaxwellCalc.Parsers.Nodes;
using MaxwellCalc.Units;
using System;
using System.Collections.Generic;

namespace MaxwellCalc.Workspaces;

/// <summary>
/// A workspace for a certain domain type.
/// </summary>
/// <typeparam name="T">The domain type.</typeparam>
public class Workspace<T> : IWorkspace<T> where T : struct, IFormattable
{
    private readonly Dictionary<string, Quantity<T>> _inputUnits = [];
    private readonly Dictionary<Unit, Dictionary<Unit, T>> _outputUnits = [];
    private readonly IVariableScope<T> _constantScope, _variableScope;
    private readonly Stack<IVariableScope<T>> _scopes = new();
    private readonly Dictionary<string, IWorkspace<T>.BuiltInFunctionDelegate> _builtInFunctions = [];

    /// <inheritdoc />
    public bool AllowUnits { get; set; } = true;

    /// <inheritdoc />
    public bool AllowVariables { get; set; } = true;

    /// <inheritdoc />
    public bool AllowUserFunctions { get; set; } = true;

    /// <inheritdoc />
    public bool AllowBuiltInFunctions { get; set; } = true;

    /// <inheritdoc />
    public bool ResolveInputUnits { get; set; } = true;

    /// <inheritdoc />
    public bool ResolveOutputUnits { get; set; } = true;

    /// <inheritdoc />
    public IDomain<T> Resolver { get; }

    /// <inheritdoc />
    public IVariableScope Variables => _variableScope;

    /// <inheritdoc />
    public IVariableScope Constants => _constantScope;

    /// <inheritdoc />
    public IVariableScope<T> Scope => _scopes.Peek();

    /// <inheritdoc />
    public event EventHandler<DiagnosticMessagePostedEventArgs>? DiagnosticMessagePosted;

    /// <inheritdoc />
    public IObservableDictionary<string, Quantity<INode>> InputUnits { get; } = new ObservableDictionary<string, Quantity<INode>>();

    /// <inheritdoc />
    public IObservableDictionary<OutputUnitKey, INode> OutputUnits { get; } = new ObservableDictionary<OutputUnitKey, INode>();

    /// <inheritdoc />
    public IObservableDictionary<UserFunctionKey, UserFunction> UserFunctions { get; } = new ObservableDictionary<UserFunctionKey, UserFunction>();

    /// <inheritdoc />
    public IObservableDictionary<string, BuiltInFunction> BuiltInFunctions { get; } = new ObservableDictionary<string, BuiltInFunction>();

    /// <summary>
    /// Creates a new <see cref="Workspace"/>.
    /// </summary>
    /// <param name="resolver">The resolver.</param>
    public Workspace(IDomain<T> resolver)
    {
        Resolver = resolver ?? throw new ArgumentNullException(nameof(resolver));
        _constantScope = new VariableScope<T>(this);
        _variableScope = new VariableScope<T>(this, _constantScope);
        _scopes.Push(_variableScope);

        // Register to our own dictionaries
        InputUnits.DictionaryChanged += InputUnitsChanged;
        OutputUnits.DictionaryChanged += OutputUnitsChanged;
        BuiltInFunctions.DictionaryChanged += BuiltInFunctionsChanged;
    }

    private void InputUnitsChanged(object sender, DictionaryChangedEventArgs<string, Quantity<INode>> e)
    {
        var oldAllowUnits = AllowUnits;
        AllowUnits = false;

        switch (e.Action)
        {
            case DictionaryChangeAction.Add:
            case DictionaryChangeAction.Replace:
                foreach (var item in e.Items)
                {
                    if (TryResolve(item.Value.Scalar, out var result))
                        _inputUnits[item.Key] = new(result.Scalar, item.Value.Unit);
                }
                break;

            case DictionaryChangeAction.Remove:
                foreach (var item in e.Items)
                    _inputUnits.Remove(item.Key);
                break;
        }

        AllowUnits = oldAllowUnits;
    }

    private void OutputUnitsChanged(object sender, DictionaryChangedEventArgs<OutputUnitKey, INode> e)
    {
        var oldAllowUnits = AllowUnits;
        AllowUnits = false;

        switch (e.Action)
        {
            case DictionaryChangeAction.Add:
                foreach (var item in e.Items)
                {
                    // Invert the scalar
                    if (TryResolve(item.Value, out var result) &&
                        Resolver.TryInvert(result, this, out var inverted))
                    {
                        if (!_outputUnits.TryGetValue(item.Key.BaseUnit, out var dict))
                        {
                            dict = [];
                            _outputUnits.Add(item.Key.BaseUnit, dict);
                        }
                        dict.Add(item.Key.OutputUnit, inverted.Scalar);
                    }
                }
                break;

            case DictionaryChangeAction.Replace:
                foreach (var item in e.Items)
                {
                    // Invert the scalar
                    if (TryResolve(item.Value, out var result) &&
                        Resolver.TryInvert(result, this, out var inverted))
                    {
                        // Replace the output unit
                        _outputUnits[item.Key.BaseUnit][item.Key.OutputUnit] = inverted.Scalar;
                    }
                }
                break;

            case DictionaryChangeAction.Remove:
                foreach (var item in e.Items)
                {
                    var dict = _outputUnits[item.Key.BaseUnit];
                    dict.Remove(item.Key.OutputUnit);
                    if (dict.Count == 0)
                        _outputUnits.Remove(item.Key.BaseUnit);
                }
                break;
        }

        AllowUnits = oldAllowUnits;
    }

    private void BuiltInFunctionsChanged(object sender, DictionaryChangedEventArgs<string, BuiltInFunction> e)
    {
        switch (e.Action)
        {
            case DictionaryChangeAction.Add:
            case DictionaryChangeAction.Replace:
                foreach (var item in e.Items)
                {
                    if (item.Value.Function is IWorkspace<T>.BuiltInFunctionDelegate function)
                        _builtInFunctions[item.Key] = function;
                    else
                        PostDiagnosticMessage(new DiagnosticMessagePostedEventArgs($"Could not register the built-in function {item.Key}"));
                }
                break;

            case DictionaryChangeAction.Remove:
                foreach (var item in e.Items)
                    _builtInFunctions.Remove(item.Key);
                break;
        }
    }

    /// <inheritdoc />
    public virtual void PostDiagnosticMessage(DiagnosticMessagePostedEventArgs args)
        => DiagnosticMessagePosted?.Invoke(this, args);

    /// <inheritdoc />
    public bool TryResolve(INode node, out Quantity<T> result)
    {
        if (node is BinaryNode bn)
        {
            switch (bn.Type)
            {
                case BinaryOperatorTypes.InUnit:
                    {
                        if (!AllowUnits)
                        {
                            PostDiagnosticMessage(new("Units are not allowed"));
                            result = default;
                            return false;
                        }

                        if (!bn.Left.TryResolve(Resolver, this, out var left) ||
                            !bn.Right.TryResolve(Resolver, this, out var right))
                        {
                            result = default;
                            return false;
                        }
                        else
                        {
                            if (left.Unit != right.Unit)
                            {
                                PostDiagnosticMessage(new("Base units do not match."));
                                result = default;
                                return false;
                            }
                            if (!Resolver.TryDivide(left, right, this, out var intermediary))
                            {
                                result = default;
                                return false;
                            }
                            result = new Quantity<T>(intermediary.Scalar, new((bn.Right.Content.ToString(), 1)));
                        }
                    }
                    break;

                case BinaryOperatorTypes.Assign:
                    if (!bn.TryResolve(Resolver, this, out result))
                    {
                        result = default;
                        return false;
                    }
                    if (bn.Left is FunctionNode fn)
                    {
                        result = default;
                        PostDiagnosticMessage(new($"Stored function '{fn.Name}'."));
                        return true;
                    }
                    break;

                default:
                    if (!bn.TryResolve(Resolver, this, out result))
                    {
                        result = default;
                        return false;
                    }

                    // Resolve output unit
                    if (AllowUnits && ResolveOutputUnits)
                        TryResolveOutputUnits(result, out result);
                    break;
            }
        }
        else
        {
            if (!node.TryResolve(Resolver, this, out result))
            {
                result = default;
                return false;
            }

            // Resolve output unit
            if (AllowUnits && ResolveOutputUnits)
                TryResolveOutputUnits(result, out result);
        }
        return true;
    }

    /// <inheritdoc />
    public bool TryCalculateFunction(string name, IReadOnlyList<INode> arguments, out Quantity<T> result)
    {
        // First we try to apply a user-defined function
        if (AllowUserFunctions)
        {
            var key = new UserFunctionKey(name, arguments.Count);
            if (UserFunctions.TryGetValue(key, out var userFunction))
            {
                // Push a new scope with the arguments
                var newScope = new VariableScope<T>(this, Scope);
                _scopes.Push(newScope);
                for (int i = 0; i < arguments.Count; i++)
                    newScope.Variables[userFunction.Parameters[i]] = arguments[i];

                // Evaluate the body
                foreach (var node in userFunction.Body)
                {
                    // Parse the function
                    if (!TryResolve(node, out result))
                        return false;
                }

                // Pop the scope with the arguments
                _scopes.Pop();
            }
        }

        // If the user-defined function doesn't exist, try to find a built-in function
        if (AllowBuiltInFunctions)
        {
            var args = new List<Quantity<T>>();
            foreach (var arg in arguments)
            {
                if (!TryResolve(arg, out var a))
                {
                    result = default;
                    return false;
                }
                args.Add(a);
            }

            if (_builtInFunctions.TryGetValue(name, out var function))
                return function(args, this, out result);
        }
        PostDiagnosticMessage(new($"Could not find a function with the name '{name}' and {arguments.Count} argument(s)."));
        result = default;
        return false;
    }

    /// <inheritdoc />
    public bool TryResolveOutputUnits(Quantity<T> quantity, out Quantity<T> result)
    {
        if (quantity.Unit.Dimension is null || quantity.Unit.Dimension.Count == 0)
        {
            result = quantity;
            return true;
        }

        // The output units are only expressable in base units, let's see if we can find some output for it
        if (!_outputUnits.TryGetValue(quantity.Unit, out var eligible) || eligible.Count == 0)
        {
            result = quantity;
            return false;
        }

        // There should be a fit for the quantity
        // Let's find the closest one that would lead to a scalar of 1-1000
        double bestFactor = double.NaN;
        Quantity<T> bestUnit = default;
        foreach (var p in eligible)
        {
            var q = new Quantity<T>(p.Value, p.Key);
            if (!Resolver.TryFactor(quantity, q, out double factor))
                continue;

            // If there wasn't a factor yet, just use the first one
            if (double.IsNaN(bestFactor))
            {
                bestFactor = factor;
                bestUnit = q;
                continue;
            }

            // Multiple factors are possible, let's try to pick the best one
            if (factor >= 1.0)
            {
                if (factor < 1000.0)
                {
                    // The new scalar is actually kind of a best fix
                    if (bestFactor < 1.0 || bestFactor >= 1000.0 || factor < bestFactor)
                    {
                        // The best scalar was not in the range 1-1000, or otherwise,
                        // let's choose the one that is the best match
                        bestFactor = factor;
                        bestUnit = q;
                    }
                }
                else if (factor < bestFactor)
                {
                    // The new scalar is too big, let's only take it if the best scalar was even bigger
                    bestFactor = factor;
                    bestUnit = q;
                }
            }
            else if (factor > bestFactor)
            {
                // The new scalar is too small, let's only take it if the best scalar was even smaller
                bestFactor = factor;
                bestUnit = q;
            }
        }

        // Make a quantity for it
        if (!Resolver.TryMultiply(quantity, bestUnit, this, out var scaled))
        {
            result = default;
            return false;
        }
        result = new Quantity<T>(scaled.Scalar, bestUnit.Unit);
        return true;
    }

    /// <inheritdoc />
    public bool TryResolveAndFormat(INode node, out Quantity<string> result)
        => TryResolveAndFormat(node, null, null, out result);

    /// <inheritdoc />
    public bool TryResolveAndFormat(INode node, string? format, IFormatProvider? formatProvider, out Quantity<string> result)
    {
        if (!TryResolve(node, out var r))
        {
            result = default;
            return false;
        }

        if (!Resolver.TryFormat(r, format, formatProvider, out result))
            return false;
        return true;
    }

    /// <inheritdoc />
    public bool TryGetUnit(string name, out Quantity<T> quantity)
        => _inputUnits.TryGetValue(name, out quantity);
}
