using Bcommerce.BuildingBlocks.Application.Abstractions.Data;
using Bcommerce.Modules.Coupons.Domain.Entities;

namespace Bcommerce.Modules.Coupons.Domain.Repositories;

public interface ICouponRepository : IRepository<Coupon>
{
    Task<Coupon?> GetByCodeAsync(string code, CancellationToken cancellationToken = default);
    Task<IEnumerable<CouponUsage>> GetUsagesByUserAsync(Guid userId, CancellationToken cancellationToken = default);
}
