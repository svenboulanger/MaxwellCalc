using System;
using System.Numerics;

namespace MaxwellCalc.Core.Workspaces.SpecialFunctions;

/// <summary>
/// Special functions that have to do with exponential integrals.
/// </summary>
/// <remarks>This is a port of the SciPy XSF library.</remarks>
public static class ExponentialIntegralFunctions
{
    /// <summary>
    /// Computes the exponential integral E1(x).
    /// </summary>
    /// <param name="x">The argument.</param>
    /// <returns>The result.</returns>
    public static double Exp1(double x)
    {
        int k, m;
        double e1, r, t, t0;
        double ga = Constants.EulerGamma;

        if (x == 0.0)
            return double.PositiveInfinity;
        if (x <= 1.0)
        {
            e1 = 1.0;
            r = 1.0;
            for (k = 1; k < 26; k++)
            {
                r = -r * k * x / ((k + 1.0) * (k + 1.0));
                e1 += r;
                if (Math.Abs(r) <= Math.Abs(e1) * 1e-15)
                    break;
            }
            return -ga - Math.Log(x) + x * e1;
        }
        m = 20 + (int)(80.0 / x);
        t0 = 0.0;
        for (k = m; k > 0; k--)
        {
            t0 = k / (1.0 + k / (x + t0));
        }
        t = 1.0 / (x + t0);
        return Math.Exp(-x) * t;
    }

    /// <summary>
    /// Computes exp(x) * E1(x).
    /// </summary>
    /// <param name="x">The argument.</param>
    /// <returns>The result.</returns>
    public static double ExpExp1(double x)
    {
        int k, m;
        double e1, r, t, t0;
        double ga = Constants.EulerGamma;

        if (x == 0.0)
            return double.PositiveInfinity;
        if (x <= 1.0)
        {
            e1 = 1.0;
            r = 1.0;
            for (k = 1; k < 26; k++)
            {
                r = -r * k * x / ((k + 1.0) * (k + 1.0));
                e1 += r;
                if (Math.Abs(r) <= Math.Abs(e1) * 1e-15)
                    break;
            }
            return Math.Exp(x) * (-ga - Math.Log(x) + x * e1);
        }
        m = 20 + (int)(80.0 / x);
        t0 = 0.0;
        for (k = m; k > 0; k--)
        {
            t0 = k / (1.0 + k / (x + t0));
        }
        t = 1.0 / (x + t0);
        return t;
    }

    /// <summary>
    /// Computes the complex exponential integral E1(z).
    /// </summary>
    /// <param name="z">The argument.</param>
    /// <returns>The result.</returns>
    public static Complex Exp1(Complex z)
    {
        // ====================================================
        // Purpose: Compute complex exponential integral E1(z)
        // Input :  z   --- Argument of E1(z)
        // Output:  CE1 --- E1(z)
        // ====================================================
        double el = Constants.EulerGamma;
        int k;
        Complex ce1, cr, zc, zd, zdc;
        double x = z.Real;
        double a0 = Complex.Abs(z);
        // Continued fraction converges slowly near negative real axis,
        // so use power series in a wedge around it until radius 40.0
        double xt = -2.0 * Math.Abs(z.Imaginary);

        if (a0 == 0.0)
            return double.PositiveInfinity;
        if ((a0 < 5.0) || ((x < xt) && (a0 < 40.0)))
        {
            // Power series
            ce1 = 1.0;
            cr = 1.0;
            for (k = 1; k < 501; k++)
            {
                cr = -cr * z * k / ((k + 1.0) * (k + 1.0));
                ce1 += cr;
                if (Complex.Abs(cr) < Complex.Abs(ce1) * 1e-15)
                    break;
            }
            if ((x <= 0.0) && (z.Imaginary == 0.0))
            {
                // Careful on the branch cut -- use the sign of the imaginary part
                //  to get the right sign on the factor if pi.
                if (z.Imaginary >= 0.0)
                    ce1 = -el - Complex.Log(-z) + z * ce1 - new Complex(0.0, Math.PI);
                else
                    ce1 = -el - Complex.Log(-z) + z * ce1 + new Complex(0.0, Math.PI);
            }
            else
            {
                ce1 = -el - Complex.Log(z) + z * ce1;
            }
        }
        else
        {
            // Continued fraction https://dlmf.nist.gov/6.9
            //                  1     1     1     2     2     3     3
            // E1 = exp(-z) * ----- ----- ----- ----- ----- ----- ----- ...
            //                Z +   1 +   Z +   1 +   Z +   1 +   Z +
            zc = 0.0;
            zd = 1.0 / z;
            zdc = zd;
            zc += zdc;
            for (k = 1; k < 501; k++)
            {
                zd = 1.0 / (zd * k + 1.0);
                zdc *= (zd - 1.0);
                zc += zdc;

                zd = 1.0 / (zd * k + z);
                zdc *= (z * zd - 1.0);
                zc += zdc;
                if ((Complex.Abs(zdc) <= Complex.Abs(zc) * 1e-15) && (k > 20))
                    break;
            }
            ce1 = Complex.Exp(-z) * zc;
            if ((x <= 0.0) && (z.Imaginary == 0.0))
                ce1 -= new Complex(0.0, Math.PI);
        }
        return ce1;
    }

