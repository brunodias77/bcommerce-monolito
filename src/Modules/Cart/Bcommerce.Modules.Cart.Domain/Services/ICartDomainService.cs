using Bcommerce.Modules.Cart.Domain.Entities;

namespace Bcommerce.Modules.Cart.Domain.Services;

public interface ICartDomainService
{
    Task MergeCartsAsync(ShoppingCart userCart, ShoppingCart sessionCart, CancellationToken cancellationToken = default);
}
