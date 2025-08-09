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

namespace MaxwellCalc.ViewModels
{
    /// <summary>
    /// A class that represents the settings of the application.
    /// </summary>
    public partial class SettingsViewModel : ViewModelBase
    {
        /// <summary>
        /// Gets or sets the file where settings may be stored.
        /// </summary>
        public static string? SettingsFile { get; set; }

        /// <summary>
        /// Gets the shared model.
        /// </summary>
        public SharedModel Shared { get; }

        [ObservableProperty]
        private int _currentTheme = 0;

        [ObservableProperty]
        private ObservableCollection<PrimaryColor> _primaryColors = [.. Enum.GetValues(typeof(PrimaryColor)).Cast<PrimaryColor>()];

        [ObservableProperty]
        private ObservableCollection<SecondaryColor> _secondaryColors = [.. Enum.GetValues(typeof(SecondaryColor)).Cast<SecondaryColor>()];

        [ObservableProperty]
        private PrimaryColor _primaryColor = PrimaryColor.Purple;

        [ObservableProperty]
        private SecondaryColor _secondaryColor = SecondaryColor.Indigo;

        /// <summary>
        /// Creates a new <see cref="SettingsViewModel"/>.
        /// </summary>
        public SettingsViewModel()
        {
            Shared = new();
        }

        /// <summary>
        /// Creates a new <see cref="SettingsViewModel"/>.
        /// </summary>
        /// <param name="sp">The settings provider.</param>
        public SettingsViewModel(IServiceProvider sp)
        {
            Shared = sp.GetRequiredService<SharedModel>();
        }

        /// <summary>
        /// Updates the current application theme.
        /// </summary>
        /// <param name="value">The value.</param>
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

        /// <summary>
        /// Saves the current settings to the settings file.
        /// </summary>
        [RelayCommand]
        public void SaveSettings()
        {
            if (SettingsFile is null)
                return;
            string json = JsonSerializer.Serialize(this, new JsonSerializerOptions { WriteIndented = true });
            File.WriteAllText(SettingsFile, json, Encoding.UTF8);
        }
    }
}
