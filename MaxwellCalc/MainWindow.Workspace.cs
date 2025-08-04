using Avalonia.Interactivity;
using Avalonia.Platform.Storage;
using MaxwellCalc.Controls;
using MaxwellCalc.Workspaces;
using System;
using System.IO;
using System.Numerics;
using System.Text;
using System.Text.Json;

namespace MaxwellCalc
{
    /*
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
            // if (File.Exists(defaultWorkspace.AbsolutePath))
            //    LoadWorkspace(defaultWorkspace.AbsolutePath);

            // Update GUI
            // Functions.ViewModel.Update(_workspace);
            // Variables.ViewModel.Update(_workspace);
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

    }
    */
}
