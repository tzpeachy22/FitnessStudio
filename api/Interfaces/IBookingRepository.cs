using api.Dtos;

namespace api.Interfaces;

public interface IBookingRepository
{
    Task<ApiResult> BookAsync(BookClassRequest request, CancellationToken cancellationToken);
    Task<ApiResult> CancelAsync(CancelBookingRequest request, CancellationToken cancellationToken);
    Task<ApiResult> JoinWaitlistAsync(JoinWaitlistRequest request, CancellationToken cancellationToken);
}
