﻿using MaxwellCalc.Units;
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
        /// <param name="workspace">The workspace.Variables.</param>
        public static void RegisterCommonConstants(IWorkspace<double> workspace)
        {
            // Pi, as expected
            workspace.Variables.TrySetVariable("pi", new Quantity<double>(Math.PI, Unit.UnitNone));

            // Euler number
            workspace.Variables.TrySetVariable("e", new Quantity<double>(Math.E, Unit.UnitNone));

            // Speed of light
            workspace.Variables.TrySetVariable("c", new Quantity<double>(299792458.0, new Unit((Unit.Meter, 1), (Unit.Second, -1))));
        }

        /// <summary>
        /// Registers constants that are common for electrical applications.
        /// </summary>
        /// <param name="workspace">The workspace.Variables.</param>
        public static void RegisterCommonElectronicsConstants(IWorkspace<double> workspace)
        {
            // Elementary charge (Coulomb)
            workspace.Variables.TrySetVariable("q", new Quantity<double>(1.60217663e-19, new Unit((Unit.Ampere, 1), (Unit.Second, 1))));

            // Permittivity of vacuum (Farad/meter)
            workspace.Variables.TrySetVariable("eps0", new Quantity<double>(8.8541878128e-12, new Unit(
                    (Unit.Kilogram, -1),
                    (Unit.Meter, -3),
                    (Unit.Second, 4),
                    (Unit.Ampere, 2))));

            // Permeability of vacuum (Newton Ampere^-2)
            workspace.Variables.TrySetVariable("mu0", new Quantity<double>(1.25663706212e-6, new Unit(
                (Unit.Kilogram, 1),
                (Unit.Meter, 1),
                (Unit.Second, -2),
                (Unit.Ampere, -2))));

            // Electron-volt (eV)
            workspace.Variables.TrySetVariable("eV", new Quantity<double>(1.60217663e-19, new Unit(
                (Unit.Kilogram, 1),
                (Unit.Meter, 2),
                (Unit.Second, -2))));

            // Planck constant (J s)
            workspace.Variables.TrySetVariable("h", new Quantity<double>(6.6260693e-34, new Unit(
                (Unit.Kilogram, 1),
                (Unit.Meter, 2),
                (Unit.Second, -1))));

            // Reduced Planck constant bar (J s)
            workspace.Variables.TrySetVariable("hbar", new Quantity<double>(6.6260693e-34 / Math.PI, new Unit(
                (Unit.Kilogram, 1),
                (Unit.Meter, 2),
                (Unit.Second, -1))));

            // Boltzmann constant (J/K)
            workspace.Variables.TrySetVariable("k", new Quantity<double>(1.3806505e-23, new Unit(
                (Unit.Kilogram, 1),
                (Unit.Meter, 2),
                (Unit.Second, -2),
                (Unit.Kelvin, -1))));
        }
    }
}
