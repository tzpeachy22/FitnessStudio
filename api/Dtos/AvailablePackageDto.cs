namespace api.Dtos;

public sealed record AvailablePackageDto(Guid BusinessId, string BusinessName, int TotalCredits, int ValidityDays);
