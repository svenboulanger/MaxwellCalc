using CommunityToolkit.Mvvm.ComponentModel;
using MaxwellCalc.Units;

namespace MaxwellCalc.ViewModels
{
    /// <summary>
    /// An output unit.
    /// </summary>
    public partial class OutputUnitViewModel : SelectableViewModelBase
    {
        [ObservableProperty]
        private Unit _unit;

        [ObservableProperty]
        private Quantity<string> _value;
    }
}
