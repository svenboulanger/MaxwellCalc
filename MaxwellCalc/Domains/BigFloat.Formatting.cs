using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace MaxwellCalc.Domains
{
    public readonly partial struct BigFloat
    {
        /// <summary>
        /// Finds the exponent of the leading decimal digit without any rounding error.
        /// This implementation is rather slow, but for a calculator we prefer a robust method.
        /// </summary>
        /// <param name="mantissa">The mantissa, which should be positive.</param>
        /// <param name="exponent">The exponent.</param>
        /// <param name="numerator">The resulting numerator for the normalized value.</param>
        /// <param name="denominator">The resulting denominator for the normalized value.</param>
        /// <returns>Returns the base 10 exponent of the leading decimal digit.</returns>
        private static int FindBase10Exponent(BigInteger mantissa, long exponent,
            out BigInteger numerator, out BigInteger denominator)
        {
            // As an initial guess, we can still use a floating point arithmetic
            int decimalPlace = (int)Math.Floor(BigInteger.Log10(mantissa) + 0.30102999566398119521 * exponent);
            numerator = mantissa;
            denominator = BigInteger.One;
            if (exponent > 0)
                numerator = ShiftLeft(numerator, exponent);
            else if (exponent < 0)
                denominator = ShiftLeft(denominator, -exponent);

            // Check for validity of the number of decimal places
            if (decimalPlace > 0)
                denominator *= BigInteger.Pow(10, decimalPlace);
            else if (decimalPlace < 0)
                numerator *= BigInteger.Pow(10, -decimalPlace);

            // Final check to make sure that there were no weird rounding errors from the log10 computation
            if (numerator < denominator)
            {
                denominator /= 10;
                decimalPlace--;
            }
            else if (numerator >= denominator * 10)
            {
                denominator *= 10;
                decimalPlace++;
            }
            return decimalPlace;
        }

        /// <summary>
        /// Formats a <see cref="BigFloat"/> to a string to infinite precision.
        /// </summary>
        /// <param name="mantissa">The mantissa (should be positive).</param>
        /// <param name="exponent">The exponent in base 2.</param>
        /// <param name="decimalPlace">The exponent in base 10 of the leading decimal digit of the result.</param>
        /// <returns>Returns the formatted result.</returns>
        private static string FormatFull(BigInteger mantissa, long exponent, out int decimalPlace)
        {
            // Trivial case
            if (mantissa.IsZero)
            {
                decimalPlace = 0;
                return "0";
            }

            var result = new StringBuilder();
            decimalPlace = FindBase10Exponent(mantissa, exponent, out var numerator, out var denominator);
            while (numerator > 0)
            {
                var (digit, remainder) = BigInteger.DivRem(numerator, denominator);
                result.Append((char)((char)digit + '0'));
                numerator = remainder * 10;
            }
            return result.ToString();
        }

        /// <summary>
        /// Formats a <see cref="BigFloat"/> to a string with fixed precision after the comma.
        /// </summary>
        /// <param name="mantissa">The mantissa (should be positive).</param>
        /// <param name="exponent">The exponent in base 2.</param>
        /// <param name="precision">The number of digits after the decimal.</param>
        /// <param name="decimalPlace">The exponent in base 10 of the leading decimal digit of the result.</param>
        /// <returns>Returns the formatted result.</returns>
        private static string FormatFixedPrecision(BigInteger mantissa, long exponent, int precision, out int decimalPlace)
        {
            // Trivial case
            if (mantissa.IsZero)
            {
                decimalPlace = 0;
                return "0";
            }

            // Rewrite the normalized value as a fraction
            decimalPlace = FindBase10Exponent(mantissa, exponent, out var numerator, out var denominator);

            // Compared to numerator / denominator, we need to have a precision of 5*10^(-precision)
            // If decimalPlace is negative, then the numerator has been multiplied by 10^-decimalPlace
            // If decimalPlace is positive, then the denominator has been multiplied by 10^decimalPlace
            var b10 = BigInteger.Pow(10, precision - 1);
            BigInteger margin = denominator;
            numerator *= b10;
            denominator *= b10;

            var result = new StringBuilder();
            BigInteger digit = 0, remainder = 0;
            bool canRoundLow = false, canRoundHigh = false, isFirst = true;
            while (numerator > 0)
            {
                (digit, remainder) = BigInteger.DivRem(numerator, denominator);
                canRoundLow = remainder < margin;
                canRoundHigh = remainder + margin > denominator;
                if (canRoundHigh || canRoundLow)
                    break;

                result.Append((char)((char)digit + '0'));
                isFirst = false;
                numerator = remainder * 10;
                margin *= 10;
            }

            if (canRoundHigh && canRoundLow)
            {
                // We need to take a look at the remainder and see which way we need to round
                remainder <<= 1;
                if (remainder >= denominator)
                {
                    if (isFirst && digit == 9)
                    {
                        // This case can happen if the first digit is a '9' and we need to round up
                        result.Append('1');
                        decimalPlace++;
                    }
                    else
                        result.Append((char)((char)digit + '1'));
                }
                else
                    result.Append((char)((char)digit + '0'));
            }
            else if (canRoundHigh)
            {
                if (isFirst && digit == 9)
                {
                    // This case can happen if the first digit is a '9' and we need to round up
                    result.Append('1');
                    decimalPlace++;
                }
                else
                    result.Append((char)((char)digit + '1'));
            }
            else if (canRoundLow)
                result.Append((char)((char)digit + '0'));

            return result.ToString();
        }

        /// <summary>
        /// Formats a <see cref="BigFloat"/> to a string with relative precision to the leading decimal.
        /// </summary>
        /// <param name="mantissa">The mantissa (should be positive).</param>
        /// <param name="exponent">The exponent in base 2.</param>
        /// <param name="precision">The number of digits after the leading digit.</param>
        /// <param name="decimalPlace">The exponent in base 10 of the leading decimal digit of the result.</param>
        /// <returns>Returns the formatted result.</returns>
        private static string FormatRelativePrecision(BigInteger mantissa, long exponent, int precision, out int decimalPlace)
        {

            // Trivial case
            if (mantissa.IsZero)
            {
                decimalPlace = 0;
                return "0";
            }

            // Rewrite the normalized value as a fraction
            decimalPlace = FindBase10Exponent(mantissa, exponent, out var numerator, out var denominator);

            // Compared to numerator / denominator, we need to have a precision of 5*10^(-precision)
            // If decimalPlace is negative, then the numerator has been multiplied by 10^-decimalPlace
            // If decimalPlace is positive, then the denominator has been multiplied by 10^decimalPlace
            var b10 = BigInteger.Pow(10, precision);
            BigInteger margin = 5 * denominator;
            numerator *= b10;
            denominator *= b10;

            var result = new StringBuilder();
            BigInteger digit = 0, remainder = 0;
            bool canRoundLow = false, canRoundHigh = false, isFirst = true;
            while (numerator > 0)
            {
                (digit, remainder) = BigInteger.DivRem(numerator, denominator);
                canRoundLow = remainder <= margin;
                canRoundHigh = remainder + margin >= denominator;
                if (canRoundHigh || canRoundLow)
                    break;

                result.Append((char)((char)digit + '0'));
                isFirst = false;
                numerator = remainder * 10;
                margin *= 10;
            }

            if (canRoundHigh && canRoundLow)
            {
                // We need to take a look at the remainder and see which way we need to round
                remainder <<= 1;
                if (remainder >= denominator)
                {
                    if (isFirst && digit == 9)
                    {
                        // This case can happen if the first digit is a '9' and we need to round up
                        result.Append('1');
                        decimalPlace++;
                    }
                    else
                        result.Append((char)((char)digit + '1'));
                }
                else
                    result.Append((char)((char)digit + '0'));
            }
            else if (canRoundHigh)
            {
                if (isFirst && digit == 9)
                {
                    // This case can happen if the first digit is a '9' and we need to round up
                    result.Append('1');
                    decimalPlace++;
                }
                else
                    result.Append((char)((char)digit + '1'));
            }
            else if (canRoundLow)
                result.Append((char)((char)digit + '0'));

            return result.ToString();
        }

        /// <summary>
        /// Helper method for formatting scientific notation.
        /// </summary>
        /// <returns>Returns the formatted string.</returns>
        private static string FormatHelperScientific(string sign, string digits, int decimalPlace, string expFormat, NumberFormatInfo format, string exponent)
        {
            if (digits.Length == 1)
            {
                if (decimalPlace >= 0)
                    return $"{sign}{digits}{exponent}{format.PositiveSign}{decimalPlace.ToString(expFormat)}";
                else
                    return $"{sign}{digits}{exponent}{decimalPlace.ToString(expFormat)}";
            }
            else
            {
                if (decimalPlace >= 0)
                    return $"{sign}{digits[0]}{format.NumberDecimalSeparator}{digits[1..]}{exponent}{format.PositiveSign}{decimalPlace.ToString(expFormat)}";
                else
                    return $"{sign}{digits[0]}{format.NumberDecimalSeparator}{digits[1..]}{exponent}{decimalPlace.ToString(expFormat, format)}";
            }
        }

        /// <summary>
        /// Helper method for formatting fixed notation.
        /// </summary>
        /// <returns>Returns the formatted string.</returns>
        private static string FormatHelperFixedPoint(string sign, string digits, int decimalPlace, NumberFormatInfo format)
        {
            if (decimalPlace == 0)
            {
                if (digits.Length > 1)
                    return $"{sign}{digits[0]}{format.NumberDecimalSeparator}{digits[1..]}";
                else
                    return $"{sign}{digits}";
            }
            else if (decimalPlace > 0)
            {
                if (decimalPlace + 1 == digits.Length)
                    return $"{sign}{digits}"; // No trailing zeros necessary
                else if (decimalPlace < digits.Length)
                {
                    // There are more digits after the decimal point
                    decimalPlace++;
                    return $"{sign}{digits[0..decimalPlace]}{format.NumberDecimalSeparator}{digits[decimalPlace..]}";
                }
                else
                    // There are trailing zeroes
                    return $"{sign}{digits}{new string('0', decimalPlace - digits.Length + 1)}";
            }
            else
            {
                // The leading digit is already after the decimal point
                return $"{sign}0.{new string('0', -decimalPlace - 1)}{digits}";
            }
        }

        /// <summary>
        /// Formats a <see cref="BigFloat"/> according to "G" rules.
        /// </summary>
        /// <param name="value">The value to be represented as a string.</param>
        /// <param name="precision">The precision. If 0 or smaller, the full version is returned.</param>
        /// <param name="format">The format.</param>
        /// <param name="exponent">The exponent base string.</param>
        /// <returns>Returns the formatted value.</returns>
        public static string FormatGeneral(BigFloat value, int precision, NumberFormatInfo format, string exponent)
        {
            // Trivial case of 0
            if (value.Mantissa.Sign == 0)
                return "0";

            // Deal with the sign of the value
            string sign;
            BigInteger mantissa;
            if (value.Mantissa.Sign < 0)
            {
                sign = format.NegativeSign;
                mantissa = -value.Mantissa;
            }
            else
            {
                sign = "";
                mantissa = value.Mantissa;
            }

            // Format the value, we don't know yet how we're going to format it though...
            string digits; // The shortest form of the significant digits
            int decimalPlace; // The base-10 exponent of the leading digit
            if (precision <= 0)
                digits = FormatFull(mantissa, value.Exponent, out decimalPlace);
            else
                digits = FormatRelativePrecision(mantissa, value.Exponent, precision, out decimalPlace);

            // Our rules are loosely based on https://learn.microsoft.com/en-us/dotnet/standard/base-types/standard-numeric-format-strings#GFormatString
            if (decimalPlace <= -5)
            {
                // Always use scientific notation
                return FormatHelperScientific(sign, digits, decimalPlace, "d2", format, exponent);
            }
            else if (decimalPlace <= 0)
                return FormatHelperFixedPoint(sign, digits, decimalPlace, format);
            else if (precision == 0)
            {
                if (digits.Length > decimalPlace)
                {
                    // 123.456 - use fixed-point notation, always shortest
                    return FormatHelperFixedPoint(sign, digits, decimalPlace, format);
                }
                else if (digits.Length < decimalPlace - 3)
                {
                    // 1230000... - Use exponential notation to avoid many zeroes
                    return FormatHelperScientific(sign, digits, decimalPlace, "d2", format, exponent);
                }
                else
                {
                    // 12300 - Add trailing zeroes
                    return FormatHelperFixedPoint(sign, digits, decimalPlace, format);
                }
            }
            else if (decimalPlace < precision)
                return FormatHelperFixedPoint(sign, digits, decimalPlace, format);
            else
                return FormatHelperScientific(sign, digits, decimalPlace, "d2", format, exponent);
        }

    }
}
