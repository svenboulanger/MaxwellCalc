using Avalonia.Styling;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Material.Colors;
using Material.Styles.Themes;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Text.Json.Serialization;

namespace MaxwellCalc.ViewModels
{
    /// <summary>
    /// A class that represents the settings of the application.
    /// </summary>
    public partial class SettingsViewModel : ViewModelBase
    {
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
        private int _format = 0;

        [ObservableProperty]
        private int _digits = 12;

        /// <summary>
        /// Creates a new <see cref="SettingsViewModel"/>.
        /// </summary>
        public SettingsViewModel()
        {
            Shared = new();
            Shared.DomainType = DomainTypes.Complex;
        }

        /// <summary>
        /// Creates a new <see cref="SettingsViewModel"/>.
        /// </summary>
        /// <param name="sp">The settings provider.</param>
        public SettingsViewModel(IServiceProvider sp)
        {
            Shared = sp.GetRequiredService<SharedModel>();
            Shared.DomainType = DomainTypes.Complex;
            LoadSettings();
            Shared.LoadWorkspace();
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
        partial void OnDigitsChanged(int value) => UpdateFormatting();
        partial void OnFormatChanged(int value) => UpdateFormatting();

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

                        case nameof(Format):
                            Format = JsonSerializer.Deserialize<int>(ref reader);
                            reader.Read();
                            break;

                        case nameof(Digits):
                            Digits = JsonSerializer.Deserialize<int>(ref reader);
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

        private void UpdateFormatting()
        {
            switch (Format)
            {
                default:
                case 0: Shared.OutputFormat = $"g{Digits}"; break;
                case 1: Shared.OutputFormat = $"e{Digits}"; break;
                case 2: Shared.OutputFormat = $"f{Digits}"; break;
            }
            SaveSettings();
        }
    }
}
