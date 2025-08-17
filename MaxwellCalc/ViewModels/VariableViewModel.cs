using CommunityToolkit.Mvvm.ComponentModel;
using MaxwellCalc.Core.Units;

namespace MaxwellCalc.ViewModels
{
    /// <summary>
    /// A definition for a variable.
    /// </summary>
    public partial class UserVariableViewModel : SelectableViewModelBase<string>
    {
        [ObservableProperty]
        private string _name = string.Empty;

        [ObservableProperty]
        private Quantity<string> _value;

        [ObservableProperty]
        private string _description = string.Empty;
    }
}
