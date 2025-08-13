using Avalonia.Controls;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using MaxwellCalc.Core.Workspaces;
using MaxwellCalc.Parsers;
using MaxwellCalc.Parsers.Nodes;
using MaxwellCalc.Units;
using MaxwellCalc.Workspaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MaxwellCalc.ViewModels
{
    public partial class OutputUnitsViewModel : FilteredCollectionViewModel<OutputUnitViewModel, OutputUnitKey, INode>
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
                if (Shared.Workspace is not null)
                {
                    Shared.Workspace.OutputUnits[new(new(("cm", 1)), Units.Unit.UnitMeter)] = new ScalarNode("0.01".AsMemory());
                }
            }
        }

        /// <summary>
        /// Creates a new <see cref="OutputUnitsViewModel"/>.
        /// </summary>
        /// <param name="sp">The service provider.</param>
        public OutputUnitsViewModel(IServiceProvider sp)
            : base(sp)
        {
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
        protected override IObservableDictionary<OutputUnitKey, INode> GetCollection(IWorkspace workspace)
            => workspace.OutputUnits;

        protected override void UpdateModel(OutputUnitViewModel model, OutputUnitKey key, INode value)
        {
            model.Unit = key.OutputUnit;
            model.Value = new Quantity<string>(value.Content.ToString(), key.BaseUnit);
        }

        [RelayCommand]
        private void AddOutputUnit()
        {
            if (Shared.Workspace is null || string.IsNullOrWhiteSpace(Unit) || string.IsNullOrWhiteSpace(Expression))
                return;

            bool oldResolveInputUnits = Shared.Workspace.ResolveInputUnits;
            bool oldResolveOutputUnits = Shared.Workspace.ResolveOutputUnits;
            bool oldAllowVariables = Shared.Workspace.AllowVariables;
            Shared.Workspace.AllowVariables = false;
            Shared.Workspace.ResolveInputUnits = false;
            Shared.Workspace.ResolveOutputUnits = false;

            // Try to evaluate the output unit
            var lexer = new Lexer(Unit);
            var unitNode = Parser.Parse(lexer, Shared.Workspace);
            if (unitNode is null)
                return;
            if (!Shared.Workspace.TryResolveAndFormat(unitNode, out var unitNodeResult))
                return;

            // Try to evaluate the expression to get to the input units
            lexer = new Lexer(Expression);
            var exprNode = Parser.Parse(lexer, Shared.Workspace);
            if (exprNode is null)
                return;
            Shared.Workspace.ResolveInputUnits = true;
            if (!Shared.Workspace.TryResolveAndFormat(unitNode, out var exprNodeResult))
                return;

            // Pass them on to the workspace
            if (unitNodeResult.Scalar != "1")
            {
                Shared.Workspace.OutputUnits[new(unitNodeResult.Unit, exprNodeResult.Unit)] = new BinaryNode(BinaryOperatorTypes.Divide,
                    new ScalarNode(exprNodeResult.Scalar.AsMemory()),
                    new ScalarNode(unitNodeResult.Scalar.AsMemory()),
                    $"{exprNodeResult.Scalar}/{exprNodeResult.Scalar}".AsMemory());
            }
            else
                Shared.Workspace.OutputUnits[new(unitNodeResult.Unit, exprNodeResult.Unit)] = new ScalarNode(exprNodeResult.Scalar.AsMemory());

            // Reset
            Shared.Workspace.ResolveInputUnits = oldResolveInputUnits;
            Shared.Workspace.ResolveOutputUnits = oldResolveOutputUnits;
            Shared.Workspace.AllowVariables = oldAllowVariables;
            Unit = string.Empty;
            Expression = string.Empty;
        }
    }
}
