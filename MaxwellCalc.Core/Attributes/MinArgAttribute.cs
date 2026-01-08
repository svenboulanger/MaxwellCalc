using System;

namespace MaxwellCalc.Core.Attributes;

/// <summary>
/// An attribute for defining the minimum number of expected arguments of a built-in method.
/// </summary>
/// <param name="argCount">The argument count.</param>
[AttributeUsage(AttributeTargets.Method, AllowMultiple = false)]
public class MinArgAttribute(int argCount) : Attribute
{
    /// <summary>
    /// Gets the minimum.
    /// </summary>
    public int Minimum => argCount;
}
