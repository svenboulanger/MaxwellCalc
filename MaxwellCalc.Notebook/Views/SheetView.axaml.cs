using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;
using Avalonia.Threading;
using Avalonia.VisualTree;
using MaxwellCalc.Notebook.Controls;
using MaxwellCalc.Notebook.ViewModels;
using System;

namespace MaxwellCalc.Notebook.Views;

/// <summary>
/// The notebook sheet view: renders the <see cref="SheetViewModel"/>'s lines with the
/// inline-highlighting editor on the left and the result gutter on the right. It also hosts the
/// notebook keyboard model (Step 7): the editor raises Enter/Backspace/Arrow events, this view maps
/// them to <see cref="SheetViewModel"/> operations, and moves keyboard focus to the line the view
/// model selects.
/// </summary>
public partial class SheetView : UserControl
{
    private SheetViewModel? _sheet;

    /// <summary>
    /// Creates a new <see cref="SheetView"/>.
    /// </summary>
    public SheetView()
    {
        InitializeComponent();
    }

    private void InitializeComponent() => AvaloniaXamlLoader.Load(this);

    // Keep the FocusRequested subscription tied to whichever SheetViewModel is the current DataContext.
    protected override void OnDataContextChanged(EventArgs e)
    {
        base.OnDataContextChanged(e);

        if (_sheet is not null)
            _sheet.FocusRequested -= OnFocusRequested;

        _sheet = DataContext as SheetViewModel;

        if (_sheet is not null)
            _sheet.FocusRequested += OnFocusRequested;
    }

    // Track which line holds focus so the auto-caption can be shown only under the focused row. A text
    // line also enters its editable state while focused (so its raw source shows in the editor).
    private void OnEditorGotFocus(object? sender, FocusChangedEventArgs e)
    {
        if (sender is Control { DataContext: LineViewModel line })
        {
            line.IsFocused = true;
            line.IsEditing = true;
            if (DataContext is SheetViewModel sheet)
                sheet.FocusedLineIndex = sheet.Lines.IndexOf(line);
        }
    }

    // Leaving the editor collapses a text line back to its rendered form.
    private void OnEditorLostFocus(object? sender, RoutedEventArgs e)
    {
        if (sender is Control { DataContext: LineViewModel line })
        {
            line.IsFocused = false;
            line.IsEditing = false;
        }
    }

    // Clicking a rendered text line either copies an inline result (if the click landed on one — the view
    // handles the copy itself) or switches the line into raw-text editing with the caret at the source
    // column under the click (reverse-mapped from the rendered prose by the view).
    private void OnInlineTextPressed(object? sender, PointerPressedEventArgs e)
    {
        if (sender is not InlineTextView view || view.DataContext is not LineViewModel line)
            return;
        if (DataContext is not SheetViewModel sheet)
            return;

        int index = sheet.Lines.IndexOf(line);
        if (index < 0)
            return;

        // A null result means the click hit an inline value and the view already copied it; leave the line
        // idle. Otherwise open the editor at the returned caret column.
        if (view.ResolveClick(e.GetPosition(view)) is { } caret)
            sheet.BeginEdit(index, caret);
        e.Handled = true;
    }

    // ---- Keyboard model (Step 7) -------------------------------------------------------------

    private void OnEnterPressed(object? sender, EventArgs e)
    {
        if (Resolve(sender, out var sheet, out var box, out int index))
            sheet.SplitLine(index, box.CaretIndex);
    }

    private void OnMergeBackRequested(object? sender, EventArgs e)
    {
        if (Resolve(sender, out var sheet, out _, out int index))
            sheet.MergeWithPrevious(index);
    }

    private void OnMergeForwardRequested(object? sender, EventArgs e)
    {
        if (Resolve(sender, out var sheet, out _, out int index))
            sheet.MergeWithNext(index);
    }

    private void OnNavigateUpRequested(object? sender, EventArgs e)
    {
        if (Resolve(sender, out var sheet, out var box, out int index))
            sheet.NavigateUp(index, box.CaretIndex);
    }

    private void OnNavigateDownRequested(object? sender, EventArgs e)
    {
        if (Resolve(sender, out var sheet, out var box, out int index))
            sheet.NavigateDown(index, box.CaretIndex);
    }

    // Resolves the editor that raised an event to its sheet view model and line index.
    private bool Resolve(object? sender, out SheetViewModel sheet, out HighlightedExpressionBox box, out int index)
    {
        sheet = null!;
        box = null!;
        index = -1;

        if (sender is not HighlightedExpressionBox editor || editor.DataContext is not LineViewModel line)
            return false;
        if (DataContext is not SheetViewModel viewModel)
            return false;

        index = viewModel.Lines.IndexOf(line);
        if (index < 0)
            return false;

        sheet = viewModel;
        box = editor;
        return true;
    }

    // Moves keyboard focus to a line's editor once its row container exists. Insert/remove regenerate
    // containers on the next layout pass, so this is posted at Loaded priority to run after layout.
    // The host is resolved by name (not a generated field): this view's hand-written InitializeComponent
    // doesn't populate x:Name fields, so those fields stay null at runtime.
    private void OnFocusRequested(int index)
    {
        Dispatcher.UIThread.Post(() =>
        {
            if (this.FindControl<ItemsControl>("LinesHost") is not { } host)
                return;
            if (index < 0 || index >= host.ItemCount)
                return;
            if (host.ContainerFromIndex(index) is not Control container)
                return;
            container.FindDescendantOfType<HighlightedExpressionBox>()?.FocusEditor();
            // Focusing the inner TextBox scrolls only the textbox into view. Bring the whole row
            // container into view afterwards so the full line panel (padding, caption, hairline)
            // is visible, not just the editor.
            container.BringIntoView();
        }, DispatcherPriority.Loaded);
    }
}
