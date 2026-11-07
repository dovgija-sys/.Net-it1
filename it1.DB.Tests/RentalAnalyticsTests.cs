using it1.DB;
using Xunit;

namespace it1.DB.Tests;

public class RentalAnalyticsTests
{
    [Fact]
    public void FindBookingAnomalies_FindsCostMismatchAndInvalidDates()
    {
        var host = CreateHost("HOST-1", "Host");
        var property = CreateProperty(host, "PROP-1", "Львів", 1800m);
        var bookings = new[]
        {
            CreateBooking(property, "BK-OK", new DateOnly(2026, 10, 1), new DateOnly(2026, 10, 5), 7200m),
            CreateBooking(property, "BK-COST", new DateOnly(2026, 10, 1), new DateOnly(2026, 10, 5), 8000m),
            CreateBooking(property, "BK-DATE", new DateOnly(2026, 10, 5), new DateOnly(2026, 10, 1), -7200m)
        };

        var result = RentalAnalytics.FindBookingAnomalies(bookings);

        Assert.Collection(
            result,
            anomaly =>
            {
                Assert.Equal("BK-COST", anomaly.BookingId);
                Assert.Equal("PROP-1", anomaly.PropertyId);
                Assert.Equal("Львів", anomaly.City);
                Assert.Equal(7200m, anomaly.ExpectedCost);
                Assert.Equal(8000m, anomaly.ActualCost);
                Assert.Equal(BookingAnomalyType.CostMismatch, anomaly.AnomalyType);
            },
            anomaly =>
            {
                Assert.Equal("BK-DATE", anomaly.BookingId);
                Assert.Equal(BookingAnomalyType.InvalidDates, anomaly.AnomalyType);
            });
    }

    [Fact]
    public void FindDoubleBookings_ReturnsOverlapsButNotTouchingPeriods()
    {
        var host = CreateHost("HOST-1", "Host");
        var property = CreateProperty(host, "PROP-1", "Львів", 100m);
        var bookings = new[]
        {
            CreateBooking(property, "BK-1", new DateOnly(2026, 10, 1), new DateOnly(2026, 10, 5), 400m),
            CreateBooking(property, "BK-2", new DateOnly(2026, 10, 4), new DateOnly(2026, 10, 8), 400m),
            CreateBooking(property, "BK-3", new DateOnly(2026, 10, 8), new DateOnly(2026, 10, 10), 200m)
        };

        var result = RentalAnalytics.FindDoubleBookings(bookings);

        var overlap = Assert.Single(result);
        Assert.Equal("PROP-1", overlap.PropertyId);
        Assert.Equal("BK-1", overlap.FirstBookingId);
        Assert.Equal("BK-2", overlap.SecondBookingId);
        Assert.Equal(new DateOnly(2026, 10, 4), overlap.OverlapStart);
        Assert.Equal(new DateOnly(2026, 10, 5), overlap.OverlapEnd);
    }

    [Fact]
    public void GetRevenueByCity_GroupsAndAggregatesBookings()
    {
        var host = CreateHost("HOST-1", "Host");
        var lvivProperty = CreateProperty(host, "PROP-1", "Львів", 1800m);
        var kyivProperty = CreateProperty(host, "PROP-2", "Київ", 1500m);
        var bookings = new[]
        {
            CreateBooking(lvivProperty, "BK-1", new DateOnly(2026, 10, 1), new DateOnly(2026, 10, 5), 7200m),
            CreateBooking(lvivProperty, "BK-2", new DateOnly(2026, 10, 5), new DateOnly(2026, 10, 7), 3000m),
            CreateBooking(kyivProperty, "BK-3", new DateOnly(2026, 11, 1), new DateOnly(2026, 11, 4), 4500m)
        };

        var result = RentalAnalytics.GetRevenueByCity(bookings);

        var lviv = Assert.Single(result.Where(city => city.City == "Львів"));
        Assert.Equal(2, lviv.BookingCount);
        Assert.Equal(10200m, lviv.TotalRevenue);
        Assert.Equal(5100m, lviv.AverageRevenue);
        Assert.Equal(3m, lviv.AverageStayNights);

        var kyiv = Assert.Single(result.Where(city => city.City == "Київ"));
        Assert.Equal(1, kyiv.BookingCount);
        Assert.Equal(4500m, kyiv.TotalRevenue);
        Assert.Equal(3m, kyiv.AverageStayNights);
    }

    [Fact]
    public void GetRevenueByCity_ExcludesInvalidDatesFromAverageStay()
    {
        var host = CreateHost("HOST-1", "Host");
        var property = CreateProperty(host, "PROP-1", "Львів", 100m);
        var bookings = new[]
        {
            CreateBooking(property, "BK-VALID", new DateOnly(2026, 10, 1), new DateOnly(2026, 10, 4), 300m),
            CreateBooking(property, "BK-INVALID", new DateOnly(2026, 10, 4), new DateOnly(2026, 10, 4), 0m)
        };

        var city = Assert.Single(RentalAnalytics.GetRevenueByCity(bookings));

        Assert.Equal(2, city.BookingCount);
        Assert.Equal(300m, city.TotalRevenue);
        Assert.Equal(150m, city.AverageRevenue);
        Assert.Equal(3m, city.AverageStayNights);
    }

    [Fact]
    public void GetHostSummary_CountsPropertiesAndSelectsMostProfitable()
    {
        var host = CreateHost("HOST-1", "Host");
        var firstProperty = CreateProperty(host, "PROP-1", "Львів", 100m);
        var secondProperty = CreateProperty(host, "PROP-2", "Київ", 200m);
        CreateProperty(host, "PROP-3", "Одеса", 300m);
        var bookings = new[]
        {
            CreateBooking(firstProperty, "BK-1", new DateOnly(2026, 10, 1), new DateOnly(2026, 10, 3), 200m),
            CreateBooking(secondProperty, "BK-2", new DateOnly(2026, 10, 1), new DateOnly(2026, 10, 3), 400m),
            CreateBooking(secondProperty, "BK-3", new DateOnly(2026, 11, 1), new DateOnly(2026, 11, 3), 400m)
        };

        var summary = Assert.Single(RentalAnalytics.GetHostSummary(bookings));

        Assert.Equal("HOST-1", summary.HostId);
        Assert.Equal("Host", summary.HostName);
        Assert.Equal(3, summary.PropertyCount);
        Assert.Equal(3, summary.BookingCount);
        Assert.Equal(1000m, summary.TotalRevenue);
        Assert.Equal("PROP-2", summary.MostProfitablePropertyId);
        Assert.Equal(800m, summary.MostProfitablePropertyRevenue);
    }

    private static Host CreateHost(string id, string name) => new()
    {
        HostId = id,
        HostName = name
    };

    private static Property CreateProperty(Host host, string id, string city, decimal pricePerNight)
    {
        var property = new Property
        {
            PropertyId = id,
            City = city,
            PricePerNight = pricePerNight,
            Host = host,
            HostId = host.HostId
        };
        host.Properties.Add(property);
        return property;
    }

    private static Booking CreateBooking(
        Property property,
        string id,
        DateOnly startDate,
        DateOnly endDate,
        decimal totalCost)
    {
        var booking = new Booking
        {
            BookingId = id,
            PropertyId = property.PropertyId,
            Property = property,
            StartDate = startDate,
            EndDate = endDate,
            TotalCost = totalCost
        };
        property.Bookings.Add(booking);
        return booking;
    }
}