using System;
using System.Diagnostics.CodeAnalysis;
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
            // This assumes both are normalized
            if (a.Exponent > b.Exponent)
                return true;
            else if (a.Exponent < b.Exponent)
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
            // This assumes both are normalized
            if (a.Exponent > b.Exponent)
                return true;
            else if (a.Exponent < b.Exponent)
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
            // This assumes both are normalized
            if (a.Exponent < b.Exponent)
                return true;
            else if (a.Exponent > b.Exponent)
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
            // This assumes both are normalized
            if (a.Exponent < b.Exponent)
                return true;
            else if (a.Exponent > b.Exponent)
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
        /// <param name="value">The big float.</param>
        /// <param name="decimalPlace">The exponent in base 10 of the leading decimal digit of the result.</param>
        /// <returns>Returns the formatted result.</returns>
        public static string FormatFull(BigFloat value, out int decimalPlace)
        {
            var result = new StringBuilder();

            // Trivial case
            if (value.Mantissa.IsZero)
            {
                decimalPlace = 0;
                return "0";
            }

            // Minus sign
            var mantissa = value.Mantissa;
            if (mantissa.Sign < 0)
            {
                result.Append('-');
                mantissa = -mantissa;
            }

            decimalPlace = FindBase10Exponent(mantissa, value.Exponent, out var numerator, out var denominator);
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
        /// <param name="value">The value.</param>
        /// <param name="precision">The number of digits after the decimal.</param>
        /// <param name="decimalPlace">The exponent in base 10 of the leading decimal digit of the result.</param>
        /// <returns>Returns the formatted result.</returns>
        // public static string FormatFixedPrecision(BigFloat value, int precision, out int decimalPlace)
        // {
        //     
        // }

        /// <summary>
        /// Formats a <see cref="BigFloat"/> to a string with relative precision to the leading decimal.
        /// </summary>
        /// <param name="value">The value.</param>
        /// <param name="precision">The number of digits after the leading digit.</param>
        /// <param name="decimalPlace">The exponent in base 10 of the leading decimal digit of the result.</param>
        /// <returns>Returns the formatted result.</returns>
        public static string FormatRelativePrecision(BigFloat value, int precision, out int decimalPlace)
        {
            var result = new StringBuilder();

            // Trivial case
            if (value.Mantissa.IsZero)
            {
                decimalPlace = 0;
                return "0";
            }

            // Minus sign
            var mantissa = value.Mantissa;
            if (mantissa.Sign < 0)
            {
                result.Append('-');
                mantissa = -mantissa;
            }

            // Rewrite the normalized value as a fraction
            decimalPlace = FindBase10Exponent(mantissa, value.Exponent, out var numerator, out var denominator);

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

        // public override string ToString()
        //    => FormatFloat(this, -1);
    }
}
