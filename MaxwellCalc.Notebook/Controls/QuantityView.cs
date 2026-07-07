using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Documents;
using Avalonia.Controls.Primitives;
using Avalonia.Media;
using MaxwellCalc.Core.Units;
using System.Globalization;
using System.Linq;
using System.Text.RegularExpressions;

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
    // Matches the exponent portion of a number in scientific notation, e.g. the "E+21" in "1.5E+21"
    // or the "e-05" in "3e-05". The mantissa in front is left as ordinary scalar text.
    private static readonly Regex ExponentPattern = new(@"[eE]([+-]?\d+)", RegexOptions.Compiled);

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

        double exponentFontSize = 0.75 * FontSize;

        // Scalar, in the control foreground (the accent color). Scientific notation such as "1.5E+21"
        // is rewritten to base-10 form ("1.5⋅10²¹") with a superscript exponent, matching the units.
        EmitScalar(inlines, Value.Scalar ?? string.Empty, exponentFontSize);

        // Units, in the unit hue, ordered for a stable layout. Exponents ≠ 1 become superscripts.
        if (Value.Unit.Dimension is null)
            return;

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

    // Emits the scalar as accent-colored runs, rewriting every scientific-notation exponent into the
    // base-10 form "⋅10" followed by a superscript exponent. Handles scalars with several parts (e.g.
    // complex values like "1E+21 + 3E-05i"); the text between/around exponents is emitted verbatim.
    private void EmitScalar(InlineCollection inlines, string scalar, double exponentFontSize)
    {
        int index = 0;
        foreach (Match match in ExponentPattern.Matches(scalar))
        {
            // Mantissa and any preceding text, verbatim.
            if (match.Index > index)
                AddScalarRun(inlines, scalar[index..match.Index]);

            // "⋅10" base, in the accent color like the mantissa.
            AddScalarRun(inlines, "⋅10");

            // Exponent, normalized (drops the leading "+" and zero padding), as a superscript.
            string exponent = int.TryParse(match.Groups[1].Value, NumberStyles.Integer,
                CultureInfo.InvariantCulture, out int e)
                ? e.ToString(CultureInfo.InvariantCulture)
                : match.Groups[1].Value;
            inlines.Add(new Run
            {
                Text = exponent,
                BaselineAlignment = BaselineAlignment.Superscript,
                Foreground = Foreground,
                FontSize = exponentFontSize,
            });

            index = match.Index + match.Length;
        }

        // Any trailing text after the last exponent (or the whole scalar when there is none).
        if (index < scalar.Length)
            AddScalarRun(inlines, scalar[index..]);
    }

    // Adds a single scalar run in the control foreground (the accent color) at the base font size.
    private void AddScalarRun(InlineCollection inlines, string text) =>
        inlines.Add(new Run
        {
            Text = text,
            Foreground = Foreground,
            FontSize = FontSize,
        });
}
