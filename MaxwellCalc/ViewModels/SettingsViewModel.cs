using Avalonia.Styling;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System;
using System.IO;
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

        [ObservableProperty]
        private int _currentTheme = 0;

        /// <summary>
        /// Creates a new <see cref="SettingsViewModel"/>.
        /// </summary>
        public SettingsViewModel()
        {

        }

        /// <summary>
        /// Creates a new <see cref="SettingsViewModel"/>.
        /// </summary>
        /// <param name="sp">The settings provider.</param>
        public SettingsViewModel(IServiceProvider sp)
        {
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
                    App.Current!.RequestedThemeVariant = new ThemeVariant("Default", null);
                    break;

                case 1:
                    App.Current!.RequestedThemeVariant = Material.Styles.Themes.Theme.MaterialLight;
                    break;

                case 2:
                    App.Current!.RequestedThemeVariant = Material.Styles.Themes.Theme.MaterialDark;
                    break;
            }
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
