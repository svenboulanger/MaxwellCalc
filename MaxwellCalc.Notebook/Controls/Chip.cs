using Avalonia;
using Avalonia.Controls.Primitives;
using Avalonia.Media;

namespace MaxwellCalc.Notebook.Controls;

/// <summary>
/// A small access-bar chip: a leading mono glyph (in a caller-chosen color), a sans label, and an
/// optional trailing count. Matches REDESIGN_UI.md → Component: Chip. Clicking is handled by the
/// host (the chip opens the overlay in later steps); this control is presentation only.
/// </summary>
public class Chip : TemplatedControl
{
    /// <summary>Identifies the <see cref="Glyph"/> property.</summary>
    public static readonly StyledProperty<string?> GlyphProperty =
        AvaloniaProperty.Register<Chip, string?>(nameof(Glyph));

    /// <summary>Identifies the <see cref="GlyphForeground"/> property.</summary>
    public static readonly StyledProperty<IBrush?> GlyphForegroundProperty =
        AvaloniaProperty.Register<Chip, IBrush?>(nameof(GlyphForeground));

    /// <summary>Identifies the <see cref="GlyphFontStyle"/> property.</summary>
    public static readonly StyledProperty<FontStyle> GlyphFontStyleProperty =
        AvaloniaProperty.Register<Chip, FontStyle>(nameof(GlyphFontStyle), FontStyle.Normal);

    /// <summary>Identifies the <see cref="Label"/> property.</summary>
    public static readonly StyledProperty<string?> LabelProperty =
        AvaloniaProperty.Register<Chip, string?>(nameof(Label));

    /// <summary>Identifies the <see cref="Count"/> property.</summary>
    public static readonly StyledProperty<string?> CountProperty =
        AvaloniaProperty.Register<Chip, string?>(nameof(Count));

    /// <summary>Gets or sets the leading mono glyph (e.g. <c>x</c>, <c>m</c>, <c>ƒ</c>).</summary>
    public string? Glyph
    {
        get => GetValue(GlyphProperty);
        set => SetValue(GlyphProperty, value);
    }

    /// <summary>Gets or sets the brush for the leading glyph (accent for x/ƒ, unit hue for m).</summary>
    public IBrush? GlyphForeground
    {
        get => GetValue(GlyphForegroundProperty);
        set => SetValue(GlyphForegroundProperty, value);
    }

    /// <summary>Gets or sets the font style of the leading glyph (x and ƒ render italic).</summary>
    public FontStyle GlyphFontStyle
    {
        get => GetValue(GlyphFontStyleProperty);
        set => SetValue(GlyphFontStyleProperty, value);
    }

    /// <summary>Gets or sets the chip label text.</summary>
    public string? Label
    {
        get => GetValue(LabelProperty);
        set => SetValue(LabelProperty, value);
    }

    /// <summary>Gets or sets the optional trailing count (empty string hides it).</summary>
    public string? Count
    {
        get => GetValue(CountProperty);
        set => SetValue(CountProperty, value);
    }
}
