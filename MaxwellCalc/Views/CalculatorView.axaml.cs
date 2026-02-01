using Avalonia.Controls;
using Avalonia.Interactivity;
using MaxwellCalc.ViewModels;

namespace MaxwellCalc.Views;

public partial class CalculatorView : UserControl
{
    /// <summary>
    /// Gets whether the calculator view is auto-scrolling to the end.
    /// </summary>
    public bool Autoscrolling { get; private set; } = true;

    public CalculatorView()
    {
        InitializeComponent();
    }

    /// <inheritdoc />
    protected override void OnLoaded(RoutedEventArgs e)
    {
        base.OnLoaded(e);
        if (DataContext is CalculatorViewModel model)
        {
            _scrollViewer.Offset = model.ScrollOffset;
            _scrollViewer.ScrollChanged += (sender, args) =>
            {
                model.ScrollOffset = _scrollViewer.Offset;

                // Detect whether we are supposed to be at the end
                Autoscrolling = _scrollViewer.Offset.Y >= _scrollViewer.Extent.Height - _scrollViewer.Viewport.Height - 50.0;
            };
        }
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