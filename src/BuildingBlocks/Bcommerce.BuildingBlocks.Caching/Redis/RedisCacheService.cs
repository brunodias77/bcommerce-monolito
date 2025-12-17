using System.Text;
using Bcommerce.BuildingBlocks.Caching.Abstractions;
using Microsoft.Extensions.Caching.Distributed;
using Newtonsoft.Json;

namespace Bcommerce.BuildingBlocks.Caching.Redis;

/// <summary>
/// Implementação de cache distribuído usando Redis.
/// </summary>
/// <remarks>
/// Ideal para produção e aplicações multi-instance.
/// - Dados persistem entre reinicializações da aplicação
/// - Compartilha cache entre múltiplas instâncias (escalável)
/// - Serializa objetos em JSON para armazenamento
/// 
/// Exemplo de uso:
/// <code>
/// // Registrado automaticamente via AddCachingServices quando Redis está configurado
/// // appsettings.json:
/// {
///   "RedisSettings": {
///     "ConnectionString": "redis.servidor.com:6379,password=senha"
///   }
/// }
/// 
/// // Uso é transparente via ICacheService
/// var produto = await _cache.GetAsync&lt;ProdutoDto&gt;("produto:123");
/// </code>
/// </remarks>
public class RedisCacheService : ICacheService
{
    private readonly IDistributedCache _distributedCache;

    public RedisCacheService(IDistributedCache distributedCache)
    {
        _distributedCache = distributedCache;
    }

    public async Task<T?> GetAsync<T>(string key, CancellationToken cancellationToken = default)
    {
        var value = await _distributedCache.GetStringAsync(key, cancellationToken);
        if (string.IsNullOrEmpty(value))
        {
            return default;
        }

        return JsonConvert.DeserializeObject<T>(value);
    }

    public async Task SetAsync<T>(string key, T value, TimeSpan? expiration = null, CancellationToken cancellationToken = default)
    {
        var options = new DistributedCacheEntryOptions();
        if (expiration.HasValue)
        {
            options.AbsoluteExpirationRelativeToNow = expiration;
        }

        var json = JsonConvert.SerializeObject(value);
        await _distributedCache.SetStringAsync(key, json, options, cancellationToken);
    }

    public async Task RemoveAsync(string key, CancellationToken cancellationToken = default)
    {
        await _distributedCache.RemoveAsync(key, cancellationToken);
    }

    public async Task RefreshAsync(string key, CancellationToken cancellationToken = default)
    {
        await _distributedCache.RefreshAsync(key, cancellationToken);
    }
}
