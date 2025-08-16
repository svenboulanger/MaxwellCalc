using Avalonia.Controls;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using MaxwellCalc.Core.Workspaces;
using MaxwellCalc.Parsers;
using MaxwellCalc.Parsers.Nodes;
using MaxwellCalc.Workspaces;
using System;
using System.Linq;
using MaxwellCalc.Units;
using System.Collections.Generic;

namespace MaxwellCalc.ViewModels
{
    public partial class InputUnitsViewModel : FilteredCollectionViewModel<InputUnitViewModel, string, Quantity<INode>>
    {
        [ObservableProperty]
        private string? _inputUnit;

        [ObservableProperty]
        private string? _expression;

        /// <summary>
        /// Creates a new <see cref="InputUnitsViewModel"/>.
        /// </summary>
        public InputUnitsViewModel()
        {
            if (Design.IsDesignMode)
            {
                if (Shared.Workspace is IWorkspace<double> workspace)
                {
                    workspace.RegisterCommonUnits();
                }
            }
        }

        /// <summary>
        /// Creates a new <see cref="InputUnitsViewModel"/>.
        /// </summary>
        /// <param name="sp">The service provider.</param>
        public InputUnitsViewModel(IServiceProvider sp)
            : base(sp)
        {
        }

        /// <inheritdoc />
        protected override bool MatchesFilter(InputUnitViewModel model)
            => string.IsNullOrWhiteSpace(Filter) ||
            (model.Name?.Contains(Filter, StringComparison.OrdinalIgnoreCase) ?? false);

        /// <inheritdoc />
        protected override int CompareModels(InputUnitViewModel a, InputUnitViewModel b)
            => StringComparer.OrdinalIgnoreCase.Compare(a.Name, b.Name);

        /// <inheritdoc />
        protected override IReadonlyObservableDictionary<string, Quantity<INode>> GetCollection(IWorkspace workspace)
            => workspace.InputUnits.AsReadOnly();

        /// <inheritdoc />
        protected override void UpdateModel(InputUnitViewModel model, string key, Quantity<INode> value)
        {
            model.Name = key;
            model.Value = new(value.Scalar.Content.ToString(), value.Unit);
        }

        /// <inheritdoc />
        protected override void RemoveItem(string key)
        {
            if (Shared.Workspace is null)
                return;
            Shared.Workspace.InputUnits.Remove(key);
        }

        [RelayCommand]
        private void AddInputUnit()
        {
            if (Shared.Workspace is null || string.IsNullOrWhiteSpace(Expression) || string.IsNullOrWhiteSpace(InputUnit))
                return;

            bool oldResolveInputUnits = Shared.Workspace.ResolveInputUnits;
            bool oldResolveOutputUnits = Shared.Workspace.ResolveOutputUnits;
            bool oldAllowVariables = Shared.Workspace.AllowVariables;
            Shared.Workspace.AllowVariables = false;
            Shared.Workspace.ResolveInputUnits = false;
            Shared.Workspace.ResolveOutputUnits = false;

            // Try to evaluate the expression
            string expression = Expression.Trim();
            var lexer = new Lexer(Expression);
            var baseUnits = Parser.Parse(lexer, Shared.Workspace);
            if (baseUnits is null)
                return;
            if (!Shared.Workspace.TryResolveAndFormat(baseUnits, "g", System.Globalization.CultureInfo.InvariantCulture, out var result))
                return;

            // Evaluate the name
            string unit = InputUnit.Trim();
            if (string.IsNullOrEmpty(unit)) // Should be non-empty
                return;
            if (!unit.All(char.IsLetter)) // Require all letters
                return;

            // Pass them on to the workspace
            Shared.Workspace.InputUnits[unit] = new(new ScalarNode(result.Scalar.AsMemory()), result.Unit);

            // Reset
            Shared.Workspace.ResolveInputUnits = oldResolveInputUnits;
            Shared.Workspace.ResolveOutputUnits = oldResolveOutputUnits;
            Shared.Workspace.AllowVariables = oldAllowVariables;
            InputUnit = string.Empty;
            Expression = string.Empty;
        }
    }
}
