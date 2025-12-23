using Bcommerce.Modules.Catalog.Domain.Entities;

namespace Bcommerce.Modules.Catalog.Domain.Services;

public interface IStockService
{
    Task ReserveStockAsync(Product product, int quantity, Guid referenceId, string referenceType, CancellationToken cancellationToken = default);
    Task ReleaseStockAsync(Guid productId, Guid referenceId, string referenceType, CancellationToken cancellationToken = default);
    Task ConfirmStockAsync(Guid productId, Guid referenceId, string referenceType, CancellationToken cancellationToken = default);
}
