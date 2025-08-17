using System;
using System.Diagnostics.CodeAnalysis;

namespace MaxwellCalc.Core.Units
{
    /// <summary>
    /// A fraction.
    /// </summary>
    public readonly struct Fraction : IEquatable<Fraction>
    {
        /// <summary>
        /// Used for converting doubles to fractions.
        /// </summary>
        private const ulong MaxD = (ulong)int.MaxValue / 2;

        /// <summary>
        /// Gets a constant for zero.
        /// </summary>
        public static Fraction Zero { get; } = new Fraction(0, 1);

        /// <summary>
        /// Gets a fraction for one.
        /// </summary>
        public static Fraction One { get; } = new Fraction(1, 1);

        /// <summary>
        /// The numerator.
        /// </summary>
        public int Numerator { get; }

        /// <summary>
        /// The denominator.
        /// </summary>
        public int Denominator { get; }

        /// <summary>
        /// Creates a <see cref="Fraction"/>.
        /// </summary>
        /// <param name="numerator">The numerator.</param>
        /// <param name="denominator">The denominator.</param>
        public Fraction(int numerator, int denominator)
        {
            if (numerator == 0)
            {
                Numerator = 0;
                Denominator = 1;
            }
            else
            {
                int gcd = Gcd(numerator, denominator);
                Numerator = numerator / gcd;
                Denominator = denominator / gcd;
            }
        }

        /// <inheritdoc />
        public override readonly int GetHashCode() => Numerator * 1021 ^ Denominator;

        /// <inheritdoc />
        public override bool Equals([NotNullWhen(true)] object? obj) => obj is Fraction f && Equals(f);

        /// <inheritdoc />
        public readonly bool Equals(Fraction other)
        {
            if (other.Numerator != Numerator)
                return false;
            if (other.Denominator != Denominator)
                return false;
            return true;
        }

        /// <inheritdoc />
        public override readonly string ToString()
        {
            if (Denominator == 1)
                return Numerator.ToString();
            else
                return $"{Numerator}/{Denominator}";
        }

        /// <summary>
        /// Gets the greatest common divisor.
        /// </summary>
        /// <param name="a">The first argument.</param>
        /// <param name="b">The second argument.</param>
        /// <returns>Returns the greatest common divisor.</returns>
        public static int Gcd(int a, int b)
        {
            if (a < 0)
                a = -a;
            if (b < 0)
                b = -b;

            while (a != 0 && b != 0)
            {
                if (a > b)
                    a %= b;
                else
                    b %= a;
            }

            return a | b;
        }

        /// <summary>
        /// Gets the greatest common divisor.
        /// </summary>
        /// <param name="a">The first argument.</param>
        /// <param name="b">The second argument.</param>
        /// <returns>Returns the greatest common divisor.</returns>
        public static long Gcd(long a, long b)
        {
            if (a < 0)
                a = -a;
            if (b < 0)
                b = -b;

            while (a != 0 && b != 0)
            {
                if (a > b)
                    a %= b;
                else
                    b %= a;
            }

            return a | b;
        }

        /// <summary>
        /// Tries to convert a double value to a fraction.
        /// </summary>
        /// <param name="value">The value.</param>
        /// <param name="result">The result.</param>
        /// <returns>Returns <c>true</c> if the conversion worked; otherwise, <c>false</c>.</returns>
        public static bool TryConvert(double value, out Fraction result)
        {
            if (value > int.MaxValue || 1.0 / value > int.MaxValue)
            {
                result = default;
                return false;
            }

            // Copied from https://bitbucket.org/heldercorreia/speedcrunch/src/master/src/math/rational.cpp
            ulong p0 = 0, q0 = 1, p1 = 1, q1 = 0;
            double val = Math.Abs(value);
            while (true)
            {
                ulong a = (ulong)Math.Floor(val);
                ulong q2 = q0 + a * q1;
                if (q2 > MaxD)
                    break;
                ulong temp1 = p0, temp2 = p1, temp3 = q1;
                p0 = temp2;
                q0 = temp3;
                p1 = temp1 + a * temp2;
                q1 = q2;
                if (val == a) break;
                val = 1 / (val - a);
            }
            if (p1 > int.MaxValue || q1 > int.MaxValue)
            {
                result = default;
                return false;
            }
            result = new Fraction(value < 0.0 ? -(int)p1 : (int)p1, (int)q1);
            return true;
        }

        /// <summary>
        /// Equality between fractions.
        /// </summary>
        /// <param name="left">The left argument.</param>
        /// <param name="right">The right argument.</param>
        /// <returns>Returns <c>true</c> if both fractions are equal; otherwise, <c>false</c>.</returns>
        public static bool operator ==(Fraction left, Fraction right) => left.Equals(right);

        /// <summary>
        /// Inquality between fractions.
        /// </summary>
        /// <param name="left">The left argument.</param>
        /// <param name="right">The right argument.</param>
        /// <returns>Returns <c>true</c> if both fractions are not equal; otherwise, <c>false</c>.</returns>
        public static bool operator !=(Fraction left, Fraction right) => !(left == right);

        /// <summary>
        /// Addition of fractions.
        /// </summary>
        /// <param name="left">The left argument.</param>
        /// <param name="right">The right argument.</param>
        /// <returns>Returns the added fractions.</returns>
        public static Fraction operator +(Fraction left, Fraction right)
            => new(left.Numerator * right.Denominator + right.Numerator * left.Denominator, left.Denominator * right.Denominator);

        /// <summary>
        /// Subtraction of fractions.
        /// </summary>
        /// <param name="left">The left argument.</param>
        /// <param name="right">The right argument.</param>
        /// <returns>Returns the subtracted fractions.</returns>
        public static Fraction operator -(Fraction left, Fraction right)
            => new(left.Numerator * right.Denominator - right.Numerator * left.Denominator, left.Denominator * right.Denominator);

        /// <summary>
        /// Multiplication of fractions.
        /// </summary>
        /// <param name="left">The left argument.</param>
        /// <param name="right">The right argument.</param>
        /// <returns>Returns the multiplied fractions.</returns>
        public static Fraction operator *(Fraction left, Fraction right)
            => new(left.Numerator * right.Numerator, left.Denominator * right.Denominator);

        /// <summary>
        /// Division of fractions.
        /// </summary>
        /// <param name="left">The left argument.</param>
        /// <param name="right">The right argument.</param>
        /// <returns>Returns the divided fractions.</returns>
        public static Fraction operator /(Fraction left, Fraction right)
            => new(left.Numerator * right.Denominator, left.Denominator * right.Numerator);

        /// <summary>
        /// Automatic conversion of a number to a fraction.
        /// </summary>
        /// <param name="number">The number.</param>
        public static implicit operator Fraction(int number)
            => new(number, 1);
    }
}
