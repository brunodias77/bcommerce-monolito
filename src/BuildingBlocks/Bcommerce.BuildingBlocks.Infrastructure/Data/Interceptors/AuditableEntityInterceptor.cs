using Bcommerce.BuildingBlocks.Application.Abstractions.Services;
using Bcommerce.BuildingBlocks.Domain.Abstractions;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;

namespace Bcommerce.BuildingBlocks.Infrastructure.Data.Interceptors;

public class AuditableEntityInterceptor(IDateTimeProvider dateTimeProvider) : SaveChangesInterceptor
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

        var entries = context.ChangeTracker.Entries<IEntity>();

        foreach (var entry in entries)
        {
            if (entry.State == EntityState.Added)
            {
                entry.Entity.CreatedAt = _dateTimeProvider.UtcNow;
                entry.Entity.UpdatedAt = _dateTimeProvider.UtcNow;
            }

            if (entry.State == EntityState.Modified || entry.HasChangedOwnedEntities())
            {
                entry.Entity.UpdatedAt = _dateTimeProvider.UtcNow;
            }
        }
    }
}

public static class EntityEntryExtensions
{
    public static bool HasChangedOwnedEntities(this Microsoft.EntityFrameworkCore.ChangeTracking.EntityEntry entry) =>
        entry.References.Any(r => 
            r.TargetEntry != null && 
            r.TargetEntry.Metadata.IsOwned() && 
            (r.TargetEntry.State == EntityState.Added || r.TargetEntry.State == EntityState.Modified));
}
