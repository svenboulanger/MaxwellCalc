using MaxwellCalc.Units;
using System;
using System.Collections.Generic;

namespace MaxwellCalc.Workspaces
{
    /// <summary>
    /// A workspace.
    /// </summary>
    public class Workspace : IWorkspace, IWorkspace<Quantity<double>>
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
        public Dictionary<BaseUnit, List<Unit>> OutputUnits { get; } = [];

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
        public Dictionary<string, IWorkspace<Quantity<double>>.FunctionDelegate> RealFunctions { get; } = [];

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
        public bool TryRegisterFunction(string name, IWorkspace<Quantity<double>>.FunctionDelegate function)
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
                list = new List<Unit>();
                OutputUnits.Add(unit.SIUnits, list);
            }
            list.Add(unit);
            return true;
        }
    }
}
