using Bcommerce.BuildingBlocks.Domain.Abstractions;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;

namespace Bcommerce.BuildingBlocks.Infrastructure.Data.Interceptors;

/// <summary>
/// Interceptor para controle de concorrência otimista.
/// </summary>
/// <remarks>
/// Incrementa automaticamente a versão da entidade em atualizações.
/// - Previne "Lost Updates"
/// - Atua sobre entidades que implementam IVersionable
/// - O EF Core lança DbUpdateConcurrencyException se houver conflito
/// 
/// Exemplo de uso:
/// <code>
/// // Registrado no BaseDbContext
/// </code>
/// </remarks>
public class OptimisticLockInterceptor : SaveChangesInterceptor
{
    public override InterceptionResult<int> SavingChanges(DbContextEventData eventData, InterceptionResult<int> result)
    {
        UpdateEntities(eventData.Context);
        return base.SavingChanges(eventData, result);
    }

    public override ValueTask<InterceptionResult<int>> SavingChangesAsync(DbContextEventData eventData, InterceptionResult<int> result, CancellationToken cancellationToken = default)
    {
        UpdateEntities(eventData.Context);
        return base.SavingChangesAsync(eventData, result, cancellationToken);
    }

    private void UpdateEntities(DbContext? context)
    {
        if (context == null) return;

        var entries = context.ChangeTracker.Entries<IVersionable>();

        foreach (var entry in entries)
        {
            if (entry.State == EntityState.Modified)
            {
                entry.Entity.Version++;
            }
        }
    }
}
