namespace it1.DB
{
    public enum BookingAnomalyType
    {
        CostMismatch,
        InvalidDates
    }

    public record BookingAnomaly(
        string BookingId,
        string PropertyId,
        string City,
        decimal ExpectedCost,
        decimal ActualCost,
        BookingAnomalyType AnomalyType);

    public record DoubleBooking(
        string PropertyId,
        string FirstBookingId,
        string SecondBookingId,
        DateOnly OverlapStart,
        DateOnly OverlapEnd);

    public record CityRevenueStatistics(
        string City,
        int BookingCount,
        decimal TotalRevenue,
        decimal AverageRevenue,
        decimal AverageStayNights);

    public record HostSummary(
        string HostId,
        string HostName,
        int PropertyCount,
        int BookingCount,
        decimal TotalRevenue,
        string? MostProfitablePropertyId,
        decimal MostProfitablePropertyRevenue);

    public static class RentalAnalytics
    {
        public static IReadOnlyList<BookingAnomaly> FindBookingAnomalies(IEnumerable<Booking> bookings)
        {
            var bookingList = GetLoadedBookings(bookings);

            return bookingList
                .SelectMany(booking =>
                {
                    var property = booking.Property;
                    var nights = booking.EndDate.DayNumber - booking.StartDate.DayNumber;
                    var expectedCost = property.PricePerNight * nights;
                    var anomalies = new List<BookingAnomaly>();

                    if (booking.TotalCost != expectedCost)
                    {
                        anomalies.Add(new BookingAnomaly(
                            booking.BookingId,
                            property.PropertyId,
                            property.City,
                            expectedCost,
                            booking.TotalCost,
                            BookingAnomalyType.CostMismatch));
                    }

                    if (booking.EndDate <= booking.StartDate)
                    {
                        anomalies.Add(new BookingAnomaly(
                            booking.BookingId,
                            property.PropertyId,
                            property.City,
                            expectedCost,
                            booking.TotalCost,
                            BookingAnomalyType.InvalidDates));
                    }

                    return anomalies;
                })
                .OrderBy(anomaly => anomaly.BookingId)
                .ThenBy(anomaly => anomaly.AnomalyType)
                .ToList();
        }

        public static IReadOnlyList<DoubleBooking> FindDoubleBookings(IEnumerable<Booking> bookings)
        {
            var bookingList = GetLoadedBookings(bookings)
                .Where(booking => booking.EndDate > booking.StartDate)
                .ToList();

            return bookingList
                .GroupBy(booking => booking.Property.PropertyId)
                .SelectMany(group =>
                {
                    var propertyBookings = group
                        .OrderBy(booking => booking.StartDate)
                        .ThenBy(booking => booking.BookingId)
                        .ToList();

                    return propertyBookings.SelectMany((first, index) =>
                        propertyBookings
                            .Skip(index + 1)
                            .Where(second => first.StartDate < second.EndDate && second.StartDate < first.EndDate)
                            .Select(second => new DoubleBooking(
                                group.Key,
                                first.BookingId,
                                second.BookingId,
                                first.StartDate > second.StartDate ? first.StartDate : second.StartDate,
                                first.EndDate < second.EndDate ? first.EndDate : second.EndDate)));
                })
                .OrderBy(overlap => overlap.PropertyId)
                .ThenBy(overlap => overlap.OverlapStart)
                .ThenBy(overlap => overlap.FirstBookingId)
                .ToList();
        }

        public static IReadOnlyList<CityRevenueStatistics> GetRevenueByCity(IEnumerable<Booking> bookings)
        {
            return GetLoadedBookings(bookings)
                .GroupBy(booking => booking.Property.City)
                .Select(group =>
                {
                    var validStayNights = group
                        .Where(booking => booking.EndDate > booking.StartDate)
                        .Select(booking => (decimal)(booking.EndDate.DayNumber - booking.StartDate.DayNumber))
                        .ToList();

                    return new CityRevenueStatistics(
                        group.Key,
                        group.Count(),
                        group.Sum(booking => booking.TotalCost),
                        group.Average(booking => booking.TotalCost),
                        validStayNights.Count == 0 ? 0 : validStayNights.Average());
                })
                .OrderBy(statistics => statistics.City)
                .ToList();
        }

        public static IReadOnlyList<HostSummary> GetHostSummary(IEnumerable<Booking> bookings)
        {
            var bookingList = GetLoadedBookings(bookings);
            if (bookingList.Any(booking => booking.Property.Host is null))
            {
                throw new ArgumentException("Every booking must have a loaded property host.", nameof(bookings));
            }

            return bookingList
                .GroupBy(booking => booking.Property.Host.HostId)
                .Select(group =>
                {
                    var hostProperties = group
                        .SelectMany(booking => booking.Property.Host.Properties ?? new List<Property>())
                        .Concat(group.Select(booking => booking.Property))
                        .GroupBy(property => property.PropertyId)
                        .Select(propertyGroup => propertyGroup.First())
                        .ToList();

                    var propertyRevenue = hostProperties
                        .Select(property => new
                        {
                            property.PropertyId,
                            Revenue = group
                                .Where(booking => booking.Property.PropertyId == property.PropertyId)
                                .Sum(booking => booking.TotalCost)
                        })
                        .OrderByDescending(property => property.Revenue)
                        .ThenBy(property => property.PropertyId)
                        .FirstOrDefault();

                    return new HostSummary(
                        group.Key,
                        group.First().Property.Host.HostName,
                        hostProperties.Count,
                        group.Count(),
                        group.Sum(booking => booking.TotalCost),
                        propertyRevenue?.PropertyId,
                        propertyRevenue?.Revenue ?? 0m);
                })
                .OrderBy(summary => summary.HostName)
                .ThenBy(summary => summary.HostId)
                .ToList();
        }

        private static List<Booking> GetLoadedBookings(IEnumerable<Booking> bookings)
        {
            ArgumentNullException.ThrowIfNull(bookings);
            var bookingList = bookings.ToList();
            if (bookingList.Any(booking => booking.Property is null))
            {
                throw new ArgumentException("Every booking must have a loaded property.", nameof(bookings));
            }

            return bookingList;
        }
    }
}