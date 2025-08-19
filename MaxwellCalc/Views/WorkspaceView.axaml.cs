using Avalonia;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;
using CommunityToolkit.Mvvm.Input;

namespace MaxwellCalc.Views;

public partial class WorkspaceView : Window
{
    public WorkspaceView()
    {
        InitializeComponent();
    }

    private void OnClose(object? sender, RoutedEventArgs e)
        => Close();
}