    /// <summary>
    /// Computes exp(z) * E1(z).
    /// </summary>
    /// <param name="z">The argument.</param>
    /// <returns>The result.</returns>
    public static Complex ExpExp1(Complex z)
    {
        // ====================================================
        // Purpose: Compute complex exponential integral E1(z)
        // Input :  z   --- Argument of E1(z)
        // Output:  CE1 --- E1(z)
        // ====================================================
        double el = Constants.EulerGamma;
        int k;
        Complex ce1, cr, zc, zd, zdc;
        double x = z.Real;
        double a0 = Complex.Abs(z);
        // Continued fraction converges slowly near negative real axis,
        // so use power series in a wedge around it until radius 40.0
        double xt = -2.0 * Math.Abs(z.Imaginary);

        if (a0 == 0.0)
            return double.PositiveInfinity;
        if ((a0 < 5.0) || ((x < xt) && (a0 < 40.0)))
        {
            // Power series
            ce1 = 1.0;
            cr = 1.0;
            for (k = 1; k < 501; k++)
            {
                cr = -cr * z * k / ((k + 1.0) * (k + 1.0));
                ce1 += cr;
                if (Complex.Abs(cr) < Complex.Abs(ce1) * 1e-15)
                    break;
            }
            if ((x <= 0.0) && (z.Imaginary == 0.0))
            {
                // Careful on the branch cut -- use the sign of the imaginary part
                //  to get the right sign on the factor if pi.
                if (z.Imaginary >= 0.0)
                    ce1 = Complex.Exp(z) * (-el - Complex.Log(-z) + z * ce1 - new Complex(0.0, Math.PI));
                else
                    ce1 = Complex.Exp(z) * (-el - Complex.Log(-z) + z * ce1 + new Complex(0.0, Math.PI));
            }
            else
            {
                ce1 = Complex.Exp(z) * (-el - Complex.Log(z) + z * ce1);
            }
        }
        else
        {
            // Continued fraction https://dlmf.nist.gov/6.9
            //                  1     1     1     2     2     3     3
            // E1 = exp(-z) * ----- ----- ----- ----- ----- ----- ----- ...
            //                Z +   1 +   Z +   1 +   Z +   1 +   Z +
            zc = 0.0;
            zd = 1.0 / z;
            zdc = zd;
            zc += zdc;
            for (k = 1; k < 501; k++)
            {
                zd = 1.0 / (zd * k + 1.0);
                zdc *= (zd - 1.0);
                zc += zdc;

                zd = 1.0 / (zd * k + z);
                zdc *= (z * zd - 1.0);
                zc += zdc;
                if ((Complex.Abs(zdc) <= Complex.Abs(zc) * 1e-15) && (k > 20))
                    break;
            }
            ce1 = zc;
            if ((x <= 0.0) && (z.Imaginary == 0.0))
                ce1 -= Complex.Exp(z) * new Complex(0.0, Math.PI);
        }
        return ce1;
    }

