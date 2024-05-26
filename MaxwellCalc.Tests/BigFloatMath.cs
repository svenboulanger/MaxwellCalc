using MaxwellCalc.Domains;
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
        [TestCaseSource(nameof(BigFloatToString))]
        public void When_BigFloatToString_Expect_Reference(BigFloat input, string expectedOutput, int expectedDecimalPlace)
        {
            string actual = BigFloat.FormatFull(input, out int decimalPlace);
            Assert.That(actual, Is.EqualTo(expectedOutput));
            Assert.That(decimalPlace, Is.EqualTo(expectedDecimalPlace));
        }

        private static IEnumerable<TestCaseData> BigFloatToString()
        {
            yield return new TestCaseData(new BigFloat(0, 0), "0", 0); // 0
            yield return new TestCaseData(new BigFloat(1, 0), "1", 0); // 1
            yield return new TestCaseData(new BigFloat(1, -2), "25", -1); // 0.25
            yield return new TestCaseData(new BigFloat(-3, -5), "-9375", -2); // -0.09375
            yield return new TestCaseData(new BigFloat(3, -1), "15", 0); // 1.5
            yield return new TestCaseData(new BigFloat(11 * 4 + 1, -2), "1125", 1); // 11.25
        }

        [Test]
        [TestCaseSource(nameof(BigFloatToStringRelative))]
        public void When_BigFloatToStringRelative(BigFloat input, int precision, string expectedOutput, int expectedDecimalPlace)
        {
            string actual = BigFloat.FormatRelativePrecision(input, precision, out int decimalPlace);
            Assert.That(actual, Is.EqualTo(expectedOutput));
            Assert.That(decimalPlace, Is.EqualTo(expectedDecimalPlace));
        }

        private static IEnumerable<TestCaseData> BigFloatToStringRelative()
        {
            yield return new TestCaseData(new BigFloat(1, -2), 1, "3", -1);
            yield return new TestCaseData(new BigFloat(16, 0), 1, "2", 1);
            yield return new TestCaseData(new BigFloat(99, 0), 1, "1", 2);
            yield return new TestCaseData(new BigFloat(95, 0), 1, "1", 2);
            yield return new TestCaseData(new BigFloat(949, 0), 1, "9", 2);
            yield return new TestCaseData(new BigFloat(9499999999L, 0), 1, "9", 9);
            yield return new TestCaseData(new BigFloat(9449999999L, 0), 2, "94", 9);
            yield return new TestCaseData(new BigFloat(9450000000L, 0), 2, "95", 9);
            yield return new TestCaseData(new BigFloat(1, -3), 1, "1", -1);
            yield return new TestCaseData(new BigFloat(1, -3), 2, "13", -1);
            yield return new TestCaseData(new BigFloat(1, -3), 3, "125", -1);
            yield return new TestCaseData(new BigFloat(1, -3), 4, "125", -1);
            yield return new TestCaseData(new BigFloat(1, -4), 1, "6", -2);
            yield return new TestCaseData(new BigFloat(1, -4), 2, "63", -2);
            yield return new TestCaseData(new BigFloat(1, -4), 3, "625", -2);
            yield return new TestCaseData(new BigFloat(1, -4), 4, "625", -2);
        }
    }
}