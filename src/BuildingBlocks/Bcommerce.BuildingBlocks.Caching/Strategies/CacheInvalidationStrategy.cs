using Bcommerce.BuildingBlocks.Caching.Abstractions;

namespace Bcommerce.BuildingBlocks.Caching.Strategies;

/// <summary>
/// Estratégia para invalidação de entradas de cache.
/// </summary>
/// <remarks>
/// Usado após operações de escrita para garantir consistência.
/// - Invalida cache após Create/Update/Delete de entidades
/// - Evita leitura de dados desatualizados (stale data)
/// - Use em conjunto com CacheAsideStrategy
/// 
/// Exemplo de uso:
/// <code>
/// public class AtualizarProdutoHandler : ICommandHandler&lt;AtualizarProdutoCommand&gt;
/// {
///     private readonly CacheInvalidationStrategy _invalidation;
///     
///     public async Task&lt;Result&gt; Handle(AtualizarProdutoCommand cmd)
///     {
///         await _repo.UpdateAsync(cmd.Produto);
///         await _unitOfWork.SaveChangesAsync();
///         
///         // Invalida cache após atualização
///         await _invalidation.InvalidateAsync($"produto:{cmd.Produto.Id}");
///         return Result.Success();
///     }
/// }
/// </code>
/// </remarks>
public class CacheInvalidationStrategy
{
    private readonly ICacheService _cacheService;

    public CacheInvalidationStrategy(ICacheService cacheService)
    {
        _cacheService = cacheService;
    }

    /// <summary>
    /// Invalida (remove) uma entrada do cache.
    /// </summary>
    /// <param name="key">Chave da entrada a ser invalidada.</param>
    /// <param name="cancellationToken">Token de cancelamento.</param>
    public async Task InvalidateAsync(string key, CancellationToken cancellationToken = default)
    {
        await _cacheService.RemoveAsync(key, cancellationToken);
    }
}
