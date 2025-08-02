using MaxwellCalc.Units;
using System.Text.Json.Serialization;

namespace MaxwellCalc.Workspaces
{
    /// <summary>
    /// An output unit.
    /// </summary>
    public readonly struct OutputUnit
    {
        /// <summary>
        /// The unit that can be used as an output.
        /// </summary>
        [JsonPropertyName("u")]
        public Unit Unit { get; }

        /// <summary>
        /// The equivalent value of the unit.
        /// </summary>
        [JsonPropertyName("v")]
        public Quantity<string> Value { get; }

        /// <summary>
        /// Creates a new <see cref="OutputUnit"/>.
        /// </summary>
        /// <param name="unit">The unit.</param>
        /// <param name="value">The value.</param>
        [JsonConstructor]
        public OutputUnit(Unit unit, Quantity<string> value)
        {
            Unit = unit;
            Value = value;
        }
    }
}
