using System.Collections.Concurrent;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace BuildingBlocks.Infrastructure.Caching;

/// <summary>
/// Implementação em memória (In-Process) do ICacheService.
/// </summary>
/// <remarks>
/// <strong>Estratégia de Key Tracking:</strong>
/// O <see cref="IMemoryCache"/> nativo do .NET Core NÃO suporta get ou remove por padrão/prefixo.
/// Para contornar isso, esta implementação mantém um <see cref="ConcurrentDictionary{TKey,TValue}"/> auxiliar (_keys)
/// que registra todas as chaves ativas.
/// 
/// <strong>Ciclo de Vida:</strong>
/// 1. Ao adicionar (Set): A chave é adicionada ao dicionário auxiliar.
/// 2. Ao remover (Remove): A chave é removida do dicionário auxiliar.
/// 3. Ao expirar (Eviction): Um callback (PostEvictionCallback) limpa a chave do dicionário auxiliar.
/// 
/// <strong>Limitações:</strong>
/// - A memória é do processo local (não compartilhado em cluster).
/// - Reiniciar a aplicação limpa todo o cache.
/// 
/// Ideal para: Dados de referência pequenos, tabelas de domínio estáticas, tokens de curta duração.
/// </remarks>
public class MemoryCacheService : ICacheService
{
    private readonly IMemoryCache _cache;
    private readonly CacheOptions _options;
    private readonly ILogger<MemoryCacheService>? _logger;

    // Rastreia chaves para suportar RemoveByPrefix
    private readonly ConcurrentDictionary<string, byte> _keys = new();

    public MemoryCacheService(
        IMemoryCache cache,
        IOptions<CacheOptions>? options = null,
        ILogger<MemoryCacheService>? logger = null)
    {
        _cache = cache;
        _options = options?.Value ?? new CacheOptions();
        _logger = logger;
    }

    public Task<T?> GetAsync<T>(string key, CancellationToken cancellationToken = default)
    {
        var fullKey = GetFullKey(key);
        var value = _cache.Get<T>(fullKey);

        _logger?.LogDebug("Cache {Hit} for key {Key}",
            value != null ? "HIT" : "MISS", key);

        return Task.FromResult(value);
    }

    public Task SetAsync<T>(
        string key,
        T value,
        TimeSpan? expiration = null,
        CancellationToken cancellationToken = default)
    {
        var fullKey = GetFullKey(key);
        var effectiveExpiration = expiration ?? _options.DefaultExpiration;

        var cacheOptions = new MemoryCacheEntryOptions();

        if (_options.UseSlidingExpiration)
        {
            cacheOptions.SlidingExpiration = effectiveExpiration;
        }
        else
        {
            cacheOptions.AbsoluteExpirationRelativeToNow = effectiveExpiration;
        }

        // Callback CRÍTICO para gerenciamento de memória:
        // Quando o item expira ou é removido por pressão de memória do IMemoryCache,
        // ele DEVE ser removido do nosso dicionário de chaves (_keys).
        // Se isso não ocorrer, teremos um memory leak no dicionário _keys.
        cacheOptions.RegisterPostEvictionCallback((evictedKey, _, _, _) =>
        {
            _keys.TryRemove(evictedKey.ToString()!, out _);
        });

        _cache.Set(fullKey, value, cacheOptions);
        _keys.TryAdd(fullKey, 0);

        _logger?.LogDebug("Cache SET for key {Key}, expiration: {Expiration}",
            key, effectiveExpiration);

        return Task.CompletedTask;
    }

    public async Task<T?> GetOrCreateAsync<T>(
        string key,
        Func<Task<T>> factory,
        TimeSpan? expiration = null,
        CancellationToken cancellationToken = default)
    {
        var value = await GetAsync<T>(key, cancellationToken);

        if (value != null)
            return value;

        value = await factory();

        if (value != null)
        {
            await SetAsync(key, value, expiration, cancellationToken);
        }

        return value;
    }

    public Task RemoveAsync(string key, CancellationToken cancellationToken = default)
    {
        var fullKey = GetFullKey(key);

        _cache.Remove(fullKey);
        _keys.TryRemove(fullKey, out _);

        _logger?.LogDebug("Cache REMOVE for key {Key}", key);

        return Task.CompletedTask;
    }

    public Task RemoveByPrefixAsync(string prefix, CancellationToken cancellationToken = default)
    {
        var fullPrefix = GetFullKey(prefix);
        
        // CUIDADO: Esta operação é O(N) onde N é o número total de chaves no cache.
        // Em um cache muito grande, isso pode ser lento.
        // O uso do ConcurrentDictionary ajuda na thread-safety, mas não elimina o custo de iteração.
        var keysToRemove = _keys.Keys
            .Where(k => k.StartsWith(fullPrefix))
            .ToList();

        foreach (var key in keysToRemove)
        {
            _cache.Remove(key);
            _keys.TryRemove(key, out _);
        }

        _logger?.LogDebug("Cache REMOVE BY PREFIX {Prefix}, removed {Count} keys",
            prefix, keysToRemove.Count);

        return Task.CompletedTask;
    }

    public Task<bool> ExistsAsync(string key, CancellationToken cancellationToken = default)
    {
        var fullKey = GetFullKey(key);
        var exists = _cache.TryGetValue(fullKey, out _);

        return Task.FromResult(exists);
    }

    public async Task<bool> RefreshAsync(
        string key,
        TimeSpan expiration,
        CancellationToken cancellationToken = default)
    {
        var fullKey = GetFullKey(key);

        if (!_cache.TryGetValue(fullKey, out var value))
            return false;

        // Re-set com nova expiração
        await SetAsync(key, value, expiration, cancellationToken);
        return true;
    }

    private string GetFullKey(string key)
    {
        return string.IsNullOrEmpty(_options.KeyPrefix)
            ? key
            : $"{_options.KeyPrefix}:{key}";
    }
}

/// <summary>
/// Extensões para configurar o MemoryCacheService.
/// </summary>
public static class MemoryCacheServiceExtensions
{
    /// <summary>
    /// Adiciona o MemoryCacheService ao DI.
    /// </summary>
    public static IServiceCollection AddMemoryCacheService(
        this IServiceCollection services,
        Action<CacheOptions>? configure = null)
    {
        services.AddMemoryCache();

        if (configure != null)
        {
            services.Configure(configure);
        }

        services.AddSingleton<ICacheService, MemoryCacheService>();

        return services;
    }
}
