using MaxwellCalc.Core.Domains;
using MaxwellCalc.Core.Parsers;
using MaxwellCalc.Core.Units;
using MaxwellCalc.Core.Workspaces;
using MaxwellCalc.Core.Workspaces.Variables;

namespace MaxwellCalc.Tests
{
    public class DifferentialDomainTests
    {
        private readonly Workspace<Differential<double>> _workspace;

        public DifferentialDomainTests()
        {
            _workspace = new Workspace<Differential<double>>(
                new DifferentialDomain<double>(
                    new DoubleDomain(),
                    DoubleMathHelper.Ln));
            _workspace.InputUnits.Add("m", new(new(1.0), Unit.UnitMeter));
            _workspace.InputUnits.Add("cm", new(new(0.01), Unit.UnitMeter));
            var scope = (IVariableScope<Differential<double>>)_workspace.Variables;
            scope.Local["a"] = new Variable<Differential<double>>(
                new(new(1.0, ("a", 1.0)), Unit.UnitMeter), null);
            scope.Local["b"] = new Variable<Differential<double>>(
                new(new(2.0, ("b", 1.0)), Unit.UnitNone), null);
        }

        [Theory]
        [MemberData(nameof(Tests))]
        public void When_ExpressionDouble_Expect_Reference(string expression, Quantity<Differential<double>> expected)
        {
            var lexer = new Lexer(expression);
            var node = Parser.Parse(lexer, _workspace);
            Assert.NotNull(node);
            Assert.True(_workspace.TryResolve(node, out var result));
            Assert.Equal(expected, result);
        }

        public static TheoryData<string, Quantity<Differential<double>>> Tests
        {
            get
            {
                var data = new TheoryData<string, Quantity<Differential<double>>>()
                {
                    { "1", new(new(1.0), Unit.UnitNone) },
                    { "a", new(new(1.0, ("a", 1.0)), Unit.UnitMeter) },
                    { "b", new(new(2.0, ("b", 1.0)), Unit.UnitNone) },
                    { "2 * a", new(new(2.0, ("a", 2.0)), Unit.UnitMeter) },
                    { "a + b * 1m", new(new(3.0, ("a", 1.0), ("b", 1.0)), Unit.UnitMeter) },
                    { "a + b * 1cm", new(new(1.02, ("a", 1.0), ("b", 0.01)), Unit.UnitMeter) },
                    { "a * b", new(new(2.0, ("a", 2.0), ("b", 1.0)), Unit.UnitMeter) }, // d(f*g) = g * df + f * dg
                    { "a / b", new(new(0.5, ("a", 0.5), ("b", -0.25)), Unit.UnitMeter) }, // d(f/g) = (g * df - f * dg) / g^2
                    { "a ^ 2", new(new(1.0, ("a", 2.0)), new Unit((Unit.Meter, 2))) }, // d(f^2) = 2 * f * df
                    { "b ^ 2", new(new(4.0, ("b", 4.0)), Unit.UnitNone) }, // d(f^2) = 2 * f * df
                    { "a ^ b", new(new(1.0, ("a", 2.0), ("b", 0.0)), new Unit((Unit.Meter, 2))) },
                };
                return data;
            }
        }
    }
}
