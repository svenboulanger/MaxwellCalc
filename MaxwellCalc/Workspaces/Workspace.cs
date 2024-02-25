using Avalonia.Win32.Interop.Automation;
using MaxwellCalc.Units;
using System;
using System.Collections.Generic;

namespace MaxwellCalc.Workspaces
{
    /// <summary>
    /// A workspace.
    /// </summary>
    public class Workspace : IWorkspace, IWorkspace<double>
    {
        /// <summary>
        /// Gets a dictionary of units for input.
        /// </summary>
        public Dictionary<string, Unit> InputUnits { get; } = new() {
            { "meter", new Unit(1.0, BaseUnit.UnitMeter, "m") },
            { "kilogram", new Unit(1.0, BaseUnit.UnitKilogram, "kg") },
            { "second", new Unit(1.0, BaseUnit.UnitSeconds, "s") },
            { "mol", new Unit(1.0, BaseUnit.UnitMole, "mol") },
            { "ampere", new Unit(1.0, BaseUnit.UnitAmperes, "A") },
            { "kelvin", new Unit(1.0, BaseUnit.UnitKelvin, "K") },
            { "radian", new Unit(1.0, BaseUnit.UnitRadian, "rad") }
        };

        /// <summary>
        /// Gets a dictionary of units for output.
        /// </summary>
        public Dictionary<BaseUnit, HashSet<Unit>> OutputUnits { get; } = [];

        /// <summary>
        /// Gets a dictionary of variables.
        /// </summary>
        public Dictionary<string, object> Variables { get; } = new() {
            { "pi", new Quantity<double>(Math.PI, Unit.Scalar) },
            { "e", new Quantity<double>(Math.E, Unit.Scalar) },
        };

        /// <summary>
        /// Gets a dictionary of functions that work on real quantities.
        /// </summary>
        public Dictionary<string, IWorkspace<double>.FunctionDelegate> RealFunctions { get; } = [];

        /// <inheritdoc />
        public bool IsUnit(string name) => InputUnits.ContainsKey(name);

        /// <inheritdoc />
        public bool IsVariable(string name) => Variables.ContainsKey(name);

        /// <inheritdoc />
        public bool TryGetUnit(string name, out Quantity<double> result)
        {
            if (InputUnits.TryGetValue(name, out var found))
            {
                result = new Quantity<double>(1.0, found);
                return true;
            }
            result = new Quantity<double>(double.NaN, Unit.Scalar);
            return false;
        }

        /// <inheritdoc />
        public bool TryGetVariable(string name, out Quantity<double> result)
        {
            if (!Variables.TryGetValue(name, out var found))
            {
                result = new Quantity<double>(double.NaN, Unit.Scalar);
                return false;
            }
            switch (found)
            {
                case Quantity<double> dbl:
                    result = dbl;
                    return true;

                default:
                    result = new Quantity<double>(double.NaN, Unit.Scalar);
                    return false;
            }
        }

        /// <inheritdoc />
        public bool TrySetVariable(string name, Quantity<double> value)
        {
            Variables[name] = value;
            return true;
        }

        /// <inheritdoc />
        public bool TryRegisterFunction(string name, IWorkspace<double>.FunctionDelegate function)
        {
            RealFunctions[name] = function;
            return true;
        }

        /// <inheritdoc />
        public bool TryFunction(string name, IReadOnlyList<Quantity<double>> arguments, out Quantity<double> result)
        {
            if (!RealFunctions.TryGetValue(name, out var function))
            {
                result = default;
                return false;
            }
            return function(arguments, this, out result);
        }

        /// <inheritdoc />
        public bool TryRegisterInputUnit(string name, Unit unit)
        {
            InputUnits[name] = unit;
            return true;
        }

        /// <inheritdoc />
        public bool TryRegisterOutputUnit(string name, Unit unit)
        {
            if (!OutputUnits.TryGetValue(unit.SIUnits, out var list))
            {
                list = new HashSet<Unit>();
                OutputUnits.Add(unit.SIUnits, list);
            }
            list.Add(unit);
            return true;
        }

        private void TrackBestUnit(double scalar, Unit unit, ref double bestScalar, ref Unit bestUnit)
        {
            double newScalar = scalar / unit.Modifier;
            if (double.IsNaN(bestScalar))
            {
                bestScalar = newScalar;
                bestUnit = unit;
            }
            else
            {
                if (newScalar > 1.0)
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
        public bool TryResolveNaming(Quantity<double> quantity, out Quantity<double> result)
        {
            if (quantity.Unit.Name is not null)
            {
                // The name is already given, don't overwrite it
                result = quantity;
                return true;
            }

            // The output units are only expressable in base units, let's see if we can find some output for it
            if (!OutputUnits.TryGetValue(quantity.Unit.SIUnits, out var eligible) || eligible.Count == 0)
            {
                result = quantity;
                return false;
            }

            // There should be a fit for the quantity
            // Let's find the closest one that would lead to a scalar of 1-1000
            double scalar = Math.Abs(quantity.Scalar * quantity.Unit.Modifier);
            double bestScalar = double.NaN;
            Unit bestUnit = quantity.Unit;
            foreach (var unit in eligible)
            {
                TrackBestUnit(scalar, unit, ref bestScalar, ref bestUnit);
            }

            // Make a quantity for it
            result = new Quantity<double>(bestScalar, bestUnit);
            return true;
        }
    }
}
