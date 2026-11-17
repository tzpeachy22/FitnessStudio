namespace api.Models;

public sealed class Customer
{
    public Guid Id { get; set; }
    public required string FullName { get; set; }
    public required string Email { get; set; }
    public List<CustomerPackage> Packages { get; set; } = [];
    public List<Booking> Bookings { get; set; } = [];
    public List<WaitlistEntry> WaitlistEntries { get; set; } = [];
}
