using CommunityToolkit.Mvvm.ComponentModel;
using MaxwellCalc.Core.Workspaces;
using System.Collections.ObjectModel;

namespace MaxwellCalc.ViewModels
{
    /// <summary>
    /// A view model for a function.
    /// </summary>
    public partial class UserFunctionViewModel : SelectableViewModelBase<UserFunctionKey>
    {
        /// <summary>
        /// Gets or sets the name of the function.
        /// </summary>
        [ObservableProperty]
        private string? _name;

        /// <summary>
        /// Gets or sets the name.
        /// </summary>
        [ObservableProperty]
        private ObservableCollection<string>? _arguments;

        /// <summary>
        /// Gets or sets the value.
        /// </summary>
        [ObservableProperty]
        private string? _value;
    }
}
