using System;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Numerics;

namespace MaxwellCalc.Domains
{
    /// <summary>
    /// Represents a floating point number to any precision.
    /// </summary>
    public readonly partial struct BigFloat : IEquatable<BigFloat>
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
            if (!Exponent.Equals(other.Exponent))
                return false;
            if (!Mantissa.Equals(other.Mantissa))
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
            long neededBits = denomBits + bitsPrecision - numBits;
            numExp -= neededBits;
            numerator = ShiftLeft(numerator, neededBits + 1); // Add one more bit for rounding off later

            // Perform the division
            var division = numerator / denominator;
            long divBits = division.GetBitLength();
            if (divBits > bitsPrecision + 1)
            {
                // This can happen for pure powers of 2
                // E.g., an equivalent in base 10 would be 100/10 = 10 (2 digits), but 100/11 = 9 (1 digit)
                division >>= 1;
                numExp++;
            }

            // Shave of one more digit for rounding
            if (division.IsEven)
                division >>= 1;
            else if (division.Sign < 0)
                division = (division >> 1) - 1;
            else
                division = (division >> 1) + 1;
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
        /// Truncates a float, rounding the LSB if necessary.
        /// </summary>
        /// <param name="bitsPrecision">The number of bits of precision.</param>
        /// <returns>Returns the truncated float.</returns>
        public BigFloat Truncate(int bitsPrecision)
        {
            // Check the length of the mantissa
            var l = Mantissa.GetBitLength();
            if (l <= bitsPrecision)
                return this;
            long excess = l - bitsPrecision;

            var mantissa = ShiftRight(Mantissa, excess - 1);
            if (mantissa.IsEven)
                mantissa >>= 1;
            else if (mantissa.Sign < 0)
                mantissa = (mantissa >> 1) - 1;
            else
                mantissa = (mantissa >> 1) + 1;
            return new BigFloat(mantissa, Exponent + excess);
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
        /// Implicitly converts an <see cref="int"/> to a <see cref="BigFloat"/>.
        /// </summary>
        /// <param name="a">The argument.</param>
        public static implicit operator BigFloat(int a) => new BigFloat(a, 0);

        /// <summary>
        /// Implicitly converts a <see cref="long"/> to a <see cref="BigFloat"/>.
        /// </summary>
        /// <param name="a">The argument.</param>
        public static implicit operator BigFloat(long a) => new BigFloat(a, 0);

        /// <summary>
        /// Explicitly converts a <see cref="BigFloat"/> to a double.
        /// </summary>
        /// <param name="f">The argument.</param>
        public static explicit operator double(BigFloat f) => (double)f.Mantissa * Math.Pow(2, f.Exponent);

        /// <inheritdoc />
        public override string ToString()
        {
            var format = CultureInfo.CurrentCulture.NumberFormat;
            return FormatGeneral(this, 0, format, "E");
        }
    }
}
