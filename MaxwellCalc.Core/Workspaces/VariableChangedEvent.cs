using System;

namespace MaxwellCalc.Workspaces
{
    /// <summary>
    /// Event arguments for when a variable changes value.
    /// </summary>
    /// <param name="name">The variable name.</param>
    public class VariableChangedEvent(string? name) : EventArgs
    {
        /// <summary>
        /// Gets the name of the variable.
        /// </summary>
        public string? Name { get; } = name;
    }
}
