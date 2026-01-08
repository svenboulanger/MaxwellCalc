using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;

namespace MaxwellCalc.Core.Domains;

/// <summary>
/// Represents a differential. This is a value that has partial derivatives to other quantities.
/// </summary>
/// <typeparam name="T">The scalar type.</typeparam>
/// <param name="value">The actual value.</param>
/// <param name="derivatives">The partial derivatives of the value to each variable.</param>
public readonly struct Differential<T>(T value, params (string, T)[] derivatives) : IEquatable<Differential<T>>, IFormattable where T : IFormattable, IEquatable<T>
{
    /// <summary>
    /// Gets the real value.
    /// </summary>
    public T Value => value;

    /// <summary>
    /// The derivatives of the differential.
    /// </summary>
    public IReadOnlyDictionary<string, T> Derivatives { get; } = derivatives is null || derivatives.Length == 0 ?
        ImmutableDictionary<string, T>.Empty :
        derivatives.ToImmutableDictionary(k => k.Item1, k => k.Item2);

    /// <inheritdoc />
    public override int GetHashCode()
    {
        int hash = 0;
        foreach (var item in Derivatives)
            hash ^= (item.Key.GetHashCode() * 1021) ^ item.Value.GetHashCode();
        return hash;
    }

    /// <inheritdoc />
    public override bool Equals([NotNullWhen(true)] object? obj)
        => obj is Differential<T> diff && Equals(diff);

    /// <inheritdoc />
    public bool Equals(Differential<T> other)
    {
        if (!Value.Equals(other.Value))
            return false;
        if (ReferenceEquals(Derivatives, other.Derivatives))
            return true;
        if (Derivatives is null || other.Derivatives is null)
            return false;
        if (Derivatives.Count != other.Derivatives.Count)
            return false;
        foreach (var pair in Derivatives)
        {
            if (!other.Derivatives.TryGetValue(pair.Key, out var otherDerivative))
                return false;
            if (!pair.Value.Equals(otherDerivative))
                return false;
        }
        return true;
    }

    /// <inheritdoc />
    public override string ToString()
    {
        var sb = new StringBuilder();
        sb.Append(Value.ToString());
        if (Derivatives is not null)
        {
            foreach (var derivative in Derivatives.OrderBy(pair => pair.Key))
            {
                sb.Append(" + ");
                sb.Append(derivative.Value.ToString());
                sb.Append("d(");
                sb.Append(derivative.Key);
                sb.Append(')');
            }
        }
        return sb.ToString();
    }

    /// <inheritdoc />
    public string ToString(string format, IFormatProvider formatProvider)
    {
        var sb = new StringBuilder();
        sb.Append(Value.ToString(format, formatProvider));
        if (Derivatives is not null)
        {
            foreach (var derivative in Derivatives.OrderBy(pair => pair.Key))
            {
                sb.Append(" + ");
                sb.Append(derivative.Value.ToString(format, formatProvider));
                sb.Append("d(");
                sb.Append(derivative.Key);
                sb.Append(')');
            }
        }
        return sb.ToString();
    }

    /// <summary>
    /// Equality between <see cref="Differential{T}"/>.
    /// </summary>
    /// <param name="left">The left argument.</param>
    /// <param name="right">The right argument.</param>
    /// <returns>Returns <c>true</c> if both differentials are equal; otherwise, <c>false</c>.</returns>
    public static bool operator ==(Differential<T> left, Differential<T> right)
        => left.Equals(right);

    /// <summary>
    /// Inequality between <see cref="Differential{T}"/>.
    /// </summary>
    /// <param name="left">The left argument.</param>
    /// <param name="right">The right argument.</param>
    /// <returns>Returns <c>true</c> if both differentials are not equal; otherwise, <c>false</c>.</returns>
    public static bool operator !=(Differential<T> left, Differential<T> right)
        => !left.Equals(right);
}
