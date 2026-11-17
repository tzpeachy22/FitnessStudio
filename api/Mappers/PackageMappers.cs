using api.Dtos;
using api.Models;

namespace api.Mappers;

public static class PackageMappers
{
    public static CustomerPackageDto ToCustomerPackageDto(this CustomerPackage package, string businessName)
    {
        return new CustomerPackageDto(package.Id, package.CustomerId, package.BusinessId,
            businessName, package.TotalCredits, package.RemainingCredits, package.ExpiryDate);
    }
}
