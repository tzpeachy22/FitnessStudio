namespace api.Models;

public sealed class TimetableSchedule
{
    public Guid Id { get; set; }
    public Guid BusinessId { get; set; }
    public required string ClassName { get; set; }
    public required string InstructorName { get; set; }
    public DateTimeOffset StartTime { get; set; }
    public DateTimeOffset EndTime { get; set; }
    public int AvailableSlots { get; set; }
    public Business? Business { get; set; }
    public List<Booking> Bookings { get; set; } = [];
    public List<WaitlistEntry> WaitlistEntries { get; set; } = [];
}
