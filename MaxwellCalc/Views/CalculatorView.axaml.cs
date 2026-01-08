using Avalonia.Controls;
using Avalonia.Interactivity;
using MaxwellCalc.ViewModels;

namespace MaxwellCalc.Views;

public partial class CalculatorView : UserControl
{
    public CalculatorView()
    {
        InitializeComponent();
    }

    /// <inheritdoc />
    protected override void OnLoaded(RoutedEventArgs e)
    {
        base.OnLoaded(e);
        if (DataContext is CalculatorViewModel model)
            _scrollViewer.Offset = model.ScrollOffset;
    }

    /// <inheritdoc />
    protected override void OnUnloaded(RoutedEventArgs e)
    {
        if (DataContext is CalculatorViewModel model)
            model.ScrollOffset = _scrollViewer.Offset;
        base.OnUnloaded(e);
    }

    private void InputTextBoxLoaded(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
    {
        InputExpressionTextBox?.Focus();
    }
}