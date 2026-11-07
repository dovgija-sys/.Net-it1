using Microsoft.EntityFrameworkCore;

namespace it1.DB
{
    public class SqlServerRentalRepository : IRentalRepository
    {
        public async Task<List<Booking>> GetAllBookingsAsync(CancellationToken ct = default)
        {
            await using var db = new RentalDBContext();
            return await db.Bookings
                           .Include(b => b.Property)
                               .ThenInclude(p => p.Host)
                                   .ThenInclude(h => h.Properties)
                           .AsNoTrackingWithIdentityResolution()
                           .ToListAsync(ct);
        }

        public async Task<int> CountBookingsAsync(CancellationToken ct = default)
        {
            await using var db = new RentalDBContext();
            return await db.Bookings.CountAsync(ct);
        }

        public async Task<List<Property>> GetAllPropertiesAsync(CancellationToken ct = default)
        {
            await using var db = new RentalDBContext();
            return await db.Properties
                           .Include(p => p.Bookings)
                           .AsNoTracking()
                           .ToListAsync(ct);
        }

        public async Task ClearAsync(CancellationToken ct = default)
        {
            await using var db = new RentalDBContext();
            db.Hosts.RemoveRange(db.Hosts);
            await db.SaveChangesAsync(ct);
        }
    }
}
