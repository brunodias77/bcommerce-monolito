using Bcommerce.BuildingBlocks.Infrastructure.Data;
using Bcommerce.Modules.Catalog.Domain.Entities;
using Bcommerce.Modules.Catalog.Domain.Repositories;
using Microsoft.EntityFrameworkCore;

namespace Bcommerce.Modules.Catalog.Infrastructure.Persistence.Repositories;

public class StockReservationRepository : Repository<StockReservation, CatalogDbContext>, IStockReservationRepository
{
    public StockReservationRepository(CatalogDbContext dbContext) : base(dbContext)
    {
    }

    public async Task<StockReservation?> GetByReferenceAsync(Guid productId, string referenceType, Guid referenceId, CancellationToken cancellationToken = default)
    {
        return await DbContext.StockReservations
            .FirstOrDefaultAsync(r => r.ProductId == productId && r.ReferenceType == referenceType && r.ReferenceId == referenceId, cancellationToken);
    }

    public async Task<IEnumerable<StockReservation>> GetExpiredReservationsAsync(CancellationToken cancellationToken = default)
    {
        return await DbContext.StockReservations
            .Where(r => r.ExpiresAt <= DateTime.UtcNow)
            .ToListAsync(cancellationToken);
    }
}
