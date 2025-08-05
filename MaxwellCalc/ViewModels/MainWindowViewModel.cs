using Avalonia.Controls;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Material.Icons;
using Material.Icons.Avalonia;
using MaxwellCalc.Domains;
using MaxwellCalc.Units;
using MaxwellCalc.Workspaces;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.ObjectModel;
using System.IO;
using System.Numerics;
using System.Text;
using System.Text.Json;

namespace MaxwellCalc.ViewModels
{
    public enum DomainTypes
    {
        Double,
        Complex
    }

    public partial class MainWindowViewModel : ViewModelBase
    {
        [ObservableProperty]
        private IWorkspace? _workspace;

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
                Text = "Settings",
                Icon = MaterialIconKind.Cog,
                ViewModel = sp.GetRequiredService<SettingsViewModel>()
            });

            _workspace = sp.GetRequiredService<IWorkspace>();
        }

        [RelayCommand]
        private void TogglePane() => IsPaneOpen = !IsPaneOpen;

        partial void OnSelectedListItemChanged(PaneMenuItemViewModel? value)
        {
            if (value is null)
                return;
            CurrentPage = value.ViewModel;
        }

        [RelayCommand]
        private void BuildNewWorkspace(DomainTypes type)
        {
            Workspace = type switch
            {
                DomainTypes.Double => new Workspace<double>(new DoubleDomain()),
                DomainTypes.Complex => new Workspace<Complex>(new ComplexDomain()),
                _ => throw new NotImplementedException(),
            };
            var defaultWorkspace = new Uri(Directory.GetCurrentDirectory());
            defaultWorkspace = new Uri(defaultWorkspace, "workspace.json");
            if (File.Exists(defaultWorkspace.AbsolutePath))
                LoadWorkspace(defaultWorkspace.AbsolutePath);
            else
            {
                // switch (Workspace)
                // {
                //     case IWorkspace<double> dws:
                //         UnitHelper.RegisterCommonUnits(dws);
                //         UnitHelper.RegisterCommonElectronicsUnits(dws);
                //         DoubleMathHelper.RegisterFunctions(dws);
                //         DoubleMathHelper.RegisterCommonConstants(dws);
                //         DoubleMathHelper.RegisterCommonElectronicsConstants(dws);
                //         break;
                // 
                //     case IWorkspace<Complex> cws:
                //         UnitHelper.RegisterCommonUnits(cws);
                //         UnitHelper.RegisterCommonElectronicsUnits(cws);
                //         ComplexMathHelper.RegisterFunctions(cws);
                //         ComplexMathHelper.RegisterCommonConstants(cws);
                //         ComplexMathHelper.RegisterCommonElectronicsConstants(cws);
                //         break;
                // }
            }
        }

        [RelayCommand]
        private void ClearWorkspace()
        {
            if (Workspace is null)
            {
                ErrorMessage = "No active workspace";
                return;
            }
            Workspace.Clear();
        }

        [RelayCommand]
        private void LoadWorkspace(string filename)
        {
            if (Workspace is null)
                return;
            using var reader = new StreamReader(filename);
            var bytes = Encoding.UTF8.GetBytes(reader.ReadToEnd());
            var jsonReader = new Utf8JsonReader(bytes);
            if (!jsonReader.Read())
                return;

            // Clear the workspace and import JSON
            Workspace.Clear();
            Workspace.ReadFromJson(ref jsonReader);
            WorkspaceFilename = filename;

            // Register built-in functions
            // if (Workspace is IWorkspace<double> dws)
            //     DoubleMathHelper.RegisterFunctions(dws);
            // else if (Workspace is IWorkspace<Complex> cws)
            //     ComplexMathHelper.RegisterFunctions(cws);

            // Make sure the rest is invalidated
            OnPropertyChanged(nameof(Workspace));
        }

        [RelayCommand]
        private void SaveWorkspace(string filename)
        {
            if (Workspace is null)
            {
                ErrorMessage = "No active workspace.";
                return;
            }

            // Write to the file
            using var streamWriter = File.OpenWrite(filename);
            using var jsonWriter = new Utf8JsonWriter(streamWriter, new JsonWriterOptions { Indented = true });
            Workspace.WriteToJson(jsonWriter);
        }

        [RelayCommand]
        private void OpenWorkspace(string filename)
        {
            if (Workspace is null)
            {
                ErrorMessage = "No active workspace";
                return;
            }

            // Read from the json file
            LoadWorkspace(filename);
        }
    }
}
