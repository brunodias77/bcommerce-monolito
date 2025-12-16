using Bcommerce.BuildingBlocks.Application.Abstractions.Data;
using Bcommerce.BuildingBlocks.Infrastructure.Data;

namespace Bcommerce.BuildingBlocks.Infrastructure.Data;

// O BaseDbContext já implementa IUnitOfWork. 
// Esta classe pode ser um wrapper ou simplesmente não ser necessária se usarmos o DbContext direto.
// Mas para seguir a estrutura solicitada, podemos criar uma implementação que injeta o DbContext.

public class UnitOfWork(BaseDbContext dbContext) : IUnitOfWork
{
    private readonly BaseDbContext _dbContext = dbContext;

    public async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        return await _dbContext.SaveChangesAsync(cancellationToken);
    }
}
