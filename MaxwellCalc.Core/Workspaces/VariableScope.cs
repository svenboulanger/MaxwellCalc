using MaxwellCalc.Units;
using System;
using System.Collections.Generic;

namespace MaxwellCalc.Workspaces
{
    /// <summary>
    /// An implementation for a variable scope.
    /// </summary>
    /// <typeparam name="T">The base type.</typeparam>
    public class VariableScope<T> : IVariableScope<T>
    {
        private readonly Dictionary<string, Quantity<T>> _variables = [];
        private readonly Dictionary<string, string> _descriptions = [];
        private readonly IVariableScope<T>? _parent;

        /// <inheritdoc />
        public IEnumerable<string> Variables => _variables.Keys;

        /// <summary>
        /// Called when a variable changes value.
        /// </summary>
        public event EventHandler<VariableChangedEvent>? VariableChanged;

        /// <summary>
        /// Creates a new <see cref="VariableScope{T}"/>.
        /// </summary>
        public VariableScope()
        {
            _parent = null;
        }

        /// <summary>
        /// Creates a <see cref="VariableScope{T}"/>
        /// </summary>
        /// <param name="parent">The parent scope.</param>
        public VariableScope(IVariableScope<T> parent)
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
            if (_parent is not null)
                return _parent.TryGetVariable(name, out result);
            result = default;
            return false;
        }

        /// <inheritdoc />
        bool IVariableScope<T>.TryGetVariable(string name, out Quantity<T> result, out string? description)
        {
            if (_variables.TryGetValue(name, out result))
            {
                _descriptions.TryGetValue(name, out description);
                return true;
            }
            if (_parent is not null)
                return _parent.TryGetVariable(name, out result, out description);
            result = default;
            description = null;
            return false;
        }

        /// <inheritdoc />
        bool IVariableScope<T>.TrySetVariable(string name, Quantity<T> value, string? description)
        {
            _variables[name] = value;
            if (description is not null)
                _descriptions[name] = description;
            VariableChanged?.Invoke(this, new VariableChangedEvent(name));
            return true;
        }

        /// <inheritdoc />
        bool IVariableScope.RemoveVariable(string name)
        {
            if (_variables.Remove(name))
            {
                _descriptions.Remove(name);
                VariableChanged?.Invoke(this, new VariableChangedEvent(name));
                return true;
            }
            return false;
        }
    }
}
