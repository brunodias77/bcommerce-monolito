using Bcommerce.BuildingBlocks.Application.Abstractions.Services;
using Bcommerce.BuildingBlocks.Domain.Abstractions;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;

namespace Bcommerce.BuildingBlocks.Infrastructure.Data.Interceptors;

/// <summary>
/// Interceptor para implementação de Soft Delete (Exclusão Lógica).
/// </summary>
/// <remarks>
/// Intercepta comandos de delete e os converte em update.
/// - Marca IsDeleted = true
/// - Preenche DeletedAt com data atual
/// - Altera o estado da entidade de Deleted para Modified
/// 
/// Exemplo de uso:
/// <code>
/// // Registrado no BaseDbContext
/// </code>
/// </remarks>
public class SoftDeleteInterceptor(IDateTimeProvider dateTimeProvider) : SaveChangesInterceptor
{
    private readonly IDateTimeProvider _dateTimeProvider = dateTimeProvider;

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

        var entries = context.ChangeTracker.Entries<ISoftDeletable>()
            .Where(e => e.State == EntityState.Deleted);

        foreach (var entry in entries)
        {
            entry.State = EntityState.Modified;
            entry.Entity.IsDeleted = true;
            entry.Entity.DeletedAt = _dateTimeProvider.UtcNow;
        }
    }
}
