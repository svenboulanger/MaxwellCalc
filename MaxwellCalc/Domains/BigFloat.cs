using System;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Numerics;
using System.Text;

namespace MaxwellCalc.Domains
{
    /// <summary>
    /// Represents a floating point number to any precision.
    /// </summary>
    public readonly struct BigFloat : IEquatable<BigFloat>
    {
        /// <summary>
        /// Gets the mantissa.
        /// </summary>
        public BigInteger Mantissa { get; }

        /// <summary>
        /// Gets the exponent.
        /// </summary>
        public long Exponent { get; }

        /// <summary>
        /// Creates a new <see cref="BigFloat"/>.
        /// </summary>
        /// <param name="mantissa">The mantissa.</param>
        /// <param name="exponent">The exponent.</param>
        public BigFloat(BigInteger mantissa, long exponent)
        {
            if (mantissa.IsZero)
            {
                Mantissa = 0;
                Exponent = 0;
            }
            else
            {
                // Normalize the floating point
                var reduce = (long)BigInteger.TrailingZeroCount(mantissa);
                Mantissa = ShiftRight(mantissa, reduce);
                Exponent = exponent + reduce;
            }
        }

        /// <inheritdoc />
        public override int GetHashCode() => HashCode.Combine(Mantissa.GetHashCode(), Exponent.GetHashCode());

        /// <inheritdoc />
        public override bool Equals([NotNullWhen(true)] object? obj)
            => obj is BigFloat bf && Equals(bf);

        /// <inheritdoc />
        public bool Equals(BigFloat other)
        {
            if (!Mantissa.Equals(other.Mantissa))
                return false;
            if (!Exponent.Equals(other.Exponent))
                return false;
            return true;
        }

        /// <summary>
        /// Normalizes two <see cref="BigFloat"/> numbers to have the same exponent without loss of precision.
        /// </summary>
        /// <param name="a">The first argument.</param>
        /// <param name="b">The second argument.</param>
        /// <param name="ma">The normalized first argument mantissa.</param>
        /// <param name="mb">The normalized second argument mantissa.</param>
        /// <param name="exponent">The shared exponent.</param>
        private static void Normalize(BigFloat a, BigFloat b, out BigInteger ma, out BigInteger mb, out long exponent)
        {
            ma = a.Mantissa;
            mb = b.Mantissa;

            // The result should have the smallest exponent of the two
            if (a.Exponent < b.Exponent)
            {
                mb = ShiftLeft(mb, b.Exponent - a.Exponent);
                exponent = a.Exponent;
            }
            else
            {
                ma = ShiftLeft(ma, a.Exponent - b.Exponent);
                exponent = b.Exponent;
            }
        }

        /// <summary>
        /// Rotates a <see cref="BigInteger"/> left by a large amount.
        /// </summary>
        /// <param name="a">The argument.</param>
        /// <param name="rotate">The bits to shift.</param>
        /// <returns>Returns the shifted result.</returns>
        private static BigInteger ShiftLeft(BigInteger a, long rotate)
        {
            if (rotate < 0)
                return ShiftRight(a, -rotate);
            while (rotate > int.MaxValue)
            {
                a <<= int.MaxValue;
                rotate -= int.MaxValue;
            }
            a <<= (int)rotate;
            return a;
        }

        /// <summary>
        /// Rotates a <see cref="BigInteger"/> left by a large amount.
        /// </summary>
        /// <param name="a">The argument.</param>
        /// <param name="rotate">The bits to shift.</param>
        /// <returns>Returns the shifted result.</returns>
        private static BigInteger ShiftRight(BigInteger a, long rotate)
        {
            if (rotate < 0)
                return ShiftLeft(a, -rotate);
            while (rotate > int.MaxValue)
            {
                a >>= int.MaxValue;
                rotate -= int.MaxValue;
            }
            a >>= (int)rotate;
            return a;
        }

        /// <summary>
        /// Divides two <see cref="BigFloat"/>, truncating to the given precision.
        /// </summary>
        /// <param name="a">The first argument.</param>
        /// <param name="b">The second argument.</param>
        /// <param name="bitsPrecision">The number of bits to be truncated after.</param>
        /// <returns>Returns the result.</returns>
        public static BigFloat Divide(BigFloat a, BigFloat b, int bitsPrecision)
        {
            // Let's figure out what we need to achieve the given accuracy
            BigInteger numerator = a.Mantissa;
            long numExp = a.Exponent;
            BigInteger denominator = b.Mantissa;
            long denomExp = b.Exponent;
            long numBits = numerator.GetBitLength();
            long denomBits = denominator.GetBitLength();

            // The minimum number of bits needed from the numerator would be denomBits + bitsPrecision
            long neededBits = denomBits + bitsPrecision - numBits; // Add 1 to be able to round off
            numerator = ShiftLeft(numerator, neededBits);
            numExp -= neededBits;

            // Perform the division
            var (division, remainder) = BigInteger.DivRem(numerator, denominator);
            denominator >>= 1;
            if (remainder > denominator)
                division++;
            return new BigFloat(division, numExp - denomExp);
        }

        /// <summary>
        /// Exponentatiation of a big integer.
        /// </summary>
        /// <param name="base">The base.</param>
        /// <param name="exponent">The exponent.</param>
        /// <returns>Returns the integer raised to the specified power.</returns>
        private static BigInteger Pow(BigInteger @base, BigInteger exponent)
        {
            if (@base == 0)
            {
                if (exponent <= 0)
                    throw new ArgumentException(nameof(exponent));
                return BigInteger.Zero;
            }

            // Base of 1 or exponent of 0 always results in 1
            if (@base == 1 || exponent.IsZero)
                return BigInteger.One;

            // Negative exponent would lead to a fraction, let's just return 0
            if (exponent < 0)
                return BigInteger.Zero;

            BigInteger y = 1;
            BigInteger mult = @base;
            while (exponent > 1)
            {
                if (!exponent.IsEven)
                {
                    y = @base * y;
                }
                @base *= @base;
                exponent >>= 1;
            }
            return @base * y;
        }

        /// <summary>
        /// Adds two <see cref="BigFloat"/> together without loss of precision.
        /// </summary>
        /// <param name="a">The left argument</param>
        /// <param name="b">The right argument.</param>
        /// <returns>Returns the sum.</returns>
        public static BigFloat operator +(BigFloat a, BigFloat b)
        {
            Normalize(a, b, out var ma, out var mb, out var exp);
            return new BigFloat(ma + mb, exp);
        }

        /// <summary>
        /// Subtracts a <see cref="BigFloat"/> from another without loss of precision.
        /// </summary>
        /// <param name="a">The left argument.</param>
        /// <param name="b">The right argument.</param>
        /// <returns>Returns the difference.</returns>
        public static BigFloat operator -(BigFloat a, BigFloat b)
        {
            Normalize(a, b, out var ma, out var mb, out var exp);
            return new BigFloat(ma - mb, exp);
        }

        /// <summary>
        /// Multiplies two <see cref="BigFloat"/> without loss of precision.
        /// </summary>
        /// <param name="a">The left argument.</param>
        /// <param name="b">The right argument.</param>
        /// <returns>Returns the sum.</returns>
        public static BigFloat operator *(BigFloat a, BigFloat b)
        {
            return new BigFloat(a.Mantissa * b.Mantissa, a.Exponent + b.Exponent);
        }

        /// <summary>
        /// Checks whether a <see cref="BigFloat"/> is greater than another.
        /// </summary>
        /// <param name="a">The left argument.</param>
        /// <param name="b">The right argument.</param>
        /// <returns>Returns <c>true</c> if <paramref name="a"/> is greater than <paramref name="b"/>; otherwise, <c>false</c>.</returns>
        public static bool operator >(BigFloat a, BigFloat b)
        {
            long la = a.Mantissa.GetBitLength();
            long lb = b.Mantissa.GetBitLength();
            long ea = la + a.Exponent;
            long eb = lb + b.Exponent;
            if (ea > eb)
                return true;
            else if (ea < eb)
                return false;
            else if (la > lb)
                return true;
            else if (la < lb)
                return false;
            else
                return a.Mantissa > b.Mantissa;
        }

        /// <summary>
        /// Checks whether a <see cref="BigFloat"/> is greater or equal to another.
        /// </summary>
        /// <param name="a">The left argument.</param>
        /// <param name="b">The right argument.</param>
        /// <returns>Returns <c>true</c> if <paramref name="a"/> is greater than <paramref name="b"/>; otherwise, <c>false</c>.</returns>
        public static bool operator >=(BigFloat a, BigFloat b)
        {
            long la = a.Mantissa.GetBitLength();
            long lb = b.Mantissa.GetBitLength();
            long ea = la + a.Exponent;
            long eb = lb + b.Exponent;
            if (ea > eb)
                return true;
            else if (ea < eb)
                return false;
            else if (la > lb)
                return true;
            else if (la < lb)
                return false;
            else
                return a.Mantissa >= b.Mantissa;
        }

        /// <summary>
        /// Checks whether a <see cref="BigFloat"/> is smaller than another.
        /// </summary>
        /// <param name="a">The left argument.</param>
        /// <param name="b">The right argument.</param>
        /// <returns>Returns <c>true</c> if <paramref name="a"/> is smaller than <paramref name="b"/>; otherwise, <c>false</c>.</returns>
        public static bool operator <(BigFloat a, BigFloat b)
        {
            long la = a.Mantissa.GetBitLength();
            long lb = b.Mantissa.GetBitLength();
            long ea = la + a.Exponent;
            long eb = lb + b.Exponent;
            if (ea < eb)
                return true;
            else if (ea > eb)
                return false;
            else if (la < lb)
                return true;
            else if (la > lb)
                return false;
            else
                return a.Mantissa < b.Mantissa;
        }

        /// <summary>
        /// Checks whether a <see cref="BigFloat"/> is smaller or equal to another.
        /// </summary>
        /// <param name="a">The left argument.</param>
        /// <param name="b">The right argument.</param>
        /// <returns>Returns <c>true</c> if <paramref name="a"/> is smaller or equal to <paramref name="b"/>; otherwise, <c>false</c>.</returns>
        public static bool operator <=(BigFloat a, BigFloat b)
        {
            long la = a.Mantissa.GetBitLength();
            long lb = b.Mantissa.GetBitLength();
            long ea = la + a.Exponent;
            long eb = lb + b.Exponent;
            if (ea < eb)
                return true;
            else if (ea > eb)
                return false;
            else if (la < lb)
                return true;
            else if (la > lb)
                return false;
            else
                return a.Mantissa <= b.Mantissa;
        }

        /// <summary>
        /// Checks for equality between two <see cref="BigFloat"/>.
        /// </summary>
        /// <param name="left">The left argument.</param>
        /// <param name="right">The right argument.</param>
        /// <returns>Returns <c>true</c> if both arguments are equal; otherwise, <c>false</c>.</returns>
        public static bool operator ==(BigFloat left, BigFloat right) => left.Equals(right);

        /// <summary>
        /// Checks for inequality between two <see cref="BigFloat"/>.
        /// </summary>
        /// <param name="left">The left argument.</param>
        /// <param name="right">The right argument.</param>
        /// <returns>Returns <c>true</c> if both arguments are not equal; otherwise, <c>false</c>.</returns>
        public static bool operator !=(BigFloat left, BigFloat right) => !left.Equals(right);

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
            var result = new StringBuilder();

            // Trivial case
            if (mantissa.IsZero)
            {
                decimalPlace = 0;
                return "0";
            }

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
            var result = new StringBuilder();

            // Rewrite the normalized value as a fraction
            decimalPlace = FindBase10Exponent(mantissa, exponent, out var numerator, out var denominator);

            // Compared to numerator / denominator, we need to have a precision of 5*10^(-precision)
            // If decimalPlace is negative, then the numerator has been multiplied by 10^-decimalPlace
            // If decimalPlace is positive, then the denominator has been multiplied by 10^decimalPlace
            var b10 = BigInteger.Pow(10, precision - 1);
            BigInteger margin = denominator;
            numerator *= b10;
            denominator *= b10;

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
            var result = new StringBuilder();

            // Rewrite the normalized value as a fraction
            decimalPlace = FindBase10Exponent(mantissa, exponent, out var numerator, out var denominator);

            // Compared to numerator / denominator, we need to have a precision of 5*10^(-precision)
            // If decimalPlace is negative, then the numerator has been multiplied by 10^-decimalPlace
            // If decimalPlace is positive, then the denominator has been multiplied by 10^decimalPlace
            var b10 = BigInteger.Pow(10, precision);
            BigInteger margin = 5 * denominator;
            numerator *= b10;
            denominator *= b10;

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
                    return $"{sign}{digits}";
                else if (decimalPlace <= digits.Length)
                {
                    decimalPlace++;
                    return $"{sign}{digits[0..decimalPlace]}{format.NumberDecimalSeparator}{digits[decimalPlace..]}";
                }
                else
                    return $"{sign}{digits}{new string('0', decimalPlace - digits.Length)}";
            }
            else
            {
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

        /// <inheritdoc />
        public override string ToString()
        {
            var format = CultureInfo.CurrentCulture.NumberFormat;
            return FormatGeneral(this, 0, format, "E");
        }
    }
}
