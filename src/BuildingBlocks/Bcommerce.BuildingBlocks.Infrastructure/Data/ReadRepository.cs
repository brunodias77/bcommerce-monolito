using Bcommerce.BuildingBlocks.Application.Abstractions.Data;
using Bcommerce.BuildingBlocks.Domain.Abstractions;
using Microsoft.EntityFrameworkCore;

namespace Bcommerce.BuildingBlocks.Infrastructure.Data;

public class ReadRepository<TEntity, TContext>(TContext dbContext) : IReadRepository<TEntity>
    where TEntity : class, IEntity
    where TContext : DbContext // Pode ser DbContext puro para leitura
{
    protected readonly TContext DbContext = dbContext;

    public async Task<TEntity?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await DbContext.Set<TEntity>()
            .AsNoTracking()
            .FirstOrDefaultAsync(e => EF.Property<Guid>(e, "Id") == id, cancellationToken);
    }

    public async Task<IReadOnlyList<TEntity>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        return await DbContext.Set<TEntity>()
            .AsNoTracking()
            .ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<TEntity>> GetListAsync(Bcommerce.BuildingBlocks.Domain.Specifications.ISpecification<TEntity> specification, CancellationToken cancellationToken = default)
    {
        return await ApplySpecification(specification)
            .AsNoTracking()
            .ToListAsync(cancellationToken);
    }

    private IQueryable<TEntity> ApplySpecification(Bcommerce.BuildingBlocks.Domain.Specifications.ISpecification<TEntity> specification)
    {
        var query = DbContext.Set<TEntity>().AsQueryable();
        
        // Avaliação mais simples da specification p/ IQueryable requeriria um Evaluator.
        // Como o ISpecification.ToExpression() retorna Expression<Func<T, bool>>, podemos usar Where.
        
        query = query.Where(specification.ToExpression());
        
        return query;
    }
}
