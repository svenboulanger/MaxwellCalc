using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;
using Avalonia.Threading;
using MaxwellCalc.Notebook.ViewModels;
using MaxwellCalc.Notebook.ViewModels.Overlay;
using System;
using System.ComponentModel;

namespace MaxwellCalc.Notebook.Views;

/// <summary>
/// The command-palette overlay (Step 8): a centered card over a dimmed scrim listing the workspace's
/// variables, units and functions with live search. Open/close and section state live on the
/// <see cref="CommandPaletteViewModel"/>; this code-behind only autofocuses the search box on open and
/// closes the overlay when the scrim (not the card) is clicked.
/// </summary>
public partial class CommandPaletteView : UserControl
{
    private CommandPaletteViewModel? _viewModel;

    /// <summary>
    /// Creates a new <see cref="CommandPaletteView"/>.
    /// </summary>
    public CommandPaletteView()
    {
        InitializeComponent();
    }

    private void InitializeComponent() => AvaloniaXamlLoader.Load(this);

    // Keep a PropertyChanged hook on whichever palette VM is the current DataContext, so we can focus
    // the search box each time the overlay opens.
    protected override void OnDataContextChanged(EventArgs e)
    {
        base.OnDataContextChanged(e);

        if (_viewModel is not null)
        {
            _viewModel.PropertyChanged -= OnViewModelPropertyChanged;
            _viewModel.OutputUnits.RevealRowRequested -= OnOutputRowRevealRequested;
        }

        _viewModel = DataContext as CommandPaletteViewModel;

        if (_viewModel is not null)
        {
            _viewModel.PropertyChanged += OnViewModelPropertyChanged;
            _viewModel.OutputUnits.RevealRowRequested += OnOutputRowRevealRequested;
        }
    }

    // After a category rename re-sorts the output list, scroll the renamed group back into view. Posted at
    // Loaded priority so the reconciled Rows have been laid out before ScrollIntoView measures them.
    private void OnOutputRowRevealRequested(int index)
    {
        Dispatcher.UIThread.Post(() =>
        {
            if (this.FindControl<ItemsControl>("OutputUnitsList") is { } list && index >= 0 && index < list.ItemCount)
                list.ScrollIntoView(index);
        }, DispatcherPriority.Loaded);
    }

    private void OnViewModelPropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (e.PropertyName is nameof(CommandPaletteViewModel.PaletteOpen) && _viewModel?.PaletteOpen == true)
            FocusSearch();
    }

    // Focus + select the search box once the overlay has been realized (posted at Loaded priority so it
    // runs after the IsVisible change lays the card out).
    private void FocusSearch()
    {
        Dispatcher.UIThread.Post(() =>
        {
            if (this.FindControl<TextBox>("PART_Search") is { } search)
            {
                search.Focus();
                search.SelectAll();
            }
        }, DispatcherPriority.Loaded);
    }

    // Autofocus the inline rename field when a category header enters edit mode. Because IsEditing is
    // part of OutputHeaderRow's value identity, the reconciler realizes a fresh container (and TextBox)
    // when editing begins, so this Loaded fires exactly then. Focus + select the current name.
    private void OnRenameFieldLoaded(object? sender, RoutedEventArgs e)
    {
        if (sender is TextBox tb && tb.DataContext is OutputHeaderRow { IsEditing: true })
        {
            tb.Focus();
            tb.SelectAll();
        }
    }

    // Close on a click that lands on the scrim itself; clicks inside the card report the card (or a
    // descendant) as the source and are ignored.
    private void OnScrimPressed(object? sender, PointerPressedEventArgs e)
    {
        if (ReferenceEquals(e.Source, sender) && DataContext is CommandPaletteViewModel viewModel)
            viewModel.CloseCommand.Execute(null);
    }
}
