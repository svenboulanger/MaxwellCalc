using MaxwellCalc.Core.Domains;
using MaxwellCalc.Core.Units;
using System.Numerics;

namespace MaxwellCalc.Tests;

public class DoubleDomainTests
{
    [Fact]
    public void When_CalculateBinary_Expect_Result()
    {
        var domain = new DoubleDomain();
        Assert.True(domain.TryAdd(new(1.0, Unit.UnitNone), new(1.0, Unit.UnitNone), null, out var result));
        Assert.Equal(new Quantity<double>(2.0, Unit.UnitNone), result);

        Assert.True(domain.TrySubtract(new(2.0, Unit.UnitNone), new(1.0, Unit.UnitNone), null, out result));
        Assert.Equal(new Quantity<double>(1.0, Unit.UnitNone), result);

        Assert.True(domain.TryMultiply(new(2.0, Unit.UnitNone), new(2.0, Unit.UnitNone), null, out result));
        Assert.Equal(new Quantity<double>(4.0, Unit.UnitNone), result);

        Assert.True(domain.TryDivide(new(2.0, Unit.UnitNone), new(2.0, Unit.UnitNone), null, out result));
        Assert.Equal(new Quantity<double>(1.0, Unit.UnitNone), result);

        Assert.True(domain.TryModulo(new(3.0, Unit.UnitNone), new(2.0, Unit.UnitNone), null, out result));
        Assert.Equal(new Quantity<double>(1.0, Unit.UnitNone), result);
    }
}
