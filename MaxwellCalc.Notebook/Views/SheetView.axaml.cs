using Avalonia.Controls;
using Avalonia.Markup.Xaml;

namespace MaxwellCalc.Notebook.Views;

/// <summary>
/// The notebook sheet view: renders the <see cref="ViewModels.SheetViewModel"/>'s lines with a plain
/// editor on the left and the result gutter on the right. The inline-highlighting editor and the
/// keyboard model are layered on in Steps 6 and 7.
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
}
