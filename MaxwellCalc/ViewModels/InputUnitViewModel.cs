using CommunityToolkit.Mvvm.ComponentModel;
using MaxwellCalc.Units;

namespace MaxwellCalc.ViewModels
{
    /// <summary>
    /// A model for an input unit.
    /// </summary>
    public partial class InputUnitViewModel : SelectableViewModelBase
    {
        [ObservableProperty]
        private string? _name;

        [ObservableProperty]
        private Quantity<string>? _value;
    }
}
