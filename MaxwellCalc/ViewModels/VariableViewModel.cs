using MaxwellCalc.Units;

namespace MaxwellCalc.ViewModels
{
    /// <summary>
    /// A definition for a variable.
    /// </summary>
    public class VariableViewModel
    {
        /// <summary>
        /// Gets or sets the name of the variable.
        /// </summary>
        public string? Name { get; set; }

        /// <summary>
        /// Gets or sets the value of the variable.
        /// </summary>
        public Quantity<string> Value { get; set; }
    }
}
