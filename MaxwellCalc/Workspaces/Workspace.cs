using MaxwellCalc.Units;
using System;
using System.Collections.Generic;
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
        private readonly Dictionary<string, (double, Unit)> _inputUnits = [];
        private readonly Dictionary<Unit, HashSet<(double, Unit)>> _outputUnits = [];

        /// <summary>
        /// Gets a dictionary of variables.
        /// </summary>
        public Dictionary<string, IQuantity> Variables { get; } = new() {
            { "pi", new Quantity<double>(Math.PI, Unit.UnitNone) },
            { "e", new Quantity<double>(Math.E, Unit.UnitNone) },
        };

        /// <summary>
        /// Gets a dictionary of functions that work on real quantities.
        /// </summary>
        public Dictionary<string, IWorkspace<double>.FunctionDelegate> RealFunctions { get; } = [];

        /// <summary>
        /// Gets a dictionary of functions that work on complex quantities.
        /// </summary>
        public Dictionary<string, IWorkspace<Complex>.FunctionDelegate> ComplexFunctions { get; } = [];

        /// <inheritdoc />
        public bool IsUnit(string name) => _inputUnits.ContainsKey(name);

        /// <inheritdoc />
        public bool IsVariable(string name) => Variables.ContainsKey(name);

        /// <inheritdoc />
        public bool TryRegisterInputUnit(string name, double modifier, Unit unit)
        {
            _inputUnits[name] = (modifier, unit);
            return true;
        }

        /// <inheritdoc />
        public bool TryRegisterDerivedUnit(Unit key, double modifier, Unit value)
        {
            if (!_outputUnits.TryGetValue(key, out var list))
            {
                list = [];
                _outputUnits.Add(key, list);
            }
            list.Add((modifier, value));
            return true;
        }

        /// <inheritdoc />
        bool IWorkspace<double>.TryGetUnit(string name, out Quantity<double> result)
        {
            if (_inputUnits.TryGetValue(name, out var found))
            {
                result = new Quantity<double>(found.Item1, found.Item2);
                return true;
            }
            result = new Quantity<double>(double.NaN, Unit.UnitNone);
            return false;
        }

        /// <inheritdoc />
        bool IWorkspace<double>.TryGetVariable(string name, out Quantity<double> result)
        {
            if (!Variables.TryGetValue(name, out var found))
            {
                result = new Quantity<double>(double.NaN, Unit.UnitNone);
                return false;
            }
            switch (found)
            {
                case Quantity<double> dbl:
                    result = dbl;
                    return true;

                case Quantity<Complex> cplx:
                    result = new Quantity<double>(cplx.Scalar.Real, cplx.Unit);
                    return true;

                default:
                    result = new Quantity<double>(double.NaN, Unit.UnitNone);
                    return false;
            }
        }

        /// <inheritdoc />
        bool IWorkspace<double>.TrySetVariable(string name, Quantity<double> value)
        {
            Variables[name] = value;
            return true;
        }

        /// <inheritdoc />
        bool IWorkspace<double>.TryRegisterFunction(string name, IWorkspace<double>.FunctionDelegate function)
        {
            RealFunctions[name] = function;
            return true;
        }

        /// <inheritdoc />
        bool IWorkspace<double>.TryFunction(string name, IReadOnlyList<Quantity<double>> arguments, out Quantity<double> result)
        {
            if (!RealFunctions.TryGetValue(name, out var function))
            {
                result = default;
                return false;
            }
            return function(arguments, this, out result);
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

        /// <inheritdoc />
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
            foreach (var pair in eligible)
            {
                TrackBestScalar(scalar, pair.Item1, pair.Item2, ref bestScalar, ref bestUnit);
            }

            // Make a quantity for it
            result = new Quantity<double>(bestScalar, bestUnit);
            return true;
        }

        /// <inheritdoc />
        bool IWorkspace<Complex>.TryGetUnit(string name, out Quantity<Complex> result)
        {
            if (_inputUnits.TryGetValue(name, out var found))
            {
                result = new Quantity<Complex>(found.Item1, found.Item2);
                return true;
            }
            result = new Quantity<Complex>(double.NaN, Unit.UnitNone);
            return false;
        }

        /// <inheritdoc />
        bool IWorkspace<Complex>.TryGetVariable(string name, out Quantity<Complex> result)
        {
            if (!Variables.TryGetValue(name, out var found))
            {
                result = new Quantity<Complex>(double.NaN, Unit.UnitNone);
                return false;
            }
            switch (found)
            {
                case Quantity<double> dbl:
                    result = new Quantity<Complex>(dbl.Scalar, dbl.Unit);
                    return true;

                case Quantity<Complex> cplx:
                    result = cplx;
                    return true;

                default:
                    result = new Quantity<Complex>(double.NaN, Unit.UnitNone);
                    return false;
            }
        }

        /// <inheritdoc />
        bool IWorkspace<Complex>.TrySetVariable(string name, Quantity<Complex> value)
        {
            Variables[name] = value;
            return true;
        }

        /// <inheritdoc />
        bool IWorkspace<Complex>.TryRegisterInputUnit(string name, Complex modifier, Unit unit)
        {
            // We will share units with doubles
            return false;
        }

        /// <inheritdoc />
        bool IWorkspace<Complex>.TryRegisterDerivedUnit(Unit key, Complex modifier, Unit value)
        {
            // We will share units with doubles
            return false;
        }

        /// <inheritdoc />
        bool IWorkspace<Complex>.TryRegisterFunction(string name, IWorkspace<Complex>.FunctionDelegate function)
        {
            ComplexFunctions[name] = function;
            return true;
        }

        /// <inheritdoc />
        bool IWorkspace<Complex>.TryFunction(string name, IReadOnlyList<Quantity<Complex>> arguments, out Quantity<Complex> result)
        {
            if (!ComplexFunctions.TryGetValue(name, out var function))
            {
                result = default;
                return false;
            }
            return function(arguments, this, out result);
        }


        private static void TrackBestScalar(Complex scalar, double modifier, Unit unit, ref Complex bestComplexScalar, ref Unit bestUnit)
        {
            var mult = scalar * modifier;
            double newScalar = Math.Max(mult.Real, mult.Imaginary);
            double bestScalar = Math.Max(bestComplexScalar.Real, bestComplexScalar.Imaginary);
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

        /// <inheritdoc />
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
            foreach (var pair in eligible)
            {
                TrackBestScalar(scalar, pair.Item1, pair.Item2, ref bestScalar, ref bestUnit);
            }

            // Make a quantity for it
            result = new Quantity<Complex>(bestScalar, bestUnit);
            return true;
        }
    }
}
