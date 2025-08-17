using Avalonia.Controls;
using MaxwellCalc.Core.Dictionaries;
using MaxwellCalc.Core.Units;
using MaxwellCalc.Core.Workspaces;
using MaxwellCalc.Core.Workspaces.Variables;
using System;

namespace MaxwellCalc.ViewModels
{
    public partial class UserVariablesViewModel : FilteredCollectionViewModel<UserVariableViewModel, string, Variable<string>>
    {
        /// <summary>
        /// Creates a new <see cref="UserVariablesViewModel"/>.
        /// </summary>
        public UserVariablesViewModel()
        {
            if (Design.IsDesignMode)
            {
                if (Shared.Workspace is IWorkspace<double> workspace && workspace.Variables is IVariableScope<double> scope)
                    scope.Local["test"] = new(new Quantity<double>(123, Unit.UnitNone), null);
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
        protected override IReadOnlyObservableDictionary<string, Variable<string>> GetCollection(IWorkspace workspace)
            => workspace.Variables.Local;

        /// <inheritdoc />
        protected override void UpdateModel(UserVariableViewModel model, string key, Variable<string> value)
        {
            model.Name = key;
            model.Value = value.Value;
        }

        /// <inheritdoc />
        protected override void RemoveItem(string key)
        {
            if (Shared.Workspace is null)
                return;
            Shared.Workspace.Variables.TryRemoveVariable(key);
        }
    }
}
