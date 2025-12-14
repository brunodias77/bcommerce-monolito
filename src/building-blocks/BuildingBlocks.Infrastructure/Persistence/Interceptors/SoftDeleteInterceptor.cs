using BuildingBlocks.Domain.Entities;
using BuildingBlocks.Infrastructure.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.Diagnostics;

namespace BuildingBlocks.Infrastructure.Persistence.Interceptors;

/// <summary>
/// Interceptor que converte operações DELETE em soft delete.
/// </summary>
/// <remarks>
/// Para entidades que implementam ISoftDeletable:
/// - Operações de DELETE são convertidas em UPDATE
/// - O campo DeletedAt é preenchido com a data/hora atual
/// - O estado da entidade muda de Deleted para Modified
/// 
/// Configuração no DbContext:
/// <code>
/// options.AddInterceptors(new SoftDeleteInterceptor(dateTimeProvider));
/// </code>
/// 
/// IMPORTANTE: Configure também um Query Filter global no OnModelCreating:
/// <code>
/// builder.HasQueryFilter(e => e.DeletedAt == null);
/// </code>
/// </remarks>
public class SoftDeleteInterceptor : SaveChangesInterceptor
{
    private readonly IDateTimeProvider _dateTimeProvider;

    public SoftDeleteInterceptor(IDateTimeProvider dateTimeProvider)
    {
        _dateTimeProvider = dateTimeProvider;
    }

    public SoftDeleteInterceptor() : this(new DateTimeProvider())
    {
    }

    public override InterceptionResult<int> SavingChanges(
        DbContextEventData eventData,
        InterceptionResult<int> result)
    {
        ConvertDeleteToSoftDelete(eventData.Context);
        return base.SavingChanges(eventData, result);
    }

    public override ValueTask<InterceptionResult<int>> SavingChangesAsync(
        DbContextEventData eventData,
        InterceptionResult<int> result,
        CancellationToken cancellationToken = default)
    {
        ConvertDeleteToSoftDelete(eventData.Context);
        return base.SavingChangesAsync(eventData, result, cancellationToken);
    }

    private void ConvertDeleteToSoftDelete(DbContext? context)
    {
        if (context is null)
            return;

        var entries = context.ChangeTracker
            .Entries<ISoftDeletable>()
            .Where(e => e.State == EntityState.Deleted)
            .ToList();

        foreach (var entry in entries)
        {
            // Converte DELETE em UPDATE
            entry.State = EntityState.Modified;

            // Define DeletedAt
            var utcNow = _dateTimeProvider.UtcNow;
            SetProperty(entry, nameof(ISoftDeletable.DeletedAt), utcNow);

            // Se também for IAuditableEntity, atualiza UpdatedAt
            if (entry.Entity is IAuditableEntity)
            {
                SetProperty(entry, nameof(IAuditableEntity.UpdatedAt), utcNow);
            }
        }
    }

    private static void SetProperty(EntityEntry entry, string propertyName, object? value)
    {
        var property = entry.Property(propertyName);
        if (property != null)
        {
            property.CurrentValue = value;
        }
    }
}
