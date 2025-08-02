using Avalonia.Controls;
using MaxwellCalc.ViewModels;
using MaxwellCalc.Workspaces;

namespace MaxwellCalc.Controls
{
    public partial class Variables : UserControl
    {
        /// <summary>
        /// Gets or sets the view model used to display variables.
        /// </summary>
        public VariablesViewModel ViewModel { get; } = new();

        /// <summary>
        /// Creates a new <see cref="Variables"/>.
        /// </summary>
        public Variables()
        {
            InitializeComponent();
            DataContext = ViewModel;
        }
    }
}
