using Avalonia;
using Avalonia.Controls.Primitives;
using Avalonia.Input;
using Avalonia.Media;
using System.Windows.Input;

namespace MaxwellCalc.Notebook.Controls;

/// <summary>
/// A small access-bar chip: a leading mono glyph (in a caller-chosen color), a sans label, and an
/// optional trailing count. Matches REDESIGN_UI.md → Component: Chip. Clicking runs <see cref="Command"/>
/// (with <see cref="CommandParameter"/>) — the access bar uses this to open the command palette on the
/// matching section (Step 8).
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

    /// <summary>Identifies the <see cref="Command"/> property.</summary>
    public static readonly StyledProperty<ICommand?> CommandProperty =
        AvaloniaProperty.Register<Chip, ICommand?>(nameof(Command));

    /// <summary>Identifies the <see cref="CommandParameter"/> property.</summary>
    public static readonly StyledProperty<object?> CommandParameterProperty =
        AvaloniaProperty.Register<Chip, object?>(nameof(CommandParameter));

    /// <summary>Gets or sets the leading mono glyph (e.g. <c>x</c>, <c>m</c>, <c>f</c>).</summary>
    public string? Glyph
    {
        get => GetValue(GlyphProperty);
        set => SetValue(GlyphProperty, value);
    }

    /// <summary>Gets or sets the brush for the leading glyph (accent for x/f, unit hue for m).</summary>
    public IBrush? GlyphForeground
    {
        get => GetValue(GlyphForegroundProperty);
        set => SetValue(GlyphForegroundProperty, value);
    }

    /// <summary>Gets or sets the font style of the leading glyph (x and f render italic).</summary>
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

    /// <summary>Gets or sets the command run when the chip is clicked.</summary>
    public ICommand? Command
    {
        get => GetValue(CommandProperty);
        set => SetValue(CommandProperty, value);
    }

    /// <summary>Gets or sets the parameter passed to <see cref="Command"/>.</summary>
    public object? CommandParameter
    {
        get => GetValue(CommandParameterProperty);
        set => SetValue(CommandParameterProperty, value);
    }

    /// <inheritdoc />
    protected override void OnPointerReleased(PointerReleasedEventArgs e)
    {
        base.OnPointerReleased(e);

        // Only fire for the primary button released while still over the chip (a genuine click).
        if (e.InitialPressMouseButton != MouseButton.Left)
            return;
        if (!new Rect(Bounds.Size).Contains(e.GetPosition(this)))
            return;

        var command = Command;
        var parameter = CommandParameter;
        if (command?.CanExecute(parameter) == true)
            command.Execute(parameter);
    }
}
