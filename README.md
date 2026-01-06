# MaxwellCalc

MaxwellCalc is a calculator application that has support for scientific units. The idea is loosely based on [SpeedCrunch](https://heldercorreia.bitbucket.io/speedcrunch/), but has some differences that are practical in nature.

The project uses the .NET framework with Avalonia for the UI.

## Differences with SpeedCrunch

- Implicit multiplication has precedence over regular division. This means that `1V/1A` will result in `1Ohm`, while SpeedCrunch would have returned `1 watt`.
- Fine control over units that are converted to SI units, and which ones are used to convert back to "displayed" units.
- No support for extended precision arithmetic.

## Setup

There are no binaries right now. You simply have to compile the project for the platform that you're interested in.

To get started, simply open the executable and start working. If you would like to quickly insert commonly used units and constants, go to the units or constants pane and right-click in the list of units or constants. You can then select `Add common input and output units`, `Add electrical input and output units` for units, or `Add common constants` to quickly add a set of units/constants that are common in physics and/or electronics engineering.