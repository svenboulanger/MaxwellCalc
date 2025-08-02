using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Documents;
using Avalonia.Controls.Primitives;
using MaxwellCalc.Units;

namespace MaxwellCalc.Controls
{
    public class CopyableQuantity : TemplatedControl
    {
        private FormattedQuantity? _output;

        public static readonly StyledProperty<Quantity<string>> ValueProperty =
            AvaloniaProperty.Register<ResultBox, Quantity<string>>(nameof(Value), new Quantity<string>("Unrecognized", Unit.UnitNone));

        public InlineCollection? Inlines => _output?.Inlines;

        public Quantity<string> Value
        {
            get => GetValue(ValueProperty);
            set => SetValue(ValueProperty, value);
        }

        protected override void OnApplyTemplate(TemplateAppliedEventArgs e)
        {
            base.OnApplyTemplate(e);
            _output = e.NameScope.Find<FormattedQuantity>("OutputBlock");
            var btnCopyOutput = e.NameScope.Find<Button>("CopyOutputButton");
            var clipboard = TopLevel.GetTopLevel(this)?.Clipboard;
            if (_output is not null)
                _output.Value = Value;

            if (clipboard is not null && _output is not null && btnCopyOutput is not null)
                btnCopyOutput.Click += (sender, args) => clipboard.SetTextAsync(_output.Inlines?.Text);
            else if (btnCopyOutput is not null)
                btnCopyOutput.IsVisible = false; // Hide as we cannot copy to clibboard anyway
        }

        /// <inheritdoc />
        protected override void OnPropertyChanged(AvaloniaPropertyChangedEventArgs change)
        {
            base.OnPropertyChanged(change);
            if (_output is not null && change.Property.Name == nameof(Value) && change.NewValue is not null)
                _output.Value = (Quantity<string>)change.NewValue;
        }
    }
}
