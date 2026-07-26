using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Documents;
using Avalonia.Controls.Primitives;
using Avalonia.Media;
using MaxwellCalc.Notebook.Evaluation;
using System.Collections.Generic;

namespace MaxwellCalc.Notebook.Controls;

/// <summary>
/// The read-only rendered form of a notebook text line: literal prose interleaved with the evaluated
/// results of its inline <c>{…}</c> expressions. This is what a text line shows when it is not being
/// edited; clicking it (handled by the sheet view) swaps back to the raw-text editor.
/// <para>
/// Literal spans are drawn in <see cref="TemplatedControl.Foreground"/>; inline value/assignment
/// results reuse the gutter's quantity layout (<see cref="QuantityInlines"/>) with the scalar in
/// <see cref="ValueForeground"/> and units in <see cref="UnitForeground"/>; a function definition shows
/// its signature; and a failed expression shows its raw <c>{…}</c> source in <see cref="ErrorForeground"/>
/// with the diagnostic as a tooltip.
/// </para>
/// </summary>
public class InlineTextView : TemplatedControl
{
    private TextBlock? _output;

    /// <summary>Identifies the <see cref="Segments"/> property.</summary>
    public static readonly StyledProperty<IReadOnlyList<TextSegment>?> SegmentsProperty =
        AvaloniaProperty.Register<InlineTextView, IReadOnlyList<TextSegment>?>(nameof(Segments));

    /// <summary>Identifies the <see cref="ValueForeground"/> property.</summary>
    public static readonly StyledProperty<IBrush?> ValueForegroundProperty =
        AvaloniaProperty.Register<InlineTextView, IBrush?>(nameof(ValueForeground));

    /// <summary>Identifies the <see cref="UnitForeground"/> property.</summary>
    public static readonly StyledProperty<IBrush?> UnitForegroundProperty =
        AvaloniaProperty.Register<InlineTextView, IBrush?>(nameof(UnitForeground));

    /// <summary>Identifies the <see cref="ErrorForeground"/> property.</summary>
    public static readonly StyledProperty<IBrush?> ErrorForegroundProperty =
        AvaloniaProperty.Register<InlineTextView, IBrush?>(nameof(ErrorForeground));

    /// <summary>Gets or sets the rendered segments of the text line.</summary>
    public IReadOnlyList<TextSegment>? Segments
    {
        get => GetValue(SegmentsProperty);
        set => SetValue(SegmentsProperty, value);
    }

    /// <summary>Gets or sets the brush for the scalar of an inline value/assignment result (accent).</summary>
    public IBrush? ValueForeground
    {
        get => GetValue(ValueForegroundProperty);
        set => SetValue(ValueForegroundProperty, value);
    }

    /// <summary>Gets or sets the brush for unit symbols of an inline result (the unit hue).</summary>
    public IBrush? UnitForeground
    {
        get => GetValue(UnitForegroundProperty);
        set => SetValue(UnitForegroundProperty, value);
    }

    /// <summary>Gets or sets the brush for a failed inline expression (error color).</summary>
    public IBrush? ErrorForeground
    {
        get => GetValue(ErrorForegroundProperty);
        set => SetValue(ErrorForegroundProperty, value);
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
        if (change.Property == SegmentsProperty ||
            change.Property == ValueForegroundProperty ||
            change.Property == UnitForegroundProperty ||
            change.Property == ErrorForegroundProperty ||
            change.Property == ForegroundProperty ||
            change.Property == FontSizeProperty ||
            change.Property == FontFamilyProperty)
        {
            Render();
        }
    }

    // Rebuilds the inline runs from the current segments.
    private void Render()
    {
        if (_output is null)
            return;

        var inlines = _output.Inlines ??= [];
        inlines.Clear();

        if (Segments is null)
            return;

        foreach (var segment in Segments)
        {
            if (segment.Literal is { } literal)
            {
                inlines.Add(new Run { Text = literal, Foreground = Foreground, FontSize = FontSize });
                continue;
            }

            if (segment.Expression is not { } result)
                continue;

            switch (result.Kind)
            {
                case LineKind.Value:
                case LineKind.Assign:
                    // An assignment renders just its value (e.g. "{m = 5 kg}" → "5 kg"), like the gutter.
                    QuantityInlines.Emit(inlines, result.Quantity, ValueForeground, UnitForeground, FontSize);
                    break;

                case LineKind.FuncDef:
                    inlines.Add(new Run
                    {
                        Text = result.DefinedName ?? segment.RawSource,
                        Foreground = ValueForeground,
                        FontWeight = FontWeight.Medium,
                        FontSize = FontSize,
                    });
                    break;

                case LineKind.Error:
                    // Show the raw source so the user sees what failed, with the diagnostic on hover.
                    var errorText = new TextBlock
                    {
                        Text = segment.RawSource,
                        Foreground = ErrorForeground,
                        FontFamily = FontFamily,
                        FontSize = FontSize,
                    };
                    if (!string.IsNullOrEmpty(result.ErrorMessage))
                        ToolTip.SetTip(errorText, result.ErrorMessage);
                    inlines.Add(new InlineUIContainer(errorText)
                    {
                        BaselineAlignment = BaselineAlignment.TextBottom,
                    });
                    break;
            }
        }
    }
}
