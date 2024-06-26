using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using MaxwellCalc.Parsers;
using MaxwellCalc.Parsers.Nodes;
using MaxwellCalc.UI;
using MaxwellCalc.Units;
using MaxwellCalc.Workspaces;
using System;
using System.Linq;

namespace MaxwellCalc.UI;

public partial class SettingsWindow : Window
{
    public static readonly StyledProperty<IWorkspace?> WorkspaceProperty =
        AvaloniaProperty.Register<SettingsWindow, IWorkspace?>(nameof(Workspace), null);

    public IWorkspace? Workspace
    {
        get => GetValue(WorkspaceProperty);
        set => SetValue(WorkspaceProperty, value);
    }

    public SettingsWindow()
    {
        InitializeComponent();
    }

    protected override void OnInitialized()
    {
        base.OnInitialized();
        UpdateInputUnits();
        UpdateOutputUnits();
    }

    protected override void OnPropertyChanged(AvaloniaPropertyChangedEventArgs change)
    {
        base.OnPropertyChanged(change);
        if (change.Property.Name == nameof(Workspace))
        {
            UpdateInputUnits();
            UpdateOutputUnits();
        }
    }

    private void UpdateInputUnits()
    {
        if (_inputUnitList is null)
            return;
        if (Workspace is null)
            return;

        _inputUnitList.Children.Clear();
        foreach (var inputUnit in Workspace.InputUnits.OrderBy(p => p.UnitName))
            AddInputUnitToList(inputUnit.UnitName, inputUnit.Value, false); // Already sorted
    }

    private void AddInputUnitToList(string name, Quantity<string> quantity, bool insertSorted = true)
    {
        if (Workspace is null)
            return;
        var item = new UnitItem
        {
            Left = new Unit((name, 1)),
            Quantity = quantity
        };
        item.RemoveClicked += (sender, args) =>
        {
            Workspace.TryRemoveInputUnit(name);
            _inputUnitList.Children.Remove(item);
        };

        if (!insertSorted)
            _inputUnitList.Children.Add(item);
        else
        {
            // Find the location where the item needs to be inserted
            int index = 0;
            while (index < _inputUnitList.Children.Count)
            {
                var sorted = _inputUnitList.Children[index] as UnitItem;
                if (sorted is not null)
                {
                    if (StringComparer.Ordinal.Compare(sorted.Left.ToString(), name) > 0)
                        break;
                }
                index++;
            }
            _inputUnitList.Children.Insert(index, item);
        }
    }

    private void UpdateOutputUnits()
    {
        if (_outputUnitList is null)
            return;
        if (Workspace is null)
            return;

        _outputUnitList.Children.Clear();
        foreach (var outputUnit in Workspace.OutputUnits.OrderBy(p => p.Unit.Dimension.FirstOrDefault().Key ?? string.Empty))
            AddOutputUnitToList(outputUnit.Unit, outputUnit.Value);
    }

    private void AddOutputUnitToList(Unit unit, Quantity<string> quantity)
    {
        if (Workspace is null)
            return;
        var item = new UnitItem
        {
            Left = unit,
            Quantity = quantity
        };
        item.RemoveClicked += (sender, args) =>
        {
            Workspace.TryRemoveOutputUnit(unit, quantity.Unit);
            _outputUnitList.Children.Remove(item);
        };
        _outputUnitList.Children.Add(item);
    }

    private void FilterInputUnits(object? sender, TextChangedEventArgs args)
    {
        if (sender is not TextBox tb)
            return;

        if (string.IsNullOrWhiteSpace(tb.Text))
        {
            // Show all
            foreach (var item in _inputUnitList.Children)
                item.IsVisible = true;
        }
        else
        {
            // Only show the items that match in some way
            string[] search = tb.Text.Split(' ', System.StringSplitOptions.RemoveEmptyEntries);
            foreach (var item in _inputUnitList.Children)
            {
                if (item is not UnitItem ui)
                    continue;

                bool hit = true;
                foreach (var s in search)
                {
                    if (!ui.Left.Dimension.Any(p => p.Key.Contains(s)))
                    {
                        hit = false;
                        break;
                    }
                }
                if (hit)
                    ui.IsVisible = true;
                else
                    ui.IsVisible = false;
            }
        }
    }

    private void FilterOutputUnits(object? sender, TextChangedEventArgs args)
    {
        if (sender is not TextBox tb)
            return;

        if (string.IsNullOrWhiteSpace(tb.Text))
        {
            // Show all
            foreach (var item in _outputUnitList.Children)
                item.IsVisible = true;
        }
        else
        {
            // Only show the items that match in some way
            string[] search = tb.Text.Split(' ', System.StringSplitOptions.RemoveEmptyEntries);
            foreach (var item in _outputUnitList.Children)
            {
                if (item is not UnitItem ui)
                    continue;

                bool hit = true;
                foreach (var s in search)
                {
                    if (!ui.Left.Dimension.Any(p => p.Key.Contains(s)))
                    {
                        hit = false;
                        break;
                    }
                }
                if (hit)
                    ui.IsVisible = true;
                else
                    ui.IsVisible = false;
            }
        }
    }