    /// <summary>
    /// Computes the exponential integral Ei(x).
    /// </summary>
    /// <param name="x">The argument.</param>
    /// <returns>The result.</returns>
    public static double ExpI(double x)
    {
        // ============================================
        // Purpose: Compute exponential integral Ei(x)
        // Input :  x  --- Argument of Ei(x)
        // Output:  EI --- Ei(x)
        // ============================================

        double ga = Constants.EulerGamma;
        double ei, r;

        if (x == 0.0)
            ei = double.NegativeInfinity;
        else if (x < 0)
            ei = -Exp1(-x);
        else if (Math.Abs(x) <= 40.0)
        {
            // Power series around x=0
            ei = 1.0;
            r = 1.0;

            for (int k = 1; k <= 100; k++)
            {
                r = r * k * x / ((k + 1.0) * (k + 1.0));
                ei += r;
                if (Math.Abs(r / ei) <= 1.0e-15)
                    break;
            }
            ei = ga + Math.Log(x) + x * ei;
        }
        else
        {
            // Asymptotic expansion (the series is not convergent)
            ei = 1.0;
            r = 1.0;
            for (int k = 1; k <= 20; k++)
            {
                r = r * k / x;
                ei += r;
            }
            ei = Math.Exp(x) / x * ei;
        }
        return ei;
    }

    /// <summary>
    /// Computes the exponential integral times the exponent exp(-x) * Ei(x).
    /// </summary>
    /// <param name="x">The argument.</param>
    /// <returns>The result.</returns>
    public static double ExpExpI(double x)
    {
        // ============================================
        // Purpose: Compute exponential integral Ei(x)
        // Input :  x  --- Argument of Ei(x)
        // Output:  EI --- Ei(x)
        // ============================================

        double ga = Constants.EulerGamma;
        double ei, r;

        if (x == 0.0)
            ei = double.NegativeInfinity;
        else if (x < 0)
            ei = -ExpExp1(-x);
        else if (Math.Abs(x) <= 40.0)
        {
            // Power series around x=0
            ei = 1.0;
            r = 1.0;

            for (int k = 1; k <= 100; k++)
            {
                r = r * k * x / ((k + 1.0) * (k + 1.0));
                ei += r;
                if (Math.Abs(r / ei) <= 1.0e-15)
                    break;
            }
            ei = Math.Exp(-x) * (ga + Math.Log(x) + x * ei);
        }
        else
        {
            // Asymptotic expansion (the series is not convergent)
            ei = 1.0;
            r = 1.0;
            for (int k = 1; k <= 20; k++)
            {
                r = r * k / x;
                ei += r;
            }
            ei = 1.0 / x * ei;
        }
        return ei;
    }

    /// <summary>
    /// Computes the complex exponential integral Ei(z).
    /// </summary>
    /// <param name="z">The argument.</param>
    /// <returns>The result.</returns>
    public static Complex ExpI(Complex z)
    {
        // ============================================
        // Purpose: Compute exponential integral Ei(x)
        // Input :  x  --- Complex argument of Ei(x)
        // Output:  EI --- Ei(x)
        // ============================================

        Complex cei;
        cei = -Exp1(-z);
        if (z.Imaginary > 0.0)
            cei += new Complex(0.0, Math.PI);
        else if (z.Imaginary < 0.0)
            cei -= new Complex(0.0, Math.PI);
        else
        {
            if (z.Real > 0.0)
                cei += new Complex(0.0, z.Imaginary > 0 ? Math.PI : -Math.PI);
        }
        return cei;
    }

