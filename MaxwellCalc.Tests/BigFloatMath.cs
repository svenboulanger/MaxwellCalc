using MaxwellCalc.Domains;
using System.Globalization;
using System.Numerics;

namespace MaxwellCalc.Tests
{
    public class BigFloatMath
    {
        [Test]
        [TestCaseSource(nameof(Addition))]
        public void When_AddBigFloats_Expect_Reference(BigFloat a, BigFloat b, BigFloat expected)
        {
            Assert.That(a + b, Is.EqualTo(expected));
        }

        private static IEnumerable<TestCaseData> Addition()
        {
            yield return new TestCaseData(new BigFloat(1, 0), new BigFloat(1, 0), new BigFloat(2, 0));
            yield return new TestCaseData(new BigFloat(-1, 0), new BigFloat(-1, 0), new BigFloat(-2, 0));
            yield return new TestCaseData(new BigFloat(2, 0), new BigFloat(1, 0), new BigFloat(3, 0));
        }

        [Test]
        [TestCaseSource(nameof(Subtraction))]
        public void When_SubtractBigFloats_Expect_Reference(BigFloat a, BigFloat b, BigFloat expected)
        {
            Assert.That(a - b, Is.EqualTo(expected));
        }

        private static IEnumerable<TestCaseData> Subtraction()
        {
            yield return new TestCaseData(new BigFloat(1, 0), new BigFloat(1, 0), new BigFloat(0, 0));
            yield return new TestCaseData(new BigFloat(2, 0), new BigFloat(1, 0), new BigFloat(1, 0));
        }

        [Test]
        [TestCaseSource(nameof(Multiplication))]
        public void When_MultiplyBigFloats_Expect_Reference(BigFloat a, BigFloat b, BigFloat expected)
        {
            Assert.That(a * b, Is.EqualTo(expected));
        }

        private static IEnumerable<TestCaseData> Multiplication()
        {
            yield return new TestCaseData(new BigFloat(1, 0), new BigFloat(1, 0), new BigFloat(1, 0));
            yield return new TestCaseData(new BigFloat(2, 0), new BigFloat(1, 0), new BigFloat(2, 0));
        }

        [Test]
        [TestCaseSource(nameof(Division))]
        public void When_DivideBigFloats_Expect_Reference(BigFloat a, BigFloat b, int bitsPrecision, BigFloat expected)
        {
            var actual = BigFloat.Divide(a, b, bitsPrecision);
            Assert.That(actual, Is.EqualTo(expected));
        }

        private static IEnumerable<TestCaseData> Division()
        {
            yield return new TestCaseData(new BigFloat(1, 0), new BigFloat(2, 0), 128, new BigFloat(1, -1)) {  TestName = "{m}(1, 2, 0.5)" };
            yield return new TestCaseData(new BigFloat(1, 0), new BigFloat(10, 0), 128, new BigFloat(BigInteger.Parse("272225893536750770770699685945414569165"), -131)) { TestName = "{m}(1, 10, 0.1)" };
        }

        [Test]
        [TestCaseSource(nameof(RelativeComparison))]
        public void When_RelativeComparisonBigFloats_Expect_Reference(BigFloat a, BigFloat b, bool greaterThan, bool equal)
        {
            if (equal)
            {
                Assert.That(a > b, Is.False);
                Assert.That(a >= b, Is.True);
                Assert.That(a < b, Is.False);
                Assert.That(a <= b, Is.True);
                Assert.That(a == b, Is.True);
                Assert.That(a != b, Is.False);
            }
            else
            {
                Assert.That(a > b, Is.EqualTo(greaterThan));
                Assert.That(a >= b, Is.EqualTo(greaterThan));
                Assert.That(a < b, Is.EqualTo(!greaterThan));
                Assert.That(a <= b, Is.EqualTo(!greaterThan));
                Assert.That(a == b, Is.False);
                Assert.That(a != b, Is.True);
            }
        }

        private static IEnumerable<TestCaseData> RelativeComparison()
        {
            // Equalities
            yield return new TestCaseData(new BigFloat(0, 0), new BigFloat(0, 0), true, true);
            yield return new TestCaseData(new BigFloat(1, 0), new BigFloat(1, 0), true, true);
            yield return new TestCaseData(new BigFloat(2, 0), new BigFloat(1, 1), true, true);
            yield return new TestCaseData(new BigFloat(1, -500), new BigFloat(1, -500), true, true);
            yield return new TestCaseData(new BigFloat(6, 500), new BigFloat(6, 500), true, true);
            yield return new TestCaseData(new BigFloat((BigInteger)(Math.PI * 1e12), 0), new BigFloat((BigInteger)(Math.PI * 1e12), 0), true, true);

            // Inequalities
            yield return new TestCaseData(new BigFloat(1, 0), new BigFloat(0, 0), true, false);
            yield return new TestCaseData(new BigFloat(0, 0), new BigFloat(1, 0), false, false);
        }

