namespace api.Dtos;

public sealed record JoinWaitlistRequest(Guid CustomerId, Guid CustomerPackageId, Guid TimetableScheduleId);
