using System;

namespace MaxwellCalc.Workspaces
{
    /// <summary>
    /// Event arguments for when an input unit changed.
    /// </summary>
    /// <param name="name">The name of the input unit.</param>
    public class InputUnitChangedEvent(string? name) : EventArgs
    {
        /// <summary>
        /// Gets the name of the input event.
        /// </summary>
        public string? Name { get; } = name;
    }
}
