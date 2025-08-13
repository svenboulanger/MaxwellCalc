using Avalonia.Controls;
using MaxwellCalc.Core.Workspaces;
using MaxwellCalc.Parsers.Nodes;
using MaxwellCalc.Workspaces;
using System;

namespace MaxwellCalc.ViewModels
{
    public partial class UserVariablesViewModel : FilteredCollectionViewModel<UserVariableViewModel, string, INode>
    {
        /// <summary>
        /// Creates a new <see cref="UserVariablesViewModel"/>.
        /// </summary>
        public UserVariablesViewModel()
        {
            if (Design.IsDesignMode)
            {
                if (Shared.Workspace is not null)
                    Shared.Workspace.Variables.Variables["test"] = new ScalarNode("123".AsMemory());
            }
        }

        /// <summary>
        /// Creates a new <see cref="UserVariablesViewModel"/>.
        /// </summary>
        /// <param name="sp">The service provider.</param>
        public UserVariablesViewModel(IServiceProvider sp)
            : base(sp)
        {
        }

        /// <inheritdoc />
        protected override bool MatchesFilter(UserVariableViewModel model)
            => string.IsNullOrWhiteSpace(Filter) ||
            (model.Name?.Contains(Filter, StringComparison.OrdinalIgnoreCase) ?? false);

        /// <inheritdoc />
        protected override int CompareModels(UserVariableViewModel a, UserVariableViewModel b)
            => StringComparer.OrdinalIgnoreCase.Compare(a.Name, b.Name);

        /// <inheritdoc />
        protected override IObservableDictionary<string, INode> GetCollection(IWorkspace workspace)
            => workspace.Variables.Variables;

        /// <inheritdoc />
        protected override void UpdateModel(UserVariableViewModel model, string key, INode value)
        {
            model.Name = key;
            model.Expression = value.Content.ToString();

            var node = new VariableNode(key.AsMemory());
            if (Shared.Workspace?.TryResolveAndFormat(node, out var result) ?? false)
                model.Value = result;
        }
    }
}
