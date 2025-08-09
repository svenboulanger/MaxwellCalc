using CommunityToolkit.Mvvm.ComponentModel;
using MaxwellCalc.Units;

namespace MaxwellCalc.ViewModels
{
    /// <summary>
    /// A model for an input unit.
    /// </summary>
    public partial class InputUnitViewModel : ViewModelBase
    {
        [ObservableProperty]
        private bool _selected = false;

        [ObservableProperty]
        private string? _name;

        [ObservableProperty]
        private Quantity<string>? _value;
    }
}
