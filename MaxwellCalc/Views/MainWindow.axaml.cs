using Avalonia.Controls;
using MaxwellCalc.ViewModels;

namespace MaxwellCalc
{
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            System.Threading.Thread.CurrentThread.CurrentCulture = System.Globalization.CultureInfo.InvariantCulture;
        }

        public void OnClosing(object? sender, WindowClosingEventArgs e)
        {
            if (DataContext is MainWindowViewModel model)
                model.Close();
        }
    }
}