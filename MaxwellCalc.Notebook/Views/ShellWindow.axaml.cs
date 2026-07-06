using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Threading;
using Avalonia.VisualTree;
using Material.Icons;
using Material.Icons.Avalonia;
using MaxwellCalc.Notebook.ViewModels;
using System.Linq;

namespace MaxwellCalc.Notebook.Views;

public partial class ShellWindow : Window
{
    public ShellWindow()
    {
        InitializeComponent();
    }

    // Selecting a workspace or opening its settings dismisses the switcher flyout (the row's own command
    // has already run); rename and delete deliberately leave it open so the list stays visible.
    private void OnWorkspaceRowActivated(object? sender, RoutedEventArgs e)
    {
        // Defer the flyout close: the Click event is raised *before* the button's Command in
        // Button.OnClick, so hiding the flyout synchronously here would detach the button mid-click and
        // suppress its Command (Select / OpenSettings). Posting the Hide lets the command run first, then
        // dismisses the flyout on the next dispatcher pass.
        Dispatcher.UIThread.Post(() =>
        {
            if (this.FindControl<Button>("WorkspaceSwitcherButton")?.Flyout is { } flyout)
                flyout.Hide();
        });
    }

    // Commit an inline rename when the field loses focus.
    private void OnRenameLostFocus(object? sender, RoutedEventArgs e) => CommitRename(sender);

    // Enter commits the rename; Escape cancels it (both just close the editor — the name has already
    // been written through the two-way binding as the user typed).
    private void OnRenameKeyDown(object? sender, KeyEventArgs e)
    {
        if (e.Key is Key.Enter or Key.Escape)
        {
            CommitRename(sender);
            e.Handled = true;
        }
    }

    private void CommitRename(object? sender)
    {
        if (sender is Control { DataContext: WorkspaceEntry entry }
            && DataContext is ShellViewModel shell)
        {
            shell.Settings.CommitRename(entry);
        }
    }

    // The whole title bar is the window drag handle (SystemDecorations="BorderOnly" gives a native
    // resize border but no native caption, so moving/maximizing are wired explicitly). Double-click
    // toggles maximize; skip the drag when the press lands on a caption button so its click isn't
    // swallowed by the move loop.
    private void OnTitleBarPointerPressed(object? sender, PointerPressedEventArgs e)
    {
        if (!e.GetCurrentPoint(this).Properties.IsLeftButtonPressed)
            return;
        if (e.Source is Visual v && v.GetSelfAndVisualAncestors().OfType<Button>().Any())
            return;
        if (e.ClickCount == 2)
        {
            ToggleMaximize();
            return;
        }
        BeginMoveDrag(e);
    }

    // Persist the session (sheet, workspaces, preferences) as the window closes (Step 11).
    private void OnClosing(object? sender, WindowClosingEventArgs e)
    {
        if (DataContext is ViewModels.ShellViewModel shell)
            shell.Save();
    }

    private void OnMinimize(object? sender, RoutedEventArgs e) => WindowState = WindowState.Minimized;

    private void OnMaximizeRestore(object? sender, RoutedEventArgs e) => ToggleMaximize();

    private void OnClose(object? sender, RoutedEventArgs e) => Close();

    private void ToggleMaximize()
    {
        WindowState = WindowState == WindowState.Maximized
            ? WindowState.Normal
            : WindowState.Maximized;
    }

    // Swap the maximize/restore glyph to reflect the current window state.
    protected override void OnPropertyChanged(AvaloniaPropertyChangedEventArgs change)
    {
        base.OnPropertyChanged(change);
        if (change.Property == WindowStateProperty
            && this.FindControl<MaterialIcon>("MaxRestoreIcon") is { } icon)
        {
            icon.Kind = WindowState == WindowState.Maximized
                ? MaterialIconKind.WindowRestore
                : MaterialIconKind.WindowMaximize;
        }
    }
}
