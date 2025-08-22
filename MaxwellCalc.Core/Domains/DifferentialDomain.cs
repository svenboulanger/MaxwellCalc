using MaxwellCalc.Core.Dictionaries;
using MaxwellCalc.Core.Parsers.Nodes;
using MaxwellCalc.Core.Units;
using MaxwellCalc.Core.Workspaces;
using MaxwellCalc.Core.Workspaces.Variables;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;

namespace MaxwellCalc.Core.Domains
{
    
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

    public class DifferentialDomain<T> : IDomain<Differential<T>> where T : IFormattable, IEquatable<T>
    {
        /// <summary>
        /// Gets the domain for <typeparamref name="T"/> that should be used.
        /// </summary>
        public IDomain<T> Domain { get; }

        /// <inheritdoc />
        public Quantity<Differential<T>> Default { get; }

        /// <inheritdoc />
        public JsonConverter<Differential<T>> Converter => throw new NotImplementedException();

        /// <summary>
        /// Creates a new <see cref="DifferentialDomain{T}"/>.
        /// </summary>
        /// <param name="domain">The scalar domain.</param>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="domain"/> is <c>null</c>.</exception>
        public DifferentialDomain(IDomain<T> domain)
        {
            Domain = domain ?? throw new ArgumentNullException(nameof(domain));
            Default = new Quantity<Differential<T>>(
                new Differential<T>(Domain.Default.Scalar), Unit.UnitNone);
        }

        /// <inheritdoc />
        public bool TryScalar(string scalar, IWorkspace<Differential<T>>? workspace, out Quantity<Differential<T>> result)
        {
            throw new NotImplementedException();
        }

        public bool TryUnit(string unit, IWorkspace<Differential<T>>? workspace, out Quantity<Differential<T>> result)
        {
            throw new NotImplementedException();
        }

        public bool TryVariable(string variable, IWorkspace<Differential<T>>? workspace, out Quantity<Differential<T>> result)
        {
            throw new NotImplementedException();
        }

        public bool TryPlus(Quantity<Differential<T>> a, IWorkspace<Differential<T>>? workspace, out Quantity<Differential<T>> result)
        {
            throw new NotImplementedException();
        }

        public bool TryMinus(Quantity<Differential<T>> a, IWorkspace<Differential<T>>? workspace, out Quantity<Differential<T>> result)
        {
            throw new NotImplementedException();
        }

        public bool TryRemoveUnits(Quantity<Differential<T>> a, IWorkspace<Differential<T>>? workspace, out Quantity<Differential<T>> result)
        {
            throw new NotImplementedException();
        }

        public bool TryFactorial(Quantity<Differential<T>> a, IWorkspace<Differential<T>>? workspace, out Quantity<Differential<T>> result)
        {
            throw new NotImplementedException();
        }

        public bool TryAdd(Quantity<Differential<T>> a, Quantity<Differential<T>> b, IWorkspace<Differential<T>>? workspace, out Quantity<Differential<T>> result)
        {
            throw new NotImplementedException();
        }

        public bool TrySubtract(Quantity<Differential<T>> a, Quantity<Differential<T>> b, IWorkspace<Differential<T>>? workspace, out Quantity<Differential<T>> result)
        {
            throw new NotImplementedException();
        }

        public bool TryMultiply(Quantity<Differential<T>> a, Quantity<Differential<T>> b, IWorkspace<Differential<T>>? workspace, out Quantity<Differential<T>> result)
        {
            throw new NotImplementedException();
        }

        public bool TryDivide(Quantity<Differential<T>> a, Quantity<Differential<T>> b, IWorkspace<Differential<T>>? workspace, out Quantity<Differential<T>> result)
        {
            throw new NotImplementedException();
        }

        public bool TryInvert(Quantity<Differential<T>> a, IWorkspace<Differential<T>>? workspace, out Quantity<Differential<T>> result)
        {
            throw new NotImplementedException();
        }

        public bool TryModulo(Quantity<Differential<T>> a, Quantity<Differential<T>> b, IWorkspace<Differential<T>>? workspace, out Quantity<Differential<T>> result)
        {
            throw new NotImplementedException();
        }

