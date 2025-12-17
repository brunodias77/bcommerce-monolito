using Bcommerce.BuildingBlocks.Caching.Abstractions;

namespace Bcommerce.BuildingBlocks.Caching.Strategies;

public class CacheAsideStrategy
{
    private readonly ICacheService _cacheService;

    public CacheAsideStrategy(ICacheService cacheService)
    {
        _cacheService = cacheService;
    }

    public async Task<T> GetOrCreateAsync<T>(string key, Func<Task<T>> factory, TimeSpan? expiration = null, CancellationToken cancellationToken = default)
    {
        var cacheValue = await _cacheService.GetAsync<T>(key, cancellationToken);
        if (cacheValue != null)
        {
            return cacheValue;
        }

        var value = await factory();
        if (value != null)
        {
            await _cacheService.SetAsync(key, value, expiration, cancellationToken);
        }

        return value;
    }
}
