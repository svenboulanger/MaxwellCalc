using Avalonia.Controls;
using CommunityToolkit.Mvvm.ComponentModel;
using MaxwellCalc.Workspaces;
using System.Collections.ObjectModel;

namespace MaxwellCalc.ViewModels
{
    public partial class VariablesViewModel : ViewModelBase
    {
        [ObservableProperty]
        private IWorkspace? _workspace;

        [ObservableProperty]
        private VariableViewModel? _selectedItem;

        [ObservableProperty]
        ObservableCollection<VariableViewModel> _variables = [];

        public VariablesViewModel()
        {
            if (Design.IsDesignMode)
            {
                // Make up some variables to show what it looks like
                _variables = [
                    new VariableViewModel() {
                        Name = "Sven",
                        Value = new Units.Quantity<string>("178", new Units.Unit(("cm", 1)))
                    },
                    new VariableViewModel() {
                        Name = "Elke",
                        Value = new Units.Quantity<string>("150", new Units.Unit(("cm", 1)))
                    }
                ];
            }
        }
    }
}
