using MaxwellCalc.Units;
using System.Collections.Generic;
using System.Numerics;

namespace MaxwellCalc.Workspaces
{
    /// <summary>
    /// An implementation for a variable scope.
    /// </summary>
    /// <typeparam name="T">The base type.</typeparam>
    public class VariableScope : IVariableScope<double>, IVariableScope<Complex>
    {
        private readonly Dictionary<string, IQuantity> _variables = [];
        private readonly VariableScope? _parent;

        /// <summary>
        /// Creates a new <see cref="VariableScope"/>.
        /// </summary>
        public VariableScope()
        {
            _parent = null;
        }

        private VariableScope(VariableScope parent)
        {
            _parent = parent;
        }

        public VariableScope CreateLocal() => new VariableScope(this);

        /// <inheritdoc />
        IVariableScope<double> IVariableScope<double>.CreateLocal() => CreateLocal();

        /// <inheritdoc />
        IVariableScope<Complex> IVariableScope<Complex>.CreateLocal() => CreateLocal();

        /// <inheritdoc />
        bool IVariableScope.IsVariable(string name) => _variables.ContainsKey(name) || (_parent is not null && ((IVariableScope)_parent).IsVariable(name));

        /// <inheritdoc />
        bool IVariableScope<double>.TryGetVariable(string name, out Quantity<double> result)
        {
            if (_variables.TryGetValue(name, out var quantity))
            {
                switch (quantity)
                {
                    case Quantity<double> dbl:
                        result = dbl;
                        return true;

                    case Quantity<Complex> cplx:
                        result = new Quantity<double>(cplx.Scalar.Real, quantity.Unit);
                        return true;

                    default:
                        result = new Quantity<double>(double.NaN, Unit.UnitNone);
                        return false;
                }
            }
            else if (_parent is not null && ((IVariableScope<double>)_parent).TryGetVariable(name, out result))
                return true;
            result = new Quantity<double>(double.NaN, Unit.UnitNone);
            return false;
        }

        /// <inheritdoc />
        bool IVariableScope<Complex>.TryGetVariable(string name, out Quantity<Complex> result)
        {
            if (_variables.TryGetValue(name, out var quantity))
            {
                switch (quantity)
                {
                    case Quantity<double> dbl:
                        result = new Quantity<Complex>(dbl.Scalar, quantity.Unit);
                        return true;

                    case Quantity<Complex> cplx:
                        result = cplx;
                        return true;

                    default:
                        result = new Quantity<Complex>(double.NaN, Unit.UnitNone);
                        return false;
                }
            }
            else if (_parent is not null && ((IVariableScope<Complex>)_parent).TryGetVariable(name, out result))
                return true;
            result = new Quantity<Complex>(double.NaN, Unit.UnitNone);
            return false;
        }

        /// <inheritdoc />
        bool IVariableScope<double>.TrySetVariable(string name, Quantity<double> value)
        {
            _variables[name] = value;
            return true;
        }

        /// <inheritdoc />
        bool IVariableScope<Complex>.TrySetVariable(string name, Quantity<Complex> value)
        {
            _variables[name] = value;
            return true;
        }
    }
}
