using it1.DB;
using Microsoft.EntityFrameworkCore;

namespace it1.WF
{
    public record ImportResult(int Hosts, int Properties, int Bookings, int Skipped);

    public class ImportService
    {
        private readonly IRentalReader _reader;
        public ImportService(IRentalReader reader) => _reader = reader;

        public async Task<ImportResult> ImportAsync(
            string path, IProgress<int>? progress = null, CancellationToken ct = default)
        {
            ArgumentException.ThrowIfNullOrWhiteSpace(path);
            ct.ThrowIfCancellationRequested();

            await using var db = new RentalDBContext();

            await db.Database.EnsureCreatedAsync(ct);  

            int hosts = 0, props = 0, bookings = 0, skipped = 0;

            await foreach (var h in _reader.ReadAsync(path, ct))
            {
                if (await db.Hosts.AnyAsync(x => x.HostId == h.HostId, ct))
                {
                    skipped++;
                    continue;
                }

                var host = new Host
                {
                    HostId = h.HostId,
                    HostName = h.HostName,
                    Properties = (h.Properties ?? new()).Select(p => new Property
                    {
                        PropertyId = p.PropertyId,
                        City = p.City,
                        PricePerNight = p.PricePerNight,
                        Bookings = (p.Bookings ?? new()).Select(b => new Booking
                        {
                            BookingId = b.BookingId,
                            StartDate = b.StartDate,
                            EndDate = b.EndDate,
                            TotalCost = b.TotalCost
                        }).ToList()
                    }).ToList()
                };

                db.Hosts.Add(host);
                await db.SaveChangesAsync(ct);

                hosts++;
                props += host.Properties.Count;
                bookings += host.Properties.Sum(p => p.Bookings.Count);
                progress?.Report(hosts);

                db.ChangeTracker.Clear();
            }

            return new ImportResult(hosts, props, bookings, skipped);
        }
    }
}
