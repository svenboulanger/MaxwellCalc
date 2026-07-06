using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Markup.Xaml;
using MaxwellCalc.Notebook.ViewModels;

namespace MaxwellCalc.Notebook.Views;

/// <summary>
/// The per-workspace settings dialog (Step 12): a centered card over a dimmed scrim with the workspace
/// name, scalar-format segmented control, digit stepper, live preview and unit presets. Open/close and
/// all state live on the <see cref="SettingsViewModel"/>; this code-behind only closes the dialog when
/// the scrim (not the card) is clicked.
/// </summary>
public partial class WorkspaceSettingsView : UserControl
{
    /// <summary>
    /// Creates a new <see cref="WorkspaceSettingsView"/>.
    /// </summary>
    public WorkspaceSettingsView()
    {
        InitializeComponent();
    }

    private void InitializeComponent() => AvaloniaXamlLoader.Load(this);

    // Close on a click that lands on the scrim itself; clicks inside the card report the card (or a
    // descendant) as the source and are ignored.
    private void OnScrimPressed(object? sender, PointerPressedEventArgs e)
    {
        if (ReferenceEquals(e.Source, sender) && DataContext is SettingsViewModel viewModel)
            viewModel.CloseWorkspaceSettingsCommand.Execute(null);
    }
}
