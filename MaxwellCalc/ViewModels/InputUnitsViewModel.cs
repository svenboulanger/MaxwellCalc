using Avalonia.Controls;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using MaxwellCalc.Core.Workspaces;
using MaxwellCalc.Parsers;
using MaxwellCalc.Units;
using MaxwellCalc.Workspaces;
using System;
using System.Collections.ObjectModel;
using System.Linq;

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
            => Shared.Workspace?.TryRemoveInputUnit(key);

        [RelayCommand]
        private void AddInputUnit()
        {
            if (Shared.Workspace is null || string.IsNullOrWhiteSpace(Expression) || string.IsNullOrWhiteSpace(InputUnit))
                return;

            // Deal with diagnostic messages
            Diagnostics.Clear();
            void AddDiagnosticMessage(object? sender, DiagnosticMessagePostedEventArgs args)
                => Diagnostics.Add(args.Message);
            Shared.Workspace.DiagnosticMessagePosted += AddDiagnosticMessage;

            var oldState = Shared.Workspace.Restrict(false, false, true, false, false);
            try
            {
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
                if (Shared.Workspace.TryAssignInputUnit(unit, baseUnits))
                {
                    // Reset
                    InputUnit = string.Empty;
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
