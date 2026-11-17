namespace api.Dtos;

public sealed record CustomerPackageDto(Guid Id, Guid CustomerId, Guid BusinessId, string BusinessName, int TotalCredits, int RemainingCredits, DateTimeOffset ExpiryDate);
