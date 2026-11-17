using api.Interfaces;
using api.Models;
using Microsoft.EntityFrameworkCore;

namespace api.Data;

public sealed class AppDbContext(DbContextOptions<AppDbContext> options) : DbContext(options), IAppDbContext
{
    public DbSet<Business> Businesses => Set<Business>();
    public DbSet<Customer> Customers => Set<Customer>();
    public DbSet<CustomerPackage> CustomerPackages => Set<CustomerPackage>();
    public DbSet<TimetableSchedule> TimetableSchedules => Set<TimetableSchedule>();
    public DbSet<Booking> Bookings => Set<Booking>();
    public DbSet<WaitlistEntry> WaitlistEntries => Set<WaitlistEntry>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Business>(entity =>
        {
            entity.Property(x => x.Name).HasMaxLength(160).IsRequired();
        });

        modelBuilder.Entity<Customer>(entity =>
        {
            entity.Property(x => x.FullName).HasMaxLength(160).IsRequired();
            entity.Property(x => x.Email).HasMaxLength(200).IsRequired();
            entity.HasIndex(x => x.Email).IsUnique();
        });

        modelBuilder.Entity<CustomerPackage>(entity =>
        {
            entity.HasOne(x => x.Customer).WithMany(x => x.Packages).HasForeignKey(x => x.CustomerId);
            entity.HasOne(x => x.Business).WithMany(x => x.Packages).HasForeignKey(x => x.BusinessId);
        });

        modelBuilder.Entity<TimetableSchedule>(entity =>
        {
            entity.Property(x => x.ClassName).HasMaxLength(160).IsRequired();
            entity.Property(x => x.InstructorName).HasMaxLength(160).IsRequired();
            entity.HasOne(x => x.Business).WithMany(x => x.TimetableSchedules).HasForeignKey(x => x.BusinessId);
        });

        modelBuilder.Entity<Booking>(entity =>
        {
            entity.HasOne(x => x.Customer).WithMany(x => x.Bookings).HasForeignKey(x => x.CustomerId);
            entity.HasOne(x => x.CustomerPackage).WithMany().HasForeignKey(x => x.CustomerPackageId);
            entity.HasOne(x => x.TimetableSchedule).WithMany(x => x.Bookings).HasForeignKey(x => x.TimetableScheduleId);
            entity.HasIndex(x => new { x.CustomerId, x.TimetableScheduleId, x.Status });
        });

        modelBuilder.Entity<WaitlistEntry>(entity =>
        {
            entity.HasOne(x => x.Customer).WithMany(x => x.WaitlistEntries).HasForeignKey(x => x.CustomerId);
            entity.HasOne(x => x.CustomerPackage).WithMany().HasForeignKey(x => x.CustomerPackageId);
            entity.HasOne(x => x.TimetableSchedule).WithMany(x => x.WaitlistEntries).HasForeignKey(x => x.TimetableScheduleId);
            entity.HasIndex(x => new { x.TimetableScheduleId, x.Status, x.JoinedAt });
        });

