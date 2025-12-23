using Bcommerce.BuildingBlocks.Domain.Base;

namespace Bcommerce.Modules.Coupons.Domain.Entities;

public class CouponUsage : Entity<Guid>
{
    public Guid CouponId { get; private set; }
    public Guid UserId { get; private set; }
    public Guid OrderId { get; private set; }
    public decimal DiscountApplied { get; private set; }
    public DateTime UsedAt { get; private set; }

    private CouponUsage() { }

    public CouponUsage(Guid couponId, Guid userId, Guid orderId, decimal discountApplied)
    {
        Id = Guid.NewGuid();
        CouponId = couponId;
        UserId = userId;
        OrderId = orderId;
        DiscountApplied = discountApplied;
        UsedAt = DateTime.UtcNow;
    }
}
