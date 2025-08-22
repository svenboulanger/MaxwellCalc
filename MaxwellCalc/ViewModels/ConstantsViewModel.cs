using Avalonia.Controls;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using MaxwellCalc.Core.Dictionaries;
using MaxwellCalc.Core.Parsers;
using MaxwellCalc.Core.Units;
using MaxwellCalc.Core.Workspaces;
using MaxwellCalc.Core.Workspaces.Variables;
using System;
using System.Collections.ObjectModel;
using System.Numerics;

namespace MaxwellCalc.ViewModels
{
    public partial class ConstantsViewModel : FilteredCollectionViewModel<UserVariableViewModel, string, Variable<string>>
    {
        [ObservableProperty]
        private string _constantName = string.Empty;

        [ObservableProperty]
        private string _expression = string.Empty;

        [ObservableProperty]
        private string _description = string.Empty;

        [ObservableProperty]
        private ObservableCollection<string> _diagnostics = [];

        /// <summary>
        /// Creates a new <see cref="ConstantsViewModel"/>.
        /// </summary>
        public ConstantsViewModel()
        {
            if (Design.IsDesignMode)
            {
                // Make up some constants to show what it looks like
                if (Shared.Workspace?.Key is IWorkspace<double> workspace && workspace.Constants is IVariableScope<double> scope)
                {
                    scope.Local["Sven"] = new(new Quantity<double>(179, Unit.UnitMeter), "Author length.");
                    scope.Local["CheckThis"] = new(new Quantity<double>(123, Unit.UnitAmperes), null);
                }
            }
        }

        /// <summary>
        /// Creates a new <see cref="ConstantsViewModel"/>.
        /// </summary>
        /// <param name="sp">The service provider.</param>
        public ConstantsViewModel(IServiceProvider sp)
            : base(sp)
        {
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
        protected override IReadOnlyObservableDictionary<string, Variable<string>> GetCollection(IWorkspace workspace)
            => workspace.Constants.Local;

        protected override void UpdateModel(UserVariableViewModel model, string key, Variable<string> value)
        {
            model.Name = key;
            model.Value = value.Value;
            model.Description = value.Description ?? string.Empty;
        }

        /// <inheritdoc />
        protected override void RemoveItem(string key)
        {
            if (Shared.Workspace?.Key is null)
                return;
            Shared.Workspace.Key.Constants.TryRemoveVariable(key);
        }

        [RelayCommand]
        private void AddConstant()
        {
            string name = ConstantName.Trim();
            string expression = Expression.Trim();
            if (Shared.Workspace?.Key is null || string.IsNullOrEmpty(name) || string.IsNullOrEmpty(expression))
                return;

            // Deal with diagnostic messages
            Diagnostics.Clear();
            void AddDiagnosticMessage(object? sender, DiagnosticMessagePostedEventArgs args)
                => Diagnostics.Add(args.Message);
            Shared.Workspace.Key.DiagnosticMessagePosted += AddDiagnosticMessage;
            var oldState = Shared.Workspace.Key.Restrict(resolveOutputUnits: false);
            try
            {
                // Evaluate the expression
                var lexer = new Lexer(expression);
                var node = Parser.Parse(lexer, Shared.Workspace.Key);
                if (node is null)
                    return;

                if (Shared.Workspace.Key.Constants.TryAssignVariable(name, node, Description))
                {
                    ConstantName = string.Empty;
                    Expression = string.Empty;
                    Description = string.Empty;
                }
            }
            finally
            {
                Shared.Workspace.Key.DiagnosticMessagePosted -= AddDiagnosticMessage;
                Shared.Workspace.Key.Restore(oldState);
            }
        }

        [RelayCommand]
        private void AddCommonConstants()
        {
            if (Shared.Workspace?.Key is null)
                return;

            if (Shared.Workspace.Key.ScalarType == typeof(double))
                Shared.Workspace.Key.RegisterConstants(typeof(DoubleMathHelper));
            if (Shared.Workspace.Key.ScalarType == typeof(Complex))
                Shared.Workspace.Key.RegisterConstants(typeof(ComplexMathHelper));
        }
    }
}
