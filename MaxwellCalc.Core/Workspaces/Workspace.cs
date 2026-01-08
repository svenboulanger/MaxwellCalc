using MaxwellCalc.Core.Dictionaries;
using MaxwellCalc.Core.Domains;
using MaxwellCalc.Core.Parsers.Nodes;
using MaxwellCalc.Core.Units;
using MaxwellCalc.Core.Workspaces.Variables;
using System;
using System.Collections.Generic;

namespace MaxwellCalc.Core.Workspaces;

/// <summary>
/// A workspace for a certain domain type.
/// </summary>
/// <typeparam name="T">The domain type.</typeparam>
public class Workspace<T> : IWorkspace<T> where T : struct, IFormattable
{
    private readonly Dictionary<Unit, Dictionary<Unit, T>> _outputUnits = [];
    private readonly IVariableScope<T> _constantScope, _variableScope;
    private readonly Stack<IVariableScope<T>> _scopes = new();
    private readonly Dictionary<string, IWorkspace<T>.BuiltInFunctionDelegate> _builtInFunctions = [];
    private readonly IReadOnlyObservableDictionary<string, Quantity<string>> _mappedInputUnits;
    private readonly IReadOnlyObservableDictionary<OutputUnitKey, string> _mappedOutputUnits;

    /// <inheritdoc />
    public Type ScalarType => typeof(T);

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
    public IObservableDictionary<string, Quantity<T>> InputUnits { get; } = new ObservableDictionary<string, Quantity<T>>();

    /// <inheritdoc />
    public IObservableDictionary<OutputUnitKey, T> OutputUnits { get; } = new ObservableDictionary<OutputUnitKey, T>();

    /// <inheritdoc />
    IReadOnlyObservableDictionary<string, Quantity<string>> IWorkspace.InputUnits => _mappedInputUnits;

    /// <inheritdoc />
    IReadOnlyObservableDictionary<OutputUnitKey, string> IWorkspace.OutputUnits => _mappedOutputUnits;

    /// <inheritdoc />
    public IObservableDictionary<UserFunctionKey, UserFunction> UserFunctions { get; } = new ObservableDictionary<UserFunctionKey, UserFunction>();

    /// <inheritdoc />
    public IObservableDictionary<string, BuiltInFunction> BuiltInFunctions { get; } = new ObservableDictionary<string, BuiltInFunction>();

