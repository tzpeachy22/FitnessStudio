using api.Models;
using api.Dtos;
using api.Interfaces;
using api.Mappers;
using Microsoft.EntityFrameworkCore;

namespace api.Repository;

public sealed class BookingRepository(IAppDbContext db, IRedisLock redisLock) : IBookingRepository
{
    private static readonly TimeSpan LockExpiry = TimeSpan.FromSeconds(10);

    public async Task<ApiResult> BookAsync(BookClassRequest request, CancellationToken cancellationToken)
    {
        await using var lockHandle = await redisLock.AcquireAsync($"schedule:{request.TimetableScheduleId}", LockExpiry, cancellationToken);
        if (lockHandle is null)
        {
            return ApiResult.Fail("Schedule is busy. Please try again.");
        }

        var validation = await ValidatePackageAndScheduleAsync(request.CustomerId, request.CustomerPackageId, request.TimetableScheduleId, cancellationToken);
        if (!validation.Success)
        {
            return validation;
        }

        var schedule = await db.TimetableSchedules.Include(x => x.Bookings).FirstAsync(x => x.Id == request.TimetableScheduleId, cancellationToken);
        var activeBookings = schedule.Bookings.Count(x => x.Status == BookingStatus.Booked);

        if (activeBookings >= schedule.AvailableSlots)
        {
            if (!request.JoinWaitlistIfFull)
            {
                return ApiResult.Fail("Schedule is full.");
            }

            return await JoinWaitlistInternalAsync(request.CustomerId, request.CustomerPackageId, request.TimetableScheduleId, cancellationToken);
        }

        var package = await db.CustomerPackages.FirstAsync(x => x.Id == request.CustomerPackageId, cancellationToken);
        package.RemainingCredits -= 1;

        db.Bookings.Add(new Booking
        {
            CustomerId = request.CustomerId,
            CustomerPackageId = request.CustomerPackageId,
            TimetableScheduleId = request.TimetableScheduleId,
            BookedAt = DateTimeOffset.UtcNow
        });

        await db.SaveChangesAsync(cancellationToken);
        return ApiResult.Ok("Booking confirmed and 1 credit deducted.");
    }

    public async Task<ApiResult> CancelAsync(CancelBookingRequest request, CancellationToken cancellationToken)
    {
        var booking = await db.Bookings
            .Include(x => x.TimetableSchedule)
            .FirstOrDefaultAsync(x => x.Id == request.BookingId, cancellationToken);

        if (booking is null)
        {
            return ApiResult.Fail("Booking does not exist.");
        }

        if (booking.Status == BookingStatus.Cancelled)
        {
            return ApiResult.Fail("Booking is already cancelled.");
        }

        await using var lockHandle = await redisLock.AcquireAsync($"schedule:{booking.TimetableScheduleId}", LockExpiry, cancellationToken);
        if (lockHandle is null)
        {
            return ApiResult.Fail("Schedule is busy. Please try again.");
        }

        var now = DateTimeOffset.UtcNow;
        booking.Status = BookingStatus.Cancelled;
        booking.CancelledAt = now;

        var refundAllowed = booking.TimetableSchedule!.StartTime - now > TimeSpan.FromHours(4);
        if (refundAllowed)
        {
            var package = await db.CustomerPackages.FirstAsync(x => x.Id == booking.CustomerPackageId, cancellationToken);
            package.RemainingCredits += 1;
            booking.CreditRefunded = true;
        }

        await PromoteFirstWaitlistEntryAsync(booking.TimetableScheduleId, cancellationToken);
        await db.SaveChangesAsync(cancellationToken);

        return ApiResult.Ok(refundAllowed
            ? "Booking cancelled, 1 credit refunded, waitlist promotion checked."
            : "Booking cancelled within 4 hours, no credit refunded, waitlist promotion checked.");
    }

    public async Task<ApiResult> JoinWaitlistAsync(JoinWaitlistRequest request, CancellationToken cancellationToken)
    {
        await using var lockHandle = await redisLock.AcquireAsync($"schedule:{request.TimetableScheduleId}", LockExpiry, cancellationToken);
        if (lockHandle is null)
        {
            return ApiResult.Fail("Schedule is busy. Please try again.");
        }

        var validation = await ValidatePackageAndScheduleAsync(request.CustomerId, request.CustomerPackageId, request.TimetableScheduleId, cancellationToken);
        if (!validation.Success)
        {
            return validation;
        }

        return await JoinWaitlistInternalAsync(request.CustomerId, request.CustomerPackageId, request.TimetableScheduleId, cancellationToken);
    }

