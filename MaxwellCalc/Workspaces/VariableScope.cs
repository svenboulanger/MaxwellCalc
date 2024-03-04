using MaxwellCalc.Units;
using System.Collections.Generic;
using System.Linq;

namespace MaxwellCalc.Workspaces
{
    /// <summary>
    /// An implementation for a variable scope.
    /// </summary>
    /// <typeparam name="T">The base type.</typeparam>
    public class VariableScope<T> : IVariableScope<T>
    {
        private readonly Dictionary<string, Quantity<T>> _variables = [];
        private readonly VariableScope<T>? _parent;

        /// <inheritdoc />
        public IEnumerable<string> Variables => _variables.Keys;

        /// <summary>
        /// Creates a new <see cref="VariableScope"/>.
        /// </summary>
        public VariableScope()
        {
            _parent = null;
        }

        private VariableScope(VariableScope<T> parent)
        {
            _parent = parent;
        }

        /// <inheritdoc />
        IVariableScope<T> IVariableScope<T>.CreateLocal()
            => new VariableScope<T>(this);

        /// <inheritdoc />
        bool IVariableScope.IsVariable(string name) => _variables.ContainsKey(name) || (_parent is not null && ((IVariableScope)_parent).IsVariable(name));

        /// <inheritdoc />
        bool IVariableScope<T>.TryGetVariable(string name, out Quantity<T> result)
        {
            if (_variables.TryGetValue(name, out result))
                return true;
            result = default;
            return false;
        }

        /// <inheritdoc />
        bool IVariableScope<T>.TrySetVariable(string name, Quantity<T> value)
        {
            _variables[name] = value;
            return true;
        }
    }
}
