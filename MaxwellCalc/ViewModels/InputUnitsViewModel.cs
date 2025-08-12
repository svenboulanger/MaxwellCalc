using Avalonia.Controls;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using MaxwellCalc.Parsers;
using MaxwellCalc.Units;
using MaxwellCalc.Workspaces;
using System;
using System.Collections.Generic;
using System.Linq;

namespace MaxwellCalc.ViewModels
{
    public partial class InputUnitsViewModel : FilteredCollectionViewModel<InputUnitViewModel>
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
                InsertModel(new InputUnitViewModel { Name = "cm", Value = new("0.01", Unit.UnitMeter) });
                InsertModel(new InputUnitViewModel { Name = "kbps", Value = new("1000", new Unit(("bps", 1))) });
            }
        }

        /// <summary>
        /// Creates a new <see cref="InputUnitsViewModel"/>.
        /// </summary>
        /// <param name="sp">The service provider.</param>
        public InputUnitsViewModel(IServiceProvider sp)
            : base(sp)
        {
            if (Shared.Workspace is not null)
                Shared.Workspace.InputUnitChanged += OnInputUnitChanged;
        }

        /// <inheritdoc />
        protected override bool MatchesFilter(InputUnitViewModel model)
            => string.IsNullOrWhiteSpace(Filter) ||
            (model.Name?.Contains(Filter, StringComparison.OrdinalIgnoreCase) ?? false);

        /// <inheritdoc />
        protected override int CompareModels(InputUnitViewModel a, InputUnitViewModel b)
            => StringComparer.OrdinalIgnoreCase.Compare(a.Name, b.Name);

        /// <inheritdoc />
        protected override IEnumerable<InputUnitViewModel> ChangeWorkspace(IWorkspace? oldWorkspace, IWorkspace? newWorkspace)
        {
            if (oldWorkspace is not null)
                oldWorkspace.InputUnitChanged -= OnInputUnitChanged;
            if (newWorkspace is null)
                yield break;
            newWorkspace.InputUnitChanged += OnInputUnitChanged;

            // Build the list
            foreach (var unit in newWorkspace.InputUnits)
            {
                yield return new InputUnitViewModel
                {
                    Name = unit.UnitName,
                    Value = unit.Value
                };
            }
        }

        /// <inheritdoc />
        protected override void RemoveModelFromWorkspace(IWorkspace workspace, InputUnitViewModel model)
        {
            if (model.Name is null)
                return;
            workspace.TryRemoveInputUnit(model.Name);
        }

        private void OnInputUnitChanged(object? sender, InputUnitChangedEvent args)
        {
            var model = Items.FirstOrDefault(item => item.Name == args.Name);
            if (Shared.Workspace is null || args.Name is null)
                return;

            if (model is null)
            {
                // This is a new input unit
                if (!Shared.Workspace.TryGetInputUnit(args.Name, out var unit))
                    return;
                InsertModel(new InputUnitViewModel
                {
                    Name = unit.UnitName,
                    Value = unit.Value
                });
            }
            else if (Shared.Workspace.TryGetInputUnit(args.Name, out var unit))
            {
                // This is an updated unit
                model.Name = unit.UnitName;
                model.Value = unit.Value;
            }
            else
            {
                // This is a removed unit
                Items.Remove(model);
            }
        }

        [RelayCommand]
        private void AddInputUnit()
        {
            if (Shared.Workspace is null || string.IsNullOrWhiteSpace(Expression) || string.IsNullOrWhiteSpace(InputUnit))
                return;

            // Try to evaluate the expression
            string expression = Expression.Trim();
            Quantity<string> formatted;
            if (!expression.All(char.IsLetter))
            {
                var lexer = new Lexer(Expression);
                var node = Parser.Parse(lexer, Shared.Workspace);
                if (node is null)
                    return;
                if (!Shared.Workspace.TryResolveAndFormat(node, false, out formatted))
                    return;
            }
            else
            {
                // If the expression is just a string, allow registering as a base unit
                formatted = new Quantity<string>("1", new Unit((expression, 1)));
            }

            // Evaluate the name
            string unit = InputUnit.Trim();
            if (string.IsNullOrEmpty(unit)) // Should be non-empty
                return;
            if (!unit.All(char.IsLetter)) // Require all letters
                return;

            // Pass them on to the workspace
            if (Shared.Workspace.TryRegisterInputUnit(new InputUnit(unit, formatted)))
            {
                Expression = string.Empty;
                InputUnit = string.Empty;
                Shared.SaveWorkspace();
            }
        }
    }
}
