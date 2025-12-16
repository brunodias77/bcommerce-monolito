using Bcommerce.BuildingBlocks.Application.Models;
using Bcommerce.BuildingBlocks.Domain.Abstractions;
using Bcommerce.BuildingBlocks.Domain.Specifications;

namespace Bcommerce.BuildingBlocks.Application.Abstractions.Data;

public interface IReadRepository<TEntity> where TEntity : class, IEntity
{
    Task<TEntity?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<TEntity>> GetAllAsync(CancellationToken cancellationToken = default);
    Task<IReadOnlyList<TEntity>> GetListAsync(ISpecification<TEntity> specification, CancellationToken cancellationToken = default);
    
    // Suporte a paginação com retorno de PaginatedList (será implementado depois)
    // Task<PaginatedList<TEntity>> GetPagedListAsync(ISpecification<TEntity> specification, PagedRequest request, CancellationToken cancellationToken = default);
}
