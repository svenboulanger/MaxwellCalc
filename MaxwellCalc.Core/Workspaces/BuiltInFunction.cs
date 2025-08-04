namespace MaxwellCalc.Workspaces
{
    /// <summary>
    /// The description for a built-in function.
    /// </summary>
    /// <param name="name">The name.</param>
    /// <param name="minArgs">The minimum number of arguments.</param>
    /// <param name="maxArgs">The maximum number of arguments.</param>
    /// <param name="description">The description.</param>
    public readonly struct BuiltInFunction(string name, int minArgs, int maxArgs, string description)
    {
        /// <summary>
        /// Gets the name of the function.
        /// </summary>
        public string Name { get; } = name;

        /// <summary>
        /// Gets the minimum argument count.
        /// </summary>
        public int MinimumArgumentCount { get; } = minArgs;

        /// <summary>
        /// Gets the maximum argument count.
        /// </summary>
        public int MaximumArgumentCount { get; } = maxArgs;

        /// <summary>
        /// Gets the description.
        /// </summary>
        public string Description { get; } = description;
    }
}
