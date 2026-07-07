using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Markup.Xaml;
using Avalonia.Threading;
using MaxwellCalc.Notebook.ViewModels;

namespace MaxwellCalc.Notebook.Views;

/// <summary>
/// The "New workspace" dialog: a centered card over a dimmed scrim collecting a name and a numeric
/// domain (Double / Complex) before the workspace is created. Open/close and all state live on the
/// <see cref="SettingsViewModel"/>; this code-behind closes the dialog on a scrim click, autofocuses the
/// name field when the dialog opens, and maps Enter (create) / Esc (cancel) in the name field.
/// </summary>
public partial class NewWorkspaceView : UserControl
{
    /// <summary>
    /// Creates a new <see cref="NewWorkspaceView"/>.
    /// </summary>
    public NewWorkspaceView()
    {
        InitializeComponent();

        // Focus the name field (and select its contents) whenever the dialog becomes visible, so the user
        // can type or overwrite the default "Untitled N" name immediately.
        if (this.FindControl<Border>("Scrim") is { } scrim)
            scrim.PropertyChanged += OnScrimPropertyChanged;
    }

    private void OnScrimPropertyChanged(object? sender, AvaloniaPropertyChangedEventArgs e)
    {
        if (e.Property == IsVisibleProperty && e.NewValue is true)
            FocusNameField();
    }

    private void InitializeComponent() => AvaloniaXamlLoader.Load(this);

    private void FocusNameField()
    {
        // Defer until the field is realized/visible so Focus takes.
        Dispatcher.UIThread.Post(() =>
        {
            if (this.FindControl<TextBox>("NameField") is { } field)
            {
                field.Focus();
                field.SelectAll();
            }
        });
    }

    // Close on a click that lands on the scrim itself; clicks inside the card report the card (or a
    // descendant) as the source and are ignored.
    private void OnScrimPressed(object? sender, PointerPressedEventArgs e)
    {
        if (ReferenceEquals(e.Source, sender) && DataContext is SettingsViewModel viewModel)
            viewModel.CloseNewWorkspaceCommand.Execute(null);
    }

    // Enter commits (creates the workspace); Escape cancels.
    private void OnNameKeyDown(object? sender, KeyEventArgs e)
    {
        if (DataContext is not SettingsViewModel viewModel)
            return;
        if (e.Key == Key.Enter)
        {
            viewModel.CreateNewWorkspaceCommand.Execute(null);
            e.Handled = true;
        }
        else if (e.Key == Key.Escape)
        {
            viewModel.CloseNewWorkspaceCommand.Execute(null);
            e.Handled = true;
        }
    }
}
