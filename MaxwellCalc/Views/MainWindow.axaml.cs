using Avalonia.Controls;
using Avalonia.Interactivity;
using MaxwellCalc.Domains;
using MaxwellCalc.Parsers;
using MaxwellCalc.Controls;
using MaxwellCalc.Units;
using MaxwellCalc.Workspaces;
using System;
using System.IO;
using System.Numerics;

namespace MaxwellCalc
{
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            System.Threading.Thread.CurrentThread.CurrentCulture = System.Globalization.CultureInfo.InvariantCulture;
        }
    }
}