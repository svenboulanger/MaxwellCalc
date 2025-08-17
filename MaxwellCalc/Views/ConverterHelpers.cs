using Avalonia.Data.Converters;
using Material.Colors;
using MaxwellCalc.Core.Units;
using System.Collections.ObjectModel;

namespace MaxwellCalc.Views
{
    public static class ConverterHelpers
    {
        /// <summary>
        /// A converter for joining multiple items with a comma.
        /// </summary>
        public static FuncValueConverter<ObservableCollection<string>?, string> CommaJoinedConverter { get; } =
            new(value => value is null ? string.Empty : string.Join(", ", value));

        /// <summary>
        /// A converter for converting a material primary color to an actual color.
        /// </summary>
        public static FuncValueConverter<PrimaryColor, string> PrimaryColorConverter { get; } =
            new(value => SwatchHelper.Lookup[(MaterialColor)value].ToString());

        /// <summary>
        /// A converter for converting a material secondary color to an actual color.
        /// </summary>
        public static FuncValueConverter<SecondaryColor, string> SecondaryColorConverter { get; } =
            new(value => SwatchHelper.Lookup[(MaterialColor)value].ToString());

        /// <summary>
        /// A converter for converting a unit to a quantity that can be displayed.
        /// </summary>
        public static FuncValueConverter<Unit, Quantity<string>> UnitToQuantityConverter { get; } =
            new(value => new Quantity<string>(string.Empty, value));

        /// <summary>
        /// A converter for converting a string representing a unit to a quantity that can be displayed.
        /// </summary>
        public static FuncValueConverter<string, Quantity<string>> StringUnitToQuantityConverter { get; } =
            new(value => string.IsNullOrEmpty(value) ? new Quantity<string>(string.Empty, Unit.UnitNone) : new Quantity<string>(string.Empty, new Unit((value, 1))));
    }
}
