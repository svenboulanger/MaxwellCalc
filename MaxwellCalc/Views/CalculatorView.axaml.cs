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
        _scrollViewer.ScrollChanged += ScrollChanged;
        _scrollViewer.ScrollToEnd();
    }

    private void ScrollChanged(object? sender, ScrollChangedEventArgs e)
    {
        // Update the model with the new scroll settings
        if (e.ExtentDelta.Y > 0.0)
        {
            // If the window was already pretty close to the end, then let's scroll to the end
            if (_scrollViewer.Offset.Y - e.OffsetDelta.Y >= _scrollViewer.ScrollBarMaximum.Y - e.ExtentDelta.Y - 100.0)
                _scrollViewer.ScrollToEnd();
        }
    }

    private void InputTextBoxLoaded(object? sender, RoutedEventArgs e)
    {
        InputExpressionTextBox?.Focus();
    }
}