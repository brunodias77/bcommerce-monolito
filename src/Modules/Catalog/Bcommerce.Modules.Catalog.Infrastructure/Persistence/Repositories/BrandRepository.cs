using Bcommerce.BuildingBlocks.Infrastructure.Data;
using Bcommerce.Modules.Catalog.Domain.Entities;
using Bcommerce.Modules.Catalog.Domain.Repositories;
using Bcommerce.Modules.Catalog.Domain.ValueObjects;
using Microsoft.EntityFrameworkCore;

namespace Bcommerce.Modules.Catalog.Infrastructure.Persistence.Repositories;

public class BrandRepository : Repository<Brand, CatalogDbContext>, IBrandRepository
{
    public BrandRepository(CatalogDbContext dbContext) : base(dbContext)
    {
    }

    public async Task<Brand?> GetBySlugAsync(Slug slug, CancellationToken cancellationToken = default)
    {
        return await DbContext.Brands
            .FirstOrDefaultAsync(b => b.Slug.Value == slug.Value, cancellationToken);
    }
}
