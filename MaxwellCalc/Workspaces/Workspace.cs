using MaxwellCalc.Parsers.Nodes;
using MaxwellCalc.Resolvers;
using MaxwellCalc.Units;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;

namespace MaxwellCalc.Workspaces
{
    /// <summary>
    /// A workspace.
    /// </summary>
    public class Workspace : IWorkspace,
        IWorkspace<double>,
        IWorkspace<Complex>
    {
        private readonly Dictionary<string, Quantity<double>> _inputUnits = [];
        private readonly Dictionary<Unit, HashSet<Quantity<double>>> _outputUnits = [];
        private readonly Stack<VariableScope> _scope = new();

        /// <inheritdoc />
        public VariableScope Variables => _scope.Peek();

        /// <inheritdoc />
        IVariableScope<double> IWorkspace<double>.Variables => _scope.Peek();

        /// <inheritdoc />
        IVariableScope<Complex> IWorkspace<Complex>.Variables => _scope.Peek();

        /// <inheritdoc />
        IEnumerable<(string, Quantity<double>)> IWorkspace<double>.InputUnits
            => _inputUnits.Select(p => (p.Key, p.Value));

        /// <inheritdoc />
        IEnumerable<(Unit, Quantity<double>)> IWorkspace<double>.OutputUnits
            => _outputUnits.SelectMany(p => p.Value.Select(p2 => (p.Key, p2)));

        /// <inheritdoc />
        IEnumerable<(string, Quantity<Complex>)> IWorkspace<Complex>.InputUnits
            => _inputUnits.Select(p => (p.Key, new Quantity<Complex>(p.Value.Scalar, p.Value.Unit)));

        /// <inheritdoc />
        IEnumerable<(Unit, Quantity<Complex>)> IWorkspace<Complex>.OutputUnits
            => _outputUnits.SelectMany(p => p.Value.Select(p2 => (p.Key, new Quantity<Complex>(p2.Scalar, p2.Unit))));

        /// <summary>
        /// Gets a dictionary of functions that work on real quantities.
        /// </summary>
        public Dictionary<string, IWorkspace<double>.FunctionDelegate> RealFunctions { get; } = [];

        /// <summary>
        /// Gets a dictionary of functions that work on complex quantities.
        /// </summary>
        public Dictionary<string, IWorkspace<Complex>.FunctionDelegate> ComplexFunctions { get; } = [];

        /// <summary>
        /// Gets a dictionary of user functions.
        /// </summary>
        public Dictionary<(string, int), (string[], INode)> UserFunctions { get; } = [];

        /// <summary>
        /// Creates a new <see cref="Workspace"/>.
        /// </summary>
        public Workspace()
        {
            _scope.Push(new VariableScope());
        }

        /// <summary>
        /// Clears all variables, units and functions.
        /// </summary>
        public void Clear()
        {
            _inputUnits.Clear();
            _outputUnits.Clear();
            RealFunctions.Clear();
            ComplexFunctions.Clear();
            UserFunctions.Clear();
        }

        /// <inheritdoc />
        public bool IsUnit(string name) => _inputUnits.ContainsKey(name);

        /// <inheritdoc />
        public bool TryRegisterInputUnit(string name, Quantity<double> quantity)
        {
            _inputUnits[name] = quantity;
            return true;
        }

        public bool TryRemoveInputUnit(string name)
            => _inputUnits.Remove(name);

        /// <inheritdoc />
        public bool TryRegisterOutputUnit(Unit key, Quantity<double> quantity)
        {
            if (!_outputUnits.TryGetValue(key, out var list))
            {
                list = [];
                _outputUnits.Add(key, list);
            }
            list.Add(quantity);
            return true;
        }

        /// <inheritdoc />
        public bool TryRemoveOutputUnit(Unit key, Quantity<double> quantity)
        {
            if (!_outputUnits.TryGetValue(key, out var set))
                return false;
            if (!set.Remove(quantity))
                return false;

            if (set.Count == 0)
                _outputUnits.Remove(key);
            return true;
        }
        bool IWorkspace<double>.TryGetUnit(string name, out Quantity<double> result)
        {
            if (_inputUnits.TryGetValue(name, out var found))
            {
                result = found;
                return true;
            }
            result = new Quantity<double>(double.NaN, Unit.UnitNone);
            return false;
        }
        bool IWorkspace<double>.TryRegisterFunction(string name, IWorkspace<double>.FunctionDelegate function)
        {
            RealFunctions[name] = function;
            return true;
        }
        bool IWorkspace<double>.TryFunction(string name, IReadOnlyList<Quantity<double>> arguments, IResolver<double> resolver, out Quantity<double> result)
        {
            if (UserFunctions.TryGetValue((name, arguments.Count), out var userFunction))
            {
                _scope.Push(Variables.CreateLocal());
                for (int i = 0; i < arguments.Count; i++)
                {
                    if (!((IVariableScope<double>)Variables).TrySetVariable(userFunction.Item1[i], arguments[i]))
                    {
                        result = resolver.Default;
                        return false;
                    }
                }
                bool r = userFunction.Item2.TryResolve(resolver, this, out result);
                _scope.Pop();
                return r;
            }
            if (RealFunctions.TryGetValue(name, out var function))
                return function(arguments, this, out result);
            resolver.Error = $"Cannot find function '{name}' with {arguments.Count} argument(s).";
            result = default;
            return false;
        }

        private static void TrackBestScalar(double scalar, double modifier, Unit unit, ref double bestScalar, ref Unit bestUnit)
        {
            double newScalar = scalar * modifier;
            if (double.IsNaN(bestScalar))
            {
                bestScalar = newScalar;
                bestUnit = unit;
            }
            else
            {
                if (newScalar >= 1.0)
                {
                    if (newScalar < 1000.0)
                    {
                        // The new scalar is actually kind of a best fix
                        if (bestScalar < 1.0 || bestScalar >= 1000.0 ||
                            newScalar < bestScalar)
                        {
                            // The best scalar was not in the range 1-1000, or otherwise,
                            // let's choose the one that is the best match
                            bestUnit = unit;
                            bestScalar = newScalar;
                        }
                    }
                    else if (newScalar < bestScalar)
                    {
                        // The new scalar is too big, let's only take it if the best scalar was even bigger
                        bestUnit = unit;
                        bestScalar = newScalar;
                    }
                }
                else if (newScalar > bestScalar)
                {
                    // The new scalar is too small, let's only take it if the best scalar was even smaller
                    bestUnit = unit;
                    bestScalar = newScalar;
                }
            }
        }

        bool IWorkspace<double>.TryResolveNaming(Quantity<double> quantity, out Quantity<double> result)
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
            double scalar = Math.Abs(quantity.Scalar);
            double bestScalar = double.NaN;
            Unit bestUnit = quantity.Unit;
            foreach (var q in eligible)
            {
                TrackBestScalar(scalar, q.Scalar, q.Unit, ref bestScalar, ref bestUnit);
            }

            // Make a quantity for it
            result = new Quantity<double>(bestScalar, bestUnit);
            return true;
        }
        bool IWorkspace<Complex>.TryGetUnit(string name, out Quantity<Complex> result)
        {
            if (_inputUnits.TryGetValue(name, out var found))
            {
                result = new Quantity<Complex>(found.Scalar, found.Unit);
                return true;
            }
            result = new Quantity<Complex>(double.NaN, Unit.UnitNone);
            return false;
        }
        bool IWorkspace<Complex>.TryRegisterInputUnit(string name, Quantity<Complex> quantity) => false;
        bool IWorkspace<Complex>.TryRemoveInputUnit(string name) => false;
        bool IWorkspace<Complex>.TryRegisterOutputUnit(Unit key, Quantity<Complex> quantity) => false;
        bool IWorkspace<Complex>.TryRemoveOutputUnit(Unit key, Quantity<System.Numerics.Complex> quantity) => false;
        bool IWorkspace<Complex>.TryRegisterFunction(string name, IWorkspace<Complex>.FunctionDelegate function)
        {
            ComplexFunctions[name] = function;
            return true;
        }
        bool IWorkspace<Complex>.TryFunction(string name, IReadOnlyList<Quantity<Complex>> arguments, IResolver<Complex> resolver, out Quantity<Complex> result)
        {
            if (UserFunctions.TryGetValue((name, arguments.Count), out var userFunction))
            {
                _scope.Push(Variables.CreateLocal());
                for (int i = 0; i < arguments.Count; i++)
                {
                    if (!((IVariableScope<Complex>)Variables).TrySetVariable(userFunction.Item1[i], arguments[i]))
                    {
                        result = resolver.Default;
                        return false;
                    }
                }
                bool r = userFunction.Item2.TryResolve(resolver, this, out result);
                _scope.Pop();
                return r;
            }

            if (ComplexFunctions.TryGetValue(name, out var function))
                return function(arguments, this, out result);
            resolver.Error = $"Cannot find function '{name}' with {arguments.Count} argument(s).";
            result = default;
            return false;
        }

