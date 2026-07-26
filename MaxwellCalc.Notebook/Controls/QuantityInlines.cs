using Avalonia.Controls.Documents;
using Avalonia.Media;
using MaxwellCalc.Core.Units;
using System.Globalization;
using System.Linq;
using System.Text.RegularExpressions;

namespace MaxwellCalc.Notebook.Controls;

/// <summary>
/// Builds the colored inline runs for a formatted <see cref="Quantity{T}"/>: the scalar in one brush
/// (scientific notation rewritten to <c>⋅10ⁿ</c> form with a superscript exponent) and each unit symbol
/// in another, exponents ≠ 1 rendered as superscripts. Shared by <see cref="QuantityView"/> (the result
/// gutter) and <see cref="InlineTextView"/> (inline results inside prose) so both lay quantities out
/// identically.
/// </summary>
internal static class QuantityInlines
{
    // Matches the exponent portion of a number in scientific notation, e.g. the "E+21" in "1.5E+21"
    // or the "e-05" in "3e-05". The mantissa in front is left as ordinary scalar text.
    private static readonly Regex ExponentPattern = new(@"[eE]([+-]?\d+)", RegexOptions.Compiled);

    /// <summary>
    /// Appends the runs for <paramref name="value"/> to <paramref name="inlines"/>.
    /// </summary>
    /// <param name="inlines">The collection to append to.</param>
    /// <param name="value">The formatted quantity.</param>
    /// <param name="scalarBrush">The brush for the scalar (and its <c>⋅10</c> base).</param>
    /// <param name="unitBrush">The brush for unit symbols and their exponents.</param>
    /// <param name="fontSize">The base font size; superscripts are drawn at <c>0.75×</c>.</param>
    public static void Emit(InlineCollection inlines, Quantity<string> value, IBrush? scalarBrush, IBrush? unitBrush, double fontSize)
    {
        double exponentFontSize = 0.75 * fontSize;

        // Scalar, in the scalar brush. Scientific notation such as "1.5E+21" is rewritten to base-10
        // form ("1.5⋅10²¹") with a superscript exponent, matching the units.
        EmitScalar(inlines, value.Scalar ?? string.Empty, scalarBrush, fontSize, exponentFontSize);

        // Units, in the unit brush, ordered for a stable layout. Exponents ≠ 1 become superscripts.
        if (value.Unit.Dimension is null)
            return;

        foreach (var dimension in value.Unit.Dimension.OrderBy(d => d.Key))
        {
            inlines.Add(new Run
            {
                Text = " " + dimension.Key,
                Foreground = unitBrush,
                FontSize = fontSize,
            });

            if (dimension.Value != Fraction.One)
            {
                inlines.Add(new Run
                {
                    Text = dimension.Value.ToString(),
                    BaselineAlignment = BaselineAlignment.Superscript,
                    Foreground = unitBrush,
                    FontSize = exponentFontSize,
                });
            }
        }
    }

    // Emits the scalar, rewriting every scientific-notation exponent into the base-10 form "⋅10"
    // followed by a superscript exponent. Handles scalars with several parts (e.g. complex values like
    // "1E+21 + 3E-05i"); the text between/around exponents is emitted verbatim.
    private static void EmitScalar(InlineCollection inlines, string scalar, IBrush? brush, double fontSize, double exponentFontSize)
    {
        int index = 0;
        foreach (Match match in ExponentPattern.Matches(scalar))
        {
            // Mantissa and any preceding text, verbatim.
            if (match.Index > index)
                AddScalarRun(inlines, scalar[index..match.Index], brush, fontSize);

            // "⋅10" base, in the scalar brush like the mantissa.
            AddScalarRun(inlines, "⋅10", brush, fontSize);

            // Exponent, normalized (drops the leading "+" and zero padding), as a superscript.
            string exponent = int.TryParse(match.Groups[1].Value, NumberStyles.Integer,
                CultureInfo.InvariantCulture, out int e)
                ? e.ToString(CultureInfo.InvariantCulture)
                : match.Groups[1].Value;
            inlines.Add(new Run
            {
                Text = exponent,
                BaselineAlignment = BaselineAlignment.Superscript,
                Foreground = brush,
                FontSize = exponentFontSize,
            });

            index = match.Index + match.Length;
        }

        // Any trailing text after the last exponent (or the whole scalar when there is none).
        if (index < scalar.Length)
            AddScalarRun(inlines, scalar[index..], brush, fontSize);
    }

    private static void AddScalarRun(InlineCollection inlines, string text, IBrush? brush, double fontSize) =>
        inlines.Add(new Run
        {
            Text = text,
            Foreground = brush,
            FontSize = fontSize,
        });
}
