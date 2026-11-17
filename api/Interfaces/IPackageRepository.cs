using api.Dtos;

namespace api.Interfaces;

public interface IPackageRepository
{
    Task<List<AvailablePackageDto>> ListAsync(CancellationToken cancellationToken);
    Task<CustomerPackageDto> PurchaseAsync(PurchasePackageRequest request, CancellationToken cancellationToken);
}
