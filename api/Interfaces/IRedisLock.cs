using api.Models;
using Microsoft.EntityFrameworkCore;

namespace api.Interfaces;

public interface IRedisLock
{
    Task<IAsyncDisposable?> AcquireAsync(string key, TimeSpan expiry, CancellationToken cancellationToken);
}
