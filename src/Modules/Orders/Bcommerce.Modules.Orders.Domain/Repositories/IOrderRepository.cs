using Bcommerce.BuildingBlocks.Application.Abstractions.Data;
using Bcommerce.Modules.Orders.Domain.Entities;
using Bcommerce.Modules.Orders.Domain.ValueObjects;

namespace Bcommerce.Modules.Orders.Domain.Repositories;

public interface IOrderRepository : IRepository<Order>
{
    Task<Order?> GetByOrderNumberAsync(OrderNumber orderNumber, CancellationToken cancellationToken = default);
    Task<IEnumerable<Order>> GetByUserIdAsync(Guid userId, CancellationToken cancellationToken = default);
}
