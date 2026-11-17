using api.Models;
using Microsoft.EntityFrameworkCore;

namespace api.Interfaces;

public interface IAppDbContext
{
    DbSet<Business> Businesses { get; }
    DbSet<Customer> Customers { get; }
    DbSet<CustomerPackage> CustomerPackages { get; }
    DbSet<TimetableSchedule> TimetableSchedules { get; }
    DbSet<Booking> Bookings { get; }
    DbSet<WaitlistEntry> WaitlistEntries { get; }
    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}
