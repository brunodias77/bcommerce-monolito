using Bcommerce.Modules.Orders.Domain.Entities;

namespace Bcommerce.Modules.Orders.Domain.Services;

public interface IOrderDomainService
{
    Task CancelOrderAsync(Order order, string reason, CancellationToken cancellationToken = default);
}
