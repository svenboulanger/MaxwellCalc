using CommunityToolkit.Mvvm.ComponentModel;

namespace MaxwellCalc.ViewModels
{
    /// <summary>
    /// A view model for a function.
    /// </summary>
    public partial class FunctionViewModel : ViewModelBase
    {
        /// <summary>
        /// Gets or sets the name of the function.
        /// </summary>
        [ObservableProperty]
        private string? _name;

        [ObservableProperty]
        private string? _value;
    }
}
