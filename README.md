# MaxwellNotebook

MaxwellNotebook is a calculator application that has support for scientific units. The idea is loosely based on [SpeedCrunch](https://heldercorreia.bitbucket.io/speedcrunch/), but has some differences that are practical in nature. By now, there are many other calculators that can also deal with units, but I wanted one that I can trust to behave predictably and correctly.

The project uses the .NET framework with Avalonia for the UI. The UI design was created using Claude Design. A worksheet is transient and never modifies the underlying workspace (you can define variables and functions, but they will not be stored in the workspace). You can modify the underlying workspace through the palettes accessible through the buttons at the bottom.

<p align="center"><img src="screenshot.png" /></p>

The unit system is supposed to be straightforward:

- **Input units** are converted to *base units* (usually SI units, like `m`, `s`, `cd`, etc., but this can be any of your choosing).
  - If you want to add a *base unit*, go to "Units", and enter a name without a value for it.
  - After this, you can specify other units that resolve to this base unit.
- The computations are done using *base units*.
  - Only rational powers of base units are supported.
- **Output units** are the possibilities that MaxwellCalc considers for showing you output.
  - By default, it will select the output unit that gives you the smallest scalar above `1.0`.
  - If there are no output units for the given combination of base units, it will simply return all the base units.
  - You can always override the output unit by using the `... in ...` operator. For example `1fF in pF` will give the result in `pF` even if it would not be chosen by default. You can also use any expression after the `in` operator, like `1C in q` with `q` the elementary charge and `C` the coulomb unit, will result in `6.2415e18 q` even though `q` is not strictly a unit.

## Setup

There are no binaries right now. You simply have to compile the project for the platform that you're interested in. To add common units for physics or electronics, go to the blue workspace button in the bottom left, and click the cog icon next to the workspace you are using. In this pane you can add units for common (scientific) situations.

## Features

- Real and complex numbers are supported.
- A unit system: any input is automatically converted through the "input unit" list to a second set of units ("base units", typically SI units). Calculations are then executed. At the end, the resulting units are converted back to "output units" from the "base units". The output unit is chosen to get as close as possible to a scalar value just above 1.0. This behavior can always be overridden through the `in` operator: `1m/s in km/hour` results in `3.6 km/hour`.
- A set of built-in functions is also available: `abs`, `acos`, `acosh`, `asin`, `asinh`, `atan`, `atan2`, `atanh`, `B`, `BLn`, `cos`, `cosh`, `E1`, `Ei`, `En`, `exp`, `expE1`, `expEi`, `factorial`, `factorialLn`, `Gamma`, `GammaLn`, `ln`, `log10`, `log2`, `max`, `min`, `multinomial`, `round`, `sin`, `sinh`, `sqrt`, `tan`, `tanh`.
- Light and dark themed.
- Different workspaces are allowed (each with their own set of units, variables, constants, etc.)
- Special precedence for implicit multiplication (`1m/1s` = `1 m/s` and not `1 m*s`).