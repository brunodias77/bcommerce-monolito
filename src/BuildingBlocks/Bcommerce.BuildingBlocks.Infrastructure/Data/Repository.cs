using Bcommerce.BuildingBlocks.Application.Abstractions.Data;
using Bcommerce.BuildingBlocks.Domain.Abstractions;
using Microsoft.EntityFrameworkCore;

namespace Bcommerce.BuildingBlocks.Infrastructure.Data;

public class Repository<TEntity, TContext>(TContext dbContext) : IRepository<TEntity>
    where TEntity : class, IAggregateRoot
    where TContext : BaseDbContext
{
    protected readonly TContext DbContext = dbContext;

    public IUnitOfWork UnitOfWork => DbContext;

    public async Task<TEntity?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        // Assume que TEntity tem uma chave primária chamada Id do tipo Guid.
        // Se a entidade usar TId genérico, a implementação precisaria adaptar.
        // Como o IRepository<T> simplificado não impõe TId, usamos FindAsync ou assumimos Id.
        
        return await DbContext.Set<TEntity>().FindAsync(new object[] { id }, cancellationToken);
    }

    public async Task AddAsync(TEntity entity, CancellationToken cancellationToken = default)
    {
        await DbContext.Set<TEntity>().AddAsync(entity, cancellationToken);
    }

    public Task UpdateAsync(TEntity entity, CancellationToken cancellationToken = default)
    {
        DbContext.Set<TEntity>().Update(entity);
        return Task.CompletedTask;
    }

    public Task DeleteAsync(TEntity entity, CancellationToken cancellationToken = default)
    {
        DbContext.Set<TEntity>().Remove(entity);
        return Task.CompletedTask;
    }
}
