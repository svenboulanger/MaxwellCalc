using Avalonia.Styling;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Material.Colors;
using Material.Styles.Themes;
using MaxwellCalc.Core.Domains;
using MaxwellCalc.Core.Workspaces;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Text.Json.Serialization;
using System.Text.RegularExpressions;

namespace MaxwellCalc.ViewModels
{
    /// <summary>
    /// A class that represents the settings of the application.
    /// </summary>
    public partial class SettingsViewModel : ViewModelBase
    {
        private JsonSerializerOptions _jsonSerializerOptions;

        /// <summary>
        /// Gets the shared model.
        /// </summary>
        [JsonIgnore]
        public SharedModel Shared { get; }

        [ObservableProperty]
        private int _currentTheme = 0;

        [ObservableProperty]
        [property: JsonIgnore]
        private ObservableCollection<PrimaryColor> _primaryColors = [.. Enum.GetValues(typeof(PrimaryColor)).Cast<PrimaryColor>()];

        [ObservableProperty]
        [property: JsonIgnore]
        private ObservableCollection<SecondaryColor> _secondaryColors = [.. Enum.GetValues(typeof(SecondaryColor)).Cast<SecondaryColor>()];

        [ObservableProperty]
        private PrimaryColor _primaryColor = PrimaryColor.Purple;

        [ObservableProperty]
        private SecondaryColor _secondaryColor = SecondaryColor.Indigo;

        [ObservableProperty]
        private ObservableCollection<WorkspaceViewModel> _workspaces = [];

        [ObservableProperty]
        private string _newWorkspaceName = string.Empty;

        [ObservableProperty]
        private int _newWorkspaceType;

        /// <summary>
        /// Creates a new <see cref="SettingsViewModel"/>.
        /// </summary>
        public SettingsViewModel()
        {
            Shared = new();
            _jsonSerializerOptions = new();
            Workspaces.Add(new() { Name = "Real workspace", Key = new Workspace<double>(new DoubleDomain()), Selected = true });
            Workspaces.Add(new() { Name = "Complex workspace", Key = new Workspace<Complex>(new ComplexDomain()) });
        }

        /// <summary>
        /// Creates a new <see cref="SettingsViewModel"/>.
        /// </summary>
        /// <param name="sp">The settings provider.</param>
        public SettingsViewModel(IServiceProvider sp)
        {
            Shared = sp.GetRequiredService<SharedModel>();
            _jsonSerializerOptions = sp.GetRequiredService<JsonSerializerOptions>();
            LoadSettings();
            Shared.LoadWorkspace();

            if (Workspaces.Count == 0)
                BuildDefaultWorkspace();
        }

        partial void OnCurrentThemeChanged(int value)
        {
            switch (value)
            {
                case 0:
                    Avalonia.Application.Current!.RequestedThemeVariant = new ThemeVariant("Default", null);
                    break;

                case 1:
                    Avalonia.Application.Current!.RequestedThemeVariant = Theme.MaterialLight;
                    break;

                case 2:
                    Avalonia.Application.Current!.RequestedThemeVariant = Theme.MaterialDark;
                    break;
            }
            SaveSettings();
        }
        partial void OnPrimaryColorChanged(PrimaryColor value)
        {
            var theme = Avalonia.Application.Current!.LocateMaterialTheme<MaterialTheme>();
            theme.PrimaryColor = value;
            SaveSettings();
        }
        partial void OnSecondaryColorChanged(SecondaryColor value)
        {
            var theme = Avalonia.Application.Current!.LocateMaterialTheme<MaterialTheme>();
            theme.SecondaryColor = value;
            SaveSettings();
        }

        [RelayCommand]
        private void SelectWorkspace(WorkspaceViewModel model)
        {
            if (model is null)
                return;

            foreach (var item in Workspaces)
                item.Selected = false;
            model.Selected = true;

            // Update the shared
            Shared.Workspace = model;
        }

        [RelayCommand]
        private void AddWorkspace()
        {
            WorkspaceViewModel model = NewWorkspaceType switch
            {
                0 => new()
                {
                    Name = NewWorkspaceName,
                    Key = new Workspace<double>(new DoubleDomain()),
                    Selected = false
                },
                1 => new()
                {
                    Name = NewWorkspaceName,
                    Key = new Workspace<Complex>(new ComplexDomain()),
                    Selected = false
                },
                _ => throw new NotImplementedException(),
            };
            Workspaces.Add(model);
        }

