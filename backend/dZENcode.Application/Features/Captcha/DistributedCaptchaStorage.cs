using dZENcode.Application.Abstractions;
using dZENcode.Application.Features.Captcha.DTOs;
using StackExchange.Redis;

namespace dZENcode.Application.Features.Captcha;

public class DistributedCaptchaStorage : ICaptchaStorage
{
    private const string Prefix = "dzen_";

    private readonly IConnectionMultiplexer _connectionMultiplexer;

    public DistributedCaptchaStorage(IConnectionMultiplexer connectionMultiplexer)
    {
        _connectionMultiplexer = connectionMultiplexer;
    }

    public async ValueTask CreateEntryAsync(CaptchaEntry entry, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();

        IDatabase database = _connectionMultiplexer.GetDatabase();

        string key = $"{Prefix}{entry.Token}";

        TimeSpan ttl = TimeSpan.FromMinutes(entry.TTLMinutes);
        
        await database.StringSetAsync(
            key, 
            entry.HashedAnswer, 
            ttl);
    }

    public async ValueTask<byte[]?> GetAndRemoveAsync(string token, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();

        IDatabase db = _connectionMultiplexer.GetDatabase();

        string key = $"{Prefix}{token}";

        RedisValue value = await db.StringGetDeleteAsync(key);

        return value.HasValue ? (byte[]) value : null;        
    }
}
