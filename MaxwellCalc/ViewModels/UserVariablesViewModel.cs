using Avalonia.Controls;
using MaxwellCalc.Units;
using MaxwellCalc.Workspaces;
using System;
using System.Collections.Generic;
using System.Linq;

namespace MaxwellCalc.ViewModels
{
    public partial class UserVariablesViewModel : FilteredCollectionViewModel<UserVariableViewModel>
    {
        /// <summary>
        /// Creates a new <see cref="UserVariablesViewModel"/>.
        /// </summary>
        public UserVariablesViewModel()
        {
            if (Design.IsDesignMode)
            {
                // Make up some variables to show what it looks like
                InsertModel(new UserVariableViewModel()
                {
                    Name = "Sven",
                    Value = new Quantity<string>("179", new Unit(("cm", 1)))
                });
                InsertModel(new UserVariableViewModel()
                {
                    Name = "Elke",
                    Value = new Quantity<string>("150", new Unit(("cm", 1)))
                });
            }
        }

        /// <summary>
        /// Creates a new <see cref="UserVariablesViewModel"/>.
        /// </summary>
        /// <param name="sp">The service provider.</param>
        public UserVariablesViewModel(IServiceProvider sp)
            : base(sp)
        {
            if (Shared.Workspace is not null)
                Shared.Workspace.VariableChanged += OnVariableChanged;
        }

        /// <inheritdoc />
        protected override bool MatchesFilter(UserVariableViewModel model)
            => string.IsNullOrWhiteSpace(Filter) ||
            (model.Name?.Contains(Filter, StringComparison.OrdinalIgnoreCase) ?? false);

        /// <inheritdoc />
        protected override int CompareModels(UserVariableViewModel a, UserVariableViewModel b)
            => StringComparer.OrdinalIgnoreCase.Compare(a.Name, b.Name);

        /// <inheritdoc />
        protected override void RemoveModelFromWorkspace(IWorkspace workspace, UserVariableViewModel model)
        {
            if (Shared.Workspace is null || model.Name is null)
                return;
            Shared.Workspace.TryRemoveVariable(model.Name);
        }

        protected override IEnumerable<UserVariableViewModel> ChangeWorkspace(IWorkspace? oldWorkspace, IWorkspace? newWorkspace)
        {
            if (oldWorkspace is not null)
                oldWorkspace.VariableChanged -= OnVariableChanged;
            if (newWorkspace is null)
                yield break;
            newWorkspace.VariableChanged += OnVariableChanged;

            // Rebuild our internal list of variables
            foreach (var variable in newWorkspace.Variables)
            {
                yield return new UserVariableViewModel()
                {
                    Name = variable.Name,
                    Value = variable.Value
                };
            }
        }

        private void OnVariableChanged(object? sender, VariableChangedEvent args)
        {
            // Check if we have a model for it
            var model = Items.FirstOrDefault(item => item.Name == args.Name);
            if (Shared.Workspace is null || args.Name is null)
                return;

            if (model is null)
            {
                // This is a new user variable
                if (!Shared.Workspace.TryGetVariable(args.Name, out var value))
                    return;
                InsertModel(new UserVariableViewModel
                {
                    Name = args.Name,
                    Value = value
                });
            }
            else if (Shared.Workspace.TryGetVariable(args.Name, out var value))
            {
                // This is an updated variable
                model.Name = args.Name;
                model.Value = value;
            }
            else
            {
                // This is a removed variable
                Items.Remove(model);
            }
        }
    }
}
