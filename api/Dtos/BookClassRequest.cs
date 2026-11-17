namespace api.Dtos;

public sealed record BookClassRequest(Guid CustomerId, Guid CustomerPackageId, Guid TimetableScheduleId, bool JoinWaitlistIfFull = true);
