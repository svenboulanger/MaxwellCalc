using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Documents;
using Avalonia.Controls.Primitives;
using Avalonia.Media;
using MaxwellCalc.Units;
using System.ComponentModel;

namespace MaxwellCalc.Controls
{
    public class FormattedQuantity : TemplatedControl
    {
        private SelectableTextBlock? _output = null;

        public static readonly StyledProperty<Quantity<string>> ValueProperty =
            AvaloniaProperty.Register<ResultBox, Quantity<string>>(nameof(Value), new Quantity<string>("Unrecognized", Unit.UnitNone));
        public static readonly StyledProperty<IBrush?> UnitForegroundProperty =
            AvaloniaProperty.Register<ResultBox, IBrush?>(nameof(UnitForeground), Brushes.Red);

        public Quantity<string> Value
        {
            get => GetValue(ValueProperty);
            set => SetValue(ValueProperty, value);
        }

        public InlineCollection? Inlines
        {
            get => _output?.Inlines;
        }

        public IBrush? UnitForeground
        {
            get => GetValue(UnitForegroundProperty);
            set => SetValue(UnitForegroundProperty, value);
        }

        public FormattedQuantity()
        {
        }

        protected override void OnApplyTemplate(TemplateAppliedEventArgs e)
        {
            base.OnApplyTemplate(e);
            _output = e.NameScope.Find<SelectableTextBlock>("OutputBlock");
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
                Text = Value.Scalar,
                Foreground = Foreground
            });
            UnitFormatter.Default.AppendInlinesFor(
                _output.Inlines, Value.Unit, UnitForeground, _output.FontSize);
        }
    }
}
