﻿using System;
using System.Diagnostics.CodeAnalysis;

namespace MaxwellCalc.Units
{
    /// <summary>
    /// Creates a new <see cref="Unit"/>.
    /// </summary>
    /// <param name="modifier">The modifier.</param>
    /// <param name="units">The SI units.</param>
    /// <param name="name">The name of units.</param>
    public readonly struct Unit(double modifier, BaseUnit units, string? name)
        : IEquatable<Unit>
    {
        /// <summary>
        /// A unit for scalars.
        /// </summary>
        public static Unit Scalar { get; } = new Unit(1.0, BaseUnit.UnitNone, null);

        /// <summary>
        /// Gets the unit modifier.
        /// </summary>
        public double Modifier { get; } = modifier;

        /// <summary>
        /// Gets the SI units.
        /// </summary>
        public BaseUnit SIUnits { get; } = units;

        /// <summary>
        /// Gets the name of the unit.
        /// </summary>
        public string? Name { get; } = name;

        /// <summary>
        /// Gets the hash code.
        /// </summary>
        /// <returns></returns>
        public override int GetHashCode()
        {
            int hash = Modifier.GetHashCode();
            hash = hash * 1021 ^ SIUnits.GetHashCode();
            hash = hash * 1021 ^ (Name?.GetHashCode() ?? 0);
            return hash;
        }

        /// <inheritdoc />
        public override bool Equals([NotNullWhen(true)] object? obj)
            => obj is Unit u && Equals(u);

        /// <inheritdoc />
        public bool Equals(Unit other)
        {
            if (!Modifier.Equals(other.Modifier))
                return false;
            if (SIUnits != other.SIUnits)
                return false;
            if (!StringComparer.Ordinal.Equals(Name, other.Name))
                return false;
            return true;
        }

        /// <summary>
        /// Computes the power of the unit.
        /// </summary>
        /// <param name="unit">The unit.</param>
        /// <param name="exponent">The exponent.</param>
        /// <returns>Returns the unit raised to the given power.</returns>
        public static Unit Pow(Unit unit, Fraction exponent)
        {
            double modifier = unit.Modifier;
            if (!modifier.Equals(1.0))
                modifier = Math.Pow(modifier, (double)exponent.Numerator / exponent.Denominator);
            return new Unit(modifier, BaseUnit.Pow(unit.SIUnits, exponent), null);
        }

        /// <summary>
        /// Equality between units.
        /// </summary>
        /// <param name="left">The left argument.</param>
        /// <param name="right">The right argument.</param>
        /// <returns>Returns <c>true</c> if both units are equal; otherwise, <c>false</c>.</returns>
        public static bool operator ==(Unit left, Unit right) => left.Equals(right);

        /// <summary>
        /// Inequality between units.
        /// </summary>
        /// <param name="left">The left argument.</param>
        /// <param name="right">The right argument.</param>
        /// <returns>Returns <c>true</c> if both units are not equal; otherwise, <c>false</c>.</returns>
        public static bool operator !=(Unit left, Unit right) => !(left == right);
    }
}