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
        /// Formats a <see cref="BigFloat"/> to a string according to the "F" format.
        /// </summary>
        /// <param name="value">The big float.</param>
        /// <param name="precision">The number of digits. If negative, the value is rounded to the precision of the <see cref="BigFloat"/> least significant bit.</param>
        /// <returns>Returns the formatted float.</returns>
        public static string FormatFloat(BigFloat value, int precision, out int decimalPlace)
        {
            var result = new StringBuilder();

            // Trivial case
            if (value.Mantissa.IsZero)
            {
                decimalPlace = 0;
                return "0";
            }

            // Minus sign
            BigInteger mantissa = value.Mantissa;
            if (mantissa.Sign < 0)
            {
                result.Append('-');
                mantissa = -mantissa;
            }    

            // Apply Steele-White
            BigInteger numerator, denominator, numerator_minus, numerator_plus;
            if (value.Exponent > 0)
            {
                numerator = ShiftLeft(mantissa, value.Exponent);
                denominator = BigInteger.One;
                numerator_minus = 1;
                numerator_plus = 1;
            }
            else
            {
                numerator = mantissa;
                denominator = ShiftLeft(BigInteger.One, -value.Exponent);
                numerator_minus = 1;
                numerator_plus = 1;
            }

            // Find the position of the leading digit
            int insertDecimalAt = 1;
            decimalPlace = (int)Math.Floor(BigInteger.Log10(mantissa) + Math.Log10(2) * value.Exponent + 0.1);
            if (decimalPlace < 0)
                denominator *= BigInteger.Pow(10, -decimalPlace - 1);

            int cDigit = 0;
            while (numerator > 0)
            {
                var (digit, remainder) = BigInteger.DivRem(numerator, denominator);
                

                // The quotient is the digit
                if (digit >= 10)
                {
                    // The
                }
                result.Append((char)((char)digit + '0'));

                numerator = remainder * 10;
                numerator_minus *= 10;
                numerator_plus = numerator_minus;
                cDigit++;
            }

            return result.ToString();
        }

        // public override string ToString()
        //    => FormatFloat(this, -1);
    }
}