    private void Input_KeyUp(object? sender, KeyEventArgs args)
    {
        if (args.Key == Key.Enter)
            AddInputUnit(sender, new RoutedEventArgs());
    }
    private void AddInputUnit(object? sender, RoutedEventArgs args)
    {
        if (Workspace is null)
            return;
        if (_textInputUnit is null || _textInputBaseUnits is null)
            return;
        string unitName = _textInputUnit.Text ?? string.Empty;
        string baseUnits = _textInputBaseUnits.Text ?? string.Empty;

        if (string.IsNullOrWhiteSpace(unitName) || string.IsNullOrWhiteSpace(baseUnits))
        {
            var dlg = new ErrorMessageBox() { Message = "Both a unit and base units need to be specified." };
            dlg.ShowDialog(this);
            return;
        }

        // Parse both left and right arguments
        var lexer = new Lexer(unitName);
        var nodeUnitName = QuantityParser.Parse(lexer, out string? errorMessage);
        if (nodeUnitName is null)
        {
            var dlg = new ErrorMessageBox { Message = errorMessage ?? "Unknown error." };
            dlg.ShowDialog(this);
            return;
        }

        lexer = new Lexer(baseUnits);
        var nodeBaseUnits = QuantityParser.Parse(lexer, out errorMessage);
        if (nodeBaseUnits is null)
        {
            var dlg = new ErrorMessageBox { Message = errorMessage ?? "Unknown error." };
            dlg.ShowDialog(this);
            return;
        }

        // Create the input unit
        if (nodeUnitName is not UnitNode un)
        {
            var dlg = new ErrorMessageBox { Message = "The the unit name should be a word." };
            dlg.ShowDialog(this);
            return;
        }

        // Add to the workspace on to our units
        if (!Workspace.TryRegisterInputUnit(un.Content.ToString(), nodeBaseUnits))
        {
            var dlg = new ErrorMessageBox
            {
                Message = Workspace.DiagnosticMessage
            };
            dlg.ShowDialog(this);
            return;
        }
        UpdateInputUnits();

        // Restart for the next input
        _textInputBaseUnits.Text = string.Empty;
        _textInputUnit.Text = string.Empty;
        _textInputUnit.Focus();
    }

    private void Output_KeyUp(object? sender, KeyEventArgs args)
    {
        if (args.Key == Key.Enter)
            AddOutputUnit(sender, new RoutedEventArgs());
    }

    private void AddOutputUnit(object? sender, RoutedEventArgs args)
    {
        if (Workspace is null)
            return;
        if (_textOutputBaseUnits is null || _textOutputUnit is null)
            return;
        string outputUnitExpression = _textOutputUnit.Text ?? string.Empty;
        string outputBaseUnitExpression = _textOutputBaseUnits.Text ?? string.Empty;

        if (string.IsNullOrWhiteSpace(outputUnitExpression) || string.IsNullOrWhiteSpace(outputBaseUnitExpression))
        {
            var dlg = new ErrorMessageBox() { Message = "Both a unit and base units need to be specified." };
            dlg.ShowDialog(this);
            return;
        }

        // Parse both left and right arguments
        var lexer = new Lexer(outputUnitExpression);
        var nodeOutputUnitExpression = QuantityParser.Parse(lexer, out string? errorMessage);
        if (nodeOutputUnitExpression is null)
        {
            var dlg = new ErrorMessageBox { Message = errorMessage ?? "Unknown error." };
            dlg.ShowDialog(this);
            return;
        }

        lexer = new Lexer(outputBaseUnitExpression);
        var nodeOutputBaseUnitExpression = QuantityParser.Parse(lexer, out errorMessage);
        if (nodeOutputBaseUnitExpression is null)
        {
            var dlg = new ErrorMessageBox { Message = errorMessage ?? "Unknown error." };
            dlg.ShowDialog(this);
            return;
        }

        if (!Workspace.TryRegisterOutputUnit(nodeOutputUnitExpression, nodeOutputBaseUnitExpression))
        {
            var dlg = new ErrorMessageBox { Message = Workspace.DiagnosticMessage };
            dlg.ShowDialog(this);
            return;
        }
        UpdateOutputUnits();

        // Restart for the next input
        _textOutputBaseUnits.Text = string.Empty;
        _textOutputUnit.Text = string.Empty;
        _textOutputUnit.Focus();
    }
}