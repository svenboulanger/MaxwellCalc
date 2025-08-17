using Avalonia.Controls;
using MaxwellCalc.Core.Dictionaries;
using MaxwellCalc.Core.Units;
using MaxwellCalc.Core.Workspaces;
using System;
using System.Collections.Generic;

namespace MaxwellCalc.ViewModels
{
    /// <summary>
    /// A view model for the built-in function list.
    /// </summary>
    public partial class BuiltInFunctionsViewModel : FilteredCollectionViewModel<BuiltInFunctionViewModel, string, BuiltInFunction>
    {
        /// <summary>
        /// Creates a new <see cref="BuiltInFunctionViewModel"/>.
        /// </summary>
        public BuiltInFunctionsViewModel()
        {
            if (Design.IsDesignMode)
            {
                if (Shared.Workspace is IWorkspace<double>)
                {
                    bool Function(IReadOnlyList<Quantity<double>> list, IWorkspace workspace, out Quantity<double> result)
                    {
                        result = default;
                        return true;
                    }
                    Shared.Workspace.BuiltInFunctions["sin"] = new("sin", 1, 1, "Calculates the sine of a real number.", Function);
                    Shared.Workspace.BuiltInFunctions["cos"] = new("cos", 1, 1, "Calculates the cosine of a real number.", Function);
                    Shared.Workspace.BuiltInFunctions["tan"] = new("tan", 1, 1, "Calculates the tangent of a real number.", Function);
                    Shared.Workspace.BuiltInFunctions["min"] = new("min", 1, int.MaxValue, "Calculates the mininimum of real arguments.", Function);
                    Shared.Workspace.BuiltInFunctions["round"] = new("round", 1, 2, "Rounds a number to some precision. If the precision is not given, then it will round to 0 digits after the comma.", Function);
                }
            }
        }

        /// <summary>
        /// Creates a new <see cref="BuiltInFunctionsViewModel"/>.
        /// </summary>
        /// <param name="sp">The service provider.</param>
        public BuiltInFunctionsViewModel(IServiceProvider sp)
            : base(sp)
        {
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
        protected override IReadOnlyObservableDictionary<string, BuiltInFunction> GetCollection(IWorkspace workspace)
            => workspace.BuiltInFunctions.AsReadOnly();

        /// <inheritdoc />
        protected override void UpdateModel(BuiltInFunctionViewModel model, string key, BuiltInFunction value)
        {
            model.Name = key;
            model.MinArgCount = value.MinimumArgumentCount;
            model.MaxArgCount = value.MaximumArgumentCount;
            model.Description = value.Description;
        }
    }
}
