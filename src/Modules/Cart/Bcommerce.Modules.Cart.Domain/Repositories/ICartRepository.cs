using Bcommerce.BuildingBlocks.Application.Abstractions.Data;
using Bcommerce.Modules.Cart.Domain.Entities;

namespace Bcommerce.Modules.Cart.Domain.Repositories;

public interface ICartRepository : IRepository<ShoppingCart>
{
    Task<ShoppingCart?> GetByUserIdAsync(Guid userId, CancellationToken cancellationToken = default);
    Task<ShoppingCart?> GetBySessionIdAsync(Guid sessionId, CancellationToken cancellationToken = default);
    Task DeleteOlderThanAsync(DateTime date, CancellationToken cancellationToken = default);
}
