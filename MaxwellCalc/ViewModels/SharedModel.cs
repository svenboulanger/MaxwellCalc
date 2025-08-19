using CommunityToolkit.Mvvm.ComponentModel;
using MaxwellCalc.Core.Domains;
using MaxwellCalc.Core.Workspaces;
using System;

namespace MaxwellCalc.ViewModels
{
    /// <summary>
    /// A model that contains settings and data that should be shared between different ViewModels.
    /// </summary>
    public partial class SharedModel : ViewModelBase
    {
        [ObservableProperty]
        private WorkspaceViewModel? _workspace;

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
        }
    }
}
