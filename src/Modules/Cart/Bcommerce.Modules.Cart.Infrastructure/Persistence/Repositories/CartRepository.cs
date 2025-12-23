using Bcommerce.BuildingBlocks.Infrastructure.Data;
using Bcommerce.Modules.Cart.Domain.Entities;
using Bcommerce.Modules.Cart.Domain.Repositories;
using Microsoft.EntityFrameworkCore;

namespace Bcommerce.Modules.Cart.Infrastructure.Persistence.Repositories;

public class CartRepository : Repository<ShoppingCart, CartDbContext>, ICartRepository
{
    public CartRepository(CartDbContext dbContext) : base(dbContext)
    {
    }

    public async Task<ShoppingCart?> GetByUserIdAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        return await DbContext.ShoppingCarts
            .Include(c => c.Items)
            .FirstOrDefaultAsync(c => c.UserId == userId && c.Status == Domain.Enums.CartStatus.Active, cancellationToken);
    }

    public async Task<ShoppingCart?> GetBySessionIdAsync(Guid sessionId, CancellationToken cancellationToken = default)
    {
        return await DbContext.ShoppingCarts
            .Include(c => c.Items)
            .FirstOrDefaultAsync(c => c.SessionId.Value == sessionId && c.Status == Domain.Enums.CartStatus.Active, cancellationToken);
    }

    public async Task DeleteOlderThanAsync(DateTime date, CancellationToken cancellationToken = default)
    {
        var oldCarts = await DbContext.ShoppingCarts
            .Where(c => c.CreatedAt < date && c.Status == Domain.Enums.CartStatus.Abandoned)
            .ToListAsync(cancellationToken);
            
        DbContext.ShoppingCarts.RemoveRange(oldCarts);
    }
}
