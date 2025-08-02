using System;
using System.Collections.Generic;

namespace MaxwellCalc.Workspaces
{
    /// <summary>
    /// Event arguments used when a function changes.
    /// </summary>
    public class FunctionChangedEvent : EventArgs
    {
        /// <summary>
        /// Gets the name of the function.
        /// </summary>
        public string? Name { get; }

        /// <summary>
        /// Gets the function argument parameters.
        /// </summary>
        public int Arguments { get; }

        /// <summary>
        /// Creates a new <see cref="FunctionChangedEvent"/>
        /// </summary>
        /// <param name="name">The name of the function.</param>
        /// <param name="arguments">The number of arguments.</param>
        public FunctionChangedEvent(string? name, int arguments)
        {
            Name = name;
            Arguments = arguments;
        }
    }
}
