using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;
using MaxwellCalc.Notebook.ViewModels;

namespace MaxwellCalc.Notebook.Views;

/// <summary>
/// The notebook sheet view: renders the <see cref="SheetViewModel"/>'s lines with the
/// inline-highlighting editor on the left and the result gutter on the right. The
/// Enter/Backspace/Arrow keyboard model is layered on in Step 7.
/// </summary>
public partial class SheetView : UserControl
{
    /// <summary>
    /// Creates a new <see cref="SheetView"/>.
    /// </summary>
    public SheetView()
    {
        InitializeComponent();
    }

    private void InitializeComponent() => AvaloniaXamlLoader.Load(this);

    // Track which line holds focus so the auto-caption can be shown only under the focused row.
    private void OnEditorGotFocus(object? sender, FocusChangedEventArgs e)
    {
        if (sender is Control { DataContext: LineViewModel line })
        {
            line.IsFocused = true;
            if (DataContext is SheetViewModel sheet)
                sheet.FocusedLineIndex = sheet.Lines.IndexOf(line);
        }
    }

    private void OnEditorLostFocus(object? sender, RoutedEventArgs e)
    {
        if (sender is Control { DataContext: LineViewModel line })
            line.IsFocused = false;
    }
}
