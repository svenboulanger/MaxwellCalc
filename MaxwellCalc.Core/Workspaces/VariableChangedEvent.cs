using System;

namespace MaxwellCalc.Workspaces
{
    /// <summary>
    /// Event args for when a variable changes value.
    /// </summary>
    public class VariableChangedEvent : EventArgs
    {
        /// <summary>
        /// Gets the name of the variable.
        /// </summary>
        public string? Name { get; }

        /// <summary>
        /// Creates a new <see cref="VariableChangedEvent"/>.
        /// </summary>
        /// <param name="name">The variable name.</param>
        public VariableChangedEvent(string? name)
        {
            Name = name;
        }
    }
}
