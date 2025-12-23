using Bcommerce.BuildingBlocks.Application.Abstractions.Data;
using Bcommerce.Modules.Catalog.Domain.Entities;
using Bcommerce.Modules.Catalog.Domain.ValueObjects;

namespace Bcommerce.Modules.Catalog.Domain.Repositories;

public interface IBrandRepository : IRepository<Brand>
{
    Task<Brand?> GetBySlugAsync(Slug slug, CancellationToken cancellationToken = default);
}
