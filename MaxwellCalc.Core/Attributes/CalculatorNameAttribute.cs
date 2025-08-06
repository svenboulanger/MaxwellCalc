using System;

namespace MaxwellCalc.Core.Attributes
{
    /// <summary>
    /// An attribute for defining the name of a built-in function.
    /// </summary>
    /// <param name="name">The name.</param>
    [AttributeUsage(AttributeTargets.All, AllowMultiple = false)]
    public class CalculatorNameAttribute(string name) : Attribute
    {
        /// <summary>
        /// Gets the name.
        /// </summary>
        public string Name => name;
    }
}
