using CommunityToolkit.Mvvm.ComponentModel;

namespace MaxwellCalc.ViewModels;

/// <summary>
/// A view model base that can be selected.
/// </summary>
/// <param name="key">The key that can be used to link to the original dictionary.</param>
public abstract partial class SelectableViewModelBase<TKey> : ViewModelBase
{
    /// <summary>
    /// Gets the key for the item.
    /// </summary>
    public TKey? Key { get; init; }

    [ObservableProperty]
    private bool _selected = false;

    [ObservableProperty]
    private bool _visible = true;
}
