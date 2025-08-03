using Avalonia.Controls;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using MaxwellCalc.Domains;
using MaxwellCalc.Parsers;
using MaxwellCalc.Units;
using MaxwellCalc.Workspaces;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.ObjectModel;

namespace MaxwellCalc.ViewModels
{
    public partial class CalculatorViewModel : ViewModelBase
    {
        [ObservableProperty]
        private IWorkspace _workspace;

        [ObservableProperty]
        ObservableCollection<ResultViewModel> _results = [];

        [ObservableProperty]
        private string? _expression;

        /// <summary>
        /// Creates a new <see cref="CalculatorViewModel"/>.
        /// </summary>
        public CalculatorViewModel()
        {
            if (Design.IsDesignMode)
            {
                for (int i = 0; i < 10; i++)
                {
                    _results.Add(new ResultViewModel()
                    {
                        Expression = "1+1",
                        Quantity = new Quantity<string>("2", Unit.UnitMeter)
                    });
                }
                _results.Add(new ResultViewModel()
                {
                    Expression = "1+c",
                    Quantity = default,
                    ErrorMessage = "This is an error message."
                });
            }

            var ws = new Workspace<double>(new DoubleDomain());
            DoubleMathHelper.RegisterCommonElectronicsConstants(ws);
            DoubleMathHelper.RegisterCommonConstants(ws);
            DoubleMathHelper.RegisterFunctions(ws);
            UnitHelper.RegisterCommonUnits(ws);
            UnitHelper.RegisterCommonElectronicsUnits(ws);
            _workspace = ws;
        }

        /// <summary>
        /// Creates a new <see cref="CalculatorViewModel"/>.
        /// </summary>
        /// <param name="sp">The service provider.</param>
        public CalculatorViewModel(IServiceProvider sp)
        {
            _workspace = sp.GetRequiredService<IWorkspace>();
        }

        [RelayCommand]
        private void Evaluate()
        {
            if (Workspace is null)
                return;

            // Use the current expression to evaluate
            var lexer = new Lexer(Expression ?? string.Empty);
            var node = Parser.Parse(lexer, Workspace);
            if (Workspace.TryResolveAndFormat(node, out var result))
            {
                Results.Add(new ResultViewModel()
                {
                    Expression = Expression,
                    Quantity = result
                });
            }
            else
            {
                Results.Add(new ResultViewModel()
                {
                    Expression = Expression,
                    ErrorMessage = Workspace.DiagnosticMessage
                });
            }
            Expression = string.Empty;
        }
    }
}
