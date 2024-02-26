using Avalonia.Controls;
using MaxwellCalc.Parsers;
using MaxwellCalc.Parsers.Nodes;
using MaxwellCalc.Resolvers;
using MaxwellCalc.Units;
using MaxwellCalc.Workspaces;

namespace MaxwellCalc
{
    public partial class MainWindow : Window
    {
        private int _historyFill = -1;
        private string _tmpLastInput = string.Empty;

        public MainWindow()
        {
            InitializeComponent();
            System.Threading.Thread.CurrentThread.CurrentCulture = System.Globalization.CultureInfo.InvariantCulture;
        }

        private void Input_KeyUp(object? sender, Avalonia.Input.KeyEventArgs e)
        {
            switch (e.Key)
            {
                case Avalonia.Input.Key.Return:
                    _historyFill = WorkspacePanel.Children.Count - 1;
                    _tmpLastInput = string.Empty;
                    Resolve();
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
            var workspace = new Workspace();
            UnitHelper.RegisterSIUnits(workspace);
            UnitHelper.RegisterShortSIUnits(workspace);
            UnitHelper.RegisterCommonElectricalUnits(workspace);
            RealMathHelper.RegisterFunctions(workspace);
            var resultNode = Parser.Parse(lexer, workspace);
            var resolver = new RealResolver();
            ResultBox rb;

            if (resultNode is BinaryNode bn && bn.Type == BinaryOperatorTypes.InUnit)
            {
                if (!bn.Right.TryResolve(resolver, workspace, out var unit) ||
                    !bn.Left.TryResolve(resolver, workspace, out var value))
                {
                    rb = new ResultBox()
                    {
                        Input = input,
                        Output = "Failed"
                    };
                }
                else if (unit.Unit != value.Unit)
                {
                    rb = new ResultBox()
                    {
                        Input = input,
                        Output = "Failed"
                    };
                }
                else
                {
                    var result = new Quantity<double>(
                        value.Scalar / unit.Scalar,
                        new((bn.Right.Content.ToString(), 1)));
                    rb = new ResultBox()
                    {
                        Input = input,
                        Output = result
                    };
                }
            }
            else if (!resultNode.TryResolve(resolver, workspace, out var result))
            {
                rb = new ResultBox()
                {
                    Input = input,
                    Output = "Failed"
                };
            }
            else
            {
                workspace.TryResolveNaming(result, out result);
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