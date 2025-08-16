using Avalonia.Controls;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using MaxwellCalc.Core.Workspaces;
using MaxwellCalc.Parsers;
using MaxwellCalc.Parsers.Nodes;
using MaxwellCalc.Units;
using MaxwellCalc.Workspaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;

namespace MaxwellCalc.ViewModels
{
    /// <summary>
    /// A view model for the function list.
    /// </summary>
    public partial class UserFunctionsViewModel : FilteredCollectionViewModel<UserFunctionViewModel, UserFunctionKey, UserFunction>
    {
        [ObservableProperty]
        private string _signature = string.Empty;

        [ObservableProperty]
        private string _expression = string.Empty;

        /// <summary>
        /// Creates a new <see cref="UserFunctionsViewModel"/>.
        /// </summary>
        public UserFunctionsViewModel()
        {
            if (Design.IsDesignMode)
            {
            }
        }

        /// <summary>
        /// Creates a new <see cref="UserFunctionsViewModel"/>.
        /// </summary>
        /// <param name="sp">The service provider.</param>
        public UserFunctionsViewModel(IServiceProvider sp)
            : base(sp)
        {
        }

        /// <inheritdoc />
        protected override bool MatchesFilter(UserFunctionViewModel model)
            => string.IsNullOrWhiteSpace(Filter) ||
            (model.Name?.Contains(Filter, StringComparison.OrdinalIgnoreCase) ?? false) ||
            (model.Value?.Contains(Filter, StringComparison.OrdinalIgnoreCase) ?? false);

        /// <inheritdoc />
        protected override int CompareModels(UserFunctionViewModel a, UserFunctionViewModel b)
            => StringComparer.OrdinalIgnoreCase.Compare(a.Name, b.Name);

        /// <inheritdoc />
        protected override IReadonlyObservableDictionary<UserFunctionKey, UserFunction> GetCollection(IWorkspace workspace)
            => workspace.UserFunctions.AsReadOnly();

        /// <inheritdoc />
        protected override void UpdateModel(UserFunctionViewModel model, UserFunctionKey key, UserFunction value)
        {
            model.Name = key.Name;
            model.Arguments = [.. value.Parameters];
            model.Value = string.Join(Environment.NewLine, value.Body.Select(n => n.Content.ToString()));
        }

        /// <inheritdoc />
        protected override void RemoveItem(UserFunctionKey key)
        {
            if (Shared.Workspace is null)
                return;
            Shared.Workspace.UserFunctions.Remove(key);
        }

        [RelayCommand]
        private void AddUserFunction()
        {
            if (Shared.Workspace is null || string.IsNullOrWhiteSpace(Signature) || string.IsNullOrWhiteSpace(Expression))
                return;

            // The name
            var lexer = new Lexer(Signature);
            var node = Parser.Parse(lexer, Shared.Workspace);
            if (node is not FunctionNode fn)
                return;
            var args = new string[fn.Arguments.Count];
            for (int i = 0; i < fn.Arguments.Count; i++)
            {
                if (fn.Arguments[i] is not VariableNode argNode)
                    return;
                args[i] = argNode.Content.ToString();
            }

            // Parse the nodes
            var lines = Expression.Split(['\r', '\n'], StringSplitOptions.RemoveEmptyEntries);
            var nodes = new List<INode>();
            foreach (var line in lines)
            {
                lexer = new Lexer(line);
                node = Parser.Parse(lexer, Shared.Workspace);
                if (node is null)
                    return;
                nodes.Add(node);
            }

            Shared.Workspace.UserFunctions[new(fn.Name, args.Length)] = new(args, nodes.ToArray());
            Signature = string.Empty;
            Expression = string.Empty;
        }
    }
}
