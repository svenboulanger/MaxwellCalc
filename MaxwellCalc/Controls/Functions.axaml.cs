using Avalonia.Controls;
using MaxwellCalc.ViewModels;

namespace MaxwellCalc.Controls
{
    public partial class Functions : UserControl
    {
        /// <summary>
        /// Gets the view model for displaying the function list.
        /// </summary>
        public FunctionsViewModel ViewModel { get; } = new();

        public Functions()
        {
            InitializeComponent();
            DataContext = ViewModel;
        }
    }
}
