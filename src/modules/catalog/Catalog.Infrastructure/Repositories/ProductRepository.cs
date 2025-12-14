using BuildingBlocks.Domain.Repositories;
using Catalog.Core.Entities;
using Catalog.Core.Enums;
using Catalog.Core.Repositories;
using Catalog.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Catalog.Infrastructure.Repositories;

/// <summary>
/// Implementação do repositório de produtos.
/// </summary>
internal class ProductRepository : IProductRepository
{
    private readonly CatalogDbContext _context;

    public ProductRepository(CatalogDbContext context)
    {
        _context = context;
    }

    public IUnitOfWork UnitOfWork => _context;

    public async Task<Product?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _context.Products
            .FirstOrDefaultAsync(p => p.Id == id && p.DeletedAt == null, cancellationToken);
    }

    public async Task<Product?> GetBySkuAsync(string sku, CancellationToken cancellationToken = default)
    {
        return await _context.Products
            .FirstOrDefaultAsync(p => p.Sku == sku && p.DeletedAt == null, cancellationToken);
    }

    public async Task<Product?> GetBySlugAsync(string slug, CancellationToken cancellationToken = default)
    {
        return await _context.Products
            .FirstOrDefaultAsync(p => p.Slug == slug && p.DeletedAt == null, cancellationToken);
    }

    public async Task<Product?> GetByIdWithImagesAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _context.Products
            .Include(p => p.Images.OrderBy(i => i.SortOrder))
            .FirstOrDefaultAsync(p => p.Id == id && p.DeletedAt == null, cancellationToken);
    }

    public async Task<Product?> GetByIdWithDetailsAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _context.Products
            .Include(p => p.Category)
            .Include(p => p.Brand)
            .Include(p => p.Images.OrderBy(i => i.SortOrder))
            .FirstOrDefaultAsync(p => p.Id == id && p.DeletedAt == null, cancellationToken);
    }

    public async Task<IReadOnlyList<Product>> GetByCategoryIdAsync(Guid categoryId, CancellationToken cancellationToken = default)
    {
        return await _context.Products
            .Where(p => p.CategoryId == categoryId && p.DeletedAt == null)
            .OrderBy(p => p.Name)
            .ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<Product>> GetByBrandIdAsync(Guid brandId, CancellationToken cancellationToken = default)
    {
        return await _context.Products
            .Where(p => p.BrandId == brandId && p.DeletedAt == null)
            .OrderBy(p => p.Name)
            .ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<Product>> GetByStatusAsync(ProductStatus status, CancellationToken cancellationToken = default)
    {
        return await _context.Products
            .Where(p => p.Status == status && p.DeletedAt == null)
            .OrderBy(p => p.Name)
            .ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<Product>> GetFeaturedAsync(int take = 10, CancellationToken cancellationToken = default)
    {
        return await _context.Products
            .Where(p => p.IsFeatured && p.Status == ProductStatus.Active && p.DeletedAt == null)
            .OrderByDescending(p => p.CreatedAt)
            .Take(take)
            .ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<Product>> GetLowStockAsync(CancellationToken cancellationToken = default)
    {
        return await _context.Products
            .Where(p => (p.Stock - p.ReservedStock) <= p.LowStockThreshold 
                        && p.Status == ProductStatus.Active 
                        && p.DeletedAt == null)
            .OrderBy(p => p.Stock - p.ReservedStock)
            .ToListAsync(cancellationToken);
    }

    public async Task<bool> SkuExistsAsync(string sku, CancellationToken cancellationToken = default)
    {
        return await _context.Products
            .AnyAsync(p => p.Sku == sku, cancellationToken);
    }

    public async Task<IReadOnlyList<Product>> GetWithExpiredReservationsAsync(CancellationToken cancellationToken = default)
    {
        var now = DateTime.UtcNow;
        return await _context.Products
            .Include(p => p.StockReservations.Where(r => r.ReleasedAt == null && r.ExpiresAt < now))
            .Where(p => p.StockReservations.Any(r => r.ReleasedAt == null && r.ExpiresAt < now))
            .ToListAsync(cancellationToken);
    }

    public async Task AddAsync(Product entity, CancellationToken cancellationToken = default)
    {
        await _context.Products.AddAsync(entity, cancellationToken);
    }

    public void Update(Product entity)
    {
        _context.Products.Update(entity);
    }

    public void Remove(Product entity)
    {
        entity.Delete();
        _context.Products.Update(entity);
    }
}
