using MaxwellCalc.Parsers.Nodes;
using MaxwellCalc.Units;
using System.Text.Json.Serialization;

namespace MaxwellCalc.Workspaces
{
    /// <summary>
    /// An input unit.
    /// </summary>
    public readonly record struct InputUnit
    {
        /// <summary>
        /// Gets the unit name.
        /// </summary>
        [JsonPropertyName("u")]
        public string UnitName { get; }

        /// <summary>
        /// Gets the value.
        /// </summary>
        [JsonPropertyName("v")]
        public Quantity<INode> Value { get; }

        /// <summary>
        /// Creates a new <see cref="InputUnit"/>.
        /// </summary>
        /// <param name="unitName">The unit name.</param>
        /// <param name="value">The unit value.</param>
        [JsonConstructor]
        public InputUnit(string unitName, Quantity<INode> value)
        {
            UnitName = unitName;
            Value = value;
        }
    }
}
