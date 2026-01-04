using System.Linq.Expressions;
using BuildingBlocks.Domain.Models;
using BuildingBlocks.Domain.Repositories;
using Microsoft.EntityFrameworkCore;

namespace BuildingBlocks.Infrastructure.Persistence;

/// <summary>
/// Implementação genérica de repositório usando Entity Framework Core
/// </summary>
/// <typeparam name="TEntity">Tipo da entidade (deve herdar de Entity)</typeparam>
/// <typeparam name="TId">Tipo do identificador da entidade</typeparam>
public class EfRepository<TEntity, TId> : IRepository<TEntity, TId>
    where TEntity : Entity<TId>
    where TId : notnull
{
    protected readonly DbContext Context;
    protected readonly DbSet<TEntity> DbSet;

    public EfRepository(DbContext context)
    {
        Context = context ?? throw new ArgumentNullException(nameof(context));
        DbSet = context.Set<TEntity>();
    }

    /// <summary>
    /// Obtém uma entidade por ID
    /// </summary>
    public virtual async Task<TEntity?> GetByIdAsync(
        TId id,
        CancellationToken cancellationToken = default)
    {
        return await DbSet.FindAsync(new object[] { id }, cancellationToken);
    }

    /// <summary>
    /// Obtém todas as entidades
    /// </summary>
    public virtual async Task<IReadOnlyList<TEntity>> GetAllAsync(
        CancellationToken cancellationToken = default)
    {
        return await DbSet.ToListAsync(cancellationToken);
    }

    /// <summary>
    /// Obtém entidades que satisfazem uma condição
    /// </summary>
    public virtual async Task<IReadOnlyList<TEntity>> FindAsync(
        Expression<Func<TEntity, bool>> predicate,
        CancellationToken cancellationToken = default)
    {
        return await DbSet.Where(predicate).ToListAsync(cancellationToken);
    }

    /// <summary>
    /// Obtém uma única entidade que satisfaz uma condição
    /// </summary>
    public virtual async Task<TEntity?> FindOneAsync(
        Expression<Func<TEntity, bool>> predicate,
        CancellationToken cancellationToken = default)
    {
        return await DbSet.FirstOrDefaultAsync(predicate, cancellationToken);
    }

    /// <summary>
    /// Verifica se existe alguma entidade que satisfaz uma condição
    /// </summary>
    public virtual async Task<bool> ExistsAsync(
        Expression<Func<TEntity, bool>> predicate,
        CancellationToken cancellationToken = default)
    {
        return await DbSet.AnyAsync(predicate, cancellationToken);
    }

    /// <summary>
    /// Conta o número de entidades que satisfazem uma condição
    /// </summary>
    public virtual async Task<int> CountAsync(
        Expression<Func<TEntity, bool>>? predicate = null,
        CancellationToken cancellationToken = default)
    {
        return predicate == null
            ? await DbSet.CountAsync(cancellationToken)
            : await DbSet.CountAsync(predicate, cancellationToken);
    }

    /// <summary>
    /// Adiciona uma nova entidade
    /// </summary>
    public virtual async Task AddAsync(
        TEntity entity,
        CancellationToken cancellationToken = default)
    {
        await DbSet.AddAsync(entity, cancellationToken);
    }

    /// <summary>
    /// Adiciona múltiplas entidades
    /// </summary>
    public virtual async Task AddRangeAsync(
        IEnumerable<TEntity> entities,
        CancellationToken cancellationToken = default)
    {
        await DbSet.AddRangeAsync(entities, cancellationToken);
    }

    /// <summary>
    /// Atualiza uma entidade existente
    /// </summary>
    public virtual void Update(TEntity entity)
    {
        DbSet.Update(entity);
    }

    /// <summary>
    /// Atualiza múltiplas entidades
    /// </summary>
    public virtual void UpdateRange(IEnumerable<TEntity> entities)
    {
        DbSet.UpdateRange(entities);
    }

    /// <summary>
    /// Remove uma entidade
    /// </summary>
    public virtual void Remove(TEntity entity)
    {
        DbSet.Remove(entity);
    }

    /// <summary>
    /// Remove uma entidade por ID
    /// </summary>
    public virtual async Task RemoveByIdAsync(
        TId id,
        CancellationToken cancellationToken = default)
    {
        var entity = await GetByIdAsync(id, cancellationToken);
        if (entity != null)
        {
            Remove(entity);
        }
    }

    /// <summary>
    /// Remove múltiplas entidades
    /// </summary>
    public virtual void RemoveRange(IEnumerable<TEntity> entities)
    {
        DbSet.RemoveRange(entities);
    }

    /// <summary>
    /// Obtém um IQueryable para consultas personalizadas
    /// ATENÇÃO: Use com cuidado para não vazar abstrações
    /// </summary>
    protected IQueryable<TEntity> Query()
    {
        return DbSet.AsQueryable();
    }

    /// <summary>
    /// Obtém um IQueryable sem tracking para consultas somente leitura
    /// </summary>
    protected IQueryable<TEntity> QueryNoTracking()
    {
        return DbSet.AsNoTracking();
    }
}
