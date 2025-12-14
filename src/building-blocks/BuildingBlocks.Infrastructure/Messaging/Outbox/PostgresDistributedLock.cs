using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System.Security.Cryptography;
using System.Text;

namespace BuildingBlocks.Infrastructure.Messaging.Outbox;

/// <summary>
/// Implementação de distributed lock usando PostgreSQL Advisory Locks.
/// </summary>
/// <remarks>
/// PostgreSQL Advisory Locks são locks de aplicação gerenciados pelo banco:
/// - pg_try_advisory_lock(key): Tenta adquirir lock sem bloquear
/// - pg_advisory_lock(key): Adquire lock (bloqueia até conseguir)
/// - pg_advisory_unlock(key): Libera o lock
///
/// Vantagens:
/// - Não requer infraestrutura adicional (já usa PostgreSQL)
/// - Locks são liberados automaticamente se a conexão cair
/// - Alta performance e confiabilidade
///
/// O lock é identificado por um bigint (64 bits). Esta implementação
/// converte a string lockKey em um hash determinístico.
/// </remarks>
public class PostgresDistributedLock : IDistributedLock
{
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly ILogger<PostgresDistributedLock> _logger;

    public PostgresDistributedLock(
        IServiceScopeFactory scopeFactory,
        ILogger<PostgresDistributedLock> logger)
    {
        _scopeFactory = scopeFactory;
        _logger = logger;
    }

    public async Task<IDistributedLockHandle?> TryAcquireAsync(
        string lockKey,
        TimeSpan? timeout = null,
        CancellationToken cancellationToken = default)
    {
        var lockId = ComputeLockId(lockKey);
        var scope = _scopeFactory.CreateScope();

        try
        {
            var dbContext = scope.ServiceProvider.GetRequiredService<DbContext>();
            var connection = dbContext.Database.GetDbConnection();

            // Abre conexão se necessário
            if (connection.State != System.Data.ConnectionState.Open)
            {
                await connection.OpenAsync(cancellationToken);
            }

            bool acquired;

            if (timeout.HasValue && timeout.Value > TimeSpan.Zero)
            {
                // Tenta adquirir com timeout (polling simples)
                var deadline = DateTime.UtcNow.Add(timeout.Value);
                acquired = false;

                while (DateTime.UtcNow < deadline && !cancellationToken.IsCancellationRequested)
                {
                    acquired = await TryAcquireLockAsync(dbContext, lockId, cancellationToken);
                    if (acquired)
                        break;

                    await Task.Delay(100, cancellationToken);
                }
            }
            else
            {
                // Tenta adquirir sem bloquear
                acquired = await TryAcquireLockAsync(dbContext, lockId, cancellationToken);
            }

            if (acquired)
            {
                _logger.LogDebug("Acquired distributed lock '{LockKey}' (id: {LockId})", lockKey, lockId);
                return new PostgresLockHandle(scope, dbContext, lockId, lockKey, _logger);
            }

            _logger.LogDebug("Failed to acquire distributed lock '{LockKey}' (id: {LockId})", lockKey, lockId);
            scope.Dispose();
            return null;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error acquiring distributed lock '{LockKey}'", lockKey);
            scope.Dispose();
            return null;
        }
    }

    private static async Task<bool> TryAcquireLockAsync(
        DbContext dbContext,
        long lockId,
        CancellationToken cancellationToken)
    {
        // pg_try_advisory_lock retorna true se adquiriu o lock
        // lockId é calculado internamente (hash), não vem de entrada externa
        var result = await dbContext.Database
            .SqlQuery<bool>($"SELECT pg_try_advisory_lock({lockId})")
            .FirstOrDefaultAsync(cancellationToken);

        return result;
    }

    /// <summary>
    /// Converte uma string em um lockId determinístico de 64 bits.
    /// </summary>
    private static long ComputeLockId(string lockKey)
    {
        var bytes = SHA256.HashData(Encoding.UTF8.GetBytes(lockKey));
        return BitConverter.ToInt64(bytes, 0);
    }

    private sealed class PostgresLockHandle : IDistributedLockHandle
    {
        private readonly IServiceScope _scope;
        private readonly DbContext _dbContext;
        private readonly long _lockId;
        private readonly string _lockKey;
        private readonly ILogger _logger;
        private bool _isAcquired = true;
        private bool _disposed;

        public bool IsAcquired => _isAcquired && !_disposed;

        public PostgresLockHandle(
            IServiceScope scope,
            DbContext dbContext,
            long lockId,
            string lockKey,
            ILogger logger)
        {
            _scope = scope;
            _dbContext = dbContext;
            _lockId = lockId;
            _lockKey = lockKey;
            _logger = logger;
        }

        public async ValueTask DisposeAsync()
        {
            if (_disposed)
                return;

            _disposed = true;

            try
            {
                if (_isAcquired)
                {
                    // lockId é calculado internamente (hash), não vem de entrada externa
                    await _dbContext.Database
                        .ExecuteSqlAsync($"SELECT pg_advisory_unlock({_lockId})");

                    _isAcquired = false;
                    _logger.LogDebug("Released distributed lock '{LockKey}' (id: {LockId})", _lockKey, _lockId);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error releasing distributed lock '{LockKey}'", _lockKey);
            }
            finally
            {
                _scope.Dispose();
            }
        }
    }
}

/// <summary>
/// Implementação no-op para desenvolvimento e testes single-instance.
/// </summary>
/// <remarks>
/// Use esta implementação quando:
/// - Rodando apenas uma instância da aplicação
/// - Em ambiente de desenvolvimento local
/// - Em testes unitários
///
/// ATENÇÃO: NÃO USE em produção com múltiplas instâncias!
/// </remarks>
public class NoOpDistributedLock : IDistributedLock
{
    public Task<IDistributedLockHandle?> TryAcquireAsync(
        string lockKey,
        TimeSpan? timeout = null,
        CancellationToken cancellationToken = default)
    {
        return Task.FromResult<IDistributedLockHandle?>(new NoOpLockHandle());
    }

    private sealed class NoOpLockHandle : IDistributedLockHandle
    {
        public bool IsAcquired => true;
        public ValueTask DisposeAsync() => ValueTask.CompletedTask;
    }
}
