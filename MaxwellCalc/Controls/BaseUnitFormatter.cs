using Avalonia.Controls.Documents;
using Avalonia.Media;
using MaxwellCalc.Core.Units;
using System.Linq;

namespace MaxwellCalc.Controls
{
    public class UnitFormatter
    {
        /// <summary>
        /// Gets a default base unit formatter for UI.
        /// </summary>
        public static UnitFormatter Default { get; } = new UnitFormatter();

        /// <summary>
        /// Creates a new <see cref="UnitFormatter"/>.
        /// </summary>
        private UnitFormatter()
        {
        }

        public void AppendInlinesFor(InlineCollection inlines, Unit unit,
            IBrush? foregroundBrush = null,
            double fontSize = 16)
        {
            if (unit.Dimension is null)
                return;
            if (inlines is null)
                return;

            foreach (var p in unit.Dimension.OrderBy(p => p.Key))
            {
                // Create a run for the base
                var run = new Run()
                {
                    Text = " " + p.Key,
                    Foreground = foregroundBrush,
                    FontSize = fontSize
                };
                inlines.Add(run);

                // Create a run for the exponent
                if (p.Value != Fraction.One)
                {
                    run = new Run()
                    {
                        Text = p.Value.ToString(),
                        BaselineAlignment = BaselineAlignment.Top,
                        Foreground = foregroundBrush,
                        FontSize = fontSize * 0.75,
                    };
                    inlines.Add(run);
                }
            }
        }
    }
}
