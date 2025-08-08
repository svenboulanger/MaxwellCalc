using CommunityToolkit.Mvvm.ComponentModel;
using MaxwellCalc.Units;

namespace MaxwellCalc.ViewModels
{
    /// <summary>
    /// A definition for a variable.
    /// </summary>
    public partial class UserVariableViewModel : ViewModelBase
    {
        [ObservableProperty]
        private string? _name;

        [ObservableProperty]
        private Quantity<string> _value;

        [ObservableProperty]
        private string? _description;
    }
}