        public bool TryIntDivide(Quantity<Differential<T>> a, Quantity<Differential<T>> b, IWorkspace<Differential<T>>? workspace, out Quantity<Differential<T>> result)
        {
            throw new NotImplementedException();
        }

        public bool TryExp(Quantity<Differential<T>> a, Quantity<Differential<T>> b, IWorkspace<Differential<T>>? workspace, out Quantity<Differential<T>> result)
        {
            throw new NotImplementedException();
        }

        public bool TryInUnit(Quantity<Differential<T>> a, Quantity<Differential<T>> b, ReadOnlyMemory<char> unit, IWorkspace<Differential<T>>? workspace, out Quantity<Differential<T>> result)
        {
            throw new NotImplementedException();
        }

        public bool TryBitwiseOr(Quantity<Differential<T>> a, Quantity<Differential<T>> b, IWorkspace<Differential<T>>? workspace, out Quantity<Differential<T>> result)
        {
            throw new NotImplementedException();
        }

        public bool TryLogicalOr(Quantity<Differential<T>> a, Quantity<Differential<T>> b, IWorkspace<Differential<T>>? workspace, out Quantity<Differential<T>> result)
        {
            throw new NotImplementedException();
        }

        public bool TryBitwiseAnd(Quantity<Differential<T>> a, Quantity<Differential<T>> b, IWorkspace<Differential<T>>? workspace, out Quantity<Differential<T>> result)
        {
            throw new NotImplementedException();
        }

        public bool TryLogicalAnd(Quantity<Differential<T>> a, Quantity<Differential<T>> b, IWorkspace<Differential<T>>? workspace, out Quantity<Differential<T>> result)
        {
            throw new NotImplementedException();
        }

        public bool TryLeftShift(Quantity<Differential<T>> a, Quantity<Differential<T>> b, IWorkspace<Differential<T>>? workspace, out Quantity<Differential<T>> result)
        {
            throw new NotImplementedException();
        }

        public bool TryRightShift(Quantity<Differential<T>> a, Quantity<Differential<T>> b, IWorkspace<Differential<T>>? workspace, out Quantity<Differential<T>> result)
        {
            throw new NotImplementedException();
        }

        public bool TryGreaterThan(Quantity<Differential<T>> a, Quantity<Differential<T>> b, IWorkspace<Differential<T>>? workspace, out Quantity<Differential<T>> result)
        {
            throw new NotImplementedException();
        }

        public bool TryGreaterThanOrEqual(Quantity<Differential<T>> a, Quantity<Differential<T>> b, IWorkspace<Differential<T>>? workspace, out Quantity<Differential<T>> result)
        {
            throw new NotImplementedException();
        }

        public bool TryLessThan(Quantity<Differential<T>> a, Quantity<Differential<T>> b, IWorkspace<Differential<T>>? workspace, out Quantity<Differential<T>> result)
        {
            throw new NotImplementedException();
        }

        public bool TryLessThanOrEqual(Quantity<Differential<T>> a, Quantity<Differential<T>> b, IWorkspace<Differential<T>>? workspace, out Quantity<Differential<T>> result)
        {
            throw new NotImplementedException();
        }

        public bool TryEquals(Quantity<Differential<T>> a, Quantity<Differential<T>> b, IWorkspace<Differential<T>>? workspace, out Quantity<Differential<T>> result)
        {
            throw new NotImplementedException();
        }

        public bool TryNotEquals(Quantity<Differential<T>> a, Quantity<Differential<T>> b, IWorkspace<Differential<T>>? workspace, out Quantity<Differential<T>> result)
        {
            throw new NotImplementedException();
        }

        public bool TryIsTrue(Quantity<Differential<T>> a, IWorkspace<Differential<T>>? workspace, out bool result)
        {
            throw new NotImplementedException();
        }

        public bool TryFactor(Quantity<Differential<T>> a, Quantity<Differential<T>> unit, out double factor)
        {
            throw new NotImplementedException();
        }

        public bool TryFormat(Quantity<Differential<T>> value, string? format, IFormatProvider? formatProvider, out Quantity<string> result)
        {
            throw new NotImplementedException();
        }
    }
}
