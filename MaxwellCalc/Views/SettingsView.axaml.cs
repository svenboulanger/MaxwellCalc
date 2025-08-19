using Avalonia.Controls;
using Avalonia.Interactivity;
using MaxwellCalc.ViewModels;

namespace MaxwellCalc.Views;

public partial class SettingsView : UserControl
{
    public SettingsView()
    {
        InitializeComponent();
    }

    private void OnEditWorkspace(object? sender, RoutedEventArgs e)
    {
        // We need to figure out the workspace that was clicked on here
        if (e.Source is not Control ctrl)
            return;
        if (ctrl.DataContext is not WorkspaceViewModel model)
            return;

        // Create a window to show
        var window = new WorkspaceView()
        {
            DataContext = model
        };
        window.Show();
    }
}