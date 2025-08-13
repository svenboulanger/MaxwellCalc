using MaxwellCalc.Domains;
using MaxwellCalc.Parsers;
using MaxwellCalc.Parsers.Nodes;
using MaxwellCalc.Units;
using MaxwellCalc.Workspaces;

namespace MaxwellCalc.Tests
{
    public class ParserTests
    {
        [Theory]
        [MemberData(nameof(Expression))]
        public void When_Expression_Expect_Reference(string expression)
        {
            var dws = new Workspace<double>(new DoubleDomain());
            dws.InputUnits.Add("cm", new(
                new BinaryNode(
                    BinaryOperatorTypes.Divide,
                    new ScalarNode("1".AsMemory()),
                    new ScalarNode("100".AsMemory()), "1/100".AsMemory()), Unit.UnitMeter));

            var lexer = new Lexer(expression);
            var node = Parser.Parse(lexer, dws);
            Assert.NotNull(node);
        }

        public static TheoryData<string> Expression
        {
            get
            {
                return new TheoryData<string>()
                {
                    { "1 + 2" },
                    { "1 * 2" },
                    { "1 / 2" },
                    { "1 % 2" },
                    { "1 << 2" },
                    { "1 >> 2" },
                    { "1 == 2" },
                    { "1 != 2" },
                    { "1 < 2" },
                    { "1 > 2" },
                    { "1 >= 2" },
                    { "1 <= 2" },
                    { "1cm" },
                    { "2a" },
                };
            }
        }
    }
}
