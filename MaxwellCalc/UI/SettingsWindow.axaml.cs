using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Markup.Xaml;
using MaxwellCalc.Units;
using MaxwellCalc.Workspaces;
using System.Linq;

namespace MaxwellCalc;

public partial class SettingsWindow : Window
{
    public static readonly StyledProperty<Workspace?> WorkspaceProperty =
        AvaloniaProperty.Register<SettingsWindow, Workspace?>(nameof(Workspace), null);

    public Workspace? Workspace
    {
        get => GetValue(WorkspaceProperty);
        set => SetValue(WorkspaceProperty, value);
    }

    public SettingsWindow()
    {
        InitializeComponent();
    }

    protected override void OnApplyTemplate(TemplateAppliedEventArgs e)
    {
        base.OnApplyTemplate(e);
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
        foreach (var (name, quantity) in ((IWorkspace<double>)Workspace).InputUnits)
        {
            string n = name;
            var item = new UnitItem
            {
                Left = new Unit((name, 1)),
                Quantity = quantity
            };
            item.RemoveClicked += (sender, args) =>
            {
                ((IWorkspace<double>)Workspace).TryRemoveInputUnit(n);
                _inputUnitList.Children.Remove(item);
            };
            _inputUnitList.Children.Add(item);
        }
    }

    private void UpdateOutputUnits()
    {
        if (_outputUnitList is null)
            return;
        if (Workspace is null)
            return;

        _outputUnitList.Children.Clear();
        foreach (var (unit, quantity) in ((IWorkspace<double>)Workspace).OutputUnits)
        {
            var item = new UnitItem
            {
                Left = quantity.Unit,
                Quantity = new Quantity<double>(1.0 / quantity.Scalar, unit)
            };
            item.RemoveClicked += (sender, args) =>
            {
                ((IWorkspace<double>)Workspace).TryRemoveOutputUnit(unit, quantity);
                _outputUnitList.Children.Remove(item);
            };
            _outputUnitList.Children.Add(item);
        }
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
}