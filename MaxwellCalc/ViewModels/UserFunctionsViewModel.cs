using Avalonia.Controls;
using MaxwellCalc.Workspaces;
using System;
using System.Collections.Generic;
using System.Linq;

namespace MaxwellCalc.ViewModels
{
    /// <summary>
    /// A view model for the function list.
    /// </summary>
    public partial class UserFunctionsViewModel : FilteredCollectionViewModel<UserFunctionViewModel>
    {
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
                Shared.Workspace.FunctionChanged += OnFunctionChanged;
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
    }
}
