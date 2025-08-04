using CommunityToolkit.Mvvm.ComponentModel;
using System.Collections.ObjectModel;

namespace MaxwellCalc.ViewModels
{
    /// <summary>
    /// A view model for a function.
    /// </summary>
    public partial class UserFunctionViewModel : ViewModelBase
    {
        /// <summary>
        /// Gets or sets the name of the function.
        /// </summary>
        [ObservableProperty]
        private string? _name;

        [ObservableProperty]
        private ObservableCollection<string>? _arguments;

        [ObservableProperty]
        private string? _value;
    }
}
