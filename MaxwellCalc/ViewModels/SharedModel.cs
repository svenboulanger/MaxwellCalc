using CommunityToolkit.Mvvm.ComponentModel;
using MaxwellCalc.Domains;
using MaxwellCalc.Workspaces;
using Microsoft.Extensions.DependencyInjection;
using System;

namespace MaxwellCalc.ViewModels
{
    /// <summary>
    /// A model that contains settings and data that should be shared between different ViewModels.
    /// </summary>
    public partial class SharedModel : ViewModelBase
    {
        [ObservableProperty]
        private IWorkspace? _workspace;

        /// <summary>
        /// Creates a new <see cref="SharedModel"/>.
        /// </summary>
        public SharedModel()
        {
            Workspace = new Workspace<double>(new DoubleDomain());
        }

        /// <summary>
        /// Creates a new <see cref="SharedModel"/>.
        /// </summary>
        /// <param name="sp">The service provider.</param>
        public SharedModel(IServiceProvider sp)
        {
            Workspace = sp.GetRequiredService<IWorkspace>();
        }
    }
}
