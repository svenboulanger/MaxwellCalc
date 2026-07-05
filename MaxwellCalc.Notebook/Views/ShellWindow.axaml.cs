using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Styling;
using Avalonia.VisualTree;
using System.Linq;

namespace MaxwellCalc.Notebook.Views;

public partial class ShellWindow : Window
{
    public ShellWindow()
    {
        InitializeComponent();
    }

    // The whole title bar is the window drag handle (the client area is extended over the
    // decorations, so dragging must be wired explicitly). Skip when the press lands on an
    // interactive child (e.g. the theme toggle) so its click isn't swallowed by the move loop.
    private void OnTitleBarPointerPressed(object? sender, PointerPressedEventArgs e)
    {
        if (!e.GetCurrentPoint(this).Properties.IsLeftButtonPressed)
            return;
        if (e.Source is Visual v && v.GetSelfAndVisualAncestors().OfType<Button>().Any())
            return;
        BeginMoveDrag(e);
    }

    // Placeholder theme toggle for the static shell. Step 10 replaces this with the
    // SettingsViewModel-driven, persisted toggle.
    private void OnToggleTheme(object? sender, RoutedEventArgs e)
    {
        if (Application.Current is { } app)
        {
            var current = app.ActualThemeVariant;
            app.RequestedThemeVariant = current == ThemeVariant.Dark
                ? ThemeVariant.Light
                : ThemeVariant.Dark;
        }
    }
}
