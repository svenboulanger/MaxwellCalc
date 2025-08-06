using System;
using System.Collections.Generic;
using System.Text;

namespace MaxwellCalc.Core.Workspaces.SpecialFunctions
{
    public static class Common
    {
        public static double polevl(double x, double[] coef, int N)
        {
            double ans;
            int i;
            
            int p_i = 0;
            ans = coef[p_i++];
            i = N;

            do {
                ans = ans * x + coef[p_i++];
            } while (--i != 0);

            return ans;
        }
    }
}
