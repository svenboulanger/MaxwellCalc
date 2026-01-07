using MaxwellCalc.Core.Domains;
using MaxwellCalc.Core.Parsers;
using MaxwellCalc.Core.Units;
using MaxwellCalc.Core.Workspaces;

namespace MaxwellCalc.Tests
{
    public class ParserTests
    {
        [Theory]
        [MemberData(nameof(Expressions))]
        public void When_Expression_Expect_Reference(string expression, Quantity<double> expected)
        {
            var dws = new Workspace<double>(new DoubleDomain());
            dws.RegisterCommonUnits();

            // Parse
            var lexer = new Lexer(expression);
            var node = Parser.Parse(lexer, dws);
            Assert.NotNull(node);

            Assert.True(dws.TryResolve(node, out var result));
            Assert.Equal(expected, result);
        }

        public static TheoryData<string, Quantity<double>> Expressions
        {
            get
            {
                return new TheoryData<string, Quantity<double>>()
                {
                    { "1 + 2", new(3.0, Unit.UnitNone) },
                    { "1 * 2", new(2.0, Unit.UnitNone) },
                    { "1 / 2", new(0.5, Unit.UnitNone) },
                    { "1 % 2", new(1.0, Unit.UnitNone) },
                    { "1 << 2", new(4.0, Unit.UnitNone) },
                    { "1 >> 2", new(0.0, Unit.UnitNone) },
                    { "1 == 2", new(0.0, Unit.UnitNone) },
                    { "1 != 2", new(1.0, Unit.UnitNone) },
                    { "1 < 2", new(1.0, Unit.UnitNone) },
                    { "1 > 2", new(0.0, Unit.UnitNone) },
                    { "1 >= 2", new(0.0, Unit.UnitNone) },
                    { "1 <= 2", new(1.0, Unit.UnitNone) },
                    { "1cm", new(1.0, new Unit(("cm", 1))) },
                    { "'(1cm)", new(0.01, Unit.UnitNone) },
                    { "1m / 1kg", new(1.0, new Unit((Unit.Kilogram, -1), (Unit.Meter, 1))) }, // Check implicit
                    { "1m / 0.5 kg^2 A", new(2.0, new Unit((Unit.Meter, 1), (Unit.Kilogram, -2), (Unit.Ampere, -1))) }
                };
            }
        }
    }
}
