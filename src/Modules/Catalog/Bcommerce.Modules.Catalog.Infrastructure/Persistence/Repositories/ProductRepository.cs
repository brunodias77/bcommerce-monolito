using Bcommerce.BuildingBlocks.Infrastructure.Data;
using Bcommerce.Modules.Catalog.Domain.Entities;
using Bcommerce.Modules.Catalog.Domain.Repositories;
using Bcommerce.Modules.Catalog.Domain.ValueObjects;
using Microsoft.EntityFrameworkCore;

namespace Bcommerce.Modules.Catalog.Infrastructure.Persistence.Repositories;

public class ProductRepository : Repository<Product, CatalogDbContext>, IProductRepository
{
    public ProductRepository(CatalogDbContext dbContext) : base(dbContext)
    {
    }

    public async Task<bool> ExistsBySkuAsync(Sku sku, CancellationToken cancellationToken = default)
    {
        return await DbContext.Products.AnyAsync(p => p.Sku.Value == sku.Value, cancellationToken);
    }

    public async Task<Product?> GetBySkuAsync(Sku sku, CancellationToken cancellationToken = default)
    {
        return await DbContext.Products
            .Include(p => p.Images)
            .Include(p => p.Stock)
            .Include(p => p.Category)
            .Include(p => p.Brand)
            .FirstOrDefaultAsync(p => p.Sku.Value == sku.Value, cancellationToken);
    }

    public async Task<Product?> GetBySlugAsync(Slug slug, CancellationToken cancellationToken = default)
    {
        return await DbContext.Products
            .Include(p => p.Images)
            .Include(p => p.Stock)
            .Include(p => p.Category)
            .Include(p => p.Brand)
            .FirstOrDefaultAsync(p => p.Slug.Value == slug.Value, cancellationToken);
    }
}
