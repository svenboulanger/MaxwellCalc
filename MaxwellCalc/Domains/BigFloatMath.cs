using System.Numerics;

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

        /// <summary>
        /// Calculates the sine.
        /// </summary>
        /// <param name="arg">The argument.</param>
        /// <param name="pi">The number pi that should be used for this precision.</param>
        /// <param name="bitsPrecision">The number of bits of precision.</param>
        /// <returns>Returns the sine of <paramref name="arg"/>.</returns>
        public static BigFloat Sin(BigFloat arg, BigFloat pi, int bitsPrecision)
        {
            var (n, rem) = BigFloat.DivRem(arg, pi);
            arg = rem;
            bool dontInvert = n.Mantissa.IsEven;
            var halfPi = new BigFloat(pi.Mantissa, pi.Exponent - 1);
            if (arg > halfPi)
            {
                arg -= pi;
                dontInvert = !dontInvert;
            }
            else if (arg < -halfPi)
            {
                arg += pi;
                dontInvert = !dontInvert;
            }

            // arg is between -pi/2 and pi/2
            BigFloat result;
            var quarterPi = new BigFloat(pi.Mantissa, pi.Exponent - 2);
            if (arg > quarterPi)
            {
                // arg is between pi/4 and pi/2
                result = CosInternal(halfPi - arg, pi, bitsPrecision + 2);
            }
            else if (arg > -quarterPi)
            {
                // arg is between -pi/4 and pi/4
                result = SinInternal(arg, pi, bitsPrecision + 2);
            }
            else
            {
                // arg is between -pi/2 and -pi/4
                result = CosInternal(halfPi + arg, pi, bitsPrecision + 2);
                dontInvert = !dontInvert;
            }
            result = result.Truncate(bitsPrecision);
            return dontInvert ? result : -result;
        }

        /// <summary>
        /// Calculates the cosine.
        /// </summary>
        /// <param name="arg">The argument.</param>
        /// <param name="pi">The number pi that should be used for this precision.</param>
        /// <param name="bitsPrecision">The number of bits of precision.</param>
        /// <returns>Returns the cosine of <paramref name="arg"/>.</returns>
        public static BigFloat Cos(BigFloat arg, BigFloat pi, int bitsPrecision)
        {
            // First, normalize the argument
            var (n, rem) = BigFloat.DivRem(arg, pi);
            arg = rem;
            bool dontInvert = n.Mantissa.IsEven;
            var halfPi = new BigFloat(pi.Mantissa, pi.Exponent - 1);
            if (arg > halfPi)
            {
                arg -= pi;
                dontInvert = !dontInvert;
            }
            else if (arg < -halfPi)
            {
                arg += pi;
                dontInvert = !dontInvert;
            }

            // arg is between -pi/2 and pi/2
            BigFloat result;
            var quarterPi = new BigFloat(pi.Mantissa, pi.Exponent - 2);
            if (arg > quarterPi)
            {
                // arg is between pi/4 and pi/2
                result = SinInternal(halfPi - arg, pi, bitsPrecision + 2);
            }
            else if (arg > -quarterPi)
            {
                // arg is between -pi/4 and pi/4
                result = CosInternal(arg, pi, bitsPrecision + 2);
            }
            else
            {
                // arg is between -pi/2 and -pi/4
                result = SinInternal(halfPi + arg, pi, bitsPrecision + 2);
            }
            result = result.Truncate(bitsPrecision);
            return dontInvert ? result : -result;
        }

        private static BigFloat SinInternal(BigFloat arg, BigFloat pi, int bitsPrecision)
        {
            BigFloat num = arg;
            BigFloat arg2 = arg * arg;
            BigInteger denom = 1;
            BigFloat xn = 0;
            BigFloat xnp1 = arg;
            BigInteger index = 2;
            bool sign = false;
            while (xn != xnp1)
            {
                xn = xnp1;
                num *= arg2;
                denom *= (index++) * (index++);
                var t1 = BigFloat.Divide(num, new BigFloat(denom, 0), bitsPrecision);
                if (sign)
                    xnp1 = xn + t1;
                else
                    xnp1 = xn - t1;
                xnp1 = xnp1.Truncate(bitsPrecision);
                sign = !sign;
            }
            return xn;
        }

        private static BigFloat CosInternal(BigFloat arg, BigFloat pi, int bitsPrecision)
        {
            BigFloat num = 1;
            BigFloat arg2 = arg * arg;
            BigInteger denom = 1;
            BigFloat xn = 0;
            BigFloat xnp1 = 1;
            BigInteger index = 1; // Used for the factorial in the denominator
            bool sign = false;
            while (xn != xnp1)
            {
                xn = xnp1;
                num *= arg2;
                denom *= (index++) * (index++);
                var t1 = BigFloat.Divide(num, new BigFloat(denom, 0), bitsPrecision);
                if (sign)
                    xnp1 = xn + t1;
                else
                    xnp1 = xn - t1;
                xnp1 = xnp1.Truncate(bitsPrecision);
                sign = !sign;
            }
            return xn;
        }
    }
}
