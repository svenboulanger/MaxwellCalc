using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;
using Avalonia.Threading;
using MaxwellCalc.Notebook.ViewModels;
using System;
using System.ComponentModel;

namespace MaxwellCalc.Notebook.Views;

/// <summary>
/// The Saved Sheets command palette (⌘O): a centered card over a dimmed scrim listing the saved sheets
/// with live search and a save footer. Open/close and all state live on the
/// <see cref="SavedSheetsViewModel"/>; this code-behind autofocuses the search box on open, closes the
/// overlay on a scrim click, submits the save on Enter in the name field, and drives the inline-rename
/// field's autofocus and its Enter (commit) / Escape (cancel) / blur (commit) keys.
/// </summary>
public partial class SavedSheetsView : UserControl
{
    private SavedSheetsViewModel? _viewModel;

    /// <summary>Creates a new <see cref="SavedSheetsView"/>.</summary>
    public SavedSheetsView()
    {
        InitializeComponent();
    }

    private void InitializeComponent() => AvaloniaXamlLoader.Load(this);

    // Keep a PropertyChanged hook on whichever palette VM is the current DataContext, so we can focus the
    // search box each time the overlay opens.
    protected override void OnDataContextChanged(EventArgs e)
    {
        base.OnDataContextChanged(e);

        if (_viewModel is not null)
            _viewModel.PropertyChanged -= OnViewModelPropertyChanged;

        _viewModel = DataContext as SavedSheetsViewModel;

        if (_viewModel is not null)
            _viewModel.PropertyChanged += OnViewModelPropertyChanged;
    }

    private void OnViewModelPropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (e.PropertyName is nameof(SavedSheetsViewModel.PaletteOpen) && _viewModel?.PaletteOpen == true)
            FocusSearch();
    }

    // Focus + select the search box once the overlay has been realized.
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

    // Close on a click that lands on the scrim itself; clicks inside the card report the card (or a
    // descendant) as the source and are ignored.
    private void OnScrimPressed(object? sender, PointerPressedEventArgs e)
    {
        if (ReferenceEquals(e.Source, sender) && DataContext is SavedSheetsViewModel viewModel)
            viewModel.CloseCommand.Execute(null);
    }

    // Enter in the footer name field submits the primary save (which arms an overwrite the first time an
    // existing name is entered, matching the button).
    private void OnNameKeyDown(object? sender, KeyEventArgs e)
    {
        if (e.Key == Key.Enter && DataContext is SavedSheetsViewModel viewModel)
        {
            viewModel.PrimarySaveCommand.Execute(null);
            e.Handled = true;
        }
    }

    // Autofocus the inline-rename field. The row swaps its normal / rename layouts by collapsing a parent
    // Grid, which does not raise the field's own IsVisible change — so key the focus off the item's
    // IsRenaming flag instead. Subscribe per-box (capturing it) and detach when the row leaves the tree.
    private void OnRenameBoxAttached(object? sender, VisualTreeAttachmentEventArgs e)
    {
        if (sender is not TextBox box || box.DataContext is not SavedSheetItem item)
            return;

        void OnItemChanged(object? _, PropertyChangedEventArgs ev)
        {
            if (ev.PropertyName is nameof(SavedSheetItem.IsRenaming) && item.IsRenaming)
                FocusRenameBox(box);
        }

        item.PropertyChanged += OnItemChanged;
        box.DetachedFromVisualTree += (_, _) => item.PropertyChanged -= OnItemChanged;

        if (item.IsRenaming)
            FocusRenameBox(box);
    }

    private static void FocusRenameBox(TextBox box)
    {
        // Post at Loaded priority so the swap has laid the field out and it is focusable.
        Dispatcher.UIThread.Post(() =>
        {
            if (box.IsEffectivelyVisible)
            {
                box.Focus();
                box.SelectAll();
            }
        }, DispatcherPriority.Loaded);
    }

    // Enter commits the rename; Escape cancels it.
    private void OnRenameKeyDown(object? sender, KeyEventArgs e)
    {
        if (DataContext is not SavedSheetsViewModel viewModel || sender is not Control control)
            return;
        if (control.DataContext is not SavedSheetItem item)
            return;

        if (e.Key == Key.Enter)
        {
            viewModel.CommitRenameCommand.Execute(item);
            e.Handled = true;
        }
        else if (e.Key == Key.Escape)
        {
            viewModel.CancelRenameCommand.Execute(item);
            e.Handled = true;
        }
    }

    // Blur commits the rename (matching the prototype). A no-op when the row is no longer renaming (e.g.
    // the field lost focus because Enter/Escape already resolved it, or the overlay closed).
    private void OnRenameLostFocus(object? sender, RoutedEventArgs e)
    {
        if (DataContext is SavedSheetsViewModel viewModel
            && sender is Control { DataContext: SavedSheetItem item })
        {
            viewModel.CommitRenameCommand.Execute(item);
        }
    }
}
