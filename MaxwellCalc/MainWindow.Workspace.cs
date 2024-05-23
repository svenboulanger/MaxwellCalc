using Avalonia.Interactivity;
using Avalonia.Platform.Storage;
using MaxwellCalc.UI;
using MaxwellCalc.Workspaces;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace MaxwellCalc
{
    public partial class MainWindow
    {

        private void ResetWorkspace(object? sender, RoutedEventArgs args)
        {
            if (_workspace is null)
            {
                var dlg = new ErrorMessageBox { Message = "No active workspace" };
                dlg.ShowDialog(this);
                return;
            }

            _workspace.Clear();

            // Load the default workspace if it exists
            var defaultWorkspace = new Uri(Directory.GetCurrentDirectory());
            defaultWorkspace = new Uri(defaultWorkspace, "default.json");
            if (File.Exists(defaultWorkspace.AbsolutePath))
                LoadWorkspace(defaultWorkspace.AbsolutePath);
        }
        private void ClearWorkspace(object? sender, RoutedEventArgs args)
        {
            if (_workspace is null)
            {
                var dlg = new ErrorMessageBox { Message = "No active workspace" };
                dlg.ShowDialog(this);
                return;
            }

            _workspace.Clear();
        }

        private void LoadWorkspace(string filename)
        {
            if (_workspace is null)
                return;
            using var reader = new StreamReader(filename);
            var bytes = Encoding.UTF8.GetBytes(reader.ReadToEnd());
            var jsonReader = new Utf8JsonReader(bytes);
            if (!jsonReader.Read())
                return;

            // Clear the workspace and import JSON
            _workspace.Clear();
            _workspace.ReadFromJson(ref jsonReader);
            _workspaceFilename = filename;
        }

        private void SaveWorkspace(bool saveAs = false)
        {
            if (_workspace is null)
            {
                var dlg = new ErrorMessageBox() { Message = "No active workspace." };
                dlg.ShowDialog(this);
                return;
            }

            // Save the workspace
            if (_workspaceFilename is null)
            {
                // Automatically open a save file dialog
                var fileTask = StorageProvider.SaveFilePickerAsync(new FilePickerSaveOptions
                {
                    Title = saveAs ? "Save workspace as ..." : "Save workspace",
                    FileTypeChoices = [
                        new FilePickerFileType("Workspace JSON") {
                            Patterns = [ "*.json" ],
                        }
                    ]
                });
                fileTask.Wait();
                var file = fileTask.Result;
                if (file is null)
                    return;

                // Write to the file
                using var streamWriter = File.OpenWrite(file.Path.AbsolutePath);
                using var jsonWriter = new Utf8JsonWriter(streamWriter, new JsonWriterOptions { Indented = true });
                _workspace.WriteToJson(jsonWriter);
                _workspaceFilename = file.Path.AbsolutePath;
            }
        }
        private void SaveWorkspace(object? sender, RoutedEventArgs args)
            => SaveWorkspace();
        private void SaveWorkspaceAs(object? sender, RoutedEventArgs args)
            => SaveWorkspace(true);
        private void OpenWorkspace(object? sender, RoutedEventArgs args)
        {
            if (_workspace is null)
            {
                var dlg = new ErrorMessageBox { Message = "No active workspace" };
                dlg.ShowDialog(this);
                return;
            }

            // Open the workspace
            var fileTask = StorageProvider.OpenFilePickerAsync(new FilePickerOpenOptions
            {
                Title = "Open workspace",
                FileTypeFilter = [
                    new FilePickerFileType("Workspace JSON") {
                        Patterns = [ "*.json" ],
                    }
                ]
            });
            fileTask.Wait();
            var file = fileTask.Result;
            if (file is null || file.Count == 0)
                return;

            // Read from the json file
            LoadWorkspace(file[0].Path.AbsolutePath);
        }
    }
}