        [Test]
        [TestCaseSource(nameof(BigFloatGeneralToString))]
        public void When_BigFloatGeneralToString_Expect_Reference(BigFloat input, int precision, NumberFormatInfo format, string expectedOutput)
        {
            string actual = BigFloat.FormatGeneral(input, precision, format, "E");
            Assert.That(actual, Is.EqualTo(expectedOutput));
        }

        private static IEnumerable<TestCaseData> BigFloatGeneralToString()
        {
            var format = new NumberFormatInfo()
            {
                NegativeSign = "-",
                PositiveSign = "+",
                NumberDecimalSeparator = "."
            };

            // Full precision
            yield return new TestCaseData(new BigFloat(0, 0), 0, format, "0") { TestName = "{m}(0 [0])" };
            yield return new TestCaseData(new BigFloat(1, 0), 0, format, "1") { TestName = "{m}(1 [1])" };
            yield return new TestCaseData(new BigFloat(1, -2), 0, format, "0.25") { TestName = "{m}(0.25 [0.25])" };
            yield return new TestCaseData(new BigFloat(-3, -5), 0, format, "-0.09375") { TestName = "{m}(-0.09375 [-0.09375])" };
            yield return new TestCaseData(new BigFloat(3, -1), 0, format, "1.5") { TestName = "{m}(1.5 [1.5])" };
            yield return new TestCaseData(new BigFloat(45, -2), 0, format, "11.25") { TestName = "{m}(11.25 [11.25])" };

            // Limited precision - testing rounding
            yield return new TestCaseData(new BigFloat(1, -2), 1, format, "0.3") { TestName = "{m}(0.3 [0.25])" };
            yield return new TestCaseData(new BigFloat(16, 0), 1, format, "2E+01") { TestName = "{m}(2E+01 [16])" };
            yield return new TestCaseData(new BigFloat(99, 0), 1, format, "1E+02") { TestName = "{m}(1E+02 [99])" };
            yield return new TestCaseData(new BigFloat(95, 0), 1, format, "1E+02") { TestName = "{m}(1E+02 [95])" };
            yield return new TestCaseData(new BigFloat(-949, 0), 1, format, "-9E+02") { TestName = "{m}(-9E+02 [-949])" };
            yield return new TestCaseData(new BigFloat(951, 0), 1, format, "1E+03") { TestName = "{m}(1E+03 [951])" };
            yield return new TestCaseData(new BigFloat(9499999999L, 0), 1, format, "9E+09") { TestName = "{m}(9E+09 [9499999999])" };
            yield return new TestCaseData(new BigFloat(9449999999L, 0), 2, format, "9.4E+09") { TestName = "{m}(9.4E+09 [9449999999])" };
            yield return new TestCaseData(new BigFloat(9450000000L, 0), 2, format, "9.5E+09") { TestName = "{m}(9.5E+09 [9450000000])" };
            yield return new TestCaseData(new BigFloat(1, -3), 1, format, "0.1") { TestName = "{m}(0.1 [0.125])" };
            yield return new TestCaseData(new BigFloat(1, -3), 2, format, "0.13") { TestName = "{m}(0.13 [0.125])" };
            yield return new TestCaseData(new BigFloat(1, -3), 3, format, "0.125") { TestName = "{m}(0.125 [0.125])" };
            yield return new TestCaseData(new BigFloat(1, -3), 4, format, "0.125") { TestName = "{m}(0.125 [0.125 _4])" };
            yield return new TestCaseData(new BigFloat(1, -4), 1, format, "0.06") { TestName = "{m}(0.06 [0.0625])" };
            yield return new TestCaseData(new BigFloat(1, -4), 2, format, "0.063") { TestName = "{m}(0.063 [0.0625])" };
            yield return new TestCaseData(new BigFloat(1, -4), 3, format, "0.0625") { TestName = "{m}(0.0625 [0.0625])" };
            yield return new TestCaseData(new BigFloat(1, -4), 4, format, "0.0625") { TestName = "{m}(0.0625 [0.0625 _4])" };
        }

