using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;

namespace BuildingBlocks.Infrastructure.Persistence;

/// <summary>
/// Métodos de extensão para enriquecer <see cref="DbContext"/> com capacidades de UnitOfWork e Resiliência.
/// </summary>
/// <remarks>
/// <strong>Cenário de Uso:</strong>
/// Essencial para <see cref="DbContext"/>s que não podem herdar de <see cref="UnitOfWork"/> (ex: herança múltipla não permitida em C#, e classe já herda de <see cref="Microsoft.AspNetCore.Identity.EntityFrameworkCore.IdentityDbContext"/>).
/// 
/// <strong>Funcionalidades:</strong>
/// - Gestão de Transações (<see cref="ExecuteInTransactionAsync{TResult}"/>).
/// - Resiliência e Retries (<see cref="ExecuteWithRetryAsync{TResult}"/>).
/// - Manipulação segura do ChangeTracker.
/// </remarks>
public static class UnitOfWorkExtensions
{
    /// <summary>
    /// Salva entidades com tratamento de exceção.
    /// </summary>
    public static async Task<bool> SaveEntitiesAsync(
        this DbContext context,
        CancellationToken cancellationToken = default)
    {
        try
        {
            await context.SaveChangesAsync(cancellationToken);
            return true;
        }
        catch
        {
            return false;
        }
    }

    /// <summary>
    /// Inicia uma transação.
    /// </summary>
    public static async Task<IDbContextTransaction> BeginTransactionAsync(
        this DbContext context,
        CancellationToken cancellationToken = default)
    {
        return await context.Database.BeginTransactionAsync(cancellationToken);
    }

    /// <summary>
    /// Executa uma operação dentro de uma transação.
    /// </summary>
    /// <typeparam name="TResult">Tipo do resultado</typeparam>
    /// <param name="context">DbContext</param>
    /// <param name="operation">Operação a executar</param>
    /// <param name="cancellationToken">Token de cancelamento</param>
    /// <returns>Resultado da operação</returns>
    public static async Task<TResult> ExecuteInTransactionAsync<TResult>(
        this DbContext context,
        Func<Task<TResult>> operation,
        CancellationToken cancellationToken = default)
    {
        // Cria uma estratégia de execução (resiliência).
        // Isso é crucial para conexões instáveis (cloud), permitindo retries automáticos
        // se a conexão cair antes de iniciar a transação.
        var strategy = context.Database.CreateExecutionStrategy();

        return await strategy.ExecuteAsync(async () =>
        {
            await using var transaction = await context.Database.BeginTransactionAsync(cancellationToken);

            try
            {
                var result = await operation();
                await transaction.CommitAsync(cancellationToken);
                return result;
            }
            catch
            {
                await transaction.RollbackAsync(cancellationToken);
                throw;
            }
        });
    }

    /// <summary>
    /// Executa uma operação dentro de uma transação (sem retorno).
    /// </summary>
    public static async Task ExecuteInTransactionAsync(
        this DbContext context,
        Func<Task> operation,
        CancellationToken cancellationToken = default)
    {
        await context.ExecuteInTransactionAsync(async () =>
        {
            await operation();
            return true;
        }, cancellationToken);
    }

    /// <summary>
    /// Executa uma operação com retry automático em caso de conflito de concorrência.
    /// </summary>
    /// <typeparam name="TResult">Tipo do resultado</typeparam>
    /// <param name="context">DbContext</param>
    /// <param name="operation">Operação a executar</param>
    /// <param name="maxRetries">Número máximo de tentativas</param>
    /// <param name="cancellationToken">Token de cancelamento</param>
    /// <returns>Resultado da operação</returns>
    public static async Task<TResult> ExecuteWithRetryAsync<TResult>(
        this DbContext context,
        Func<Task<TResult>> operation,
        int maxRetries = 3,
        CancellationToken cancellationToken = default)
    {
        var retryCount = 0;

        while (true)
        {
            try
            {
                return await operation();
            }
            catch (DbUpdateConcurrencyException) when (retryCount < maxRetries)
            {
                retryCount++;

                // Limpa o ChangeTracker para permitir nova tentativa
                foreach (var entry in context.ChangeTracker.Entries())
                {
                    await entry.ReloadAsync(cancellationToken);
                }

                // Pequeno delay exponencial
                await Task.Delay(TimeSpan.FromMilliseconds(100 * Math.Pow(2, retryCount)), cancellationToken);
            }
        }
    }

    /// <summary>
    /// Limpa todas as entidades rastreadas pelo ChangeTracker.
    /// </summary>
    public static void ClearChangeTracker(this DbContext context)
    {
        context.ChangeTracker.Clear();
    }

    /// <summary>
    /// Descarta todas as mudanças pendentes.
    /// </summary>
    public static void DiscardChanges(this DbContext context)
    {
        foreach (var entry in context.ChangeTracker.Entries())
        {
            switch (entry.State)
            {
                case EntityState.Added:
                    entry.State = EntityState.Detached;
                    break;
                case EntityState.Modified:
                case EntityState.Deleted:
                    entry.Reload();
                    entry.State = EntityState.Unchanged;
                    break;
            }
        }
    }
}
