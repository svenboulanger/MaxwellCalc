using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Documents;
using Avalonia.Controls.Primitives;
using Avalonia.Input;
using Avalonia.Input.Platform;
using Avalonia.Media;
using Avalonia.Threading;
using MaxwellCalc.Core.Units;
using System;

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
    // How long the "Copied!" tooltip stays up after a click, before reverting to the hint.
    private static readonly TimeSpan CopiedFeedbackDuration = TimeSpan.FromSeconds(1.1);

    private TextBlock? _output;

    // Pending revert of the "Copied!" tooltip back to the hint; disposed/replaced on each copy so
    // rapid clicks don't leave the feedback stuck or revert early.
    private IDisposable? _feedbackReset;

    /// <summary>Identifies the <see cref="Value"/> property.</summary>
    public static readonly StyledProperty<Quantity<string>> ValueProperty =
        AvaloniaProperty.Register<QuantityView, Quantity<string>>(nameof(Value));

    /// <summary>Identifies the <see cref="UnitForeground"/> property.</summary>
    public static readonly StyledProperty<IBrush?> UnitForegroundProperty =
        AvaloniaProperty.Register<QuantityView, IBrush?>(nameof(UnitForeground));

    /// <summary>Identifies the <see cref="CopyOnClick"/> property.</summary>
    public static readonly StyledProperty<bool> CopyOnClickProperty =
        AvaloniaProperty.Register<QuantityView, bool>(nameof(CopyOnClick));

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

    /// <summary>
    /// Gets or sets whether clicking the quantity copies it to the clipboard in its original text
    /// form — <c>e</c>-exponent scalar and <c>^</c>-notation units, e.g. <c>1.5e21 m^2 s^-1</c>.
    /// When on, the control shows a hand cursor and a "Click to copy" hint. Off by default so the
    /// read-only uses (command palette, settings) are unaffected.
    /// </summary>
    public bool CopyOnClick
    {
        get => GetValue(CopyOnClickProperty);
        set => SetValue(CopyOnClickProperty, value);
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
        else if (change.Property == CopyOnClickProperty)
        {
            // A hand cursor and a hint make the affordance discoverable; clear both when disabled.
            Cursor = CopyOnClick ? new Cursor(StandardCursorType.Hand) : Cursor.Default;
            ToolTip.SetTip(this, CopyOnClick ? "Click to copy" : null);
        }
    }

    /// <inheritdoc />
    protected override void OnPointerReleased(PointerReleasedEventArgs e)
    {
        base.OnPointerReleased(e);

        if (!CopyOnClick)
            return;

        // Only fire for the primary button released while still over the control (a genuine click).
        if (e.InitialPressMouseButton != MouseButton.Left)
            return;
        if (!new Rect(Bounds.Size).Contains(e.GetPosition(this)))
            return;

        var clipboard = TopLevel.GetTopLevel(this)?.Clipboard;
        if (clipboard is null)
            return;

        _ = clipboard.SetTextAsync(BuildPlainText());
        ShowCopiedFeedback();
        e.Handled = true;
    }

    // Builds the original text form of the quantity: the scalar exactly as Core formatted it (already
    // in e-exponent notation) followed by the unit in ^-notation (Unit.ToString(), space-separated),
    // e.g. "1.5e21 m^2 s^-1". Unitless quantities are just the scalar.
    private string BuildPlainText()
    {
        string scalar = Value.Scalar ?? string.Empty;
        string unit = Value.Unit.ToString();
        return unit.Length > 0 ? $"{scalar} {unit}" : scalar;
    }

    // Flashes the tooltip to "Copied!" for a moment, then reverts to the hint. Replacing any pending
    // revert keeps rapid clicks from reverting early or leaving the confirmation stuck open.
    private void ShowCopiedFeedback()
    {
        _feedbackReset?.Dispose();

        ToolTip.SetTip(this, "Copied!");
        ToolTip.SetIsOpen(this, true);

        _feedbackReset = DispatcherTimer.RunOnce(() =>
        {
            ToolTip.SetIsOpen(this, false);
            ToolTip.SetTip(this, CopyOnClick ? "Click to copy" : null);
        }, CopiedFeedbackDuration);
    }

    // Rebuilds the inline runs from the current Value: scalar in the control foreground (the accent
    // color), unit symbols in the unit hue. The shared builder is also used by the inline text view.
    private void Render()
    {
        if (_output is null)
            return;

        var inlines = _output.Inlines ??= [];
        inlines.Clear();
        QuantityInlines.Emit(inlines, Value, Foreground, UnitForeground, FontSize);
    }
}
