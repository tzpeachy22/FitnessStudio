namespace api.Dtos;

public sealed record TimetableDto(Guid ScheduleId, string ClassName, string InstructorName, DateTimeOffset StartTime, DateTimeOffset EndTime, int AttendanceCount, int AvailableSlots, string BusinessName);
