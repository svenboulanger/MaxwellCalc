using Avalonia.Controls;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using MaxwellCalc.Core.Parsers;
using MaxwellCalc.Core.Parsers.Nodes;
using MaxwellCalc.Core.Units;
using MaxwellCalc.Core.Workspaces;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace MaxwellCalc.ViewModels;

public partial class CalculatorViewModel : ViewModelBase
{
    private readonly JsonSerializerOptions _jsonSerializerOptions;
    private int _historyFill = -1;
    private string _tmpLastInput = string.Empty;

    /// <summary>
    /// Gets the shared model.
    /// </summary>
    [property: JsonIgnore]
    public SharedModel Shared { get; }

    [ObservableProperty]
    ObservableCollection<ResultViewModel> _results = [];

    [ObservableProperty]
    [property: JsonIgnore]
    private string? _expression;

    [ObservableProperty]
    [property: JsonIgnore]
    private int _caretIndex;

    [ObservableProperty]
    [property: JsonIgnore]
    private string? _historyFile;

    /// <summary>
    /// This property makes the scroll viewer of the calculator persistent.
    /// It is updated when the calculator view is unloaded, and used again
    /// when the calculator view is loaded. It is not a real-time reflection
    /// of the current scroll offset!
    /// </summary>
    [ObservableProperty]
    private Avalonia.Vector _scrollOffset;

    [ObservableProperty]
    [property: JsonIgnore]
    private Avalonia.Size _scrollExtent;

    /// <summary>
    /// Creates a new <see cref="CalculatorViewModel"/>.
    /// </summary>
    public CalculatorViewModel()
    {
        Shared = new SharedModel();
        _jsonSerializerOptions = new();
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
        HistoryFile = Path.Combine(Directory.GetCurrentDirectory(), "history.json");
        _jsonSerializerOptions = sp.GetRequiredService<JsonSerializerOptions>();
        LoadHistory();
    }

    [RelayCommand]
    [property: JsonIgnore]
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
                if (Shared.Workspace?.Key is null)
                    return;
                var workspace = Shared.Workspace.Key;

                // Use the current expression to evaluate
                var diagnostics = new List<string>();
                void StoreDiagnostic(object? sender, DiagnosticMessagePostedEventArgs args) => diagnostics.Add(args.Message);
                workspace.DiagnosticMessagePosted += StoreDiagnostic;
                Quantity<string> result = default;

                try
                {
                    var lexer = new Lexer(Expression ?? string.Empty);
                    var node = Parser.Parse(lexer, workspace);
                    if (node is not null)
                    {
                        workspace.TryResolveAndFormat(node, Shared.Workspace.OutputFormat, System.Globalization.CultureInfo.InvariantCulture, out result);
                    }
                }
                finally
                {
                    workspace.DiagnosticMessagePosted -= StoreDiagnostic;
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
    [property: JsonIgnore]
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
    [property: JsonIgnore]
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

    [RelayCommand]
    [property: JsonIgnore]
    public void SaveHistory()
    {
        // Save the history to the file
        if (string.IsNullOrEmpty(HistoryFile))
            return;
        string json = JsonSerializer.Serialize(this, _jsonSerializerOptions);
        File.WriteAllText(HistoryFile, json);
    }

    [RelayCommand]
    [property: JsonIgnore]
    public void LoadHistory()
    {
        if (string.IsNullOrEmpty(HistoryFile) || !File.Exists(HistoryFile))
            return;
        string json = File.ReadAllText(HistoryFile);

        Results.Clear();
        var obj = JsonSerializer.Deserialize<CalculatorViewModel>(json, _jsonSerializerOptions);

        if (obj is not null)
        {
            foreach (var model in obj.Results)
                Results.Add(model);
            ScrollOffset = obj.ScrollOffset;
        }
    }
}