    /// <inheritdoc />
    public string AnswerVariable { get; set; } = string.Empty;

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
        _mappedInputUnits = new MappedObservableDictionary<string, Quantity<string>, Quantity<T>>(InputUnits, MapInputUnits);
        _mappedOutputUnits = new MappedObservableDictionary<OutputUnitKey, string, T>(OutputUnits, MapOutputUnits);
        OutputUnits.DictionaryChanged += OutputUnitsChanged;
        BuiltInFunctions.DictionaryChanged += BuiltInFunctionsChanged;
    }

    private Quantity<string> MapInputUnits(Quantity<T> quantity)
    {
        if (Resolver.TryFormat(quantity, null, null, out var formatted))
            return formatted;
        return default;
    }

    private string MapOutputUnits(T scalar)
    {
        if (Resolver.TryFormat(new(scalar, Unit.UnitNone), null, null, out var formatted))
            return formatted.Scalar;
        return string.Empty;
    }

    /// <summary>
    /// This method keeps our internal dictionary in sync with the definitions of the output units.
    /// </summary>
    private void OutputUnitsChanged(object sender, DictionaryChangedEventArgs<OutputUnitKey, T> e)
    {
        var oldAllowUnits = AllowUnits;
        AllowUnits = false;

        switch (e.Action)
        {
            case DictionaryChangeAction.Add:
                foreach (var item in e.Items)
                {
                    // Invert the scalar
                    if (Resolver.TryInvert(new(item.Value, Unit.UnitNone), this, out var inverted))
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
                    if (Resolver.TryInvert(new(item.Value, Unit.UnitNone), this, out var inverted))
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

                        if (!TryResolveNode(bn.Left, out var left) ||
                            !TryResolveNode(bn.Right, out var right))
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
                            return true;
                        }
                    }
            }
        }

        // Resolve the nodes
        if (!TryResolveNode(node, out result))
            return false;

        // Store the last quantity in a dedicated variable if required
        if (!string.IsNullOrEmpty(AnswerVariable) && AllowVariables)
        {
            if (_variableScope.Local.TryGetValue(AnswerVariable, out var existing))
                _variableScope.Local[AnswerVariable] = new Variable<T>(result, existing.Description);
            else
                _variableScope.Local.Add(AnswerVariable, new Variable<T>(result, "The last answer."));
        }

        // Resolve output units
        if (AllowUnits && ResolveOutputUnits)
            TryResolveOutputUnits(result, out result);
        return true;
    }

    private bool TryResolveNode(INode node, out Quantity<T> result)
    {
        return node switch
        {
            BinaryNode binary => TryResolveNode(binary, out result),
            ScalarNode scalar => TryResolveNode(scalar, out result),
            FunctionNode function => TryResolveNode(function, out result),
            VariableNode variable => TryResolveNode(variable, out result),
            UnaryNode unary => TryResolveNode(unary, out result),
            TernaryNode ternary => TryResolveNode(ternary, out result),
            UnitNode unit => TryResolveNode(unit, out result),
            _ => throw new NotImplementedException(),
        };
    }

    private bool TryResolveNode(BinaryNode binary, out Quantity<T> result)
    {
        switch (binary.Type)
        {
            case BinaryOperatorTypes.Assign:
                {
                    if (binary.Left is VariableNode variableNode)
                    {
                        if (!TryResolveNode(binary.Right, out result))
                        {
                            result = Resolver.Default;
                            return false;
                        }
                        string name = variableNode.Content.ToString();
                        Scope.Local[name] = new(result, null);
                        return true;
                    }
                    else if (binary.Left is FunctionNode function)
                    {
                        var args = new List<string>();
                        for (int i = 0; i < function.Arguments.Count; i++)
                        {
                            if (function.Arguments[i] is not VariableNode argNode)
                            {
                                PostDiagnosticMessage(new($"Function argument has to be a simple variable"));
                                result = Resolver.Default;
                                return false;
                            }
                        }
                        UserFunctions[new(function.Name, args.Count)] = new([.. args], [binary.Right]);
                        result = Resolver.Default;
                        return true;
                    }
                    else
                    {
                        PostDiagnosticMessage(new($"Can only assign to variables or user functions"));
                        result = Resolver.Default;
                        return false;
                    }
                }

            default:
                {
                    // Should be a regular operator
                    if (!TryResolveNode(binary.Left, out var left) ||
                        !TryResolveNode(binary.Right, out var right))
                    {
                        result = Resolver.Default;
                        return false;
                    }

                    return binary.Type switch
                    {
                        BinaryOperatorTypes.Add => Resolver.TryAdd(left, right, this, out result),
                        BinaryOperatorTypes.Subtract => Resolver.TrySubtract(left, right, this, out result),
                        BinaryOperatorTypes.Multiply => Resolver.TryMultiply(left, right, this, out result),
                        BinaryOperatorTypes.Divide => Resolver.TryDivide(left, right, this, out result),
                        BinaryOperatorTypes.IntDivide => Resolver.TryIntDivide(left, right, this, out result),
                        BinaryOperatorTypes.Modulo => Resolver.TryModulo(left, right, this, out result),
                        BinaryOperatorTypes.LeftShift => Resolver.TryLeftShift(left, right, this, out result),
                        BinaryOperatorTypes.RightShift => Resolver.TryRightShift(left, right, this, out result),
                        BinaryOperatorTypes.BitwiseAnd => Resolver.TryBitwiseAnd(left, right, this, out result),
                        BinaryOperatorTypes.BitwiseOr => Resolver.TryBitwiseOr(left, right, this, out result),
                        BinaryOperatorTypes.Exponent => Resolver.TryPow(left, right, this, out result),
                        BinaryOperatorTypes.GreaterThan => Resolver.TryGreaterThan(left, right, this, out result),
                        BinaryOperatorTypes.GreaterThanOrEqual => Resolver.TryGreaterThanOrEqual(left, right, this, out result),
                        BinaryOperatorTypes.LessThan => Resolver.TryLessThan(left, right, this, out result),
                        BinaryOperatorTypes.LessThanOrEqual => Resolver.TryLessThanOrEqual(left, right, this, out result),
                        BinaryOperatorTypes.Equal => Resolver.TryEquals(left, right, this, out result),
                        BinaryOperatorTypes.NotEqual => Resolver.TryNotEquals(left, right, this, out result),
                        // BinaryOperatorTypes.InUnit => Resolver.TryInUnit(left, right, binary.Right.Content, this, out result),
                        BinaryOperatorTypes.LogicalAnd => Resolver.TryLogicalAnd(left, right, this, out result),
                        BinaryOperatorTypes.LogicalOr => Resolver.TryLogicalOr(left, right, this, out result),
                        _ => throw new NotImplementedException()
                    };
                }
        }
    }

    private bool TryResolveNode(FunctionNode function, out Quantity<T> result)
    {
        // First we try to apply a user-defined function
        if (AllowUserFunctions)
        {
            var key = new UserFunctionKey(function.Name, function.Arguments.Count);
            if (UserFunctions.TryGetValue(key, out var userFunction))
            {
                // Push a new scope with the arguments
                var newScope = new VariableScope<T>(this, Scope);
                _scopes.Push(newScope);

                // Avoid that our arguments are converted back to output units
                bool oldResolveOutput = ResolveOutputUnits;
                ResolveOutputUnits = false;

                try
                {
                    for (int i = 0; i < function.Arguments.Count; i++)
                    {
                        if (!TryResolveNode(function.Arguments[i], out var arg))
                        {
                            result = default;
                            return false;
                        }
                        newScope.Local[userFunction.Parameters[i]] = new(arg, null);
                    }

                    // Evaluate the body
                    result = default;
                    foreach (var node in userFunction.Body)
                    {
                        // Parse the function
                        if (!TryResolveNode(node, out result))
                            return false;
                    }
                }
                finally
                {
                    ResolveOutputUnits = oldResolveOutput;
                }

                // Pop the scope with the arguments
                _scopes.Pop();
                return true;
            }
        }

        // If the user-defined function doesn't exist, try to find a built-in function
        if (AllowBuiltInFunctions && _builtInFunctions.TryGetValue(function.Name, out var builtInFunction))
        {
            // Avoid that our arguments are converted back to output units
            bool oldResolveOutput = ResolveOutputUnits;
            ResolveOutputUnits = false;

            var args = new List<Quantity<T>>();
            try
            {
                foreach (var arg in function.Arguments)
                {
                    if (!TryResolveNode(arg, out var a))
                    {
                        result = default;
                        return false;
                    }
                    args.Add(a);
                }
            }
            finally
            {
                ResolveOutputUnits = oldResolveOutput;
            }

            return builtInFunction(args, this, out result);
        }

        PostDiagnosticMessage(new($"Could not find a function with the name '{function.Name}' and {function.Arguments.Count} argument(s)."));
        result = default;
        return false;
    }

    private bool TryResolveNode(ScalarNode scalar, out Quantity<T> result)
        => Resolver.TryScalar(scalar.Content.ToString(), this, out result);

    private bool TryResolveNode(TernaryNode ternary, out Quantity<T> result)
    {
        switch (ternary.Type)
        {
            case TernaryNodeTypes.Condition:
                if (!TryResolveNode(ternary.A, out var condition) ||
                    !Resolver.TryIsTrue(condition, this, out bool conditionResult))
                {
                    result = Resolver.Default;
                    return false;
                }

                if (conditionResult)
                {
                    if (!TryResolveNode(ternary.B, out result))
                        return false;
                }
                else
                {
                    if (!TryResolveNode(ternary.C, out result))
                        return false;
                }
                return true;

            default:
                throw new NotImplementedException();
        }
    }

    private bool TryResolveNode(UnaryNode unary, out Quantity<T> result)
    {
        switch (unary.Type)
        {
            case UnaryOperatorTypes.RemoveUnits:
                if (unary.Argument is BinaryNode binary && binary.Type == BinaryOperatorTypes.InUnit)
                {
                    if (!TryResolveNode(binary.Left, out var value) ||
                        !TryResolveNode(binary.Right, out var unit))
                    {
                        result = Resolver.Default;
                        return false;
                    }
                    if (unit.Unit != value.Unit)
                    {
                        PostDiagnosticMessage(new($"Cannot convert units as they don't match"));
                        result = Resolver.Default;
                        return false;
                    }
                    else
                    {
                        if (!Resolver.TryDivide(value, unit, this, out result))
                            return false;
                        result = new Quantity<T>(result.Scalar, Unit.UnitNone);
                        return true;
                    }
                }

                if (!TryResolveNode(unary.Argument, out result))
                    return false;
                result = new Quantity<T>(result.Scalar, Unit.UnitNone);
                return true;

            case UnaryOperatorTypes.Plus:
                return TryResolveNode(unary.Argument, out result) && Resolver.TryPlus(result, this, out result);

            case UnaryOperatorTypes.Minus:
                return TryResolveNode(unary.Argument, out result) && Resolver.TryMinus(result, this, out result);

            case UnaryOperatorTypes.Factorial:
                return TryResolveNode(unary.Argument, out result) && Resolver.TryFactorial(result, this, out result);

            default:
                throw new NotImplementedException();
        }
    }

    private bool TryResolveNode(UnitNode unit, out Quantity<T> result)
    {
        if (!AllowUnits)
        {
            PostDiagnosticMessage(new("Units are not allowed"));
            result = default;
            return false;
        }

        if (ResolveInputUnits)
        {
            if (InputUnits.TryGetValue(unit.Content.ToString(), out result))
                return true;
            PostDiagnosticMessage(new($"Could not recognize unit '{unit}'."));
            return false;
        }

        result = new Quantity<T>(Resolver.One, new Unit((unit.Content.ToString(), 1)));
        return true;
    }

    private bool TryResolveNode(VariableNode variable, out Quantity<T> result)
    {
        if (Scope.TryGetComputedVariable(variable.Content.ToString(), out result))
            return true;
        PostDiagnosticMessage(new($"Could not find a variable with the name '{variable.Content}'"));
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
    public bool TryRemoveInputUnit(string key) => InputUnits.Remove(key);

    /// <inheritdoc />
    public bool TryRemoveOutputUnit(OutputUnitKey key) => OutputUnits.Remove(key);

    /// <inheritdoc />
    public bool TryAssignInputUnit(string key, INode node)
    {
        // Let's just make sure that we resolve input units
        bool oldResolveInputUnits = ResolveInputUnits;
        ResolveInputUnits = true;

        // Evaluate the node and assign the result
        try
        {
            if (!TryResolveNode(node, out var result))
                return false;
            InputUnits[key] = result;
            return true;
        }
        finally
        {
            ResolveInputUnits = oldResolveInputUnits;
        }
    }

    /// <inheritdoc />
    public bool TryAssignOutputUnit(INode outputUnits, INode node)
    {
        bool oldResolveInputUnits = ResolveInputUnits;
        try
        {
            // Resolve the output units
            ResolveInputUnits = false;
            if (!TryResolveNode(outputUnits, out var outputResult))
                return false;

            // Resolve the same in input units
            ResolveInputUnits = true;
            if (!TryResolveNode(node, out var baseResult) ||
                !Resolver.TryDivide(baseResult, outputResult, this, out var division))
                return false;

            // Assign
            var key = new OutputUnitKey(outputResult.Unit, baseResult.Unit);
            OutputUnits[key] = division.Scalar;
            return true;
        }
        finally
        {
            ResolveInputUnits = oldResolveInputUnits;
        }
    }
}
