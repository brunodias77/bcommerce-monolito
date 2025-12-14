using BuildingBlocks.Domain.Entities;
using BuildingBlocks.Infrastructure.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.Diagnostics;

namespace BuildingBlocks.Infrastructure.Persistence.Interceptors;

/// <summary>
/// Interceptor que preenche automaticamente CreatedAt e UpdatedAt.
/// </summary>
/// <remarks>
/// Para entidades que implementam IAuditableEntity:
/// - CreatedAt é definido quando a entidade é Added
/// - UpdatedAt é atualizado quando a entidade é Added ou Modified
/// 
/// Configuração no DbContext:
/// <code>
/// options.AddInterceptors(new AuditableEntityInterceptor(dateTimeProvider));
/// </code>
/// </remarks>
public class AuditableEntityInterceptor : SaveChangesInterceptor
{
    private readonly IDateTimeProvider _dateTimeProvider;

    public AuditableEntityInterceptor(IDateTimeProvider dateTimeProvider)
    {
        _dateTimeProvider = dateTimeProvider;
    }

    public AuditableEntityInterceptor() : this(new DateTimeProvider())
    {
    }

    public override InterceptionResult<int> SavingChanges(
        DbContextEventData eventData, 
        InterceptionResult<int> result)
    {
        UpdateAuditableEntities(eventData.Context);
        return base.SavingChanges(eventData, result);
    }

    public override ValueTask<InterceptionResult<int>> SavingChangesAsync(
        DbContextEventData eventData, 
        InterceptionResult<int> result,
        CancellationToken cancellationToken = default)
    {
        UpdateAuditableEntities(eventData.Context);
        return base.SavingChangesAsync(eventData, result, cancellationToken);
    }

    private void UpdateAuditableEntities(DbContext? context)
    {
        if (context is null)
            return;

        var entries = context.ChangeTracker
            .Entries<IAuditableEntity>()
            .Where(e => e.State == EntityState.Added || e.State == EntityState.Modified);

        foreach (var entry in entries)
        {
            var utcNow = _dateTimeProvider.UtcNow;

            if (entry.State == EntityState.Added)
            {
                SetProperty(entry, nameof(IAuditableEntity.CreatedAt), utcNow);
            }

            SetProperty(entry, nameof(IAuditableEntity.UpdatedAt), utcNow);
        }
    }

    private static void SetProperty(EntityEntry entry, string propertyName, object value)
    {
        var property = entry.Property(propertyName);
        if (property != null)
        {
            property.CurrentValue = value;
        }
    }
}
