using Avalonia;
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

    // Drag-selection state: the row the left-button press landed on, and whether the drag has left that
    // row and become a whole-row selection.
    private int? _dragAnchorRow;
    private bool _isDragSelecting;

    /// <summary>
    /// Creates a new <see cref="SheetView"/>.
    /// </summary>
    public SheetView()
    {
        InitializeComponent();

        // Row drag-selection watches the pointer on the tunnel route (and sees handled events) so it works
        // no matter which row control — editor TextBox, rendered prose, gutter value — took the press.
        AddHandler(PointerPressedEvent, OnSheetPointerPressed, RoutingStrategies.Tunnel, handledEventsToo: true);
        AddHandler(PointerMovedEvent, OnSheetPointerMoved, RoutingStrategies.Tunnel, handledEventsToo: true);
        AddHandler(PointerReleasedEvent, OnSheetPointerReleased, RoutingStrategies.Tunnel, handledEventsToo: true);
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
            {
                sheet.ClearRowSelection();
                sheet.FocusedLineIndex = sheet.Lines.IndexOf(line);
            }
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

    // Shift+Up/Down in an editor starts (or extends) a whole-row selection. Keyboard focus then moves to the
    // sheet itself, so no caret shows and the sheet's KeyDown handles the follow-up keys.
    private void OnExtendSelectionUpRequested(object? sender, EventArgs e) => ExtendSelectionFromEditor(sender, -1);

    private void OnExtendSelectionDownRequested(object? sender, EventArgs e) => ExtendSelectionFromEditor(sender, 1);

    private void ExtendSelectionFromEditor(object? sender, int delta)
    {
        if (!Resolve(sender, out var sheet, out _, out int index))
            return;
        sheet.ExtendRowSelection(index, delta);
        Focus();
        BringRowIntoView(sheet.SelectionActive);
    }

    // ---- Row selection -----------------------------------------------------------------------

    // Keys while whole rows are selected (the sheet holds focus): Shift+Up/Down move the active end,
    // Delete/Backspace remove the rows, Escape/Enter return to editing the active row, and plain Up/Down
    // return to editing the row above/below it.
    private void OnSheetKeyDown(object? sender, KeyEventArgs e)
    {
        if (_sheet is not { HasRowSelection: true } sheet)
            return;

        switch (e.Key, e.KeyModifiers)
        {
            case (Key.Up, KeyModifiers.Shift):
                sheet.ExtendRowSelection(-1, -1);
                BringRowIntoView(sheet.SelectionActive);
                break;
            case (Key.Down, KeyModifiers.Shift):
                sheet.ExtendRowSelection(-1, 1);
                BringRowIntoView(sheet.SelectionActive);
                break;
            case (Key.Back, KeyModifiers.None):
                sheet.DeleteSelectedLines(backward: true);
                break;
            case (Key.Delete, KeyModifiers.None):
                sheet.DeleteSelectedLines(backward: false);
                break;
            case (Key.Escape or Key.Enter, KeyModifiers.None):
                sheet.CollapseRowSelection();
                break;
            case (Key.Up, KeyModifiers.None):
                sheet.CollapseRowSelection(-1);
                break;
            case (Key.Down, KeyModifiers.None):
                sheet.CollapseRowSelection(1);
                break;
            default:
                return;
        }
        e.Handled = true;
    }

    // Row under the press: the anchor of a potential drag-selection. A Shift+click on a different row
    // selects the range from the current selection anchor (or the line being edited) to that row.
    private void OnSheetPointerPressed(object? sender, PointerPressedEventArgs e)
    {
        _dragAnchorRow = null;
        _isDragSelecting = false;

        if (_sheet is not { } sheet || !e.GetCurrentPoint(this).Properties.IsLeftButtonPressed)
            return;

        int row = RowAt(e.GetPosition(this).Y);
        if (row < 0)
            return;

        if (e.KeyModifiers == KeyModifiers.Shift && (sheet.SelectionAnchor ?? sheet.FocusedLineIndex) is { } anchor && anchor != row)
        {
            sheet.SelectRows(anchor, row);
            StartDragSelection(e.Pointer, anchor);
            e.Handled = true;
            return;
        }

        sheet.ClearRowSelection();
        _dragAnchorRow = row;
    }

    // Once a drag leaves the row it started in, it turns into a whole-row selection: the pointer is taken
    // from the row's editor (ending its in-line text selection) and the range follows the pointer.
    private void OnSheetPointerMoved(object? sender, PointerEventArgs e)
    {
        if (_dragAnchorRow is not { } anchor || _sheet is not { } sheet)
            return;
        if (!e.GetCurrentPoint(this).Properties.IsLeftButtonPressed)
        {
            _dragAnchorRow = null;
            _isDragSelecting = false;
            return;
        }

        int row = RowAt(e.GetPosition(this).Y);
        if (!_isDragSelecting)
        {
            if (row == anchor)
                return;
            StartDragSelection(e.Pointer, anchor);
        }

        if (row != sheet.SelectionActive)
        {
            sheet.SelectRows(anchor, row);
            BringRowIntoView(row);
        }
        e.Handled = true;
    }

    private void OnSheetPointerReleased(object? sender, PointerReleasedEventArgs e)
    {
        _dragAnchorRow = null;
        _isDragSelecting = false;
    }

    private void StartDragSelection(IPointer pointer, int anchor)
    {
        _dragAnchorRow = anchor;
        _isDragSelecting = true;
        pointer.Capture(this);
        Focus();
    }

    // Index of the row at vertical position y (in this view's coordinates); positions above the first row
    // map to it and positions below the last row map to the last. -1 when there are no rows.
    private int RowAt(double y)
    {
        if (this.FindControl<ItemsControl>("LinesHost") is not { } host)
            return -1;

        for (int i = 0; i < host.ItemCount; i++)
        {
            if (host.ContainerFromIndex(i) is Control container
                && container.TranslatePoint(default, this) is { } topLeft
                && y < topLeft.Y + container.Bounds.Height)
                return i;
        }
        return host.ItemCount - 1;
    }

    private void BringRowIntoView(int? index)
    {
        if (index is { } i && this.FindControl<ItemsControl>("LinesHost")?.ContainerFromIndex(i) is Control container)
            container.BringIntoView();
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
