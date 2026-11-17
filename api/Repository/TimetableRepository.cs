using api.Models;
using api.Dtos;
using api.Interfaces;
using api.Mappers;
using Microsoft.EntityFrameworkCore;

namespace api.Repository;

public sealed class TimetableRepository(IAppDbContext db) : ITimetableRepository
{
    public async Task<List<TimetableDto>> ListAsync(Guid? businessId, DateOnly? date, CancellationToken cancellationToken)
    {
        var query = db.TimetableSchedules
            .AsNoTracking()
            .Include(x => x.Business)
            .Include(x => x.Bookings)
            .AsQueryable();

        if (businessId.HasValue)
        {
            query = query.Where(x => x.BusinessId == businessId);
        }

        if (date.HasValue)
        {
            var start = date.Value.ToDateTime(TimeOnly.MinValue, DateTimeKind.Utc);
            var end = date.Value.ToDateTime(TimeOnly.MaxValue, DateTimeKind.Utc);
            query = query.Where(x => x.StartTime >= start && x.StartTime <= end);
        }

        return await query
            .OrderBy(x => x.StartTime)
            .Select(x => new TimetableDto(
                x.Id,
                x.ClassName,
                x.InstructorName,
                x.StartTime,
                x.EndTime,
                x.Bookings.Count(b => b.Status == BookingStatus.Booked),
                x.AvailableSlots,
                x.Business!.Name))
            .ToListAsync(cancellationToken);
    }
}
