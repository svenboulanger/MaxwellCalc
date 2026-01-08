using CommunityToolkit.Mvvm.ComponentModel;
using MaxwellCalc.Core.Units;
using MaxwellCalc.Core.Workspaces;

namespace MaxwellCalc.ViewModels;

/// <summary>
/// An output unit.
/// </summary>
public partial class OutputUnitViewModel : SelectableViewModelBase<OutputUnitKey>
{
    [ObservableProperty]
    private Unit _unit;

    [ObservableProperty]
    private Quantity<string> _value;
}
