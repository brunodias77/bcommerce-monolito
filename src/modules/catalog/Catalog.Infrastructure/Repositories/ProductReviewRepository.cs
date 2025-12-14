using BuildingBlocks.Domain.Repositories;
using Catalog.Core.Entities;
using Catalog.Core.Repositories;
using Catalog.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Catalog.Infrastructure.Repositories;

/// <summary>
/// Implementação do repositório de avaliações de produtos.
/// </summary>
internal class ProductReviewRepository : IProductReviewRepository
{
    private readonly CatalogDbContext _context;

    public ProductReviewRepository(CatalogDbContext context)
    {
        _context = context;
    }

    public IUnitOfWork UnitOfWork => _context;

    public async Task<ProductReview?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _context.ProductReviews
            .FirstOrDefaultAsync(r => r.Id == id && r.DeletedAt == null, cancellationToken);
    }

    public async Task<IReadOnlyList<ProductReview>> GetByProductIdAsync(
        Guid productId,
        CancellationToken cancellationToken = default)
    {
        return await _context.ProductReviews
            .Where(r => r.ProductId == productId && r.DeletedAt == null)
            .OrderByDescending(r => r.CreatedAt)
            .ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<ProductReview>> GetApprovedByProductIdAsync(
        Guid productId,
        CancellationToken cancellationToken = default)
    {
        return await _context.ProductReviews
            .Where(r => r.ProductId == productId && r.IsApproved && r.DeletedAt == null)
            .OrderByDescending(r => r.CreatedAt)
            .ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<ProductReview>> GetByUserIdAsync(
        Guid userId,
        CancellationToken cancellationToken = default)
    {
        return await _context.ProductReviews
            .Where(r => r.UserId == userId && r.DeletedAt == null)
            .OrderByDescending(r => r.CreatedAt)
            .ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<ProductReview>> GetPendingApprovalAsync(
        CancellationToken cancellationToken = default)
    {
        return await _context.ProductReviews
            .Where(r => !r.IsApproved && r.DeletedAt == null)
            .OrderBy(r => r.CreatedAt)
            .ToListAsync(cancellationToken);
    }

    public async Task<bool> UserHasReviewedAsync(
        Guid userId,
        Guid productId,
        CancellationToken cancellationToken = default)
    {
        return await _context.ProductReviews
            .AnyAsync(r => r.UserId == userId && r.ProductId == productId && r.DeletedAt == null, cancellationToken);
    }

    public async Task<double> GetAverageRatingAsync(
        Guid productId,
        CancellationToken cancellationToken = default)
    {
        var reviews = await _context.ProductReviews
            .Where(r => r.ProductId == productId && r.IsApproved && r.DeletedAt == null)
            .Select(r => r.Rating)
            .ToListAsync(cancellationToken);

        return reviews.Count > 0 ? reviews.Average() : 0;
    }

    public async Task<int> GetApprovedCountAsync(
        Guid productId,
        CancellationToken cancellationToken = default)
    {
        return await _context.ProductReviews
            .CountAsync(r => r.ProductId == productId && r.IsApproved && r.DeletedAt == null, cancellationToken);
    }

    public async Task AddAsync(ProductReview entity, CancellationToken cancellationToken = default)
    {
        await _context.ProductReviews.AddAsync(entity, cancellationToken);
    }

    public void Update(ProductReview entity)
    {
        _context.ProductReviews.Update(entity);
    }

    public void Remove(ProductReview entity)
    {
        entity.Delete();
        _context.ProductReviews.Update(entity);
    }
}
