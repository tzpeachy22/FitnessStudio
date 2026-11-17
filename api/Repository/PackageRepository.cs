using api.Models;
using api.Dtos;
using api.Interfaces;
using api.Mappers;
using Microsoft.EntityFrameworkCore;

namespace api.Repository;

public sealed class PackageRepository(IAppDbContext db) : IPackageRepository
{
    public async Task<List<AvailablePackageDto>> ListAsync(CancellationToken cancellationToken)
    {
        return await db.Businesses
            .AsNoTracking()
            .OrderBy(x => x.Id)
            .Select(x => new AvailablePackageDto(
                x.Id,
                x.Name,
                10,
                90))
            .ToListAsync(cancellationToken);
    }

    public async Task<CustomerPackageDto> PurchaseAsync(PurchasePackageRequest request, CancellationToken cancellationToken)
    {
        var customerExists = await db.Customers.AnyAsync(x => x.Id == request.CustomerId, cancellationToken);
        var business = await db.Businesses.FirstOrDefaultAsync(x => x.Id == request.BusinessId, cancellationToken);

        if (!customerExists)
        {
            throw new InvalidOperationException("Customer does not exist.");
        }

        if (business is null)
        {
            throw new InvalidOperationException("Business does not exist.");
        }

        var credits = request.TotalCredits <= 0 ? 10 : request.TotalCredits;
        var package = new CustomerPackage
        {
            CustomerId = request.CustomerId,
            BusinessId = request.BusinessId,
            TotalCredits = credits,
            RemainingCredits = credits,
            ExpiryDate = request.ExpiryDate ?? DateTimeOffset.UtcNow.AddMonths(3)
        };

        db.CustomerPackages.Add(package);
        await db.SaveChangesAsync(cancellationToken);

        return package.ToCustomerPackageDto(business.Name);
    }
}
