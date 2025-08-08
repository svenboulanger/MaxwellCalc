using CommunityToolkit.Mvvm.ComponentModel;
using MaxwellCalc.Units;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MaxwellCalc.ViewModels
{
    public partial class InputUnitViewModel : ViewModelBase
    {
        [ObservableProperty]
        private string? _name;

        [ObservableProperty]
        private Quantity<string>? _value;
    }
}
