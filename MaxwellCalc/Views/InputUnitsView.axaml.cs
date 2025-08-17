using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using MaxwellCalc.ViewModels;
using System;
using System.Collections.Specialized;

namespace MaxwellCalc.Views;

public partial class InputUnitsView : UserControl
{
    private InputUnitsViewModel? _lastModel;

    public InputUnitsView()
    {
        InitializeComponent();
    }

    protected override void OnDataContextChanged(EventArgs e)
    {
        base.OnDataContextChanged(e);

        if (_lastModel is not null)
            _lastModel.Diagnostics.CollectionChanged -= DiagnosticsModified;
        _lastModel = (InputUnitsViewModel?)DataContext;
        if (_lastModel is not null)
            _lastModel.Diagnostics.CollectionChanged += DiagnosticsModified;
    }

    private void DiagnosticsModified(object? sender, NotifyCollectionChangedEventArgs e)
    {
        switch (e.Action)
        {
            case NotifyCollectionChangedAction.Add:
            case NotifyCollectionChangedAction.Replace:
                FlyoutBase.ShowAttachedFlyout(Input);
                break;
        }
    }
}