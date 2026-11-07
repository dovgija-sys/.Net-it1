using Microsoft.EntityFrameworkCore;

namespace it1.DB
{
    public class RentalDBContext : DbContext
    {
        public DbSet<Host> Hosts { get; set; } = null!;
        public DbSet<Property> Properties { get; set; } = null!;
        public DbSet<Booking> Bookings { get; set; } = null!;

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            var connectionString = Environment.GetEnvironmentVariable("RENTAL_DB_CONNECTION")
                ?? "Server=.\\SQLEXPRESS;Database=RentalDb;Trusted_Connection=True;TrustServerCertificate=True;";
            optionsBuilder.UseSqlServer(connectionString);
        }
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Host>(e =>
            {
                e.HasKey(x => x.HostId);
                e.Property(x => x.HostId).HasMaxLength(100);
                e.Property(x => x.HostName).HasMaxLength(200);
                e.HasMany(x => x.Properties).WithOne(p => p.Host)
                .HasForeignKey(p => p.HostId).OnDelete(DeleteBehavior.Cascade);
            });
            modelBuilder.Entity<Property>(e =>
            {
                e.HasKey(x => x.PropertyId);
                e.Property(x => x.PropertyId).HasMaxLength(100);
                e.Property(x => x.HostId).HasMaxLength(100);
                e.Property(x => x.City).HasMaxLength(100);
                e.Property(x => x.PricePerNight).HasPrecision(18, 2);
                e.HasIndex(x => x.City);
                e.HasMany(x => x.Bookings).WithOne(bk => bk.Property)
                .HasForeignKey(bk => bk.PropertyId).OnDelete(DeleteBehavior.Cascade);
            });
            modelBuilder.Entity<Booking>(e => {
                e.HasKey(x => x.BookingId);
                e.Property(x => x.BookingId).HasMaxLength(100);
                e.Property(x => x.PropertyId).HasMaxLength(100);
                e.Property(x => x.TotalCost).HasPrecision(18, 2);
                e.HasIndex(x => new { x.PropertyId, x.StartDate });
            });
        }
    }
}
