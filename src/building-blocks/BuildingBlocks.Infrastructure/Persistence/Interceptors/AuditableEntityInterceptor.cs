using BuildingBlocks.Domain.Entities;
using BuildingBlocks.Infrastructure.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.Diagnostics;

namespace BuildingBlocks.Infrastructure.Persistence.Interceptors;

/// <summary>
/// Interceptor responsável pela <strong>Auditoria Básica</strong> dos registros.
/// </summary>
/// <remarks>
/// <strong>Objetivo:</strong>
/// Centralizar a lógica de preenchimento de metadados temporais (<c>CreatedAt</c>, <c>UpdatedAt</c>).
/// Isso tira a responsabilidade das camadas de aplicação/domínio e garante consistência.
/// 
/// <strong>Funcionamento:</strong>
/// - <strong>Insert:</strong> Define <c>CreatedAt</c> e <c>UpdatedAt</c> com UTC Now.
/// - <strong>Update:</strong> Atualiza <c>UpdatedAt</c> com UTC Now.
/// 
/// Depende de <see cref="IDateTimeProvider"/> para facilitar testes (evitando DateTime.UtcNow direto).
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
