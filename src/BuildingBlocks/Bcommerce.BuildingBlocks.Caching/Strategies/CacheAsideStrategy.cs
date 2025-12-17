using Bcommerce.BuildingBlocks.Caching.Abstractions;

namespace Bcommerce.BuildingBlocks.Caching.Strategies;

/// <summary>
/// Implementação do padrão Cache-Aside (Lazy Loading).
/// </summary>
/// <remarks>
/// Padrão que carrega dados no cache sob demanda.
/// - Primeiro verifica se o dado está em cache
/// - Se não estiver, busca da fonte (banco) e armazena no cache
/// - Reduz carga no banco para dados frequentemente acessados
/// 
/// Exemplo de uso:
/// <code>
/// public class ProdutoQueryHandler
/// {
///     private readonly CacheAsideStrategy _cache;
///     private readonly IProdutoRepository _repo;
///     
///     public async Task&lt;ProdutoDto&gt; Handle(ObterProdutoQuery query)
///     {
///         return await _cache.GetOrCreateAsync(
///             $"produto:{query.Id}",
///             async () => await _repo.ObterPorIdAsync(query.Id),
///             TimeSpan.FromMinutes(30));
///     }
/// }
/// </code>
/// </remarks>
public class CacheAsideStrategy
{
    private readonly ICacheService _cacheService;

    public CacheAsideStrategy(ICacheService cacheService)
    {
        _cacheService = cacheService;
    }

    /// <summary>
    /// Obtém um valor do cache ou cria usando a factory se não existir.
    /// </summary>
    /// <typeparam name="T">Tipo do valor.</typeparam>
    /// <param name="key">Chave do cache.</param>
    /// <param name="factory">Função que obtém o valor caso não esteja em cache.</param>
    /// <param name="expiration">Tempo de expiração.</param>
    /// <param name="cancellationToken">Token de cancelamento.</param>
    /// <returns>Valor do cache ou criado pela factory.</returns>
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