    /// <summary>
    /// Computes the complex exponential integral Ei(z).
    /// </summary>
    /// <param name="z">The argument.</param>
    /// <returns>The result.</returns>
    public static Complex ExpExpI(Complex z)
    {
        // ============================================
        // Purpose: Compute exponential integral Ei(x)
        // Input :  x  --- Complex argument of Ei(x)
        // Output:  EI --- Ei(x)
        // ============================================

        Complex cei;
        cei = -ExpExp1(-z);
        if (z.Imaginary > 0.0)
            cei += Complex.Exp(-z) * new Complex(0.0, Math.PI);
        else if (z.Imaginary < 0.0)
            cei -= Complex.Exp(-z) * new Complex(0.0, Math.PI);
        else
        {
            if (z.Real > 0.0)
                cei += Complex.Exp(-z) * new Complex(0.0, z.Imaginary > 0 ? Math.PI : -Math.PI);
        }
        return cei;
    }

    /// <summary>
    /// Computes the generalized exponential integral En(n, x).
    /// </summary>
    /// <param name="n">The </param>
    /// <param name="x"></param>
    /// <returns>The result.</returns>
    public static double ExpN(int n, double x)
    {
        double ans, r, t, yk, xk;
        double pk, pkm1, pkm2, qk, qkm1, qkm2;
        double psi, z;
        int i, k;
        const double big = 1.44115188075855872E+17;

        if (double.IsNaN(x))
            return double.NaN;
        else if (n < 0 || x < 0)
            return double.NaN;

        if (x > Constants.MAXLOG)
            return 0.0;

        if (x == 0.0)
        {
            if (n < 2)
                return double.PositiveInfinity;
            else
            {
                return (1.0 / (n - 1.0));
            }
        }

        if (n == 0)
            return (Math.Exp(-x) / x);

        /* Asymptotic expansion for large n, DLMF 8.20(ii) */
        if (n > 50)
        {
            ans = ExpNLargeN(n, x);
            return ans;
        }

        /* Continued fraction, DLMF 8.19.17 */
        if (x > 1.0)
        {
            k = 1;
            pkm2 = 1.0;
            qkm2 = x;
            pkm1 = 1.0;
            qkm1 = x + n;
            ans = pkm1 / qkm1;

            do
            {
                k += 1;
                if ((k & 1) != 0)
                {
                    yk = 1.0;
                    xk = n + (k - 1) / 2;
                }
                else
                {
                    yk = x;
                    xk = k / 2;
                }
                pk = pkm1 * yk + pkm2 * xk;
                qk = qkm1 * yk + qkm2 * xk;
                if (qk != 0)
                {
                    r = pk / qk;
                    t = Math.Abs((ans - r) / r);
                    ans = r;
                }
                else
                {
                    t = 1.0;
                }
                pkm2 = pkm1;
                pkm1 = pk;
                qkm2 = qkm1;
                qkm1 = qk;
                if (Math.Abs(pk) > big)
                {
                    pkm2 /= big;
                    pkm1 /= big;
                    qkm2 /= big;
                    qkm1 /= big;
                }
            } while (t > Constants.MACHEP);

            ans *= Math.Exp(-x);
            return ans;
        }

        /* Power series expansion, DLMF 8.19.8 */
        psi = -Constants.EulerGamma - Math.Log(x);
        for (i = 1; i < n; i++)
        {
            psi = psi + 1.0 / i;
        }

        z = -x;
        xk = 0.0;
        yk = 1.0;
        pk = 1.0 - n;
        if (n == 1)
        {
            ans = 0.0;
        }
        else
        {
            ans = 1.0 / pk;
        }
        do
        {
            xk += 1.0;
            yk *= z / xk;
            pk += 1.0;
            if (pk != 0.0)
            {
                ans += yk / pk;
            }
            if (ans != 0.0)
                t = Math.Abs(yk / ans);
            else
                t = 1.0;
        } while (t > Constants.MACHEP);
        // k = xk; // Note used anymore...?
        t = n;
        r = n - 1;
        ans = (Math.Pow(z, r) * psi / GammaFunctions.Gamma(t)) - ans;
        return ans;
    }

