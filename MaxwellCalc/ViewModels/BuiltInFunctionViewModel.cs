using CommunityToolkit.Mvvm.ComponentModel;
using Material.Styles.Converters;

namespace MaxwellCalc.ViewModels
{
    /// <summary>
    /// A view model for representing a built-in function.
    /// </summary>
    public partial class BuiltInFunctionViewModel : ViewModelBase
    {
        [ObservableProperty]
        private string _name = string.Empty;

        [ObservableProperty]
        private int _minArgCount;

        [ObservableProperty]
        private int _maxArgCount;

        [ObservableProperty]
        private string _description = string.Empty;

        [ObservableProperty]
        private string _argCountDescription = string.Empty;

        public BuiltInFunctionViewModel()
        {
        }

        partial void OnMinArgCountChanged(int value) => UpdateArgCountDescription();

        partial void OnMaxArgCountChanged(int value) => UpdateArgCountDescription();

        private void UpdateArgCountDescription()
        {
            if (MinArgCount == MaxArgCount)
                ArgCountDescription = MinArgCount.ToString();
            else if (MaxArgCount == int.MaxValue)
                ArgCountDescription = $"> {MinArgCount}";
            else
                ArgCountDescription = $"{MinArgCount} - {MaxArgCount}";
        }
    }
}
