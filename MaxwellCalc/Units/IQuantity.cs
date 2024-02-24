namespace MaxwellCalc.Units
{
    /// <summary>
    /// Describes a quantity, potentially with units.
    /// </summary>
    public interface IQuantity
    {
        /// <summary>
        /// Gets the unit.
        /// </summary>
        public Unit Unit { get; }

        /// <summary>
        /// Gets the scalar.
        /// </summary>
        public object? Scalar { get; }
    }
}
