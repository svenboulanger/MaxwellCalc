using Avalonia;
using Avalonia.Controls;
using CommunityToolkit.Mvvm.ComponentModel;
using System.ComponentModel;

namespace MaxwellCalc.ViewModels
{
    public partial class PaneMenuItemViewModel : ViewModelBase
    {
        [ObservableProperty]
        private string _text = string.Empty;

        [ObservableProperty]
        private string _iconName = string.Empty;

        [ObservableProperty]
        private object? _iconData = null;

        [ObservableProperty]
        private ViewModelBase? _viewModel = null;

        protected override void OnPropertyChanged(PropertyChangedEventArgs e)
        {
            base.OnPropertyChanged(e);

            // We'll update some extra stuff
            if (e.PropertyName == nameof(IconName))
            {
                if (!string.IsNullOrWhiteSpace(IconName))
                {
                    Application.Current!.TryFindResource(IconName, out var resource);
                    IconData = resource;
                }
                else
                    IconData = null;
            }
        }
    }
}
