using System;
using Xunit;

namespace AwesomeAssertions.Builders;

public class DateBuilding
{
#if NET6_0_OR_GREATER
    [Fact]
    public void Casts_implictly_to_DateOnly()
    {
        DateOnly dt = 11.Jun(2017);
        dt.Should().Be(new DateOnly(2017, 06, 11));
    }
#endif

    [Fact]
    public void Casts_implictly_to_DateTime()
    {
        DateTime dt = 11.Jun(2017);
        dt.Should().Be(new DateTime(2017, 06, 11, 00, 00, 00, DateTimeKind.Utc));
    }

    [Fact]
    public void Casts_implictly_to_DateTimeOffset()
    {
        DateTimeOffset dt = 11.Jun(2017);
        dt.Should().Be(new DateTimeOffset(2017, 06, 11, 00, 00, 00, TimeSpan.Zero));
    }
}

public class DateTimeBuilding
{
    [Fact]
    public void Casts_implictly_to_DateTime()
    {
        DateTime dt = 11.Jun(2017).At(05, 15);
        dt.Should().Be(new DateTime(2017, 06, 11, 05, 15, 00, DateTimeKind.Utc));
    }

    [Fact]
    public void Casts_implictly_to_DateTimeOffset()
    {
        DateTimeOffset dt = 11.Jun(2017).At(06, 15).WithOffset(+1);
        dt.Should().Be(new DateTimeOffset(2017, 06, 11, 06, 15, 00, TimeSpan.FromHours(1)));
    }
}

public class BeEquivalentTo
{
    [Fact]
    public void DateBuilder_properties()
    {
        var model = new MyModel { Now = 11.Jun(2017) };

        model.Should().BeEquivalentTo(new
        {
            Now = 11.Jun(2017),
        });
    }
}

file class MyModel
{
    public DateTime Now { get; init; }
}
