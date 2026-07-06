using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Documents;
using Avalonia.Controls.Primitives;
using Avalonia.Media;
using MaxwellCalc.Core.Units;
using System.Linq;

namespace MaxwellCalc.Notebook.Controls;

/// <summary>
/// Renders a <see cref="Quantity{T}"/> of <see cref="string"/> as colored inline runs: the scalar in
/// the control <see cref="TemplatedControl.Foreground"/> (the accent color), each unit symbol in
/// <see cref="UnitForeground"/> (the unit hue), and each exponent as a superscript run at
/// <c>0.75×</c> the base font size. Written fresh for the notebook gutter (Step 5); Core's formatter
/// has already produced the scalar string and the unit dimension, so this control only lays out what
/// it is given.
/// </summary>
public class QuantityView : TemplatedControl
{
    private TextBlock? _output;

    /// <summary>Identifies the <see cref="Value"/> property.</summary>
    public static readonly StyledProperty<Quantity<string>> ValueProperty =
        AvaloniaProperty.Register<QuantityView, Quantity<string>>(nameof(Value));

    /// <summary>Identifies the <see cref="UnitForeground"/> property.</summary>
    public static readonly StyledProperty<IBrush?> UnitForegroundProperty =
        AvaloniaProperty.Register<QuantityView, IBrush?>(nameof(UnitForeground));

    /// <summary>Gets or sets the quantity to render.</summary>
    public Quantity<string> Value
    {
        get => GetValue(ValueProperty);
        set => SetValue(ValueProperty, value);
    }

    /// <summary>Gets or sets the brush used for unit symbols and their exponents (the unit hue).</summary>
    public IBrush? UnitForeground
    {
        get => GetValue(UnitForegroundProperty);
        set => SetValue(UnitForegroundProperty, value);
    }

    /// <inheritdoc />
    protected override void OnApplyTemplate(TemplateAppliedEventArgs e)
    {
        base.OnApplyTemplate(e);
        _output = e.NameScope.Find<TextBlock>("PART_Text");
        Render();
    }

    /// <inheritdoc />
    protected override void OnPropertyChanged(AvaloniaPropertyChangedEventArgs change)
    {
        base.OnPropertyChanged(change);
        if (change.Property == ValueProperty ||
            change.Property == UnitForegroundProperty ||
            change.Property == ForegroundProperty ||
            change.Property == FontSizeProperty)
        {
            Render();
        }
    }

    // Rebuilds the inline runs from the current Value.
    private void Render()
    {
        if (_output is null)
            return;

        var inlines = _output.Inlines ??= [];
        inlines.Clear();

        // Scalar, in the control foreground (the accent color).
        inlines.Add(new Run
        {
            Text = Value.Scalar ?? string.Empty,
            Foreground = Foreground,
            FontSize = FontSize,
        });

        // Units, in the unit hue, ordered for a stable layout. Exponents ≠ 1 become superscripts.
        if (Value.Unit.Dimension is null)
            return;

        double exponentFontSize = 0.75 * FontSize;
        foreach (var dimension in Value.Unit.Dimension.OrderBy(d => d.Key))
        {
            inlines.Add(new Run
            {
                Text = " " + dimension.Key,
                Foreground = UnitForeground,
                FontSize = FontSize,
            });

            if (dimension.Value != Fraction.One)
            {
                inlines.Add(new Run
                {
                    Text = dimension.Value.ToString(),
                    BaselineAlignment = BaselineAlignment.Superscript,
                    Foreground = UnitForeground,
                    FontSize = exponentFontSize,
                });
            }
        }
    }
}
