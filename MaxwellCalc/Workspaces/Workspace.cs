using Avalonia.Remote.Protocol.Designer;
using MaxwellCalc.Domains;
using MaxwellCalc.Parsers;
using MaxwellCalc.Parsers.Nodes;
using MaxwellCalc.Units;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text.Json;

namespace MaxwellCalc.Workspaces
{
    /// <summary>
    /// A workspace for a certain domain type.
    /// </summary>
    /// <typeparam name="T">The domain type.</typeparam>
    public class Workspace<T> : IWorkspace<T> where T : struct, IFormattable
    {
        private readonly Dictionary<string, Quantity<T>> _inputUnits = [];
        private readonly Dictionary<Unit, Dictionary<Unit, T>> _outputUnits = [];
        private readonly Stack<IVariableScope<T>> _scopes = new();
        private readonly Dictionary<string, IWorkspace<T>.FunctionDelegate> _builtInFunctions = [];
        private readonly Dictionary<(string, int), (string[], string)> _userFunctions = [];

        /// <inheritdoc />
        public IDomain<T> Resolver { get; }

        /// <inheritdoc />
        public string? DiagnosticMessage { get; set; } = null;

        /// <inheritdoc />
        public IVariableScope<T> Scope => _scopes.Peek();

        /// <inheritdoc />
        public IEnumerable<Variable> Variables
        {
            get
            {
                var scope = _scopes.Peek();
                foreach (var p in scope.Variables)
                {
                    if (!scope.TryGetVariable(p, out var value) ||
                        !Resolver.TryFormat(value, "g", CultureInfo.InvariantCulture, out var formatted))
                        continue;
                    yield return new(p, formatted);
                }
            }
        }

        /// <inheritdoc />
        public IEnumerable<InputUnit> InputUnits
        {
            get
            {
                foreach (var p in _inputUnits)
                {
                    if (!Resolver.TryFormat(p.Value, "g", System.Globalization.CultureInfo.CurrentCulture, out var formatted))
                        continue;
                    yield return new(p.Key, formatted);
                }
            }
        }

        /// <inheritdoc />
        public IEnumerable<OutputUnit> OutputUnits
        {
            get
            {
                foreach (var p in _outputUnits)
                {
                    foreach (var p2 in p.Value)
                    {
                        if (!Resolver.TryInvert(new(p2.Value, Unit.UnitNone), this, out var inverted) ||
                            !Resolver.TryFormat(inverted, "g3", CultureInfo.InvariantCulture, out var formatted))
                            continue;
                        yield return new(p2.Key, new(formatted.Scalar, p.Key));
                    }
                }
            }
        }

        /// <inheritdoc />
        public IEnumerable<UserFunction> UserFunctions => _userFunctions.Select(p => new UserFunction(p.Key.Item1, p.Value.Item1, p.Value.Item2));

        /// <summary>
        /// Creates a new <see cref="Workspace"/>.
        /// </summary>
        /// <param name="resolver">The resolver.</param>
        public Workspace(IDomain<T> resolver)
        {
            Resolver = resolver ?? throw new ArgumentNullException(nameof(resolver));
            _scopes.Push(new VariableScope<T>());
        }

        /// <inheritdoc />
        public bool TryGetUnit(string name, out Quantity<T> result)
        {
            if (_inputUnits.TryGetValue(name, out result))
                return true;
            result = Resolver.Default;
            return false;
        }

        /// <inheritdoc />
        public bool TryRegisterBuiltInFunction(string name, IWorkspace<T>.FunctionDelegate function)
        {
            _builtInFunctions[name] = function;
            return true;
        }

        /// <inheritdoc />
        public bool TryFunction(string name, IReadOnlyList<Quantity<T>> arguments, IDomain<T> resolver, out Quantity<T> result)
        {
            if (_userFunctions.TryGetValue((name, arguments.Count), out var userFunction))
            {
                // Push a new scope with the arguments
                _scopes.Push(Scope.CreateLocal());
                for (int i = 0; i < arguments.Count; i++)
                {
                    if (!Scope.TrySetVariable(userFunction.Item1[i], arguments[i]))
                    {
                        result = resolver.Default;
                        return false;
                    }
                }

                // Parse the function
                var lexer = new Lexer(userFunction.Item2);
                var node = Parser.Parse(lexer, this);
                bool r = node.TryResolve(resolver, this, out result);

                // Pop the scope with the arguments
                _scopes.Pop();
                return r;
            }
            if (_builtInFunctions.TryGetValue(name, out var function))
                return function(arguments, this, out result);
            DiagnosticMessage = $"Cannot find function '{name}' with {arguments.Count} argument(s).";
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
        public bool IsUnit(string name) => _inputUnits.ContainsKey(name);

        /// <inheritdoc />
        public bool TryResolveAndFormat(INode node, out Quantity<string> result)
            => TryResolveAndFormat(node, null, null, out result);

        /// <inheritdoc />
        public bool TryResolveAndFormat(INode node, string? format, IFormatProvider? formatProvider, out Quantity<string> result)
        {
            Quantity<T> r;
            if (node is BinaryNode bn)
            {
                switch (bn.Type)
                {
                    case BinaryOperatorTypes.InUnit:
                        {
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
                                    DiagnosticMessage = "Base units do not match.";
                                    result = default;
                                    return false;
                                }
                                if (!Resolver.TryDivide(left, right, this, out var intermediary))
                                {
                                    result = default;
                                    return false;
                                }
                                r = new Quantity<T>(intermediary.Scalar, new((bn.Right.Content.ToString(), 1)));
                            }
                        }
                        break;

                    case BinaryOperatorTypes.Assign:
                        if (!bn.TryResolve(Resolver, this, out r))
                        {
                            result = default;
                            return false;
                        }
                        if (bn.Left is FunctionNode fn)
                        {
                            result = default;
                            DiagnosticMessage = $"Stored function '{fn.Name}'.";
                            return true;
                        }
                        break;

                    default:
                        if (!bn.TryResolve(Resolver, this, out r))
                        {
                            result = default;
                            return false;
                        }

                        // Resolve output unit
                        TryResolveOutputUnits(r, out r);
                        break;
                }
            }
            else
            {
                if (!node.TryResolve(Resolver, this, out r))
                {
                    result = default;
                    return false;
                }

                // Resolve output unit
                if (!TryResolveOutputUnits(r, out r))
                {
                    result = default;
                    return false;
                }
            }

