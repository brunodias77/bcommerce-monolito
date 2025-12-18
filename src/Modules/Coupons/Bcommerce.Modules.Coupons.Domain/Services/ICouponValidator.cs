using Bcommerce.Modules.Coupons.Domain.Entities;

namespace Bcommerce.Modules.Coupons.Domain.Services;

public interface ICouponValidator
{
    Task<bool> ValidateAsync(Coupon coupon, Guid userId, decimal orderTotal, IEnumerable<Guid> productIds, CancellationToken cancellationToken = default);
}
