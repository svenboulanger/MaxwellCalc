using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
        /// <param name="bitsPrecision">The bits precision.</param>
        /// <returns>Returns the square root.</returns>
        public static BigFloat Sqrt(BigFloat arg, int bitsPrecision)
        {
            var two = new BigFloat(1, 1);
            var xn = new BigFloat(arg.Mantissa, arg.Exponent - 1);
            var xnp1 = BigFloat.Divide(arg + xn * xn, two * xn, bitsPrecision);
            xnp1 = xnp1.Truncate(bitsPrecision);

            while (xnp1 != xn)
            {
                xn = xnp1;
                xnp1 = BigFloat.Divide(arg + xn * xn, two * xn, bitsPrecision);
                xnp1 = xnp1.Truncate(bitsPrecision);
            }
            return xn;
        }
    }
}
