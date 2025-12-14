using BuildingBlocks.Domain.Repositories;
using Cart.Core.Enums;
using Cart.Core.Repositories;
using Cart.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Cart.Infrastructure.Repositories;

/// <summary>
/// Implementação do repositório de carrinhos.
/// </summary>
public class CartRepository : ICartRepository
{
    private readonly CartDbContext _context;

    public CartRepository(CartDbContext context)
    {
        _context = context;
    }

    public IUnitOfWork UnitOfWork => _context;

    public async Task<Core.Entities.Cart?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _context.Carts
            .FirstOrDefaultAsync(c => c.Id == id, cancellationToken);
    }

    public async Task<Core.Entities.Cart?> GetActiveByUserIdAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        return await _context.Carts
            .Where(c => c.UserId == userId && c.Status == CartStatus.Active)
            .FirstOrDefaultAsync(cancellationToken);
    }

    public async Task<Core.Entities.Cart?> GetActiveBySessionIdAsync(string sessionId, CancellationToken cancellationToken = default)
    {
        return await _context.Carts
            .Where(c => c.SessionId == sessionId && c.Status == CartStatus.Active)
            .FirstOrDefaultAsync(cancellationToken);
    }

    public async Task<Core.Entities.Cart?> GetByIdWithItemsAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _context.Carts
            .Include(c => c.Items)
            .FirstOrDefaultAsync(c => c.Id == id, cancellationToken);
    }

    public async Task<bool> UserHasActiveCartAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        return await _context.Carts
            .AnyAsync(c => c.UserId == userId && c.Status == CartStatus.Active, cancellationToken);
    }

    public async Task AddAsync(Core.Entities.Cart cart, CancellationToken cancellationToken = default)
    {
        await _context.Carts.AddAsync(cart, cancellationToken);
    }

    public void Update(Core.Entities.Cart cart)
    {
        _context.Carts.Update(cart);
    }
}
