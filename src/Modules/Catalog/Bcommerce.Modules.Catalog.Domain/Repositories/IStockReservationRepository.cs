using Bcommerce.BuildingBlocks.Application.Abstractions.Data;
using Bcommerce.Modules.Catalog.Domain.Entities;

namespace Bcommerce.Modules.Catalog.Domain.Repositories;

public interface IStockReservationRepository : IRepository<StockReservation>
{
    Task<IEnumerable<StockReservation>> GetExpiredReservationsAsync(CancellationToken cancellationToken = default);
    Task<StockReservation?> GetByReferenceAsync(Guid productId, string referenceType, Guid referenceId, CancellationToken cancellationToken = default);
}
