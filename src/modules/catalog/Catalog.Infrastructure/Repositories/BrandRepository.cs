using BuildingBlocks.Domain.Repositories;
using Catalog.Core.Entities;
using Catalog.Core.Repositories;
using Catalog.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Catalog.Infrastructure.Repositories;

/// <summary>
/// Implementação do repositório de marcas.
/// </summary>
internal class BrandRepository : IBrandRepository
{
    private readonly CatalogDbContext _context;

    public BrandRepository(CatalogDbContext context)
    {
        _context = context;
    }

    public IUnitOfWork UnitOfWork => _context;

    public async Task<Brand?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _context.Brands
            .FirstOrDefaultAsync(b => b.Id == id && b.DeletedAt == null, cancellationToken);
    }

    public async Task<Brand?> GetBySlugAsync(string slug, CancellationToken cancellationToken = default)
    {
        return await _context.Brands
            .FirstOrDefaultAsync(b => b.Slug == slug && b.DeletedAt == null, cancellationToken);
    }

    public async Task<Brand?> GetByNameAsync(string name, CancellationToken cancellationToken = default)
    {
        return await _context.Brands
            .FirstOrDefaultAsync(b => b.Name == name && b.DeletedAt == null, cancellationToken);
    }

    public async Task<IReadOnlyList<Brand>> GetActiveAsync(CancellationToken cancellationToken = default)
    {
        return await _context.Brands
            .Where(b => b.IsActive && b.DeletedAt == null)
            .OrderBy(b => b.SortOrder)
            .ThenBy(b => b.Name)
            .ToListAsync(cancellationToken);
    }

    public async Task<bool> SlugExistsAsync(string slug, CancellationToken cancellationToken = default)
    {
        return await _context.Brands
            .AnyAsync(b => b.Slug == slug, cancellationToken);
    }

    public async Task AddAsync(Brand entity, CancellationToken cancellationToken = default)
    {
        await _context.Brands.AddAsync(entity, cancellationToken);
    }

    public void Update(Brand entity)
    {
        _context.Brands.Update(entity);
    }

    public void Remove(Brand entity)
    {
        entity.Delete();
        _context.Brands.Update(entity);
    }
}