        [Test]
        [TestCaseSource(nameof(Sqrt))]
        public void When_Sqrt_Expect_Reference(BigFloat input, int bitsPrecision, int decPrecision, string expected)
        {
            var output = Domains.BigFloatMath.Sqrt(input, bitsPrecision);
            Assert.That(BigFloat.FormatGeneral(output, decPrecision, CultureInfo.InvariantCulture.NumberFormat, "e"),
                Is.EqualTo(expected));
        }

        private static IEnumerable<TestCaseData> Sqrt()
        {
            yield return new TestCaseData(new BigFloat(1, 0), 16, 5, "1");
            yield return new TestCaseData(new BigFloat(1, 2), 16, 5, "2");
            yield return new TestCaseData(new BigFloat(1, 1), 80, 21, "1.4142135623730950488");
        }

        [Test]
        [TestCaseSource(nameof(Pi))]
        public void When_Pi_Expect_Reference(int bitsPrecision, int decPrecision, string expected)
        {
            var output = Domains.BigFloatMath.Pi(bitsPrecision);
            Assert.That(BigFloat.FormatGeneral(output, decPrecision, CultureInfo.InvariantCulture.NumberFormat, "e"),
                Is.EqualTo(expected));
        }

        private static IEnumerable<TestCaseData> Pi()
        {
            yield return new TestCaseData(16, 5, "3.1416");
            yield return new TestCaseData(80, 21, "3.14159265358979323846");
        }

        [Test]
        [TestCaseSource(nameof(Cos))]
        public void When_Cos_Expect_Reference(BigFloat input, BigFloat pi, int bitsPrecision, int decPrecision, string expected)
        {
            var output = Domains.BigFloatMath.Cos(input, pi, bitsPrecision);
            Assert.That(BigFloat.FormatGeneral(output, decPrecision, CultureInfo.InvariantCulture.NumberFormat, "e"),
                Is.EqualTo(expected));
        }

        private static IEnumerable<TestCaseData> Cos()
        {
            var pi = Domains.BigFloatMath.Pi(128);

            yield return new TestCaseData(new BigFloat(0, 0), pi, 20, 5, "1");
            yield return new TestCaseData(BigFloat.Divide(pi, 3, 128), pi, 20, 5, "0.5");
            yield return new TestCaseData(BigFloat.Divide(pi, 2, 128), pi, 20, 5, "0");
            yield return new TestCaseData(BigFloat.Divide(pi * 2, 3, 128), pi, 20, 5, "-0.5");
            yield return new TestCaseData(pi, pi, 20, 5, "-1");
            yield return new TestCaseData(-pi, pi, 20, 5, "-1");
            yield return new TestCaseData(BigFloat.Divide(-pi * 2, 3, 128), pi, 20, 5, "-0.5");
            yield return new TestCaseData(BigFloat.Divide(-pi, 2, 128), pi, 20, 5, "0");
            yield return new TestCaseData(BigFloat.Divide(-pi, 3, 128), pi, 20, 5, "0.5");
        }

        [Test]
        [TestCaseSource(nameof(Sin))]
        public void When_Sin_Expect_Reference(BigFloat input, BigFloat pi, int bitsPrecision, int decPrecision, string expected)
        {
            var output = Domains.BigFloatMath.Sin(input, pi, bitsPrecision);
            Assert.That(BigFloat.FormatGeneral(output, decPrecision, CultureInfo.InvariantCulture.NumberFormat, "e"),
                Is.EqualTo(expected));
        }

        private static IEnumerable<TestCaseData> Sin()
        {
            var pi = Domains.BigFloatMath.Pi(128);

            yield return new TestCaseData(new BigFloat(0, 0), pi, 20, 5, "0");
            yield return new TestCaseData(BigFloat.Divide(pi, 6, 128), pi, 20, 5, "0.5");
            yield return new TestCaseData(BigFloat.Divide(pi, 2, 128), pi, 20, 5, "1");
            yield return new TestCaseData(BigFloat.Divide(pi * 5, 6, 128), pi, 20, 5, "0.5");
            yield return new TestCaseData(pi, pi, 20, 5, "0");
            yield return new TestCaseData(-pi, pi, 20, 5, "0");
            yield return new TestCaseData(BigFloat.Divide(-pi * 5, 6, 128), pi, 20, 5, "-0.5");
            yield return new TestCaseData(BigFloat.Divide(-pi, 2, 128), pi, 20, 5, "-1");
            yield return new TestCaseData(BigFloat.Divide(-pi, 6, 128), pi, 20, 5, "-0.5");
        }
    }
}