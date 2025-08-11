using Avalonia.Controls;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using MaxwellCalc.Parsers;
using MaxwellCalc.Parsers.Nodes;
using MaxwellCalc.Units;
using MaxwellCalc.Workspaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace MaxwellCalc.ViewModels
{
    /// <summary>
    /// A view model for the function list.
    /// </summary>
    public partial class UserFunctionsViewModel : FilteredCollectionViewModel<UserFunctionViewModel>
    {
        [ObservableProperty]
        private string _signature;

        [ObservableProperty]
        private string _expression;

        /// <summary>
        /// Creates a new <see cref="UserFunctionsViewModel"/>.
        /// </summary>
        public UserFunctionsViewModel()
        {
            if (Design.IsDesignMode)
            {
                for (int i = 0; i < 5; i++)
                {
                    InsertModel(new UserFunctionViewModel() { Name = $"Test", Arguments = ["a", "b"], Value = "a + b" });
                }
            }
        }

        /// <summary>
        /// Creates a new <see cref="UserFunctionsViewModel"/>.
        /// </summary>
        /// <param name="sp">The service provider.</param>
        public UserFunctionsViewModel(IServiceProvider sp)
            : base(sp)
        {
            if (Shared.Workspace is not null)
            {
                Shared.Workspace.FunctionChanged += OnFunctionChanged;
                Shared.Workspace.TryRegisterUserFunction(new UserFunction(
                    "help",
                    ["x"],
                    string.Join(Environment.NewLine, "a = x * 2", "a * a")));
            }
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
        protected override void RemoveModelFromWorkspace(IWorkspace workspace, UserFunctionViewModel model)
        {
            if (Shared.Workspace is null || model.Name is null || model.Arguments is null)
                return;
            Shared.Workspace.TryRemoveUserFunction(model.Name, model.Arguments.Count);
        }

        /// <inheritdoc />
        protected override IEnumerable<UserFunctionViewModel> ChangeWorkspace(IWorkspace? oldWorkspace, IWorkspace? newWorkspace)
        {
            if (oldWorkspace is not null)
                oldWorkspace.FunctionChanged -= OnFunctionChanged;
            if (newWorkspace is null)
                yield break;
            newWorkspace.FunctionChanged += OnFunctionChanged;

            // Go through all user functions
            foreach (var function in newWorkspace.UserFunctions)
            {
                yield return new UserFunctionViewModel()
                {
                    Name = function.Name,
                    Arguments = [.. function.Parameters],
                    Value = function.Body
                };
            }
        }

        private void OnFunctionChanged(object? sender, FunctionChangedEvent args)
        {
            // Find the model
            var model = Items.FirstOrDefault(item => item.Name == args.Name);
            if (Shared.Workspace is null || args.Name is null)
                throw new ArgumentNullException();

            if (model is null)
            {
                // This is a new built-in function
                if (!Shared.Workspace.TryGetUserFunction(args.Name, args.Arguments, out var function))
                    return;
                InsertModel(new UserFunctionViewModel
                {
                    Name = function.Name,
                    Arguments = [.. function.Parameters],
                    Value = function.Body
                });
            }
            else if (Shared.Workspace.TryGetUserFunction(args.Name, args.Arguments, out var function))
            {
                // This is an updated function
                model.Name = function.Name;
                model.Arguments = [.. function.Parameters];
                model.Value = function.Body;
            }
            else
            {
                // This is a removed function
                Items.Remove(model);
                FilteredItems.Remove(model);
            }
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
            Shared.Workspace.TryRegisterUserFunction(new(fn.Name, args, Expression));

            Signature = string.Empty;
            Expression = string.Empty;
        }
    }
}
