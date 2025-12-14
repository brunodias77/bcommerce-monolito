namespace BuildingBlocks.Infrastructure.Caching;

/// <summary>
/// Interface para serviço de cache.
/// </summary>
/// <remarks>
/// Abstração sobre diferentes provedores de cache:
/// - IMemoryCache (in-process)
/// - Redis (distributed)
/// - SQL Server (distributed)
/// 
/// Uso:
/// <code>
/// // Obter ou criar
/// var user = await cacheService.GetOrCreateAsync(
///     $"user:{userId}",
///     async () => await userRepository.GetByIdAsync(userId),
///     TimeSpan.FromMinutes(5));
/// 
/// // Remover
/// await cacheService.RemoveAsync($"user:{userId}");
/// 
/// // Remover por padrão
/// await cacheService.RemoveByPrefixAsync("user:");
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
    /// Obtém um valor do cache ou cria se não existir.
    /// </summary>
    /// <typeparam name="T">Tipo do valor</typeparam>
    /// <param name="key">Chave do cache</param>
    /// <param name="factory">Função para criar o valor</param>
    /// <param name="expiration">Tempo de expiração (opcional)</param>
    /// <param name="cancellationToken">Token de cancelamento</param>
    /// <returns>Valor do cache ou criado</returns>
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