    private const int expn_nA = 13;
    private static readonly double[] expn_A0 = { 1.00000000000000000 };
    private static readonly double[] expn_A1 = { 1.00000000000000000 };
    private static readonly double[] expn_A2 = { -2.00000000000000000, 1.00000000000000000 };
    private static readonly double[] expn_A3 = { 6.00000000000000000, -8.00000000000000000, 1.00000000000000000 };
    private static readonly double[] expn_A4 = { -24.0000000000000000, 58.0000000000000000, -22.0000000000000000, 1.00000000000000000 };
    private static readonly double[] expn_A5 = { 120.000000000000000, -444.000000000000000, 328.000000000000000, -52.0000000000000000, 1.00000000000000000 };
    private static readonly double[] expn_A6 = {-720.000000000000000, 3708.00000000000000,  -4400.00000000000000, 1452.00000000000000,  -114.000000000000000, 1.00000000000000000};
    private static readonly double[] expn_A7 = {5040.00000000000000,  -33984.0000000000000, 58140.0000000000000, -32120.0000000000000, 5610.00000000000000,  -240.000000000000000, 1.00000000000000000};
    private static readonly double[] expn_A8 = {-40320.0000000000000, 341136.000000000000,  -785304.000000000000, 644020.000000000000,  -195800.000000000000, 19950.0000000000000, -494.000000000000000, 1.00000000000000000};
    private static readonly double[] expn_A9 = {362880.000000000000,  -3733920.00000000000, 11026296.0000000000, -12440064.0000000000, 5765500.00000000000,  -1062500.00000000000, 67260.0000000000000,  -1004.00000000000000, 1.00000000000000000};
    private static readonly double[] expn_A10 = {-3628800.00000000000, 44339040.0000000000,  -162186912.000000000, 238904904.000000000,  -155357384.000000000, 44765000.0000000000, -5326160.00000000000, 218848.000000000000,  -2026.00000000000000, 1.00000000000000000};
    private static readonly double[] expn_A11 = {39916800.0000000000,  -568356480.000000000, 2507481216.00000000, -4642163952.00000000, 4002695088.00000000,  -1648384304.00000000, 314369720.000000000,  -25243904.0000000000, 695038.000000000000, -4072.00000000000000, 1.00000000000000000};
    private static readonly double[] expn_A12 = {-479001600.000000000, 7827719040.00000000,  -40788301824.0000000, 92199790224.0000000,  -101180433024.000000, 56041398784.0000000, -15548960784.0000000, 2051482776.00000000,  -114876376.000000000, 2170626.00000000000,  -8166.00000000000000, 1.00000000000000000};
    private static readonly double[][] expn_A = {expn_A0, expn_A1, expn_A2, expn_A3,  expn_A4,  expn_A5, expn_A6, expn_A7, expn_A8, expn_A9, expn_A10, expn_A11, expn_A12};
    private static readonly int[] expn_Adegs = { 0, 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11 };

    private static double ExpNLargeN(int n, double x)
    {
        int k;
        double p = n;
        double lambda = x / p;
        double multiplier = 1 / p / (lambda + 1) / (lambda + 1);
        double fac = 1;
        double res = 1; /* A[0] = 1 */
        double expfac, term;

        expfac = Math.Exp(-lambda * p) / (lambda + 1) / p;
        if (expfac == 0)
        {
            // set_error("expn", SF_ERROR_UNDERFLOW, NULL);
            return 0;
        }

        /* Do the k = 1 term outside the loop since A[1] = 1 */
        fac *= multiplier;
        res += fac;

        for (k = 2; k < expn_nA; k++)
        {
            fac *= multiplier;
            term = fac * Common.polevl(lambda, expn_A[k], expn_Adegs[k]);
            res += term;
            if (Math.Abs(term) < Constants.MACHEP * Math.Abs(res))
            {
                break;
            }
        }

        return expfac * res;
    }
}
