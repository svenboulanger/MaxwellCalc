using Avalonia.Data.Converters;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
