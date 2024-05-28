namespace MaxwellCalc.Domains
{
    /// <summary>
    /// Describes math functions for <see cref="BigFloat"/>.
    /// </summary>
    public static class BigFloatMath
    {
        /// <summary>
        /// Calculates the square root up to a given precision.
        /// </summary>
        /// <remarks>
        /// Uses Newton's method for converging iteratively to the solution.
        /// </remarks>
        /// <param name="arg">The argument.</param>
        /// <param name="bitsPrecision">The number of bits precision.</param>
        /// <returns>Returns the square root.</returns>
        public static BigFloat Sqrt(BigFloat arg, int bitsPrecision)
        {
            int internalPrecision = bitsPrecision + 2;
            var xn = new BigFloat(arg.Mantissa, arg.Exponent - 1);
            var xnp1 = BigFloat.Divide(arg + xn * xn, 2 * xn, internalPrecision);
            xnp1 = xnp1.Truncate(internalPrecision);

            while (xnp1 != xn)
            {
                xn = xnp1;
                xnp1 = BigFloat.Divide(arg + xn * xn, 2 * xn, internalPrecision);
                xnp1 = xnp1.Truncate(internalPrecision);
            }
            xn = xn.Truncate(bitsPrecision);
            return xn;
        }

        /// <summary>
        /// Calculates pi up to the given number of bits.
        /// </summary>
        /// <param name="bitsPrecision">The number of bits precision.</param>
        /// <returns>Returns pi.</returns>
        public static BigFloat Pi(int bitsPrecision)
        {
            int internalPrecision = bitsPrecision + 2;
            BigFloat xn = 1;
            BigFloat xnp1 = 0;
            long k = 0;
            while (xn != xnp1)
            {
                xn = xnp1;
                var t1 = BigFloat.Divide(4, 8 * k + 1, internalPrecision);
                var t2 = BigFloat.Divide(2, 8 * k + 4, internalPrecision);
                var t3 = BigFloat.Divide(1, 8 * k + 5, internalPrecision);
                var t4 = BigFloat.Divide(1, 8 * k + 6, internalPrecision);
                var sum = t1 - t2 - t3 - t4;
                xnp1 = xn + new BigFloat(sum.Mantissa, sum.Exponent - 4 * k);
                xnp1 = xnp1.Truncate(internalPrecision);
                k++;
            }
            xn = xn.Truncate(bitsPrecision);
            return xn;
        }
    }
}
