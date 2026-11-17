namespace api.Dtos;

public sealed record PurchasePackageRequest(Guid CustomerId, Guid BusinessId, int TotalCredits, DateTimeOffset? ExpiryDate);
