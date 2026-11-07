namespace it1.DB
{
    public class Host
    {
        public string HostId { get; set; } = "";
        public string HostName { get; set; } = "";
        public List<Property> Properties { get; set; } = new();

    }
    public class Property 
    {
        public string PropertyId { get; set; } = "";
        public string HostId { get; set; } = "";
        public Host Host { get; set; } = null!;
        public string City { get; set; } = "";
        public decimal PricePerNight { get; set; }
        public List<Booking> Bookings { get; set; } = new();
    }
    public class Booking
    {
        public string BookingId { get; set; } = "";
        public string PropertyId { get; set; } = "";
        public Property Property { get; set; } = null!;
        public DateOnly StartDate { get; set; }
        public DateOnly EndDate { get; set; }
        public decimal TotalCost { get; set; }
    }
}
