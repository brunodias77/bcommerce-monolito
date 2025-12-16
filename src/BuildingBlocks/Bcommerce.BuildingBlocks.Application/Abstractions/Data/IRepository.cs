using Bcommerce.BuildingBlocks.Domain.Abstractions;

namespace Bcommerce.BuildingBlocks.Application.Abstractions.Data;

public interface IRepository<TEntity> where TEntity : class, IAggregateRoot
{
    IUnitOfWork UnitOfWork { get; }
    
    Task<TEntity?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task AddAsync(TEntity entity, CancellationToken cancellationToken = default);
    Task UpdateAsync(TEntity entity, CancellationToken cancellationToken = default);
    Task DeleteAsync(TEntity entity, CancellationToken cancellationToken = default);
}
