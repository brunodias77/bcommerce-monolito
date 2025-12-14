using BuildingBlocks.Domain.Entities;
using BuildingBlocks.Infrastructure.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;

namespace BuildingBlocks.Infrastructure.Persistence.Interceptors;

/// <summary>
/// Interceptor que preenche automaticamente CreatedAt e UpdatedAt em entidades auditáveis.
/// </summary>
/// <remarks>
/// Este interceptor funciona EM CONJUNTO com os triggers do PostgreSQL:
/// 
/// PostgreSQL Triggers (seu schema):
/// - created_at: DEFAULT NOW()
/// - updated_at: trigger shared.trigger_set_timestamp()
/// 
/// Este Interceptor (EF Core):
/// - Garante que os valores sejam definidos ANTES de salvar
/// - Útil para testes sem banco de dados
/// - Garante consistência mesmo se triggers não estiverem ativos
/// 
/// IMPORTANTE: O interceptor tem precedência sobre os defaults do banco.
/// Os valores definidos aqui serão enviados ao PostgreSQL.
/// </remarks>
public sealed class AuditableEntityInterceptor : SaveChangesInterceptor
{
    private readonly IDateTimeProvider _dateTimeProvider;

    public AuditableEntityInterceptor(IDateTimeProvider dateTimeProvider)
    {
        _dateTimeProvider = dateTimeProvider;
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

        var utcNow = _dateTimeProvider.UtcNow;

        foreach (var entry in context.ChangeTracker.Entries<IAuditableEntity>())
        {
            if (entry.State == EntityState.Added)
            {
                // Definir CreatedAt apenas na inserção
                entry.Property(nameof(IAuditableEntity.CreatedAt)).CurrentValue = utcNow;
                entry.Property(nameof(IAuditableEntity.UpdatedAt)).CurrentValue = utcNow;
            }

            if (entry.State == EntityState.Modified)
            {
                // Atualizar apenas UpdatedAt em modificações
                // CreatedAt nunca deve mudar
                entry.Property(nameof(IAuditableEntity.CreatedAt)).IsModified = false;
                entry.Property(nameof(IAuditableEntity.UpdatedAt)).CurrentValue = utcNow;
            }
        }
    }
}
