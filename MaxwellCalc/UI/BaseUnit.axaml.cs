using Avalonia;
using Avalonia.Controls.Primitives;
using Avalonia.Media;

namespace MaxwellCalc;

public class BaseUnit : TemplatedControl
{
    public static readonly StyledProperty<string?> UnitProperty =
        AvaloniaProperty.Register<BaseUnit, string?>(nameof(Unit), "Unit");
    public static readonly StyledProperty<IBrush?> InputForegroundProperty =
        AvaloniaProperty.Register<BaseUnit, IBrush?>(nameof(InputForeground), Brushes.Gray);
    public static readonly StyledProperty<IBrush?> OutputForegroundProperty =
        AvaloniaProperty.Register<BaseUnit, IBrush?>(nameof(OutputForeground), Brushes.Black);
    public static readonly StyledProperty<IBrush?> UnitForegroundProperty =
        AvaloniaProperty.Register<BaseUnit, IBrush?>(nameof(UnitForeground), Brushes.Red);

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

    public string? Unit
    {
        get => GetValue(UnitProperty);
        set => SetValue(UnitProperty, value);
    }
}