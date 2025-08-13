using MaxwellCalc.Core.Workspaces;
using MaxwellCalc.Parsers.Nodes;
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
        where T : struct, IFormattable
    {
        private readonly IVariableScope<T>? _parent;
        private readonly IWorkspace<T> _workspace;
        private readonly Dictionary<string, string> _descriptions = [];
        private readonly Dictionary<string, Quantity<T>> _computed = [];

        /// <inheritdoc />
        public IObservableDictionary<string, INode> Variables { get; } = new ObservableDictionary<string, INode>();

        /// <summary>
        /// Creates a new <see cref="VariableScope{T}"/>.
        /// </summary>
        public VariableScope(IWorkspace<T> workspace)
        {
            _parent = null;
            _workspace = workspace;
            Variables.DictionaryChanged += VariablesChanged;
        }

        /// <summary>
        /// Creates a <see cref="VariableScope{T}"/>
        /// </summary>
        /// <param name="parent">The parent scope.</param>
        public VariableScope(IWorkspace<T> workspace, IVariableScope<T> parent)
        {
            _parent = parent;
            _workspace = workspace;
            Variables.DictionaryChanged += VariablesChanged;
        }

        private void VariablesChanged(object sender, DictionaryChangedEventArgs<string, INode> e)
        {
            switch (e.Action)
            {
                case DictionaryChangeAction.Replace:
                    foreach (var item in e.Items)
                        _computed.Remove(item.Key);
                    break;

                case DictionaryChangeAction.Remove:
                    foreach (var item in e.Items)
                    {
                        _computed.Remove(item.Key);
                        _descriptions.Remove(item.Key);
                    }
                    break;
            }
        }

        /// <inheritdoc />
        bool IVariableScope<T>.TryGetComputedVariable(string name, out Quantity<T> result)
        {
            if (_computed.TryGetValue(name, out result))
                return true;
            if (Variables.TryGetValue(name, out var node))
            {
                // We need to compute it first
                if (!_workspace.TryResolve(node, out result))
                    return false;
                _computed[name] = result;
                return true;
            }

            // If we couldn't figure it out, ask the parent scope
            if (_parent is not null)
                return _parent.TryGetComputedVariable(name, out result);
            result = default;
            return false;
        }

        /// <inheritdoc />
        public bool TrySetDescription(string name, string? description)
        {
            if (Variables.ContainsKey(name))
            {
                if (description is null)
                    _descriptions.Remove(name);
                else
                    _descriptions[name] = description;
                return true;
            }
            return false;
        }

        /// <inheritdoc />
        public bool TryGetDescription(string name, out string? description)
            => _descriptions.TryGetValue(name, out description);
    }
}
