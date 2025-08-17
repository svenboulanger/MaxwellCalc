using CommunityToolkit.Mvvm.ComponentModel;
using MaxwellCalc.Core.Units;

namespace MaxwellCalc.ViewModels
{
    /// <summary>
    /// A model for an input unit.
    /// </summary>
    public partial class InputUnitViewModel : SelectableViewModelBase<string>
    {
        [ObservableProperty]
        private string? _name;

        [ObservableProperty]
        private Quantity<string> _value = default;
    }
}
