namespace api.Models;

public sealed class WaitlistEntry
{
    public Guid Id { get; set; }
    public Guid CustomerId { get; set; }
    public Guid CustomerPackageId { get; set; }
    public Guid TimetableScheduleId { get; set; }
    public WaitlistStatus Status { get; set; } = WaitlistStatus.Waiting;
    public DateTimeOffset JoinedAt { get; set; }
    public DateTimeOffset? PromotedAt { get; set; }
    public Customer? Customer { get; set; }
    public CustomerPackage? CustomerPackage { get; set; }
    public TimetableSchedule? TimetableSchedule { get; set; }
}
