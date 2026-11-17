namespace api.Models;

public sealed class CustomerPackage
{
    public Guid Id { get; set; }
    public Guid CustomerId { get; set; }
    public Guid BusinessId { get; set; }
    public int TotalCredits { get; set; }
    public int RemainingCredits { get; set; }
    public DateTimeOffset ExpiryDate { get; set; }
    public Customer? Customer { get; set; }
    public Business? Business { get; set; }
}
