using MaxwellCalc.Domains;
using MaxwellCalc.Parsers.Nodes;
using MaxwellCalc.Units;
using System;
using System.Collections.Generic;
using System.Linq;

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
        private readonly Dictionary<(string, int), (string[], INode)> _userFunctions = [];

        /// <inheritdoc />
        public IDomain<T> Resolver { get; }

        /// <inheritdoc />
        public string? ErrorMessage { get; set; } = null;

        /// <inheritdoc />
        public IVariableScope<T> Scope => _scopes.Peek();

        /// <inheritdoc />
        public IEnumerable<(string, Quantity<string>)> Variables
        {
            get
            {
                var scope = _scopes.Peek();
                foreach (var p in scope.Variables)
                {
                    if (!scope.TryGetVariable(p, out var value) ||
                        !Resolver.TryFormat(value, "g3", System.Globalization.CultureInfo.CurrentCulture, out var formatted))
                        continue;
                    yield return (p, formatted);
                }
            }
        }

        /// <inheritdoc />
        public IEnumerable<(string, Quantity<string>)> InputUnits
        {
            get
            {
                foreach (var p in _inputUnits)
                {
                    if (!Resolver.TryFormat(p.Value, "g3", System.Globalization.CultureInfo.CurrentCulture, out var formatted))
                        continue;
                    yield return (p.Key, formatted);
                }
            }
        }

        /// <inheritdoc />
        public IEnumerable<(Unit, Quantity<string>)> OutputUnits
        {
            get
            {
                foreach (var p in _outputUnits)
                {
                    foreach (var p2 in p.Value)
                    {
                        if (!Resolver.TryInvert(new(p2.Value, Unit.UnitNone), this, out var inverted) ||
                            !Resolver.TryFormat(inverted, "g3", System.Globalization.CultureInfo.CurrentCulture, out var formatted))
                            continue;
                        yield return (p2.Key, new(formatted.Scalar, p.Key));
                    }
                }
            }
        }

        /// <inheritdoc />
        public IEnumerable<(string Name, string[] parameters, INode body)> UserFunctions => _userFunctions.Select(p => (p.Key.Item1, p.Value.Item1, p.Value.Item2));

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
                var scope = _scopes.Peek();
                _scopes.Push(scope.CreateLocal());
                for (int i = 0; i < arguments.Count; i++)
                {
                    if (!scope.TrySetVariable(userFunction.Item1[i], arguments[i]))
                    {
                        result = resolver.Default;
                        return false;
                    }
                }
                bool r = userFunction.Item2.TryResolve(resolver, this, out result);
                _scopes.Pop();
                return r;
            }
            if (_builtInFunctions.TryGetValue(name, out var function))
                return function(arguments, this, out result);
            ErrorMessage = $"Cannot find function '{name}' with {arguments.Count} argument(s).";
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
                                    ErrorMessage = "Base units do not match.";
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

                    default:
                        if (!bn.TryResolve(Resolver, this, out r))
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
        public bool TryRegisterInputUnit(string name, Quantity<string> quantity)
        {
            if (quantity.Unit == Unit.UnitNone)
            {
                ErrorMessage = "Cannot add an input unit without a base unit.";
                return false;
            }
            if (!Resolver.TryScalar(quantity.Scalar, this, out var q))
                return false;
            _inputUnits[name] = new(q.Scalar, quantity.Unit);
            return true;
        }

        /// <inheritdoc />
        public bool TryRegisterInputUnit(string name, INode node)
        {
            if (!node.TryResolve(Resolver, null, out var q))
                return false;
            if (q.Unit == Unit.UnitNone)
            {
                ErrorMessage = "Cannot add an input unit without a base unit.";
                return false;
            }
            _inputUnits[name] = q;
            return true;
        }

        /// <inheritdoc />
        public bool TryRemoveInputUnit(string name)
            => _inputUnits.Remove(name);

        /// <inheritdoc />
        public bool TryRegisterOutputUnit(Unit outputUnits, Quantity<string> quantity)
        {
            if (!Resolver.TryScalar(quantity.Scalar, this, out var scalar) ||
                !Resolver.TryInvert(scalar, this, out var inv))
                return false;
            if (!_outputUnits.TryGetValue(quantity.Unit, out var dict))
            {
                dict = [];
                _outputUnits.Add(quantity.Unit, dict);
            }
            dict[outputUnits] = inv.Scalar;
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
        public bool TryRegisterUserFunction(string name, List<string> arguments, INode expression)
        {
            _userFunctions[(name, arguments.Count)] = (arguments.ToArray(), expression);
            return true;
        }

        /// <inheritdoc />
        public bool TryRemoveUserFunction(string name, int argumentCount)
            => _userFunctions.Remove((name, argumentCount));
    }
}
