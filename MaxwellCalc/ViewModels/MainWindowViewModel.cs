using Avalonia.Controls;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Material.Icons;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.ObjectModel;

namespace MaxwellCalc.ViewModels;

public enum DomainTypes
{
    Double,
    Complex
}

public partial class MainWindowViewModel : ViewModelBase
{
    [ObservableProperty]
    private string? _workspaceFilename;

    [ObservableProperty]
    private string? _errorMessage;

    [ObservableProperty]
    private bool _isPaneOpen;

    [ObservableProperty]
    private PaneMenuItemViewModel? _selectedListItem;

    [ObservableProperty]
    private ViewModelBase? _currentPage = null;

    [ObservableProperty]
    ObservableCollection<PaneMenuItemViewModel> _panes = [];

    /// <summary>
    /// Creates a new <see cref="MainWindowViewModel"/>.
    /// </summary>
    public MainWindowViewModel()
    {
        if (Design.IsDesignMode)
        {
            // Add the different panes
            _panes.Add(new PaneMenuItemViewModel()
            {
                Text = "Calculator",
                Icon = MaterialIconKind.Calculator,
                ViewModel = new CalculatorViewModel()
            });
            _panes.Add(new PaneMenuItemViewModel()
            {
                Text = "Variables",
                Icon = MaterialIconKind.Alphabetical,
                ViewModel = new VariablesViewModel()
            });
            _panes.Add(new PaneMenuItemViewModel()
            {
                Text = "Functions",
                Icon = MaterialIconKind.FunctionVariant,
                ViewModel = new FunctionsViewModel()
            });
            _panes.Add(new PaneMenuItemViewModel()
            {
                Text = "Units",
                Icon = MaterialIconKind.Atom,
                ViewModel = new UnitsViewModel()
            });
            _panes.Add(new PaneMenuItemViewModel()
            {
                Text = "Settings",
                Icon = MaterialIconKind.Cog,
                ViewModel = new SettingsViewModel()
            });

        }
    }

    /// <summary>
    /// Creates a new <see cref="MainWindowViewModel"/>.
    /// </summary>
    /// <param name="sp">The service provider.</param>
    public MainWindowViewModel(IServiceProvider sp)
    {
        // Add the different panes
        _panes.Add(new PaneMenuItemViewModel()
        {
            Text = "Calculator",
            Icon = MaterialIconKind.Calculator,
            ViewModel = sp.GetRequiredService<CalculatorViewModel>()
        });
        _panes.Add(new PaneMenuItemViewModel()
        {
            Text = "Variables",
            Icon = MaterialIconKind.Abc,
            ViewModel = sp.GetRequiredService<VariablesViewModel>()
        });
        _panes.Add(new PaneMenuItemViewModel()
        {
            Text = "Functions",
            Icon = MaterialIconKind.FunctionVariant,
            ViewModel = sp.GetRequiredService<FunctionsViewModel>()
        });
        _panes.Add(new PaneMenuItemViewModel()
        {
            Text = "Units",
            Icon = MaterialIconKind.Atom,
            ViewModel = sp.GetRequiredService<UnitsViewModel>()
        });
        _panes.Add(new PaneMenuItemViewModel()
        {
            Text = "Settings",
            Icon = MaterialIconKind.Cog,
            ViewModel = sp.GetRequiredService<SettingsViewModel>()
        });
    }

    [RelayCommand]
    private void TogglePane() => IsPaneOpen = !IsPaneOpen;

    partial void OnSelectedListItemChanged(PaneMenuItemViewModel? value)
    {
        if (value is null)
            return;
        CurrentPage = value.ViewModel;
    }

    /// <summary>
    /// Should be called when closing.
    /// </summary>
    public void Close()
    {
        foreach (var pane in Panes)
        {
            if (pane.ViewModel is SettingsViewModel settings)
                settings.SaveWorkspaces();
        }
    }
}
