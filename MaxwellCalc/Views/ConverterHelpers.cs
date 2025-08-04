using Avalonia.Data.Converters;
using System.Collections.ObjectModel;

namespace MaxwellCalc.Views
{
    public static class ConverterHelpers
    {
        /// <summary>
        /// A converter for joining multiple items with a comma.
        /// </summary>
        public static FuncValueConverter<ObservableCollection<string>?, string> CommaJoinedConverter =
            new(value => value is null ? string.Empty : string.Join(", ", value));
    }
}