    private async Task<ApiResult> ValidatePackageAndScheduleAsync(Guid customerId, Guid customerPackageId, Guid timetableScheduleId, CancellationToken cancellationToken)
    {
        var package = await db.CustomerPackages.FirstOrDefaultAsync(x => x.Id == customerPackageId, cancellationToken);
        var schedule = await db.TimetableSchedules.FirstOrDefaultAsync(x => x.Id == timetableScheduleId, cancellationToken);

        if (package is null)
        {
            return ApiResult.Fail("Package does not exist.");
        }

        if (schedule is null)
        {
            return ApiResult.Fail("Timetable schedule does not exist.");
        }

        if (package.CustomerId != customerId)
        {
            return ApiResult.Fail("Package does not belong to the customer.");
        }

        if (package.ExpiryDate <= DateTimeOffset.UtcNow)
        {
            return ApiResult.Fail("Package is expired.");
        }

        if (package.RemainingCredits < 1)
        {
            return ApiResult.Fail("Customer has insufficient credits.");
        }

        if (package.BusinessId != schedule.BusinessId)
        {
            return ApiResult.Fail("Package business does not match timetable schedule business.");
        }

        var hasOverlap = await db.Bookings
            .Include(x => x.TimetableSchedule)
            .AnyAsync(x =>
                x.CustomerId == customerId &&
                x.Status == BookingStatus.Booked &&
                x.TimetableScheduleId != timetableScheduleId &&
                x.TimetableSchedule!.StartTime < schedule.EndTime &&
                schedule.StartTime < x.TimetableSchedule.EndTime,
                cancellationToken);

        if (hasOverlap)
        {
            return ApiResult.Fail("Customer already has an overlapping booking.");
        }

        return ApiResult.Ok("Valid.");
    }

    private async Task<ApiResult> JoinWaitlistInternalAsync(Guid customerId, Guid customerPackageId, Guid timetableScheduleId, CancellationToken cancellationToken)
    {
        var existing = await db.WaitlistEntries.AnyAsync(x =>
            x.CustomerId == customerId &&
            x.TimetableScheduleId == timetableScheduleId &&
            x.Status == WaitlistStatus.Waiting,
            cancellationToken);

        if (existing)
        {
            return ApiResult.Fail("Customer is already waiting for this schedule.");
        }

        db.WaitlistEntries.Add(new WaitlistEntry
        {
            CustomerId = customerId,
            CustomerPackageId = customerPackageId,
            TimetableScheduleId = timetableScheduleId,
            JoinedAt = DateTimeOffset.UtcNow
        });

        await db.SaveChangesAsync(cancellationToken);
        return ApiResult.Ok("Schedule is full. Customer added to waitlist with no credit deducted yet.");
    }

    private async Task PromoteFirstWaitlistEntryAsync(Guid timetableScheduleId, CancellationToken cancellationToken)
    {
        var schedule = await db.TimetableSchedules.Include(x => x.Bookings).FirstAsync(x => x.Id == timetableScheduleId, cancellationToken);
        if (schedule.EndTime <= DateTimeOffset.UtcNow)
        {
            var expiredEntries = await db.WaitlistEntries
                .Where(x => x.TimetableScheduleId == timetableScheduleId && x.Status == WaitlistStatus.Waiting)
                .ToListAsync(cancellationToken);

            foreach (var entry in expiredEntries)
            {
                entry.Status = WaitlistStatus.Expired;
            }

            return;
        }

        var activeBookings = schedule.Bookings.Count(x => x.Status == BookingStatus.Booked);
        if (activeBookings >= schedule.AvailableSlots)
        {
            return;
        }

        var entryToPromote = await db.WaitlistEntries
            .Where(x => x.TimetableScheduleId == timetableScheduleId && x.Status == WaitlistStatus.Waiting)
            .OrderBy(x => x.JoinedAt)
            .FirstOrDefaultAsync(cancellationToken);

        if (entryToPromote is null)
        {
            return;
        }

        var package = await db.CustomerPackages.FirstAsync(x => x.Id == entryToPromote.CustomerPackageId, cancellationToken);
        if (package.ExpiryDate <= DateTimeOffset.UtcNow || package.RemainingCredits < 1)
        {
            entryToPromote.Status = WaitlistStatus.Cancelled;
            return;
        }

        package.RemainingCredits -= 1;
        entryToPromote.Status = WaitlistStatus.Promoted;
        entryToPromote.PromotedAt = DateTimeOffset.UtcNow;

        db.Bookings.Add(new Booking
        {
            CustomerId = entryToPromote.CustomerId,
            CustomerPackageId = entryToPromote.CustomerPackageId,
            TimetableScheduleId = timetableScheduleId,
            BookedAt = DateTimeOffset.UtcNow
        });
    }
}
