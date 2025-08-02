using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Media;
using MaxwellCalc.Units;

namespace MaxwellCalc.Controls;

public class ResultBox : TemplatedControl
{
    private CopyableQuantity? _output = null;

    public static readonly StyledProperty<string> InputProperty =
        AvaloniaProperty.Register<ResultBox, string>(nameof(Input), "Input");
    public static readonly StyledProperty<Quantity<string>> OutputProperty =
        AvaloniaProperty.Register<ResultBox, Quantity<string>>(nameof(Output), new Quantity<string>("Unrecognized", Unit.UnitNone));
    public static readonly StyledProperty<IBrush?> InputForegroundProperty =
        AvaloniaProperty.Register<ResultBox, IBrush?>(nameof(InputForeground), Brushes.Gray);
    
    public IBrush? InputForeground
    {
        get => GetValue(InputForegroundProperty);
        set => SetValue(InputForegroundProperty, value);
    }

    public string Input
    {
        get => GetValue(InputProperty);
        set => SetValue(InputProperty, value);
    }

    public Quantity<string> Output
    {
        get => GetValue(OutputProperty);
        set => SetValue(OutputProperty, value);
    }

    /// <inheritdoc />
    protected override void OnApplyTemplate(TemplateAppliedEventArgs e)
    {
        base.OnApplyTemplate(e);
        _output = e.NameScope.Find<CopyableQuantity>("OutputBlock");
        var input = e.NameScope.Find<SelectableTextBlock>("InputBlock");
        var btnCopyInput = e.NameScope.Find<Button>("CopyInputButton");
        var btnCopyOutput = e.NameScope.Find<Button>("CopyOutputButton");
        if (_output is not null)
            _output.Value = Output;

        // Attach events
        var clipboard = TopLevel.GetTopLevel(this)?.Clipboard;
        if (clipboard is not null)
        {
            if (btnCopyInput is not null && input is not null)
            {
                btnCopyInput.Click += (s, e) =>
                {
                    clipboard.SetTextAsync(input.Text);
                };
            }
            if (btnCopyOutput is not null && _output is not null)
            {
                btnCopyOutput.Click += (s, e) =>
                {
                    clipboard.SetTextAsync(_output.Text);
                };
            }
        }
        else
        {
            if (btnCopyInput is not null)
                btnCopyInput.IsVisible = false;
            if (btnCopyOutput is not null)
                btnCopyOutput.IsVisible = false;
        }
    }
}