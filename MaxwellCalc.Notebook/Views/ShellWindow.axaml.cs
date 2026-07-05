using Avalonia;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Styling;

namespace MaxwellCalc.Notebook.Views;

public partial class ShellWindow : Window
{
    public ShellWindow()
    {
        InitializeComponent();
    }

    // Step 2 throwaway: flip the built-in theme variant so the token preview recolors live.
    // Step 10 replaces this with the SettingsViewModel-driven theme toggle.
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
