using Avalonia.Controls;
using MaxwellCalc.Units;
using MaxwellCalc.Workspaces;
using System;
using System.Collections.Generic;
using System.Linq;

namespace MaxwellCalc.ViewModels
{
    public partial class ConstantsViewModel : FilteredCollectionViewModel<UserVariableViewModel>
    {
        /// <summary>
        /// Creates a new <see cref="ConstantsViewModel"/>.
        /// </summary>
        public ConstantsViewModel()
        {
            if (Design.IsDesignMode)
            {
                // Make up some constants to show what it looks like
                InsertModel(new()
                {
                    Name = "Sven",
                    Value = new Quantity<string>("179", new Unit(("cm", 1))),
                    Description = "The length of the author."
                });
                InsertModel(new()
                {
                    Name = "CheckThis",
                    Value = new Quantity<string>("123", Unit.UnitAmperes)
                });
            }
        }

        /// <summary>
        /// Creates a new <see cref="ConstantsViewModel"/>.
        /// </summary>
        /// <param name="sp">The service provider.</param>
        public ConstantsViewModel(IServiceProvider sp)
            : base(sp)
        {
            if (Shared.Workspace is not null)
                Shared.Workspace.ConstantChanged += OnConstantChanged;
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
        protected override void RemoveModelFromWorkspace(IWorkspace workspace, UserVariableViewModel model)
            => workspace.TryRemoveConstant(model.Name ?? string.Empty);

        /// <inheritdoc />
        protected override IEnumerable<UserVariableViewModel> ChangeWorkspace(IWorkspace? oldWorkspace, IWorkspace? newWorkspace)
        {
            if (oldWorkspace is not null)
                oldWorkspace.ConstantChanged -= OnConstantChanged;
            if (newWorkspace is null)
                yield break; ;
            newWorkspace.ConstantChanged += OnConstantChanged;

            // Go through all the constants and convert
            foreach (var constant in newWorkspace.Constants)
            {
                yield return new UserVariableViewModel()
                {
                    Name = constant.Name,
                    Value = constant.Value,
                    Description = constant.Description
                };
            }
        }

        private void OnConstantChanged(object? sender, VariableChangedEvent args)
        {
            // Find the variabel with the same name
            var model = Items.FirstOrDefault(item => item.Name == args.Name);
            if (Shared.Workspace is null || args.Name is null)
                throw new ArgumentNullException();

            if (model is null)
            {
                // This is a new constant
                if (!Shared.Workspace.TryGetConstant(args.Name, out var value, out string? description))
                    return;
                InsertModel(new UserVariableViewModel
                {
                    Name = args.Name,
                    Value = value,
                    Description = description
                });
            }
            else if (Shared.Workspace.TryGetConstant(args.Name, out var value, out string? description))
            {
                // This is an updated constant
                model.Name = args.Name;
                model.Value = value;
                model.Description = description;
            }
            else
            {
                // This is a removed constant
                Items.Remove(model);
                FilteredItems.Remove(model);
            }
        }
    }
}
