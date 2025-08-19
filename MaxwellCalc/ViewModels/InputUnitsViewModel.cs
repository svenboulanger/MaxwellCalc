using Avalonia.Controls;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using MaxwellCalc.Core.Dictionaries;
using MaxwellCalc.Core.Parsers;
using MaxwellCalc.Core.Units;
using MaxwellCalc.Core.Workspaces;
using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Numerics;

namespace MaxwellCalc.ViewModels
{
    public partial class InputUnitsViewModel : FilteredCollectionViewModel<InputUnitViewModel, string, Quantity<string>>
    {
        [ObservableProperty]
        private string? _inputUnit;

        [ObservableProperty]
        private string? _expression;

        [ObservableProperty]
        private ObservableCollection<string> _diagnostics = [];

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
        protected override IReadOnlyObservableDictionary<string, Quantity<string>> GetCollection(IWorkspace workspace)
            => workspace.InputUnits;

        /// <inheritdoc />
        protected override void UpdateModel(InputUnitViewModel model, string key, Quantity<string> value)
        {
            model.Name = key;
            model.Value = value;
        }

        /// <inheritdoc />
        protected override void RemoveItem(string key)
            => Shared.Workspace.Key?.TryRemoveInputUnit(key);

        [RelayCommand]
        private void AddInputUnit()
        {
            if (Shared.Workspace.Key is null || string.IsNullOrWhiteSpace(Expression) || string.IsNullOrWhiteSpace(InputUnit))
                return;

            // Deal with diagnostic messages
            Diagnostics.Clear();
            void AddDiagnosticMessage(object? sender, DiagnosticMessagePostedEventArgs args)
                => Diagnostics.Add(args.Message);
            Shared.Workspace.Key.DiagnosticMessagePosted += AddDiagnosticMessage;

            var oldState = Shared.Workspace.Key.Restrict(false, false, true, false, false);
            try
            {
                // Try to evaluate the expression
                string expression = Expression.Trim();
                var lexer = new Lexer(Expression);
                var baseUnits = Parser.Parse(lexer, Shared.Workspace.Key);
                if (baseUnits is null)
                    return;
                if (!Shared.Workspace.Key.TryResolveAndFormat(baseUnits, "g", System.Globalization.CultureInfo.InvariantCulture, out var result))
                    return;

                // Evaluate the name
                string unit = InputUnit.Trim();
                if (string.IsNullOrEmpty(unit)) // Should be non-empty
                    return;
                if (!unit.All(char.IsLetter)) // Require all letters
                    return;

                // Pass them on to the workspace
                if (Shared.Workspace.Key.TryAssignInputUnit(unit, baseUnits))
                {
                    // Reset
                    InputUnit = string.Empty;
                    Expression = string.Empty;
                }
            }
            finally
            {
                Shared.Workspace.Key.Restore(oldState);
                Shared.Workspace.Key.DiagnosticMessagePosted -= AddDiagnosticMessage;
            }
        }

        [RelayCommand]
        private void AddCommonUnits()
        {
            if (Shared.Workspace.Key is null)
                return;
            switch (Shared.Workspace.Key)
            {
                case IWorkspace<double> dblWorkspace:
                    dblWorkspace.RegisterCommonUnits();
                    break;

                case IWorkspace<Complex> cplxWorkspace:
                    cplxWorkspace.RegisterCommonUnits();
                    break;
            }
        }

        [RelayCommand]
        private void AddCommonElectronicsUnits()
        {
            if (Shared.Workspace.Key is null)
                return;
            switch (Shared.Workspace.Key)
            {
                case IWorkspace<double> dblWorkspace:
                    UnitHelper.RegisterCommonElectronicsUnits(dblWorkspace);
                    break;

                case IWorkspace<Complex> cplxWorkspace:
                    UnitHelper.RegisterCommonElectronicsUnits(cplxWorkspace);
                    break;
            }
        }
    }
}
