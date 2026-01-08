using CommunityToolkit.Mvvm.ComponentModel;
using Material.Icons;

namespace MaxwellCalc.ViewModels;

public partial class PaneMenuItemViewModel : ViewModelBase
{
    [ObservableProperty]
    private string _text = string.Empty;

    [ObservableProperty]
    private MaterialIconKind? _icon = null;

    [ObservableProperty]
    private ViewModelBase? _viewModel = null;
}
