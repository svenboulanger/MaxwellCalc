using Avalonia.Controls;
using MaxwellCalc.Parsers;
using MaxwellCalc.Parsers.Nodes;
using MaxwellCalc.Resolvers;
using MaxwellCalc.Units;
using MaxwellCalc.Workspaces;
using System.Numerics;

namespace MaxwellCalc
{
    public partial class MainWindow : Window
    {
        private int _historyFill = -1;
        private string _tmpLastInput = string.Empty;
        private readonly Workspace _workspace = new();

        public MainWindow()
        {
            InitializeComponent();
            System.Threading.Thread.CurrentThread.CurrentCulture = System.Globalization.CultureInfo.InvariantCulture;

            _workspace = new Workspace();
            UnitHelper.RegisterSIUnits(_workspace);
            UnitHelper.RegisterCommonUnits(_workspace);
            UnitHelper.RegisterCommonElectronicsUnits(_workspace);

            RealHelper.RegisterCommonConstants(_workspace);
            RealHelper.RegisterCommonElectronicsConstants(_workspace);

            RealMathHelper.RegisterFunctions(_workspace);
            ComplexMathHelper.RegisterFunctions(_workspace);
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
            var resolver = new ComplexResolver();
            ResultBox rb;

            if (resultNode is BinaryNode bn && bn.Type == BinaryOperatorTypes.InUnit)
            {
                if (!bn.Right.TryResolve(resolver, _workspace, out var unit) ||
                    !bn.Left.TryResolve(resolver, _workspace, out var value))
                {
                    rb = new ResultBox()
                    {
                        Input = input,
                        Output = resolver.Error
                    };
                }
                else if (unit.Unit != value.Unit)
                {
                    rb = new ResultBox()
                    {
                        Input = input,
                        Output = "Cannot convert units as units don't match."
                    };
                }
                else
                {
                    var result = new Quantity<Complex>(
                        value.Scalar / unit.Scalar,
                        new((bn.Right.Content.ToString(), 1)));
                    rb = new ResultBox()
                    {
                        Input = input,
                        Output = result
                    };
                }
            }
            else if (!resultNode.TryResolve(resolver, _workspace, out var result))
            {
                rb = new ResultBox()
                {
                    Input = input,
                    Output = resolver.Error
                };
            }
            else
            {
                ((IWorkspace<Complex>)_workspace).TryResolveNaming(result, out result);
                rb = new ResultBox()
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
                Input.Text = item.Input;
        }
    }
}