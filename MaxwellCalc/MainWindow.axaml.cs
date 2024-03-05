using Avalonia.Controls;
using MaxwellCalc.Parsers;
using MaxwellCalc.Resolvers;
using MaxwellCalc.Units;
using MaxwellCalc.Workspaces;
using System;
using System.Numerics;

namespace MaxwellCalc
{
    public partial class MainWindow : Window
    {
        private int _historyFill = -1;
        private string _tmpLastInput = string.Empty;
        private IWorkspace? _workspace = null;
        private SettingsWindow? _settings = null;

        public enum DomainTypes
        {
            Double,
            Complex
        }

        public MainWindow()
        {
            InitializeComponent();
            Input.AttachedToVisualTree += (sender, args) => Input.Focus();
            System.Threading.Thread.CurrentThread.CurrentCulture = System.Globalization.CultureInfo.InvariantCulture;

            BuildWorkspace(DomainTypes.Complex);
        }

        private void BuildWorkspace(DomainTypes type)
        {
            switch (type)
            {
                case DomainTypes.Double:
                    var dws = new Workspace<double>(new DoubleResolver());
                    UnitHelper.RegisterCommonUnits(dws);
                    UnitHelper.RegisterCommonElectronicsUnits(dws);
                    DoubleMathHelper.RegisterFunctions(dws);
                    _workspace = dws;
                    break;

                case DomainTypes.Complex:
                    var cws = new Workspace<Complex>(new ComplexResolver());
                    UnitHelper.RegisterCommonUnits(cws);
                    UnitHelper.RegisterCommonElectronicsUnits(cws);
                    ComplexMathHelper.RegisterFunctions(cws);
                    _workspace = cws;
                    break;

                default:
                    throw new NotImplementedException();
            }
        }

        private void Input_KeyUp(object? sender, Avalonia.Input.KeyEventArgs e)
        {
            switch (e.Key)
            {
                case Avalonia.Input.Key.Return:
                    Resolve();
                    _tmpLastInput = string.Empty;
                    _historyFill = WorkspacePanel.Children.Count - 1;
                    break;

                case Avalonia.Input.Key.Up:
                    // Go through the history and replace the current text if it starts with the same text
                    _historyFill--;
                    FillHistory();
                    break;

                case Avalonia.Input.Key.Down:
                    _historyFill++;
                    FillHistory();
                    break;

                default:
                    // Any other general input resets the history fill index
                    _tmpLastInput = Input.Text ?? string.Empty;
                    _historyFill = WorkspacePanel.Children.Count - 1;
                    break;
            }
        }

        private void Resolve()
        {
            if (_workspace is null)
                return;

            int index = WorkspacePanel.Children.Count - 1;
            string input = Input.Text ?? string.Empty;
            if (string.IsNullOrWhiteSpace(input))
            {
                // Remove any whitespaces and avoid evaluation
                Input.Text = string.Empty;
                return;
            }

            // Lexer
            var lexer = new Lexer(input);
            var resultNode = Parser.Parse(lexer, _workspace);

            ResultBox rb;
            if (!_workspace.TryResolveAndFormat(resultNode, out var result))
            {
                rb = new ResultBox
                {
                    Input = input,
                    Output = new(_workspace.ErrorMessage ?? string.Empty, Unit.UnitNone)
                };
            }
            else
            {
                rb = new ResultBox
                {
                    Input = input,
                    Output = result
                };
            }

            // Make sure we can keep going
            WorkspacePanel.Children.Insert(index, rb);
            Input.Text = string.Empty;
            WorkspaceScroller.ScrollToEnd();
        }
        private void FillHistory()
        {
            if (_historyFill < 0)
            {
                // We reached the start of our history
                if (_historyFill < -1)
                    _historyFill = -1; // Don't allow too much back
                Input.Text = string.Empty;
                return;
            }

            if (_historyFill >= WorkspacePanel.Children.Count - 1)
            {
                // We reached the end of our history, show the last input from before
                if (_tmpLastInput is not null)
                    Input.Text = _tmpLastInput;
                return;
            }

            // Use the history to repeat
            var item = WorkspacePanel.Children[_historyFill] as ResultBox;
            if (item is not null)
            {
                Input.Text = item.Input;
                Input.CaretIndex = item.Input.Length;
            }
        }

        private void OpenUnitSettings(object? sender, Avalonia.Interactivity.RoutedEventArgs args)
        {
            if (_settings is null)
            {
                _settings = new SettingsWindow() { Workspace = _workspace };
                _settings.Closed += (sender, args) => _settings = null;
                _settings.Show();
            }
        }
    }
}