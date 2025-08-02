using MaxwellCalc.Units;
using System.Text.Json.Serialization;

namespace MaxwellCalc.Workspaces
{
    /// <summary>
    /// A variable.
    /// </summary>
    public readonly struct Variable
    {
        /// <summary>
        /// The name of the variable.
        /// </summary>
        [JsonPropertyName("n")]
        public string Name { get; }

        /// <summary>
        /// The value of the variable.
        /// </summary>
        [JsonPropertyName("v")]
        public Quantity<string> Value { get; }

        /// <summary>
        /// Creates a new <see cref="Variable"/>.
        /// </summary>
        /// <param name="name">The name.</param>
        /// <param name="value">The value.</param>
        [JsonConstructor]
        public Variable(string name, Quantity<string> value)
        {
            Name = name;
            Value = value;
        }
    }
}
