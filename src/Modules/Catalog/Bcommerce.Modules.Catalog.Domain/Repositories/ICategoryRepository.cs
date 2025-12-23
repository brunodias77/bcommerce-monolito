using Bcommerce.BuildingBlocks.Application.Abstractions.Data;
using Bcommerce.Modules.Catalog.Domain.Entities;
using Bcommerce.Modules.Catalog.Domain.ValueObjects;

namespace Bcommerce.Modules.Catalog.Domain.Repositories;

public interface ICategoryRepository : IRepository<Category>
{
    Task<Category?> GetBySlugAsync(Slug slug, CancellationToken cancellationToken = default);
    Task<IEnumerable<Category>> GetRootCategoriesAsync(CancellationToken cancellationToken = default);
}
