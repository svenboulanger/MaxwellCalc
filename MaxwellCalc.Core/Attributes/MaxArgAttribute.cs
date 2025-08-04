using System;

namespace MaxwellCalc.Core.Attributes
{
    /// <summary>
    /// An attribute for defining the maximum number of expected arguments of a built-in method.
    /// </summary>
    /// <param name="argCount">The argument count. If -1, any number of arguments is allowed.</param>
    [AttributeUsage(AttributeTargets.Method, AllowMultiple = false)]
    public class MaxArgAttribute(int argCount) : Attribute
    {
        /// <summary>
        /// Gets the maximum.
        /// </summary>
        public int Maximum => argCount;
    }
}
