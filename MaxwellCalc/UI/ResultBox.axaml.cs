using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Documents;
using Avalonia.Controls.Primitives;
using Avalonia.Media;
using MaxwellCalc.Units;
using System.Linq;

namespace MaxwellCalc;

public class ResultBox : TemplatedControl
{
    private SelectableTextBlock? _output = null;

    public static readonly StyledProperty<string> InputProperty =
        AvaloniaProperty.Register<ResultBox, string>(nameof(Input), "Input");
    public static readonly StyledProperty<object?> OutputProperty =
        AvaloniaProperty.Register<ResultBox, object?>(nameof(Output), "Output");
    public static readonly StyledProperty<IBrush?> InputForegroundProperty =
        AvaloniaProperty.Register<ResultBox, IBrush?>(nameof(InputForeground), Brushes.Gray);
    public static readonly StyledProperty<IBrush?> OutputForegroundProperty =
        AvaloniaProperty.Register<ResultBox, IBrush?>(nameof(OutputForeground), Brushes.Black);
    public static readonly StyledProperty<IBrush?> UnitForegroundProperty =
        AvaloniaProperty.Register<ResultBox, IBrush?>(nameof(UnitForeground), Brushes.Red);

    public IBrush? InputForeground
    {
        get => GetValue(InputForegroundProperty);
        set => SetValue(InputForegroundProperty, value);
    }

    public IBrush? OutputForeground
    {
        get => GetValue(OutputForegroundProperty);
        set => SetValue(OutputForegroundProperty, value);
    }

    public IBrush? UnitForeground
    {
        get => GetValue(UnitForegroundProperty);
        set => SetValue(UnitForegroundProperty, value);
    }

    public string Input
    {
        get => GetValue(InputProperty);
        set => SetValue(InputProperty, value);
    }

    public object? Output
    {
        get => GetValue(OutputProperty);
        set => SetValue(OutputProperty, value);
    }

    protected override void OnApplyTemplate(TemplateAppliedEventArgs e)
    {
        base.OnApplyTemplate(e);
        _output = e.NameScope.Find("OutputBlock") as SelectableTextBlock;
        if (_output is not null)
            Format();
    }

    protected override void OnPropertyChanged(AvaloniaPropertyChangedEventArgs change)
    {
        base.OnPropertyChanged(change);
        Format();
    }

    private void Format()
    {
        // Make sure we have the output
        if (_output is null)
            return;
        if (_output.Inlines is null)
            _output.Inlines = [];
        else
            _output.Inlines.Clear();

        // No output to show
        if (Output is null)
        {
            _output.Inlines.Add(new Run());
            return;
        }

        // First add the scalar
        if (Output is IQuantity quantity)
            FormatQuantity(quantity);
        else
        {
            var run = new Run()
            {
                Text = Output.ToString(),
                Foreground = OutputForeground
            };
            _output.Inlines.Add(run);
        }
    }

    private void FormatQuantity(IQuantity quantity)
    {
        if (quantity.Scalar is not null)
        {
            var run = new Run()
            {
                Text = quantity.Scalar.ToString(),
                Foreground = OutputForeground
            };
            _output?.Inlines?.Add(run);
        }

        // Deal with the units
        if (quantity.Unit.Name is not null)
        {
            var run = new Run()
            {
                Text = " " + quantity.Unit.Name,
                Foreground = UnitForeground
            };
            _output?.Inlines?.Add(run);
        }
        else
        {
            if (!quantity.Unit.Modifier.Equals(1.0))
            {
                // Account for weird modifiers
                var run = new Run()
                {
                    Text = " " + quantity.Unit.Modifier.ToString(),
                    Foreground = UnitForeground,
                    FontSize = 12
                };
                _output?.Inlines?.Add(run);
            }

            // We will show the dimension as is
            if (quantity.Unit.SIUnits.Dimension is not null)
            {
                foreach (var p in quantity.Unit.SIUnits.Dimension.OrderBy(p => p.Key))
                {
                    // Base
                    var run = new Run()
                    {
                        Text = " " + p.Key,
                        Foreground = UnitForeground
                    };
                    _output?.Inlines?.Add(run);

                    // Exponent
                    if (p.Value != Fraction.One)
                    {
                        run = new Run()
                        {
                            Text = p.Value.ToString(),
                            BaselineAlignment = BaselineAlignment.Top,
                            Foreground = UnitForeground,
                            FontSize = 12
                        };
                        _output?.Inlines?.Add(run);
                    }
                }
            }
        }
    }
}