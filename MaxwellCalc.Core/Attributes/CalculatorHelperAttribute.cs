using System;

namespace MaxwellCalc.Core.Attributes
{
    /// <summary>
    /// An attribute that indicates that a class can be used as a helper class for built-in functions.
    /// </summary>
    /// <param name="type">The workspace type.</param>
    [AttributeUsage(AttributeTargets.Class)]
    public class CalculatorHelperAttribute(Type type) : Attribute
    {
        /// <summary>
        /// Gets the type for which the class is a helper.
        /// </summary>
        public Type Type { get; } = type ?? throw new ArgumentNullException(nameof(type));
    }
}
