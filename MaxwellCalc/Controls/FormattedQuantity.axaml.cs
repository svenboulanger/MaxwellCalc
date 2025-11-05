using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Documents;
using Avalonia.Controls.Primitives;
using Avalonia.Media;
using MaxwellCalc.Core.Units;
using System.Linq;

namespace MaxwellCalc.Controls
{
    /// <summary>
    /// A class that can display a <see cref="Quantity{T}"/> with type <seealso cref="string"/>.
    /// </summary>
    public class FormattedQuantity : TemplatedControl
    {
        private SelectableTextBlock? _output = null;

        /// <summary>
        /// The value.
        /// </summary>
        public static readonly StyledProperty<Quantity<string>> ValueProperty =
            AvaloniaProperty.Register<FormattedQuantity, Quantity<string>>(nameof(Value), new Quantity<string>("Unrecognized", Unit.UnitNone));

        /// <summary>
        /// The foreground color for units.
        /// </summary>
        public static readonly StyledProperty<IBrush?> UnitForegroundProperty =
            AvaloniaProperty.Register<FormattedQuantity, IBrush?>(nameof(UnitForeground), Brushes.Red);

        /// <summary>
        /// The 
        /// </summary>
        public static readonly StyledProperty<double?> UnitFontSizeProperty =
            AvaloniaProperty.Register<FormattedQuantity, double?>(nameof(UnitFontSize), 14.0);

        /// <summary>
        /// Gets or sets the value.
        /// </summary>
        public Quantity<string> Value
        {
            get => GetValue(ValueProperty);
            set => SetValue(ValueProperty, value);
        }

        /// <summary>
        /// Gets or sets the foreground color for units.
        /// </summary>
        public IBrush? UnitForeground
        {
            get => GetValue(UnitForegroundProperty);
            set => SetValue(UnitForegroundProperty, value);
        }

        /// <summary>
        /// Gets or sets the font size for units.
        /// </summary>
        public double? UnitFontSize
        {
            get => GetValue(UnitFontSizeProperty);
            set => SetValue(UnitFontSizeProperty, value);
        }

        /// <summary>
        /// Gets the text.
        /// </summary>
        public string Text => _output?.Inlines?.Text ?? string.Empty;

        /// <summary>
        /// Creates a new <see cref="FormattedQuantity"/>.
        /// </summary>
        public FormattedQuantity()
        {
        }

        /// <inheritdoc />
        protected override void OnApplyTemplate(TemplateAppliedEventArgs e)
        {
            base.OnApplyTemplate(e);

            // Make sure we can add the inlines
            _output = e.NameScope.Find<SelectableTextBlock>("OutputBlock");
        }

        /// <inheritdoc />
        protected override void OnPropertyChanged(AvaloniaPropertyChangedEventArgs change)
        {
            base.OnPropertyChanged(change);
            FormatOutput();
        }

        // Formats the output
        private void FormatOutput()
        {
            // Make sure we have the output
            if (_output is null)
                return;
            if (_output.Inlines is null)
                _output.Inlines = [];
            else
                _output.Inlines.Clear();

            // Show the value
            var scalar = new Run
            {
                Text = Value.Scalar,
                Foreground = Foreground,
                FontSize = FontSize
            };
            _output.Inlines.Add(scalar);
            double exponentFontSize = 0.75 * FontSize; // Use the font size as the baseline for our exponents later

            // Show the units
            if (Value.Unit.Dimension is null)
                return;
            foreach (var p in Value.Unit.Dimension.OrderBy(p => p.Key))
            {
                // Create a run for the base
                var run = new Run()
                {
                    Text = " " + p.Key,
                    Foreground = UnitForeground,
                };
                _output.Inlines.Add(run);

                // Create a run for the exponent
                if (p.Value != Fraction.One)
                {
                    run = new Run()
                    {
                        Text = p.Value.ToString(),
                        BaselineAlignment = BaselineAlignment.Superscript,
                        Foreground = UnitForeground,
                        FontSize = exponentFontSize,
                    };
                    _output.Inlines.Add(run);
                }
            }
        }
    }
}