        Seed(modelBuilder);
    }

    private static Guid SeedId(int id) => Guid.Parse($"00000000-0000-0000-0000-{id:D12}");

    private static void Seed(ModelBuilder modelBuilder)
    {
        var now = new DateTimeOffset(2026, 7, 9, 0, 0, 0, TimeSpan.Zero);

        modelBuilder.Entity<Business>().HasData(
            new Business { Id = SeedId(1), Name = "Rezerv Fitness" },
            new Business { Id = SeedId(2), Name = "Downtown Yoga" });

        modelBuilder.Entity<Customer>().HasData(
            Enumerable.Range(1, 10).Select(i => new Customer
            {
                Id = SeedId(i),
                FullName = $"Customer {i}",
                Email = $"customer{i}@example.com"
            }));

        modelBuilder.Entity<CustomerPackage>().HasData(
            new CustomerPackage { Id = SeedId(1), CustomerId = SeedId(1), BusinessId = SeedId(1), TotalCredits = 10, RemainingCredits = 8, ExpiryDate = now.AddMonths(2) },
            new CustomerPackage { Id = SeedId(2), CustomerId = SeedId(2), BusinessId = SeedId(1), TotalCredits = 5, RemainingCredits = 0, ExpiryDate = now.AddMonths(2) },
            new CustomerPackage { Id = SeedId(3), CustomerId = SeedId(3), BusinessId = SeedId(2), TotalCredits = 10, RemainingCredits = 10, ExpiryDate = now.AddMonths(2) },
            new CustomerPackage { Id = SeedId(4), CustomerId = SeedId(4), BusinessId = SeedId(1), TotalCredits = 10, RemainingCredits = 10, ExpiryDate = now.AddDays(-1) },
            new CustomerPackage { Id = SeedId(5), CustomerId = SeedId(5), BusinessId = SeedId(1), TotalCredits = 10, RemainingCredits = 9, ExpiryDate = now.AddMonths(2) },
            new CustomerPackage { Id = SeedId(6), CustomerId = SeedId(6), BusinessId = SeedId(1), TotalCredits = 10, RemainingCredits = 10, ExpiryDate = now.AddMonths(2) },
            new CustomerPackage { Id = SeedId(7), CustomerId = SeedId(7), BusinessId = SeedId(1), TotalCredits = 10, RemainingCredits = 10, ExpiryDate = now.AddMonths(2) },
            new CustomerPackage { Id = SeedId(8), CustomerId = SeedId(8), BusinessId = SeedId(2), TotalCredits = 10, RemainingCredits = 10, ExpiryDate = now.AddMonths(2) },
            new CustomerPackage { Id = SeedId(9), CustomerId = SeedId(9), BusinessId = SeedId(2), TotalCredits = 10, RemainingCredits = 10, ExpiryDate = now.AddMonths(2) },
            new CustomerPackage { Id = SeedId(10), CustomerId = SeedId(10), BusinessId = SeedId(1), TotalCredits = 10, RemainingCredits = 10, ExpiryDate = now.AddMonths(2) });

        modelBuilder.Entity<TimetableSchedule>().HasData(
            new TimetableSchedule { Id = SeedId(1), BusinessId = SeedId(1), ClassName = "Yoga Class", InstructorName = "John Doe", StartTime = now.AddDays(2).AddHours(10), EndTime = now.AddDays(2).AddHours(11), AvailableSlots = 2 },
            new TimetableSchedule { Id = SeedId(2), BusinessId = SeedId(1), ClassName = "HIIT", InstructorName = "May Lin", StartTime = now.AddDays(2).AddHours(10.5), EndTime = now.AddDays(2).AddHours(11.5), AvailableSlots = 5 },
            new TimetableSchedule { Id = SeedId(3), BusinessId = SeedId(1), ClassName = "Pilates", InstructorName = "Aung Ko", StartTime = now.AddDays(3).AddHours(9), EndTime = now.AddDays(3).AddHours(10), AvailableSlots = 1 },
            new TimetableSchedule { Id = SeedId(4), BusinessId = SeedId(2), ClassName = "Hot Yoga", InstructorName = "Nora", StartTime = now.AddDays(3).AddHours(10), EndTime = now.AddDays(3).AddHours(11), AvailableSlots = 3 },
            new TimetableSchedule { Id = SeedId(5), BusinessId = SeedId(2), ClassName = "Stretch", InstructorName = "Chris", StartTime = now.AddDays(4).AddHours(8), EndTime = now.AddDays(4).AddHours(9), AvailableSlots = 4 },
            new TimetableSchedule { Id = SeedId(6), BusinessId = SeedId(1), ClassName = "Spin", InstructorName = "David", StartTime = now.AddDays(4).AddHours(18), EndTime = now.AddDays(4).AddHours(19), AvailableSlots = 2 },
            new TimetableSchedule { Id = SeedId(7), BusinessId = SeedId(1), ClassName = "Boxing", InstructorName = "Thiri", StartTime = now.AddDays(5).AddHours(17), EndTime = now.AddDays(5).AddHours(18), AvailableSlots = 8 },
            new TimetableSchedule { Id = SeedId(8), BusinessId = SeedId(2), ClassName = "Meditation", InstructorName = "Zin", StartTime = now.AddDays(5).AddHours(7), EndTime = now.AddDays(5).AddHours(8), AvailableSlots = 10 },
            new TimetableSchedule { Id = SeedId(9), BusinessId = SeedId(1), ClassName = "Strength", InstructorName = "Myo", StartTime = now.AddDays(6).AddHours(12), EndTime = now.AddDays(6).AddHours(13), AvailableSlots = 6 },
            new TimetableSchedule { Id = SeedId(10), BusinessId = SeedId(2), ClassName = "Flow Yoga", InstructorName = "Ei", StartTime = now.AddDays(6).AddHours(16), EndTime = now.AddDays(6).AddHours(17), AvailableSlots = 1 });

        modelBuilder.Entity<Booking>().HasData(
            new Booking { Id = SeedId(1), CustomerId = SeedId(1), CustomerPackageId = SeedId(1), TimetableScheduleId = SeedId(1), Status = BookingStatus.Booked, BookedAt = now },
            new Booking { Id = SeedId(2), CustomerId = SeedId(5), CustomerPackageId = SeedId(5), TimetableScheduleId = SeedId(1), Status = BookingStatus.Booked, BookedAt = now },
            new Booking { Id = SeedId(3), CustomerId = SeedId(3), CustomerPackageId = SeedId(3), TimetableScheduleId = SeedId(10), Status = BookingStatus.Booked, BookedAt = now });

        modelBuilder.Entity<WaitlistEntry>().HasData(
            new WaitlistEntry { Id = SeedId(1), CustomerId = SeedId(6), CustomerPackageId = SeedId(6), TimetableScheduleId = SeedId(1), Status = WaitlistStatus.Waiting, JoinedAt = now.AddMinutes(1) },
            new WaitlistEntry { Id = SeedId(2), CustomerId = SeedId(7), CustomerPackageId = SeedId(7), TimetableScheduleId = SeedId(1), Status = WaitlistStatus.Waiting, JoinedAt = now.AddMinutes(2) });
    }
}
