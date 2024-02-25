using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;

namespace MaxwellCalc.Units
{
    /// <summary>
    /// Creates a new <see cref="BaseUnit"/>.
    /// </summary>
    /// <param name="dimension">The dimension of the base units.</param>
    public readonly struct BaseUnit(params (string, Fraction)[] dimension)
        : IEquatable<BaseUnit>
    {
        /// <summary>
        /// Gets a standard name of the base unit for meters.
        /// </summary>
        public const string Meter = "m";

        /// <summary>
        /// Gets a standard name of the base unit for seconds.
        /// </summary>
        public const string Second = "s";

        /// <summary>
        /// Gets a standard name of the base unit for amperes.
        /// </summary>
        public const string Ampere = "A";

        /// <summary>
        /// Gets a standard name of the base unit for kilograms.
        /// </summary>
        public const string Kilogram = "kg";

        /// <summary>
        /// Gets a standard name for the base unit of moles.
        /// </summary>
        public const string Mole = "mol";

        /// <summary>
        /// Gets a standard name for the base unit of candela.
        /// </summary>
        public const string Candela = "cd";

        /// <summary>
        /// Gets a standard name for the base unit or radian.
        /// </summary>
        public const string Radian = "rad";

        /// <summary>
        /// Gets a standard name for the base unit of Kelvin.
        /// </summary>
        public const string Kelvin = "K";

        /// <summary>
        /// Gets a unitless unit.
        /// </summary>
        public static BaseUnit UnitNone { get; } = new();

        /// <summary>
        /// Gets the unit for meters.
        /// </summary>
        public static BaseUnit UnitMeter { get; } = new((Meter, 1));

        /// <summary>
        /// Gets the unit for seconds.
        /// </summary>
        public static BaseUnit UnitSeconds { get; } = new((Second, 1));

        /// <summary>
        /// Gets the unit for amperes.
        /// </summary>
        public static BaseUnit UnitAmperes { get; } = new((Ampere, 1));

        /// <summary>
        /// Gets the unit for Kelvin.
        /// </summary>
        public static BaseUnit UnitKelvin { get; } = new((Kelvin, 1));

        /// <summary>
        /// Gets the unit for moles.
        /// </summary>
        public static BaseUnit UnitMole { get; } = new((Mole, 1));

        /// <summary>
        /// Gets the unit for candela.
        /// </summary>
        public static BaseUnit UnitCandela { get; } = new((Candela, 1));

        /// <summary>
        /// Gets the unit for kilogram.
        /// </summary>
        public static BaseUnit UnitKilogram { get; } = new((Kilogram, 1));

        /// <summary>
        /// Gets the unit for radians.
        /// </summary>
        /// <remarks>
        /// This is not really an SI unit, but useful nonetheless.
        /// </remarks>
        public static BaseUnit UnitRadian { get; } = new((Radian, 1));

        /// <inheritdoc />
        public IReadOnlyDictionary<string, Fraction> Dimension { get; } = dimension.ToImmutableDictionary(k => k.Item1, k => k.Item2);

        /// <inheritdoc />
        public override int GetHashCode()
        {
            int hash = 0;
            foreach (var item in Dimension)
                hash ^= (item.Key.GetHashCode() * 1021) ^ item.Value.GetHashCode();
            return hash;
        }

        /// <inheritdoc />
        public override bool Equals([NotNullWhen(true)] object? obj)
            => obj is BaseUnit u && Equals(u);

        /// <inheritdoc />
        public bool Equals(BaseUnit other)
        {
            if (Dimension is null)
                return other.Dimension is null;
            else if (other.Dimension is null)
                return false;

            if (Dimension.Count != other.Dimension.Count)
                return false;
            foreach (var pair in Dimension)
            {
                if (!other.Dimension.TryGetValue(pair.Key, out var otherFraction))
                    return false;
                if (pair.Value != otherFraction)
                    return false;
            }
            return true;
        }

        /// <inheritdoc />
        public override string ToString()
        {
            var sb = new StringBuilder();
            foreach (var unit in Dimension.OrderBy(p => p.Key))
                AppendUnit(sb, unit.Key, unit.Value);
            return sb.ToString();
        }

        private static void AppendUnit(StringBuilder sb, string unit, Fraction exponent)
        {
            if (exponent.Numerator != 0)
            {
                if (sb.Length > 0)
                    sb.Append(' ');
                sb.Append(unit);
                if (exponent != Fraction.One)
                {
                    sb.Append('^');
                    sb.Append(exponent.ToString());
                }
            }
        }

        /// <inheritdoc />
        public static BaseUnit Pow(BaseUnit unit, Fraction exponent) =>
            new(unit.Dimension?.Select(p => (p.Key, p.Value * exponent))?.ToArray() ?? Array.Empty<(string, Fraction)>());

        /// <summary>
        /// Equality between SI units.
        /// </summary>
        /// <param name="left">The left argument.</param>
        /// <param name="right">The right argument.</param>
        /// <returns>Returns <c>true</c> if both fractions are equal; otherwise, <c>false</c>.</returns>
        public static bool operator ==(BaseUnit left, BaseUnit right) => left.Equals(right);

        /// <summary>
        /// Inequality between SI units.
        /// </summary>
        /// <param name="left">The left argument.</param>
        /// <param name="right">The right argument.</param>
        /// <returns>Returns <c>true</c> if both fractions are not equal; otherwise, <c>false</c>.</returns>
        public static bool operator !=(BaseUnit left, BaseUnit right) => !(left == right);

        /// <summary>
        /// Multiplies SI units
        /// </summary>
        /// <param name="left">The left SI unit.</param>
        /// <param name="right">The right SI unit.</param>
        /// <returns>Returns the multiplied SI units.</returns>
        public static BaseUnit operator *(BaseUnit left, BaseUnit right)
        {
            var dict = new Dictionary<string, Fraction>();
            if (left.Dimension is not null)
            {
                foreach (var pair in left.Dimension)
                    dict.Add(pair.Key, pair.Value);
            }
            if (right.Dimension is not null)
            {
                foreach (var pair in right.Dimension)
                {
                    if (dict.TryGetValue(pair.Key, out var existing))
                    {
                        var newFraction = existing + pair.Value;
                        if (newFraction.Numerator == 0)
                            dict.Remove(pair.Key);
                        else
                            dict[pair.Key] = newFraction;
                    }
                    else
                        dict.Add(pair.Key, pair.Value);
                }
            }
            return new BaseUnit(dict.Select(p => (p.Key, p.Value)).ToArray());
        }

        /// <summary>
        /// Divides SI units.
        /// </summary>
        /// <param name="left">The left SI unit.</param>
        /// <param name="right">The right SI unit.</param>
        /// <returns>Returns the divided SI units.</returns>
        public static BaseUnit operator /(BaseUnit left, BaseUnit right)
        {
            var dict = new Dictionary<string, Fraction>();
            if (left.Dimension is not null)
            {
                foreach (var pair in left.Dimension)
                    dict.Add(pair.Key, pair.Value);
            }
            if (right.Dimension is not null)
            {
                foreach (var pair in right.Dimension)
                {
                    if (dict.TryGetValue(pair.Key, out var existing))
                    {
                        var newFraction = existing - pair.Value;
                        if (newFraction.Numerator == 0)
                            dict.Remove(pair.Key);
                        else
                            dict[pair.Key] = newFraction;
                    }
                    else
                        dict.Add(pair.Key, new(-pair.Value.Numerator, pair.Value.Denominator));
                }
            }
            return new BaseUnit(dict.Select(p => (p.Key, p.Value)).ToArray());
        }
    }
}
