namespace api.Models;

public sealed class Booking
{
    public Guid Id { get; set; }
    public Guid CustomerId { get; set; }
    public Guid CustomerPackageId { get; set; }
    public Guid TimetableScheduleId { get; set; }
    public BookingStatus Status { get; set; } = BookingStatus.Booked;
    public DateTimeOffset BookedAt { get; set; }
    public DateTimeOffset? CancelledAt { get; set; }
    public bool CreditRefunded { get; set; }
    public Customer? Customer { get; set; }
    public CustomerPackage? CustomerPackage { get; set; }
    public TimetableSchedule? TimetableSchedule { get; set; }
}