        [RelayCommand]
        private void DuplicateWorkspace(WorkspaceViewModel model)
        {
            string name = model.Name;
            int index = 1;
            var m = DuplicatedName().Match(name);
            if (m.Success)
            {
                name = m.Groups["name"].Value;
                index = int.Parse(m.Groups["index"].Value) + 1;
            }

            // Find a name that doesn't exist yet
            while (Workspaces.Any(m => m.Name.Equals($"{name} ({index})")))
                index++;

            // Let's deserialize and reserialize to capture everything into the new workspace
            string json = model.Key switch
            {
                IWorkspace<double> dblWorkspace => JsonSerializer.Serialize(dblWorkspace, _jsonSerializerOptions),
                IWorkspace<Complex> cplxWorkspace => JsonSerializer.Serialize(cplxWorkspace, _jsonSerializerOptions),
                _ => throw new NotImplementedException()
            };
            IWorkspace newWorkspace = model.DomainType switch
            {
                DomainTypes.Double => JsonSerializer.Deserialize<IWorkspace<double>>(json, _jsonSerializerOptions) ?? throw new ArgumentException(),
                DomainTypes.Complex => JsonSerializer.Deserialize<IWorkspace<Complex>>(json, _jsonSerializerOptions) ?? throw new ArgumentException(),
                _ => throw new NotImplementedException(),
            };
            var newModel = new WorkspaceViewModel()
            {
                Key = newWorkspace,
                Name = $"{name} ({index})",
                Selected = false
            };
            Workspaces.Add(newModel);
        }

        [RelayCommand]
        private void RemoveWorkspace(WorkspaceViewModel model)
        {
            if (model is null)
                return;

            if (Workspaces.Remove(model))
            {
                if (model.Selected)
                {
                    // Let's select another workspace
                    if (Workspaces.Count == 0)
                        BuildDefaultWorkspace();
                    else
                    {
                        Workspaces[0].Selected = true;
                        Shared.Workspace = Workspaces[0];
                    }
                }
            }
        }

        private WorkspaceViewModel BuildDefaultWorkspace(bool selected = true)
        {
            var workspace = new Workspace<double>(new DoubleDomain());
            workspace.RegisterBuiltInMethods(typeof(DoubleMathHelper));
            var model = new WorkspaceViewModel()
            {
                Key = workspace,
                Name = "Workspace",
                Selected = selected
            };
            Workspaces.Add(model);
            if (selected)
                Shared.Workspace = model;
            return model;
        }

        /// <summary>
        /// Saves the current settings to the settings file.
        /// </summary>
        [RelayCommand]
        [property: JsonIgnore]
        public void SaveSettings()
        {
            if (string.IsNullOrEmpty(Shared.SettingsFile))
                return;
            string json = JsonSerializer.Serialize(this, new JsonSerializerOptions { WriteIndented = true });
            File.WriteAllText(Shared.SettingsFile, json, Encoding.UTF8);
        }

        [RelayCommand]
        [property: JsonIgnore]
        public void LoadSettings()
        {
            if (string.IsNullOrEmpty(Shared.SettingsFile))
                return;
            if (!File.Exists(Shared.SettingsFile))
                return; // Regular settings

            // Load from JSON
            string content = File.ReadAllText(Shared.SettingsFile);
            var reader = new Utf8JsonReader(Encoding.UTF8.GetBytes(content));
            reader.Read();
            if (reader.TokenType == JsonTokenType.StartObject)
            {
                reader.Read();
                while (reader.TokenType != JsonTokenType.EndObject)
                {
                    if (reader.TokenType != JsonTokenType.PropertyName)
                        return;
                    string propertyName = reader.GetString() ?? string.Empty;
                    reader.Read();

                    switch (propertyName)
                    {
                        case nameof(CurrentTheme):
                            CurrentTheme = JsonSerializer.Deserialize<int>(ref reader);
                            reader.Read();
                            break;

                        case nameof(PrimaryColor):
                            PrimaryColor = JsonSerializer.Deserialize<PrimaryColor>(ref reader);
                            reader.Read();
                            break;

                        case nameof(SecondaryColor):
                            SecondaryColor = JsonSerializer.Deserialize<SecondaryColor>(ref reader);
                            reader.Read();
                            break;

                        default:
                            JsonSerializer.Deserialize<JsonNode>(ref reader);
                            reader.Read();
                            break;
                    }
                }
            }
        }

        [GeneratedRegex(@"(?<name>.*) \((?<index>\d+)\)")]
        private static partial Regex DuplicatedName();
    }
}
