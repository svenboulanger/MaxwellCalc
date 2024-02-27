using MaxwellCalc.Units;
using MaxwellCalc.Workspaces;
using System;

namespace MaxwellCalc.Resolvers
{
    /// <summary>
    /// Helper methods for real scalars.
    /// </summary>
    public static class RealHelper
    {
        /// <summary>
        /// Registers common constants.
        /// </summary>
        /// <param name="workspace">The workspace.</param>
        public static void RegisterCommonConstants(IWorkspace<double> workspace)
        {
            // Pi, as expected
            workspace.TrySetVariable("pi", new Quantity<double>(Math.PI, Unit.UnitNone));

            // Euler number
            workspace.TrySetVariable("e", new Quantity<double>(Math.E, Unit.UnitNone));

            // Speed of light
            workspace.TrySetVariable("c", new Quantity<double>(299792458.0, new Unit((Unit.Meter, 1), (Unit.Second, -1))));
        }

        /// <summary>
        /// Registers constants that are common for electrical applications.
        /// </summary>
        /// <param name="workspace">The workspace.</param>
        public static void RegisterCommonElectronicsConstants(IWorkspace<double> workspace)
        {
            // Elementary charge
            workspace.TrySetVariable("q", new Quantity<double>(1.60217663e-19, new Unit((Unit.Ampere, 1), (Unit.Second, 1))));
        }
    }
}
