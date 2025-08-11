using Avalonia.Controls;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using MaxwellCalc.Parsers;
using MaxwellCalc.Units;
using MaxwellCalc.Workspaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MaxwellCalc.ViewModels
{
    public partial class OutputUnitsViewModel : FilteredCollectionViewModel<OutputUnitViewModel>
    {
        [ObservableProperty]
        private string _unit = string.Empty;

        [ObservableProperty]
        private string _expression = string.Empty;

        /// <summary>
        /// Creates a new <see cref="OutputUnitsViewModel"/>.
        /// </summary>
        public OutputUnitsViewModel()
        {
            if (Design.IsDesignMode)
            {
                InsertModel(new OutputUnitViewModel
                {
                    Unit = new(("cm", 2)),
                    Value = new("0.0001", new(("m", 2)))
                });
            }
        }

        /// <summary>
        /// Creates a new <see cref="OutputUnitsViewModel"/>.
        /// </summary>
        /// <param name="sp">The service provider.</param>
        public OutputUnitsViewModel(IServiceProvider sp)
            : base(sp)
        {
            if (Shared.Workspace is not null)
                Shared.Workspace.OutputUnitChanged += OnOutputUnitChanged;
        }

        private void OnOutputUnitChanged(object? sender, OutputUnitchangedEvent args)
        {
            var model = Items.FirstOrDefault(item => item.Unit.Equals(args.OutputUnit) && item.Value.Unit.Equals(args.BaseUnit));
            if (Shared.Workspace is null)
                throw new ArgumentNullException();

            if (model is null)
            {
                // This is a new output unit
                if (!Shared.Workspace.TryGetOutputUnit(args.BaseUnit, args.OutputUnit, out var unit))
                    return;
                InsertModel(new OutputUnitViewModel
                {
                    Unit = unit.Unit,
                    Value = unit.Value
                });
            }
            else if (Shared.Workspace.TryGetOutputUnit(args.BaseUnit, args.OutputUnit, out var unit))
            {
                // This is an updated unit
                model.Unit = unit.Unit;
                model.Value = unit.Value;
            }
            else
            {
                // This is a removed unit
                Items.Remove(model);
                FilteredItems.Remove(model);
            }
        }

        /// <inheritdoc />
        protected override bool MatchesFilter(OutputUnitViewModel model)
        {
            if (string.IsNullOrWhiteSpace(Filter))
                return true;
            if (model.Unit.ToString().Contains(Filter, StringComparison.OrdinalIgnoreCase))
                return true;
            if (model.Value.ToString().Contains(Filter, StringComparison.OrdinalIgnoreCase))
                return true;
            return false;
        }

        /// <inheritdoc />
        protected override int CompareModels(OutputUnitViewModel a, OutputUnitViewModel b)
        {
            int c = a.Unit.ToString().CompareTo(b.Unit.ToString());
            if (c == 0)
                return a.Value.Unit.ToString().CompareTo(b.Value.Unit.ToString());
            return c;
        }

        /// <inheritdoc />
        protected override IEnumerable<OutputUnitViewModel> ChangeWorkspace(IWorkspace? oldWorkspace, IWorkspace? newWorkspace)
        {
            if (oldWorkspace is not null)
                oldWorkspace.OutputUnitChanged -= OnOutputUnitChanged;
            if (newWorkspace is null)
                yield break;
            newWorkspace.OutputUnitChanged += OnOutputUnitChanged;

            // Build the list
            foreach (var unit in newWorkspace.OutputUnits)
            {
                yield return new OutputUnitViewModel
                {
                    Unit = unit.Unit,
                    Value = unit.Value
                };
            }
        }

        /// <inheritdoc />
        protected override void RemoveModelFromWorkspace(IWorkspace workspace, OutputUnitViewModel model)
        {
            workspace.TryRemoveOutputUnit(model.Unit, model.Value.Unit);
        }

        [RelayCommand]
        private void AddOutputUnit()
        {
            if (Shared.Workspace is null || string.IsNullOrWhiteSpace(Unit) || string.IsNullOrWhiteSpace(Expression))
                return;

            // Try to evaluate the unit
            var lexer = new Lexer(Unit);
            var unitNode = Parser.Parse(lexer, Shared.Workspace);
            if (unitNode is null)
                return;

            // Try to evaluate the expression
            lexer = new Lexer(Expression);
            var exprNode = Parser.Parse(lexer, Shared.Workspace);
            if (exprNode is null)
                return;

            // Pass them on to the workspace
            Shared.Workspace.TryRegisterOutputUnit(unitNode, exprNode);
            Unit = string.Empty;
            Expression = string.Empty;
        }
    }
}
