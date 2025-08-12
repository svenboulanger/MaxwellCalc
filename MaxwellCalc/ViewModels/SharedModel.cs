using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using MaxwellCalc.Domains;
using MaxwellCalc.Units;
using MaxwellCalc.Workspaces;
using System;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Numerics;
using System.Reflection;
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
        private IWorkspace? _workspace;

        [ObservableProperty]
        private DomainTypes _domainType;

        [ObservableProperty]
        private ObservableCollection<DomainTypes> _domainTypeOptions = [.. Enum.GetValues(typeof(DomainTypes)).Cast<DomainTypes>()];

        [ObservableProperty]
        private string? _workspaceFile;

        [ObservableProperty]
        private string? _settingsFile;

        [ObservableProperty]
        private string? _outputFormat;

        /// <summary>
        /// Creates a new <see cref="SharedModel"/>.
        /// </summary>
        public SharedModel()
        {
        }

        /// <summary>
        /// Creates a new <see cref="SharedModel"/>.
        /// </summary>
        /// <param name="sp">The service provider.</param>
        public SharedModel(IServiceProvider sp)
        {
            WorkspaceFile = Path.Combine(Directory.GetCurrentDirectory(), "workspace.json");
            SettingsFile = Path.Combine(Directory.GetCurrentDirectory(), "settings.json");

            LoadWorkspace();
        }

        partial void OnDomainTypeChanged(DomainTypes value)
        {
            switch (DomainType)
            {
                case DomainTypes.Double:
                    Workspace = new Workspace<double>(new DoubleDomain());
                    Workspace.RegisterBuiltInMethods(typeof(DoubleMathHelper));
                    break;

                case DomainTypes.Complex:
                    Workspace = new Workspace<Complex>(new ComplexDomain());
                    Workspace.RegisterBuiltInMethods(typeof(ComplexMathHelper));
                    break;

                default:
                    throw new NotImplementedException();
            }
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
                Workspace.WriteToJson(writer, new JsonSerializerOptions { WriteIndented = true });
                writer.Flush();
                content = stream.ToArray();
            }
            File.WriteAllBytes(WorkspaceFile, content);
        }

        [RelayCommand]
        public void ClearWorkspace()
        {
            if (Workspace is not null)
                Workspace.Clear();
        }

        [RelayCommand]
        public void LoadWorkspace()
        {   
            if (Workspace is not null && !string.IsNullOrEmpty(WorkspaceFile))
            {
                Workspace.Clear();
                var content = File.ReadAllText(WorkspaceFile);
                var reader = new Utf8JsonReader(Encoding.UTF8.GetBytes(content));
                reader.Read();
                Workspace.ReadFromJson(ref reader);
            }
        }

        [RelayCommand]
        private void AddCommonUnits()
        {
            if (Workspace is null)
                return;

            switch (DomainType)
            {
                case DomainTypes.Double:
                    UnitHelper.RegisterCommonUnits((IWorkspace<double>)Workspace);
                    break;

                case DomainTypes.Complex:
                    UnitHelper.RegisterCommonUnits((IWorkspace<Complex>)Workspace);
                    break;
            }
        }

        [RelayCommand]
        private void AddElectricalUnits()
        {
            if (Workspace is null)
                return;

            switch (DomainType)
            {
                case DomainTypes.Double:
                    UnitHelper.RegisterCommonElectronicsUnits((IWorkspace<double>)Workspace);
                    break;

                case DomainTypes.Complex:
                    UnitHelper.RegisterCommonElectronicsUnits((IWorkspace<Complex>)Workspace);
                    break;
            }
        }

        [RelayCommand]
        private void AddCommonConstants()
        {
            if (Workspace is null)
                return;

            switch (DomainType)
            {
                case DomainTypes.Double:
                    Workspace.RegisterConstants(typeof(DoubleMathHelper));
                    break;

                case DomainTypes.Complex:
                    Workspace.RegisterConstants(typeof(ComplexMathHelper));
                    break;
            }
        }
    }
}
