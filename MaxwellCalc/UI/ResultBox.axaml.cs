using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Documents;
using Avalonia.Controls.Primitives;
using Avalonia.LogicalTree;
using Avalonia.Media;
using MaxwellCalc.Units;
using System.Linq;
using System.Numerics;

namespace MaxwellCalc.UI;

public class ResultBox : TemplatedControl
{
    private SelectableTextBlock? _output = null;

    public static readonly StyledProperty<string> InputProperty =
        AvaloniaProperty.Register<ResultBox, string>(nameof(Input), "Input");
    public static readonly StyledProperty<Quantity<string>> OutputProperty =
        AvaloniaProperty.Register<ResultBox, Quantity<string>>(nameof(Output), new Quantity<string>("Unrecognized", Unit.UnitNone));
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

    public Quantity<string> Output
    {
        get => GetValue(OutputProperty);
        set => SetValue(OutputProperty, value);
    }

    /// <inheritdoc />
    protected override void OnApplyTemplate(TemplateAppliedEventArgs e)
    {
        base.OnApplyTemplate(e);
        _output = e.NameScope.Find<SelectableTextBlock>("OutputBlock");
        var input = e.NameScope.Find<SelectableTextBlock>("InputBlock");
        var btnCopyInput = e.NameScope.Find<Button>("CopyInputButton");
        var btnCopyOutput = e.NameScope.Find<Button>("CopyOutputButton");
        FormatOutput();

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
                    clipboard.SetTextAsync(_output.Inlines?.Text);
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

    /// <inheritdoc />
    protected override void OnPropertyChanged(AvaloniaPropertyChangedEventArgs change)
    {
        base.OnPropertyChanged(change);
        FormatOutput();
    }

    private void FormatOutput()
    {
        // Make sure we have the output
        if (_output is null)
            return;
        if (_output.Inlines is null)
            _output.Inlines = [];
        else
            _output.Inlines.Clear();

        // Show the dimension
        _output.Inlines.Add(new Run
        {
            Text = Output.Scalar,
            Foreground = OutputForeground
        });
        UnitFormatter.Default.AppendInlinesFor(
            _output.Inlines, Output.Unit, UnitForeground, _output.FontSize);
    }
}