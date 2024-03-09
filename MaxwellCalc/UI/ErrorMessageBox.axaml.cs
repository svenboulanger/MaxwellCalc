using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using MaxwellCalc.Workspaces;

namespace MaxwellCalc.UI;

public partial class ErrorMessageBox : Window
{
    public static readonly StyledProperty<string?> MessageProperty =
        AvaloniaProperty.Register<SettingsWindow, string?>(nameof(Message), "Error message");

    public string? Message
    {
        get => GetValue(MessageProperty);
        set => SetValue(MessageProperty, value);
    }

    public ErrorMessageBox()
    {
        InitializeComponent();
    }

    protected override void OnPropertyChanged(AvaloniaPropertyChangedEventArgs change)
    {
        base.OnPropertyChanged(change);
        if (change.Property.Name == nameof(Message))
        {
            MessageBlock.Text = Message ?? string.Empty;
        }
    }
}