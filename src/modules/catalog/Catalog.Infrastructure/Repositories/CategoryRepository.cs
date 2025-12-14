using BuildingBlocks.Domain.Repositories;
using Catalog.Core.Entities;
using Catalog.Core.Repositories;
using Catalog.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Catalog.Infrastructure.Repositories;

/// <summary>
/// Implementação do repositório de categorias.
/// </summary>
internal class CategoryRepository : ICategoryRepository
{
    private readonly CatalogDbContext _context;

    public CategoryRepository(CatalogDbContext context)
    {
        _context = context;
    }

    public IUnitOfWork UnitOfWork => _context;

    public async Task<Category?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _context.Categories
            .FirstOrDefaultAsync(c => c.Id == id && c.DeletedAt == null, cancellationToken);
    }

    public async Task<Category?> GetBySlugAsync(string slug, CancellationToken cancellationToken = default)
    {
        return await _context.Categories
            .FirstOrDefaultAsync(c => c.Slug == slug && c.DeletedAt == null, cancellationToken);
    }

    public async Task<IReadOnlyList<Category>> GetRootCategoriesAsync(CancellationToken cancellationToken = default)
    {
        return await _context.Categories
            .Where(c => c.ParentId == null && c.DeletedAt == null)
            .OrderBy(c => c.SortOrder)
            .ThenBy(c => c.Name)
            .ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<Category>> GetChildrenAsync(Guid parentId, CancellationToken cancellationToken = default)
    {
        return await _context.Categories
            .Where(c => c.ParentId == parentId && c.DeletedAt == null)
            .OrderBy(c => c.SortOrder)
            .ThenBy(c => c.Name)
            .ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<Category>> GetActiveAsync(CancellationToken cancellationToken = default)
    {
        return await _context.Categories
            .Where(c => c.IsActive && c.DeletedAt == null)
            .OrderBy(c => c.SortOrder)
            .ThenBy(c => c.Name)
            .ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<Category>> GetTreeAsync(CancellationToken cancellationToken = default)
    {
        return await _context.Categories
            .Include(c => c.Children.Where(ch => ch.DeletedAt == null))
            .Where(c => c.ParentId == null && c.DeletedAt == null)
            .OrderBy(c => c.SortOrder)
            .ToListAsync(cancellationToken);
    }

    public async Task<Category?> GetByIdWithChildrenAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _context.Categories
            .Include(c => c.Children.Where(ch => ch.DeletedAt == null))
            .FirstOrDefaultAsync(c => c.Id == id && c.DeletedAt == null, cancellationToken);
    }

    public async Task<bool> SlugExistsAsync(string slug, CancellationToken cancellationToken = default)
    {
        return await _context.Categories
            .AnyAsync(c => c.Slug == slug, cancellationToken);
    }

    public async Task AddAsync(Category entity, CancellationToken cancellationToken = default)
    {
        await _context.Categories.AddAsync(entity, cancellationToken);
    }

    public void Update(Category entity)
    {
        _context.Categories.Update(entity);
    }

    public void Remove(Category entity)
    {
        entity.Delete();
        _context.Categories.Update(entity);
    }
}
