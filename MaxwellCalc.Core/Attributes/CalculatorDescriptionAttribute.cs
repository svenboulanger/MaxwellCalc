using System;

namespace MaxwellCalc.Core.Attributes
{
    /// <summary>
    /// An attribute for defining the description of a built-in method.
    /// </summary>
    /// <param name="description">The description.</param>
    [AttributeUsage(AttributeTargets.All, AllowMultiple = false)]
    public class CalculatorDescriptionAttribute(string description) : Attribute
    {
        /// <summary>
        /// Gets or sets the description.
        /// </summary>
        public string Description => description;
    }
}
