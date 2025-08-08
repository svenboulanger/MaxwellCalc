using Avalonia.Controls;
using MaxwellCalc.Workspaces;
using System;
using System.Collections.Generic;
using System.Linq;

namespace MaxwellCalc.ViewModels
{
    /// <summary>
    /// A view model for the built-in function list.
    /// </summary>
    public partial class BuiltInFunctionsViewModel : FilteredCollectionViewModel<BuiltInFunctionViewModel>
    {
        /// <summary>
        /// Creates a new <see cref="BuiltInFunctionViewModel"/>.
        /// </summary>
        public BuiltInFunctionsViewModel()
        {
            if (Design.IsDesignMode)
            {
                InsertModel(new()
                {
                    Name = "sin",
                    MinArgCount = 1,
                    MaxArgCount = 1,
                    Description = "Calculates the sine of a real number"
                });
                InsertModel(new()
                {
                    Name = "cos",
                    MinArgCount = 1,
                    MaxArgCount = 1,
                    Description = "Calculates the cosine of a real number"
                });
                InsertModel(new()
                {
                    Name = "tan",
                    MinArgCount = 1,
                    MaxArgCount = 1,
                    Description = "Calculates the tangent of a real number"
                });
                InsertModel(new()
                {
                    Name = "min",
                    MinArgCount = 1,
                    MaxArgCount = int.MaxValue,
                    Description = "Calculates the min of a real number"
                });
                InsertModel(new()
                {
                    Name = "round",
                    MinArgCount = 1,
                    MaxArgCount = 2,
                    Description = "Rounds a number to some precision. If the precision is not given, then it will round to 0 digits after the comma."
                });
            }
        }

        /// <summary>
        /// Creates a new <see cref="BuiltInFunctionsViewModel"/>.
        /// </summary>
        /// <param name="sp">The service provider.</param>
        public BuiltInFunctionsViewModel(IServiceProvider sp)
            : base(sp)
        {
            if (Shared.Workspace is not null)
                Shared.Workspace.BuiltInFunctionChanged += OnBuiltInFunctionChanged;
        }

        /// <inheritdoc />
        protected override bool MatchesFilter(BuiltInFunctionViewModel model)
            => string.IsNullOrWhiteSpace(Filter) ||
            (model.Name?.Contains(Filter, StringComparison.OrdinalIgnoreCase) ?? false) ||
            (model.Description?.Contains(Filter, StringComparison.OrdinalIgnoreCase) ?? false);

        /// <inheritdoc />
        protected override int CompareModels(BuiltInFunctionViewModel a, BuiltInFunctionViewModel b)
            => StringComparer.OrdinalIgnoreCase.Compare(a.Name, b.Name);

        /// <inheritdoc />
        protected override void RemoveModelFromWorkspace(IWorkspace workspace, BuiltInFunctionViewModel model)
            => throw new NotImplementedException();

        /// <inheritdoc />
        protected override IEnumerable<BuiltInFunctionViewModel> ChangeWorkspace(IWorkspace? oldWorkspace, IWorkspace? newWorkspace)
        {
            if (oldWorkspace is not null)
                oldWorkspace.BuiltInFunctionChanged -= OnBuiltInFunctionChanged;
            if (newWorkspace is null)
                yield break;
            newWorkspace.BuiltInFunctionChanged += OnBuiltInFunctionChanged;

            // Go through all built-in functions
            foreach (var function in newWorkspace.BuiltInFunctions)
            {
                yield return new BuiltInFunctionViewModel()
                {
                    Name = function.Name,
                    MinArgCount = function.MinimumArgumentCount,
                    MaxArgCount = function.MaximumArgumentCount,
                    Description = function.Description
                };
            }
        }

        private void OnBuiltInFunctionChanged(object? sender, FunctionChangedEvent args)
        {
            // Find the model
            var model = Items.FirstOrDefault(item => item.Name == args.Name);
            if (Shared.Workspace is null || args.Name is null)
                throw new ArgumentNullException();

            if (model is null)
            {
                // This is a new built-in function
                if (!Shared.Workspace.TryGetBuiltInFunction(args.Name, out var function))
                    return;
                InsertModel(new BuiltInFunctionViewModel
                {
                    Name = function.Name,
                    MinArgCount = function.MinimumArgumentCount,
                    MaxArgCount = function.MaximumArgumentCount,
                    Description = function.Description
                });
            }
            else if (Shared.Workspace.TryGetBuiltInFunction(args.Name, out var function))
            {
                // This is an updated function
                model.Name = function.Name;
                model.MinArgCount = function.MinimumArgumentCount;
                model.MaxArgCount = function.MaximumArgumentCount;
                model.Description = function.Description;
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
