namespace it1.DB
{
    public interface IRentalRepository
    {
        Task ClearAsync(CancellationToken ct = default);
        Task<int> CountBookingsAsync(CancellationToken ct = default);
        Task<List<Booking>> GetAllBookingsAsync(CancellationToken ct = default);
        Task<List<Property>> GetAllPropertiesAsync(CancellationToken ct = default);
    }
}