using Bcommerce.BuildingBlocks.Infrastructure.Data;
using Bcommerce.Modules.Coupons.Domain.Entities;
using Bcommerce.Modules.Coupons.Domain.Repositories;
using Microsoft.EntityFrameworkCore;

namespace Bcommerce.Modules.Coupons.Infrastructure.Persistence.Repositories;

public class CouponRepository : Repository<Coupon, CouponsDbContext>, ICouponRepository
{
    public CouponRepository(CouponsDbContext dbContext) : base(dbContext)
    {
    }

    public async Task<Coupon?> GetByCodeAsync(string code, CancellationToken cancellationToken = default)
    {
        return await DbContext.Coupons
            .Include(c => c.Usages)
            .Include(c => c.Eligibilities)
            .FirstOrDefaultAsync(c => c.Code.Value == code, cancellationToken);
    }

    public async Task<IEnumerable<CouponUsage>> GetUsagesByUserAsync(Guid userId, CancellationToken cancellationToken = default)
    {
         return await DbContext.CouponUsages
            .Where(u => u.UserId == userId)
            .ToListAsync(cancellationToken);
    }
}
