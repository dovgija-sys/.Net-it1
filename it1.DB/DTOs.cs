
namespace it1.DB
{
    public record BookingDto(string BookingId, DateOnly StartDate, DateOnly EndDate, decimal TotalCost);
    public record PropertyDto(string PropertyId, string City, decimal PricePerNight, List<BookingDto> Bookings);
    public record HostDto(string HostId, string HostName, List<PropertyDto> Properties);
}
