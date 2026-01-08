using Avalonia.Controls;
using CommunityToolkit.Mvvm.ComponentModel;
using MaxwellCalc.Core.Units;

namespace MaxwellCalc.ViewModels;

public partial class ResultViewModel : ViewModelBase
{
    [ObservableProperty]
    private string? _expression;

    [ObservableProperty]
    private string? _errorMessage;

    [ObservableProperty]
    private Quantity<string> _quantity;

    public ResultViewModel()
    {
        if (Design.IsDesignMode)
        {
            _expression = "1cm^2/deg + 2m^2/deg";
            _quantity = new Quantity<string>("2.00001", new Unit(("m", 2)));
        }
    }
}
