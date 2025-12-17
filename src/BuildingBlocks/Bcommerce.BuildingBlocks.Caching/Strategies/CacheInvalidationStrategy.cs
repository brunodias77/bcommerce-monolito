using Bcommerce.BuildingBlocks.Caching.Abstractions;

namespace Bcommerce.BuildingBlocks.Caching.Strategies;

public class CacheInvalidationStrategy
{
    private readonly ICacheService _cacheService;

    public CacheInvalidationStrategy(ICacheService cacheService)
    {
        _cacheService = cacheService;
    }

    public async Task InvalidateAsync(string key, CancellationToken cancellationToken = default)
    {
        await _cacheService.RemoveAsync(key, cancellationToken);
    }
}
