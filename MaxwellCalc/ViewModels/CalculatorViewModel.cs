using Avalonia.Controls;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using MaxwellCalc.Parsers;
using MaxwellCalc.Units;
using MaxwellCalc.Workspaces;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace MaxwellCalc.ViewModels
{
    public partial class CalculatorViewModel : ViewModelBase
    {
        private int _historyFill = -1;
        private string _tmpLastInput = string.Empty;

        /// <summary>
        /// Gets the shared model.
        /// </summary>
        public SharedModel Shared { get; }

        [ObservableProperty]
        ObservableCollection<ResultViewModel> _results = [];

        [ObservableProperty]
        private string? _expression;

        [ObservableProperty]
        private int _caretIndex;

        [ObservableProperty]
        private Avalonia.Vector _scrollOffset;

        [ObservableProperty]
        private Avalonia.Size _scrollExtent;

        /// <summary>
        /// Creates a new <see cref="CalculatorViewModel"/>.
        /// </summary>
        public CalculatorViewModel()
        {
            Shared = new SharedModel();
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
        }

        /// <summary>
        /// Creates a new <see cref="CalculatorViewModel"/>.
        /// </summary>
        /// <param name="sp">The service provider.</param>
        public CalculatorViewModel(IServiceProvider sp)
        {
            Shared = sp.GetRequiredService<SharedModel>();
        }

        [RelayCommand]
        private void Evaluate()
        {
            // Deal with some commands
            switch (Expression)
            {
                case "":
                    // Don't do anything
                    break;

                case "cls":
                case "clc":
                case "clear":
                    // Clear all results
                    Results.Clear();
                    break;

                default:
                    if (Shared.Workspace is null)
                        return;

                    // Use the current expression to evaluate
                    var diagnostics = new List<string>();
                    void StoreDiagnostic(object? sender, DiagnosticMessagePostedEventArgs args) => diagnostics.Add(args.Message);
                    Shared.Workspace.DiagnosticMessagePosted += StoreDiagnostic;
                    Quantity<string> result = default;

                    try
                    {
                        var lexer = new Lexer(Expression ?? string.Empty);
                        var node = Parser.Parse(lexer, Shared.Workspace);
                        if (node is not null)
                            Shared.Workspace.TryResolveAndFormat(node, Shared.OutputFormat, System.Globalization.CultureInfo.InvariantCulture, out result);
                    }
                    finally
                    {
                        Shared.Workspace.DiagnosticMessagePosted -= StoreDiagnostic;
                    }

                    Results.Add(new ResultViewModel
                    {
                        Expression = Expression,
                        Quantity = result,
                        ErrorMessage = diagnostics.Count > 0 ? string.Join(Environment.NewLine, diagnostics) : null
                    });
                    break;
            }

            // Reset
            _historyFill = Results.Count;
            Expression = string.Empty;
            CaretIndex = 0;
        }

        [RelayCommand]
        private void TrackHistoryUp()
        {
            // Store the current input for the future
            if (_historyFill == Results.Count)
                _tmpLastInput = Expression ?? string.Empty;

            // Move to the last history
            if (_historyFill > -1)
            {
                _historyFill--;
                FillHistory();
            }
        }

        [RelayCommand]
        private void TrackHistoryDown()
        {
            if (_historyFill < Results.Count)
            {
                _historyFill++;
                FillHistory();
            }
        }

        private void FillHistory()
        {
            if (_historyFill < 0)
            {
                // We reached the start of our history
                if (_historyFill < -1)
                    _historyFill = -1;
                Expression = string.Empty;
                CaretIndex = 0;
                return;
            }

            if (_historyFill >= Results.Count)
            {
                Expression = _tmpLastInput;
                CaretIndex = _tmpLastInput.Length;
                return;
            }

            // Use the history
            Expression = Results[_historyFill].Expression ?? string.Empty;
            CaretIndex = Expression.Length - 1; // I don't know why, but removing this line makes the caret appear on index 0 instead of the end
            CaretIndex = Expression.Length;
        }
    }
}