            if (!Resolver.TryFormat(r, format, formatProvider, out result))
                return false;
            return true;
        }

        /// <inheritdoc />
        public bool TryRegisterInputUnit(InputUnit inputUnit)
        {
            if (inputUnit.Value.Unit == Unit.UnitNone)
            {
                DiagnosticMessage = "Cannot add an input unit without a base unit.";
                return false;
            }
            if (!Resolver.TryScalar(inputUnit.Value.Scalar, this, out var q))
                return false;
            _inputUnits[inputUnit.UnitName] = new(q.Scalar, inputUnit.Value.Unit);
            return true;
        }

        /// <inheritdoc />
        public bool TryRegisterInputUnit(string name, INode node)
        {
            if (!node.TryResolve(Resolver, null, out var q))
                return false;
            if (q.Unit == Unit.UnitNone)
            {
                DiagnosticMessage = "Cannot add an input unit without a base unit.";
                return false;
            }
            _inputUnits[name] = q;
            return true;
        }

        /// <inheritdoc />
        public bool TryRemoveInputUnit(string name)
            => _inputUnits.Remove(name);

        /// <inheritdoc />
        public bool TryRegisterOutputUnit(OutputUnit outputUnit)
        {
            if (!Resolver.TryScalar(outputUnit.Value.Scalar, this, out var scalar) ||
                !Resolver.TryInvert(scalar, this, out var inv))
                return false;
            if (!_outputUnits.TryGetValue(outputUnit.Value.Unit, out var dict))
            {
                dict = [];
                _outputUnits.Add(outputUnit.Value.Unit, dict);
            }
            dict[outputUnit.Unit] = inv.Scalar;
            return true;
        }

        /// <inheritdoc />
        public bool TryRegisterOutputUnit(INode outputUnits, INode quantity)
        {
            if (!outputUnits.TryResolve(Resolver, null, out var ou) ||
                !quantity.TryResolve(Resolver, null, out var bu))
                return false;
            if (!Resolver.TryDivide(ou, bu, this, out var div))
                return false;
            if (!_outputUnits.TryGetValue(bu.Unit, out var dict))
            {
                dict = [];
                _outputUnits.Add(bu.Unit, dict);
            }
            dict[ou.Unit] = div.Scalar;
            return true;
        }

        /// <inheritdoc />
        public bool TryRemoveOutputUnit(Unit outputUnits, Unit baseUnits)
        {
            if (!_outputUnits.TryGetValue(baseUnits, out var dict))
                return false;
            return dict.Remove(outputUnits);
        }

        /// <inheritdoc />
        public bool TryRegisterUserFunction(UserFunction userFunction)
        {
            _userFunctions[(userFunction.Name, userFunction.Parameters.Length)] = (userFunction.Parameters, userFunction.Body);
            return true;
        }

        /// <inheritdoc />
        public bool TryRemoveUserFunction(string name, int argumentCount)
            => _userFunctions.Remove((name, argumentCount));

        /// <inheritdoc />
        public void Clear()
        {
            _inputUnits.Clear();
            _outputUnits.Clear();
            _builtInFunctions.Clear();
            _userFunctions.Clear();
            _scopes.Clear();
            _scopes.Push(new VariableScope<T>());
            DiagnosticMessage = string.Empty;
        }

        /// <inheritdoc />
        public bool TrySetVariable(Variable variable)
        {
            var scope = _scopes.Peek();
            if (!Resolver.TryScalar(variable.Value.Scalar, this, out var scalarQuantity) ||
                !scope.TrySetVariable(variable.Name, new(scalarQuantity.Scalar, variable.Value.Unit)))
                return false;
            return true;
        }

        /// <inheritdoc />
        public bool TryGetVariable(string name, out Quantity<string> value)
        {
            var scope = _scopes.Peek();
            if (!scope.TryGetVariable(name, out var typedValue) ||
                !Resolver.TryFormat(typedValue, "g", CultureInfo.InvariantCulture, out value))
            {
                value = default;
                return false;
            }
            return true;
        }
    }
}
