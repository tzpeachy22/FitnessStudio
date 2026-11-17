using api.Interfaces;
using StackExchange.Redis;

namespace api.Repository;

public sealed class RedisLock(IConnectionMultiplexer connectionMultiplexer) : IRedisLock
{
    public async Task<IAsyncDisposable?> AcquireAsync(string key, TimeSpan expiry, CancellationToken cancellationToken)
    {
        var database = connectionMultiplexer.GetDatabase();
        var lockKey = $"lock:{key}";
        var token = Guid.NewGuid().ToString("N");

        for (var attempt = 0; attempt < 5; attempt++)
        {
            cancellationToken.ThrowIfCancellationRequested();

            if (await database.StringSetAsync(lockKey, token, expiry, When.NotExists))
            {
                return new RedisLockHandle(database, lockKey, token);
            }

            await Task.Delay(100, cancellationToken);
        }

        return null;
    }

    private sealed class RedisLockHandle(IDatabase database, string lockKey, string token) : IAsyncDisposable
    {
        public async ValueTask DisposeAsync()
        {
            const string script = """
                if redis.call('get', KEYS[1]) == ARGV[1] then
                    return redis.call('del', KEYS[1])
                else
                    return 0
                end
                """;

            await database.ScriptEvaluateAsync(script, [lockKey], [token]);
        }
    }
}
