using BuildingBlocks.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;

namespace BuildingBlocks.Infrastructure.Persistence.Interceptors;

/// <summary>
/// Interceptor que incrementa automaticamente a versão para Optimistic Concurrency Control.
/// </summary>
/// <remarks>
/// Este interceptor funciona EM CONJUNTO com o trigger do PostgreSQL:
/// 
/// PostgreSQL Trigger (seu schema):
/// - CREATE TRIGGER trg_xxx_version BEFORE UPDATE
/// - EXECUTE FUNCTION shared.trigger_increment_version()
/// 
/// Este Interceptor (EF Core):
/// - Incrementa version ANTES de salvar
/// - EF Core usa IsConcurrencyToken() para detectar conflitos
/// - Se version no banco ≠ version esperada → DbUpdateConcurrencyException
/// 
/// Fluxo de Concorrência Otimista:
/// 
/// Thread A                          Thread B
/// ────────                          ────────
/// 1. Lê Order (version=1)           1. Lê Order (version=1)
/// 2. Modifica Order                 2. Modifica Order
/// 3. SaveChanges()                  3. SaveChanges()
///    → version=2 ✓                     → ERRO! (version esperada=1, real=2)
///    
/// Vantagens:
/// - Sem locks pessimistas
/// - Alto throughput
/// - Detecta conflitos automaticamente
/// 
/// Exemplo de tratamento:
/// <code>
/// try
/// {
///     await dbContext.SaveChangesAsync();
/// }
/// catch (DbUpdateConcurrencyException ex)
/// {
///     // Recarregar entidade e tentar novamente
///     await entry.ReloadAsync();
///     // Reaplicar mudanças ou notificar usuário
/// }
/// </code>
/// </remarks>
public sealed class OptimisticConcurrencyInterceptor : SaveChangesInterceptor
{
    public override InterceptionResult<int> SavingChanges(
        DbContextEventData eventData,
        InterceptionResult<int> result)
    {
        IncrementVersion(eventData.Context);
        return base.SavingChanges(eventData, result);
    }

    public override ValueTask<InterceptionResult<int>> SavingChangesAsync(
        DbContextEventData eventData,
        InterceptionResult<int> result,
        CancellationToken cancellationToken = default)
    {
        IncrementVersion(eventData.Context);
        return base.SavingChangesAsync(eventData, result, cancellationToken);
    }

    private static void IncrementVersion(DbContext? context)
    {
        if (context is null)
            return;

        foreach (var entry in context.ChangeTracker.Entries<AggregateRoot>())
        {
            if (entry.State == EntityState.Modified)
            {
                // Incrementa version
                var currentVersion = entry.Property("Version").CurrentValue as int? ?? 1;
                entry.Property("Version").CurrentValue = currentVersion + 1;
            }
        }
    }
}

/// <summary>
/// Extensões para trabalhar com Optimistic Concurrency.
/// </summary>
public static class OptimisticConcurrencyExtensions
{
    /// <summary>
    /// Recarrega a entidade do banco de dados (útil após concurrency exception).
    /// </summary>
    public static async Task ReloadAsync<TEntity>(
        this DbContext context,
        TEntity entity,
        CancellationToken cancellationToken = default)
        where TEntity : AggregateRoot
    {
        var entry = context.Entry(entity);
        await entry.ReloadAsync(cancellationToken);
    }

    /// <summary>
    /// Tenta salvar com retry automático em caso de conflito de concorrência.
    /// </summary>
    public static async Task<bool> SaveChangesWithRetryAsync(
        this DbContext context,
        int maxRetries = 3,
        CancellationToken cancellationToken = default)
    {
        var retries = 0;

        while (retries < maxRetries)
        {
            try
            {
                await context.SaveChangesAsync(cancellationToken);
                return true;
            }
            catch (DbUpdateConcurrencyException)
            {
                retries++;

                if (retries >= maxRetries)
                    throw;

                // Recarrega todas as entidades modificadas
                foreach (var entry in context.ChangeTracker.Entries()
                    .Where(e => e.State == EntityState.Modified))
                {
                    await entry.ReloadAsync(cancellationToken);
                }

                // Aguarda um pouco antes de tentar novamente
                await Task.Delay(TimeSpan.FromMilliseconds(100 * retries), cancellationToken);
            }
        }

        return false;
    }
}