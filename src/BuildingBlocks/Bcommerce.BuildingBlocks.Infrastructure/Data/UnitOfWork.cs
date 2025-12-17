using Bcommerce.BuildingBlocks.Application.Abstractions.Data;
using Bcommerce.BuildingBlocks.Infrastructure.Data;

namespace Bcommerce.BuildingBlocks.Infrastructure.Data;

/// <summary>
/// Implementação do padrão Unit of Work sobre o EF Core.
/// </summary>
/// <remarks>
/// Encapsula a persistência de todas as alterações feitas na transação.
/// - Wrapper sobre SaveChangesAsync do DbContext
/// - Garante atomicidade das operações de negócio
/// 
/// Exemplo de uso:
/// <code>
/// await _unitOfWork.SaveChangesAsync(cancellationToken);
/// </code>
/// </remarks>
public class UnitOfWork(BaseDbContext dbContext) : IUnitOfWork
{
    private readonly BaseDbContext _dbContext = dbContext;

    public async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        return await _dbContext.SaveChangesAsync(cancellationToken);
    }
}
