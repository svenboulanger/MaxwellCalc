using System;

namespace MaxwellCalc.Core.Units;

/// <summary>
/// A quantity with a scalar and a unit.
/// </summary>
/// <param name="Scalar">The scalar.</param>
/// <param name="Unit">The unit.</param>
public record struct Quantity<T>(T Scalar, Unit Unit) : IEquatable<Quantity<T>>
{
}
