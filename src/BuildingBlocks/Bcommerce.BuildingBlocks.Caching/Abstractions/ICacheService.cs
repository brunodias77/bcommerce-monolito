namespace Bcommerce.BuildingBlocks.Caching.Abstractions;

/// <summary>
/// Contrato para operações de cache distribuído ou em memória.
/// </summary>
/// <remarks>
/// Abstração que permite trocar implementações (Redis/Memory) sem alterar código de negócio.
/// - Suporta serialização automática de objetos complexos
/// - Operações assíncronas com suporte a CancellationToken
/// - Expiração configurável por entrada
/// 
/// Exemplo de uso:
/// <code>
/// public class ProdutoQueryHandler
/// {
///     private readonly ICacheService _cache;
///     
///     public async Task&lt;ProdutoDto&gt; Handle(ObterProdutoQuery query)
///     {
///         var key = $"produto:{query.Id}";
///         var produto = await _cache.GetAsync&lt;ProdutoDto&gt;(key);
///         if (produto == null)
///         {
///             produto = await _repo.ObterPorIdAsync(query.Id);
///             await _cache.SetAsync(key, produto, TimeSpan.FromMinutes(10));
///         }
///         return produto;
///     }
/// }
/// </code>
/// </remarks>
public interface ICacheService
{
    /// <summary>Obtém um valor do cache pela chave.</summary>
    Task<T?> GetAsync<T>(string key, CancellationToken cancellationToken = default);
    /// <summary>Armazena um valor no cache com expiração opcional.</summary>
    Task SetAsync<T>(string key, T value, TimeSpan? expiration = null, CancellationToken cancellationToken = default);
    /// <summary>Remove uma entrada do cache.</summary>
    Task RemoveAsync(string key, CancellationToken cancellationToken = default);
    /// <summary>Renova o tempo de expiração de uma entrada (sliding expiration).</summary>
    Task RefreshAsync(string key, CancellationToken cancellationToken = default);
}
