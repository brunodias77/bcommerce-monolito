namespace BuildingBlocks.Infrastructure.Caching;

/// <summary>
/// Interface para serviço de cache distribuído ou local.
/// Define os contratos para operações de cache agnósticas à implementação.
/// </summary>
/// <remarks>
/// <strong>Arquitetura de Caching:</strong>
/// Esta interface permite alternar entre implementações (Memory, Redis, SQL) sem alterar o código de negócio.
/// 
/// <strong>Padrões Suportados:</strong>
/// 1. <strong>Cache-Aside (Lazy Loading):</strong> O padrão mais comum, suportado via <see cref="GetOrCreateAsync{T}"/>.
///    - A aplicação tenta ler do cache.
///    - Se não encontrar (Miss), a aplicação busca na fonte de dados (Banco).
///    - A aplicação salva no cache e retorna o dado.
/// 2. <strong>Explicit Caching:</strong> Controle manual via <see cref="SetAsync{T}"/> e <see cref="GetAsync{T}"/>.
/// 
/// <strong>Exemplo de uso (Cache-Aside):</strong>
/// <code>
/// // O método GetOrCreateAsync encapsula toda a lógica do Cache-Aside
/// var product = await _cacheService.GetOrCreateAsync(
///     key: $"product:{id}",
///     factory: async () => await _repository.GetByIdAsync(id),
///     expiration: TimeSpan.FromMinutes(10));
/// </code>
/// </remarks>
public interface ICacheService
{
    /// <summary>
    /// Obtém um valor do cache.
    /// </summary>
    /// <typeparam name="T">Tipo do valor</typeparam>
    /// <param name="key">Chave do cache</param>
    /// <param name="cancellationToken">Token de cancelamento</param>
    /// <returns>Valor ou null se não existir</returns>
    Task<T?> GetAsync<T>(string key, CancellationToken cancellationToken = default);

    /// <summary>
    /// Define um valor no cache.
    /// </summary>
    /// <typeparam name="T">Tipo do valor</typeparam>
    /// <param name="key">Chave do cache</param>
    /// <param name="value">Valor a armazenar</param>
    /// <param name="expiration">Tempo de expiração (opcional)</param>
    /// <param name="cancellationToken">Token de cancelamento</param>
    Task SetAsync<T>(
        string key,
        T value,
        TimeSpan? expiration = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Obtém um valor do cache ou cria/busca na fonte se não existir (Cache-Aside Pattern).
    /// </summary>
    /// <remarks>
    /// <strong>Cache Stampede Protection:</strong>
    /// Implementações ideais devem prevenir que múltiplas threads executem a 'factory' simultaneamente para a mesma chave.
    /// </remarks>
    /// <typeparam name="T">Tipo do valor</typeparam>
    /// <param name="key">Chave única do cache</param>
    /// <param name="factory">Função assíncrona que busca o dado na fonte original (ex: Database)</param>
    /// <param name="expiration">Tempo de expiração (TTL).</param>
    /// <param name="cancellationToken">Token de cancelamento</param>
    /// <returns>O valor do cache (Hit) ou o novo valor gerado pela factory (Miss)</returns>
    Task<T?> GetOrCreateAsync<T>(
        string key,
        Func<Task<T>> factory,
        TimeSpan? expiration = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Remove um valor do cache.
    /// </summary>
    /// <param name="key">Chave do cache</param>
    /// <param name="cancellationToken">Token de cancelamento</param>
    Task RemoveAsync(string key, CancellationToken cancellationToken = default);

    /// <summary>
    /// Remove todos os valores que começam com um prefixo.
    /// </summary>
    /// <param name="prefix">Prefixo das chaves</param>
    /// <param name="cancellationToken">Token de cancelamento</param>
    /// <remarks>
    /// Esta operação pode não ser suportada por todos os provedores.
    /// Para IMemoryCache, requer rastreamento manual das chaves.
    /// </remarks>
    Task RemoveByPrefixAsync(string prefix, CancellationToken cancellationToken = default);

    /// <summary>
    /// Verifica se uma chave existe no cache.
    /// </summary>
    /// <param name="key">Chave do cache</param>
    /// <param name="cancellationToken">Token de cancelamento</param>
    /// <returns>True se existir</returns>
    Task<bool> ExistsAsync(string key, CancellationToken cancellationToken = default);

    /// <summary>
    /// Atualiza o tempo de expiração de uma chave.
    /// </summary>
    /// <param name="key">Chave do cache</param>
    /// <param name="expiration">Novo tempo de expiração</param>
    /// <param name="cancellationToken">Token de cancelamento</param>
    /// <returns>True se a chave existia e foi atualizada</returns>
    Task<bool> RefreshAsync(
        string key,
        TimeSpan expiration,
        CancellationToken cancellationToken = default);
}

/// <summary>
/// Opções de configuração do cache.
/// </summary>
public class CacheOptions
{
    /// <summary>
    /// Tempo de expiração padrão.
    /// </summary>
    public TimeSpan DefaultExpiration { get; set; } = TimeSpan.FromMinutes(5);

    /// <summary>
    /// Prefixo para todas as chaves (útil para ambientes compartilhados).
    /// </summary>
    public string? KeyPrefix { get; set; }

    /// <summary>
    /// Se true, expiração usa sliding window (renovada a cada acesso).
    /// </summary>
    public bool UseSlidingExpiration { get; set; } = false;
}
