using api.Dtos;

namespace api.Interfaces;

public interface ITimetableRepository
{
    Task<List<TimetableDto>> ListAsync(Guid? businessId, DateOnly? date, CancellationToken cancellationToken);
}
