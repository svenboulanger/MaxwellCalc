using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Documents;
using Avalonia.Controls.Primitives;
using Avalonia.Media;
using MaxwellCalc.Core.Units;
using System.Linq;

namespace MaxwellCalc.Notebook.Controls;

/// <summary>
/// Renders a bare <see cref="Unit"/> as colored inline runs: each unit symbol in the control
/// <see cref="TemplatedControl.Foreground"/> and each exponent ≠ 1 as a superscript run at <c>0.75×</c>
/// the base font size. A unit-only sibling of <see cref="QuantityView"/> (which pairs a scalar with a
/// unit), used where just a unit is shown — e.g. the output-unit command palette's left-hand label.
/// </summary>
public class UnitView : TemplatedControl
{
    private TextBlock? _output;

    /// <summary>Identifies the <see cref="Value"/> property.</summary>
    public static readonly StyledProperty<Unit> ValueProperty =
        AvaloniaProperty.Register<UnitView, Unit>(nameof(Value));

    /// <summary>Gets or sets the unit to render.</summary>
    public Unit Value
    {
        get => GetValue(ValueProperty);
        set => SetValue(ValueProperty, value);
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
            change.Property == ForegroundProperty ||
            change.Property == FontSizeProperty)
        {
            Render();
        }
    }

    // Rebuilds the inline runs from the current Value: each symbol at the base font size, each exponent
    // ≠ 1 as a superscript, symbols separated by a single space (none before the first).
    private void Render()
    {
        if (_output is null)
            return;

        var inlines = _output.Inlines ??= [];
        inlines.Clear();

        if (Value.Dimension is null)
            return;

        double exponentFontSize = 0.75 * FontSize;
        bool first = true;
        foreach (var dimension in Value.Dimension.OrderBy(d => d.Key))
        {
            inlines.Add(new Run
            {
                Text = first ? dimension.Key : " " + dimension.Key,
                Foreground = Foreground,
                FontSize = FontSize,
            });
            first = false;

            if (dimension.Value != Fraction.One)
            {
                inlines.Add(new Run
                {
                    Text = dimension.Value.ToString(),
                    BaselineAlignment = BaselineAlignment.Superscript,
                    Foreground = Foreground,
                    FontSize = exponentFontSize,
                });
            }
        }
    }
}
