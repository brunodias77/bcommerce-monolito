using BuildingBlocks.Domain.Entities;
using BuildingBlocks.Infrastructure.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;

namespace BuildingBlocks.Infrastructure.Persistence.Interceptors;

/// <summary>
/// Interceptor que converte operações de DELETE em UPDATE (soft delete).
/// </summary>
/// <remarks>
/// Quando você faz:
/// <code>
/// dbContext.Products.Remove(product);
/// await dbContext.SaveChangesAsync();
/// </code>
/// 
/// Este interceptor:
/// 1. Detecta que a entidade implementa ISoftDeletable
/// 2. Cancela o DELETE físico
/// 3. Converte para UPDATE definindo DeletedAt = NOW()
/// 
/// No PostgreSQL:
/// - Registros nunca são removidos fisicamente
/// - deleted_at é definido com timestamp atual
/// - Índices filtrados (WHERE deleted_at IS NULL) mantêm performance
/// 
/// Query Filter global:
/// - BaseEntityConfiguration já configura: HasQueryFilter(e => e.DeletedAt == null)
/// - Queries automáticas ignoram registros deletados
/// - Use IgnoreQueryFilters() para incluir deletados
/// </remarks>
public sealed class SoftDeleteInterceptor : SaveChangesInterceptor
{
    private readonly IDateTimeProvider _dateTimeProvider;

    public SoftDeleteInterceptor(IDateTimeProvider dateTimeProvider)
    {
        _dateTimeProvider = dateTimeProvider;
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

        foreach (var entry in context.ChangeTracker.Entries<ISoftDeletable>())
        {
            if (entry.State == EntityState.Deleted)
            {
                // Cancela o DELETE físico
                entry.State = EntityState.Modified;

                // Define DeletedAt (soft delete)
                entry.Property(nameof(ISoftDeletable.DeletedAt)).CurrentValue = _dateTimeProvider.UtcNow;
            }
        }
    }
}

/// <summary>
/// Extensões para trabalhar com soft delete.
/// </summary>
public static class SoftDeleteExtensions
{
    /// <summary>
    /// Restaura uma entidade soft-deleted.
    /// </summary>
    public static void Restore<TEntity>(this DbContext context, TEntity entity)
        where TEntity : Entity, ISoftDeletable
    {
        if (entity.DeletedAt == null)
            return;

        entity.Restore();
        context.Entry(entity).State = EntityState.Modified;
    }

    /// <summary>
    /// Força exclusão física (use com MUITO cuidado).
    /// </summary>
    public static void HardDelete<TEntity>(this DbContext context, TEntity entity)
        where TEntity : Entity, ISoftDeletable
    {
        // Remove o query filter temporariamente para permitir hard delete
        context.Entry(entity).State = EntityState.Deleted;
    }

    /// <summary>
    /// Query que inclui registros soft-deleted.
    /// </summary>
    public static IQueryable<TEntity> IncludeDeleted<TEntity>(this IQueryable<TEntity> query)
        where TEntity : Entity, ISoftDeletable
    {
        return query.IgnoreQueryFilters();
    }

    /// <summary>
    /// Query que retorna APENAS registros soft-deleted.
    /// </summary>
    public static IQueryable<TEntity> OnlyDeleted<TEntity>(this IQueryable<TEntity> query)
        where TEntity : Entity, ISoftDeletable
    {
        return query.IgnoreQueryFilters().Where(e => EF.Property<DateTime?>(e, "DeletedAt") != null);
    }
}