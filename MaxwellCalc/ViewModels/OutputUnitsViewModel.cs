using Avalonia.Controls;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using MaxwellCalc.Core.Dictionaries;
using MaxwellCalc.Core.Parsers;
using MaxwellCalc.Core.Units;
using MaxwellCalc.Core.Workspaces;
using System;
using System.Collections.ObjectModel;

namespace MaxwellCalc.ViewModels
{
    public partial class OutputUnitsViewModel : FilteredCollectionViewModel<OutputUnitViewModel, OutputUnitKey, string>
    {
        [ObservableProperty]
        private string _unit = string.Empty;

        [ObservableProperty]
        private string _expression = string.Empty;

        [ObservableProperty]
        private ObservableCollection<string> _diagnostics = [];

        /// <summary>
        /// Creates a new <see cref="OutputUnitsViewModel"/>.
        /// </summary>
        public OutputUnitsViewModel()
        {
            if (Design.IsDesignMode)
            {
                if (Shared.Workspace is IWorkspace<double> workspace)
                {
                    workspace.OutputUnits[new(new(("cm", 1)), Core.Units.Unit.UnitMeter)] = 0.01;
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
        protected override IReadOnlyObservableDictionary<OutputUnitKey, string> GetCollection(IWorkspace workspace)
            => workspace.OutputUnits;

        /// <inheritdoc />
        protected override void UpdateModel(OutputUnitViewModel model, OutputUnitKey key, string value)
        {
            model.Unit = key.OutputUnit;
            model.Value = new Quantity<string>(value, key.BaseUnit);
        }

        /// <inheritdoc />
        protected override void RemoveItem(OutputUnitKey key)
        {
            if (Shared.Workspace is null)
                return;
            Shared.Workspace.TryRemoveOutputUnit(key);
        }

        [RelayCommand]
        private void AddOutputUnit()
        {
            if (Shared.Workspace is null || string.IsNullOrWhiteSpace(Unit) || string.IsNullOrWhiteSpace(Expression))
                return;

            // Deal with diagnostic messages
            Diagnostics.Clear();
            void AddDiagnosticMessage(object? sender, DiagnosticMessagePostedEventArgs args)
                => Diagnostics.Add(args.Message);
            Shared.Workspace.DiagnosticMessagePosted += AddDiagnosticMessage;

            var oldState = Shared.Workspace.Restrict(false, false, true, false, false);
            try
            {
                // Try to evaluate the output unit
                var lexer = new Lexer(Unit);
                var outputUnitsNode = Parser.Parse(lexer, Shared.Workspace);
                if (outputUnitsNode is null)
                    return;

                // Try to evaluate the expression to get to the input units
                lexer = new Lexer(Expression);
                var baseUnitsNode = Parser.Parse(lexer, Shared.Workspace);
                if (baseUnitsNode is null)
                    return;

                if (Shared.Workspace.TryAssignOutputUnit(outputUnitsNode, baseUnitsNode))
                {
                    // Reset
                    Unit = string.Empty;
                    Expression = string.Empty;
                }
            }
            finally
            {
                Shared.Workspace.Restore(oldState);
                Shared.Workspace.DiagnosticMessagePosted -= AddDiagnosticMessage;
            }
        }
    }
}
