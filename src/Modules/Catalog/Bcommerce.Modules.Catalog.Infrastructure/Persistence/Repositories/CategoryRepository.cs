using Bcommerce.BuildingBlocks.Infrastructure.Data;
using Bcommerce.Modules.Catalog.Domain.Entities;
using Bcommerce.Modules.Catalog.Domain.Repositories;
using Bcommerce.Modules.Catalog.Domain.ValueObjects;
using Microsoft.EntityFrameworkCore;

namespace Bcommerce.Modules.Catalog.Infrastructure.Persistence.Repositories;

public class CategoryRepository : Repository<Category, CatalogDbContext>, ICategoryRepository
{
    public CategoryRepository(CatalogDbContext dbContext) : base(dbContext)
    {
    }

    public async Task<Category?> GetBySlugAsync(Slug slug, CancellationToken cancellationToken = default)
    {
        return await DbContext.Categories
            .FirstOrDefaultAsync(c => c.Slug.Value == slug.Value, cancellationToken);
    }

    public async Task<IEnumerable<Category>> GetRootCategoriesAsync(CancellationToken cancellationToken = default)
    {
        return await DbContext.Categories
            .Where(c => c.ParentId == null)
            .ToListAsync(cancellationToken);
    }
}
