using Bcommerce.BuildingBlocks.Application.Abstractions.Data;
using Bcommerce.Modules.Catalog.Domain.Entities;
using Bcommerce.Modules.Catalog.Domain.ValueObjects;

namespace Bcommerce.Modules.Catalog.Domain.Repositories;

public interface IProductRepository : IRepository<Product>
{
    Task<Product?> GetBySkuAsync(Sku sku, CancellationToken cancellationToken = default);
    Task<Product?> GetBySlugAsync(Slug slug, CancellationToken cancellationToken = default);
    Task<bool> ExistsBySkuAsync(Sku sku, CancellationToken cancellationToken = default);
}