        private static void TrackBestScalar(Complex scalar, double modifier, Unit unit, ref Complex bestComplexScalar, ref Unit bestUnit)
        {
            var mult = scalar * modifier;
            double newScalar = Math.Max(Math.Abs(mult.Real), Math.Abs(mult.Imaginary));
            double bestScalar = Math.Max(Math.Abs(bestComplexScalar.Real), Math.Abs(bestComplexScalar.Imaginary));
            if (double.IsNaN(bestScalar))
            {
                bestComplexScalar = mult;
                bestUnit = unit;
            }
            else
            {
                if (newScalar >= 1.0)
                {
                    if (newScalar < 1000.0)
                    {
                        // The new scalar is actually kind of a best fix
                        if (bestScalar < 1.0 || bestScalar >= 1000.0 ||
                            newScalar < bestScalar)
                        {
                            // The best scalar was not in the range 1-1000, or otherwise,
                            // let's choose the one that is the best match
                            bestUnit = unit;
                            bestComplexScalar = mult;
                        }
                    }
                    else if (newScalar < bestScalar)
                    {
                        // The new scalar is too big, let's only take it if the best scalar was even bigger
                        bestUnit = unit;
                        bestComplexScalar = mult;
                    }
                }
                else if (newScalar > bestScalar)
                {
                    // The new scalar is too small, let's only take it if the best scalar was even smaller
                    bestUnit = unit;
                    bestComplexScalar = mult;
                }
            }
        }

        bool IWorkspace<Complex>.TryResolveNaming(Quantity<Complex> quantity, out Quantity<Complex> result)
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
            Complex scalar = quantity.Scalar;
            Complex bestScalar = double.NaN;
            Unit bestUnit = quantity.Unit;
            foreach (var q in eligible)
            {
                TrackBestScalar(scalar, q.Scalar, q.Unit, ref bestScalar, ref bestUnit);
            }

            // Make a quantity for it
            result = new Quantity<Complex>(bestScalar, bestUnit);
            return true;
        }

        /// <inheritdoc />
        public bool TryRegisterUserFunction(string name, List<string> arguments, INode expression)
        {
            UserFunctions[(name, arguments.Count)] = (arguments.ToArray(), expression);
            return true;
        }
    }
}
