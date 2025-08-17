using Avalonia.Controls;

namespace MaxwellCalc.Views;

public partial class CalculatorView : UserControl
{
    public CalculatorView()
    {
        InitializeComponent();
    }

    private void InputTextBoxLoaded(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
    {
        InputExpressionTextBox?.Focus();
    }
}