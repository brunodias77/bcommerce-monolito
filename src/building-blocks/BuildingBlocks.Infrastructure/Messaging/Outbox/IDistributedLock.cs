namespace BuildingBlocks.Infrastructure.Messaging.Outbox;

/// <summary>
/// Interface para distributed lock, permitindo coordenação entre múltiplas instâncias.
/// </summary>
/// <remarks>
/// Uso típico:
/// <code>
/// await using var handle = await _lock.TryAcquireAsync("outbox:processing");
/// if (handle != null)
/// {
///     // Possui o lock - pode processar
///     await ProcessMessagesAsync();
/// }
/// // Lock é liberado automaticamente ao sair do using
/// </code>
///
/// Implementações disponíveis:
/// - PostgresDistributedLock: Usa pg_advisory_lock (recomendado para este projeto)
/// - NoOpDistributedLock: Sem lock (para desenvolvimento/testes single-instance)
/// </remarks>
public interface IDistributedLock
{
    /// <summary>
    /// Tenta adquirir um lock distribuído.
    /// </summary>
    /// <param name="lockKey">Chave única para o lock</param>
    /// <param name="timeout">Tempo máximo para aguardar o lock (null = não bloqueia)</param>
    /// <param name="cancellationToken">Token de cancelamento</param>
    /// <returns>Handle do lock se adquirido, null caso contrário</returns>
    Task<IDistributedLockHandle?> TryAcquireAsync(
        string lockKey,
        TimeSpan? timeout = null,
        CancellationToken cancellationToken = default);
}

/// <summary>
/// Handle para um lock distribuído adquirido.
/// </summary>
/// <remarks>
/// Implemente IAsyncDisposable para liberar o lock automaticamente.
/// </remarks>
public interface IDistributedLockHandle : IAsyncDisposable
{
    /// <summary>
    /// Indica se o lock ainda está ativo.
    /// </summary>
    bool IsAcquired { get; }
}
