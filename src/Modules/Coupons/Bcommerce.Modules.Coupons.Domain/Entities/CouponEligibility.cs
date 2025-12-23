using Bcommerce.BuildingBlocks.Domain.Base;
using Bcommerce.Modules.Coupons.Domain.Enums;

namespace Bcommerce.Modules.Coupons.Domain.Entities;

public class CouponEligibility : Entity<Guid>
{
    public Guid CouponId { get; private set; }
    public CouponScope Scope { get; private set; }
    public string? ReferenceId { get; private set; } // CategoryId, BrandId, ProductId, UserId

    private CouponEligibility() { }

    public CouponEligibility(Guid couponId, CouponScope scope, string? referenceId = null)
    {
        Id = Guid.NewGuid();
        CouponId = couponId;
        Scope = scope;
        ReferenceId = referenceId;
    }
}
