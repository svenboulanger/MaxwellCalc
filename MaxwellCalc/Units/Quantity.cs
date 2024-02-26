using System;
using System.Diagnostics.CodeAnalysis;

namespace MaxwellCalc.Units
{
    /// <summary>
    /// Creates a new <see cref="Quantity{T}"/>.
    /// </summary>
    /// <param name="scalar">The scalar.</param>
    /// <param name="unit">The unit.</param>
    public readonly struct Quantity<T>(T scalar, Unit unit) : IQuantity, IEquatable<Quantity<T>>
    {
        /// <summary>
        /// Gets the scalar.
        /// </summary>
        public T Scalar { get; } = scalar;

        /// <inheritdoc />
        object? IQuantity.Scalar => Scalar;

        /// <summary>
        /// Gets the unit.
        /// </summary>
        public Unit Unit { get; } = unit;

        /// <inheritdoc />
        public override int GetHashCode()
            => (Scalar?.GetHashCode() ?? 0) * 1021 ^ Unit.GetHashCode();

        /// <inheritdoc />
        public override bool Equals([NotNullWhen(true)] object? obj)
            => obj is T q && Equals(q);

        /// <inheritdoc />
        public bool Equals(Quantity<T> other)
        {
            if (Scalar is not null)
            {
                if (!Scalar.Equals(other.Scalar))
                    return false;
            }
            else
            {
                if (other.Scalar is not null)
                    return false;
            }
            if (!Unit.Equals(other.Unit))
                return false;
            return true;
        }

        /// <inheritdoc />
        public override string ToString()
        {
            if (Unit == Unit.UnitNone)
                return Scalar?.ToString() ?? "0";
            else
                return $"{Scalar?.ToString() ?? "0"} {Unit}";
        }

        /// <summary>
        /// Equality.
        /// </summary>
        /// <param name="left">The left argument.</param>
        /// <param name="right">The right argument.</param>
        /// <returns>Returns <c>true</c> if both argument are equal; otherwise, <c>false</c>.</returns>
        public static bool operator ==(Quantity<T> left, Quantity<T> right) => left.Equals(right);

        /// <summary>
        /// Inequality.
        /// </summary>
        /// <param name="left">The left argument.</param>
        /// <param name="right">The right argument.</param>
        /// <returns>Returns <c>true</c> if both argument are not equal; otherwise, <c>false</c>.</returns>
        public static bool operator !=(Quantity<T> left, Quantity<T> right) => !(left == right);
    }
}
