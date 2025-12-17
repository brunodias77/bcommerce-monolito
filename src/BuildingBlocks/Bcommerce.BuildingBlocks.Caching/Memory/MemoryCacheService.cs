using Bcommerce.BuildingBlocks.Caching.Abstractions;
using Microsoft.Extensions.Caching.Memory;

namespace Bcommerce.BuildingBlocks.Caching.Memory;

public class MemoryCacheService : ICacheService
{
    private readonly IMemoryCache _memoryCache;

    public MemoryCacheService(IMemoryCache memoryCache)
    {
        _memoryCache = memoryCache;
    }

    public Task<T?> GetAsync<T>(string key, CancellationToken cancellationToken = default)
    {
        if (_memoryCache.TryGetValue(key, out T? value))
        {
            return Task.FromResult(value);
        }

        return Task.FromResult(default(T));
    }

    public Task SetAsync<T>(string key, T value, TimeSpan? expiration = null, CancellationToken cancellationToken = default)
    {
        var options = new MemoryCacheEntryOptions();
        if (expiration.HasValue)
        {
            options.AbsoluteExpirationRelativeToNow = expiration;
        }

        _memoryCache.Set(key, value, options);
        return Task.CompletedTask;
    }

    public Task RemoveAsync(string key, CancellationToken cancellationToken = default)
    {
        _memoryCache.Remove(key);
        return Task.CompletedTask;
    }

    public Task RefreshAsync(string key, CancellationToken cancellationToken = default)
    {
        // IMemoryCache doesn't support refresh in the same way as DistributedCache for sliding expiration if not set explicitly, 
        // but looking up the item refreshes it if sliding expiration is used.
        // Here we just get it to simulate touch.
        _memoryCache.TryGetValue(key, out _);
        return Task.CompletedTask;
    }
}
