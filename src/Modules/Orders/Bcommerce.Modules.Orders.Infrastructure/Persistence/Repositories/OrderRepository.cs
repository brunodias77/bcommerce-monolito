using Bcommerce.BuildingBlocks.Infrastructure.Data;
using Bcommerce.Modules.Orders.Domain.Entities;
using Bcommerce.Modules.Orders.Domain.Repositories;
using Bcommerce.Modules.Orders.Domain.ValueObjects;
using Microsoft.EntityFrameworkCore;

namespace Bcommerce.Modules.Orders.Infrastructure.Persistence.Repositories;

public class OrderRepository : Repository<Order, OrdersDbContext>, IOrderRepository
{
    public OrderRepository(OrdersDbContext dbContext) : base(dbContext)
    {
    }

    public async Task<Order?> GetByOrderNumberAsync(OrderNumber orderNumber, CancellationToken cancellationToken = default)
    {
        return await DbContext.Orders
            .Include(o => o.Items)
            .FirstOrDefaultAsync(o => o.OrderNumber.Value == orderNumber.Value, cancellationToken);
    }

    public async Task<IEnumerable<Order>> GetByUserIdAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        return await DbContext.Orders
            .Include(o => o.Items)
            .Where(o => o.UserId == userId)
            .OrderByDescending(o => o.CreatedAt)
            .ToListAsync(cancellationToken);
    }
}
