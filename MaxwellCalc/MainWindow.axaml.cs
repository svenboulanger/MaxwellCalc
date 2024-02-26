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
        public MainWindow()
        {
            InitializeComponent();
            System.Threading.Thread.CurrentThread.CurrentCulture = System.Globalization.CultureInfo.InvariantCulture;
        }

        private void Input_KeyUp(object? sender, Avalonia.Input.KeyEventArgs e)
        {
            if (e.Key == Avalonia.Input.Key.Return)
            {
                int index = WorkspacePanel.Children.Count - 1;
                string input = Input.Text ?? string.Empty;

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
        }
    }
}