using CommunityToolkit.Mvvm.ComponentModel;

namespace MaxwellCalc.ViewModels
{
    /// <summary>
    /// A view model base that can be selected.
    /// </summary>
    public abstract partial class SelectableViewModelBase : ViewModelBase
    {
        [ObservableProperty]
        private bool _selected;
    }
}
