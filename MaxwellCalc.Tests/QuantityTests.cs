using MaxwellCalc.Core.Units;
using System.Text.Json;

namespace MaxwellCalc.Tests
{
    public class QuantityTests
    {
        private readonly JsonSerializerOptions _options;

        public QuantityTests()
        {
            _options = new JsonSerializerOptions();
            _options.Converters.Add(new QuantityJsonConverterFactory());
        }

        [Theory]
        [MemberData(nameof(Tests))]
        public void When_ConvertToJSON_Expect_Reference(Quantity<double> quantity)
        {
            string json = JsonSerializer.Serialize(quantity, _options);
            var result = JsonSerializer.Deserialize<Quantity<double>>(json, _options);
            Assert.Equal(quantity, result);
        }

        public static TheoryData<Quantity<double>> Tests
        {
            get
            {
                var result = new TheoryData<Quantity<double>>
                {
                    { new(1.0, Unit.UnitNone) },
                    { new(10.0, new Unit((Unit.Meter, 2))) },
                    { new(2.5, new Unit((Unit.Meter, 1), (Unit.Second, -1))) },
                    { new(0.5, new Unit(("nV", 1), ("Hz", new Fraction(-1, 2)))) }
                };
                return result;
            }
        }
    }
}
