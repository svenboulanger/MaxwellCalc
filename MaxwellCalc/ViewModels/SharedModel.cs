using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using MaxwellCalc.Core.Domains;
using MaxwellCalc.Core.Workspaces;
using System;
using System.IO;
using System.Text;
using System.Text.Json;

namespace MaxwellCalc.ViewModels
{
    /// <summary>
    /// A model that contains settings and data that should be shared between different ViewModels.
    /// </summary>
    public partial class SharedModel : ViewModelBase
    {
        [ObservableProperty]
        private WorkspaceViewModel _workspace;

        [ObservableProperty]
        private string? _workspaceFile;

        [ObservableProperty]
        private string? _settingsFile;

        /// <summary>
        /// Creates a new <see cref="SharedModel"/>.
        /// </summary>
        public SharedModel()
        {
            Workspace = new()
            {
                Key = new Workspace<double>(new DoubleDomain())
            };
        }

        /// <summary>
        /// Creates a new <see cref="SharedModel"/>.
        /// </summary>
        /// <param name="sp">The service provider.</param>
        public SharedModel(IServiceProvider sp)
        {
            WorkspaceFile = Path.Combine(Directory.GetCurrentDirectory(), "workspace.json");
            SettingsFile = Path.Combine(Directory.GetCurrentDirectory(), "settings.json");
            Workspace = new WorkspaceViewModel();
            LoadWorkspace();
        }

        /// <summary>
        /// Saves the workspace to a JSON file in the working directory.
        /// </summary>
        [RelayCommand]
        public void SaveWorkspace()
        {
            if (Workspace is null)
                return;
            if (string.IsNullOrEmpty(WorkspaceFile))
                return;
            if (File.Exists(WorkspaceFile))
                File.Delete(WorkspaceFile);

            byte[] content;
            using (var stream = new MemoryStream())
            using (var writer = new Utf8JsonWriter(stream, new JsonWriterOptions { Indented = true }))
            {
                // Workspace.WriteToJson(writer, new JsonSerializerOptions { WriteIndented = true });
                writer.Flush();
                content = stream.ToArray();
            }
            File.WriteAllBytes(WorkspaceFile, content);
        }

        [RelayCommand]
        public void LoadWorkspace()
        {   
            if (Workspace is not null && !string.IsNullOrEmpty(WorkspaceFile))
            {
                if (!File.Exists(WorkspaceFile))
                    return;
                var content = File.ReadAllText(WorkspaceFile);
                var reader = new Utf8JsonReader(Encoding.UTF8.GetBytes(content));
                reader.Read();
                // Workspace.ReadFromJson(ref reader);
            }
        }
    }
}
