using Avalonia.Controls;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using MaxwellCalc.Core.Workspaces;
using MaxwellCalc.Parsers;
using MaxwellCalc.Parsers.Nodes;
using MaxwellCalc.Workspaces;
using System;

namespace MaxwellCalc.ViewModels
{
    public partial class ConstantsViewModel : FilteredCollectionViewModel<UserVariableViewModel, string, INode>
    {
        [ObservableProperty]
        private string _constantName = string.Empty;

        [ObservableProperty]
        private string _expression = string.Empty;

        [ObservableProperty]
        private string _description = string.Empty;

        /// <summary>
        /// Creates a new <see cref="ConstantsViewModel"/>.
        /// </summary>
        public ConstantsViewModel()
        {
            if (Design.IsDesignMode)
            {
                // Make up some constants to show what it looks like
                if (Shared.Workspace is not null)
                {
                    Shared.Workspace.Constants.Variables["Sven"] = new BinaryNode(BinaryOperatorTypes.Multiply, new ScalarNode("179".AsMemory()), new UnitNode("cm".AsMemory()), "179cm".AsMemory());
                    Shared.Workspace.Constants.TrySetDescription("Sven", "The length of the author.");
                    Shared.Workspace.Constants.Variables["CheckThis"] = new BinaryNode(BinaryOperatorTypes.Multiply, new ScalarNode("123".AsMemory()), new UnitNode("A".AsMemory()), "123A".AsMemory());
                }
            }
        }

        /// <summary>
        /// Creates a new <see cref="ConstantsViewModel"/>.
        /// </summary>
        /// <param name="sp">The service provider.</param>
        public ConstantsViewModel(IServiceProvider sp)
            : base(sp)
        {
        }

        /// <inheritdoc />
        protected override bool MatchesFilter(UserVariableViewModel model)
            => string.IsNullOrWhiteSpace(Filter) ||
            (model.Name?.Contains(Filter, StringComparison.OrdinalIgnoreCase) ?? false) ||
            (model.Description?.Contains(Filter, StringComparison.OrdinalIgnoreCase) ?? false);

        /// <inheritdoc />
        protected override int CompareModels(UserVariableViewModel a, UserVariableViewModel b)
            => StringComparer.OrdinalIgnoreCase.Compare(a.Name, b.Name);

        /// <inheritdoc />
        protected override IObservableDictionary<string, INode> GetCollection(IWorkspace workspace)
            => workspace.Constants.Variables;

        protected override void UpdateModel(UserVariableViewModel model, string key, INode value)
        {
            model.Name = key;
            model.Expression = value.Content.ToString();

            var node = new VariableNode(key.AsMemory());
            if (Shared.Workspace?.TryResolveAndFormat(node, out var result) ?? false)
                model.Value = result;

            if (Shared.Workspace?.Constants.TryGetDescription(key, out var description) ?? false)
                model.Description = description ?? string.Empty;
        }

        [RelayCommand]
        private void AddConstant()
        {
            string name = ConstantName.Trim();
            string expression = Expression.Trim();
            if (Shared.Workspace is null || string.IsNullOrEmpty(name) || string.IsNullOrEmpty(expression))
                return;

            var lexer = new Lexer(expression);
            var node = Parser.Parse(lexer, Shared.Workspace);
            if (node is null)
                return;

            Shared.Workspace.Constants.Variables[name] = node;
            Shared.Workspace.Constants.TrySetDescription(name, Description);
            ConstantName = string.Empty;
            Expression = string.Empty;
        }
    }
}
