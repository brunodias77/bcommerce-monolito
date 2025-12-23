using Bcommerce.BuildingBlocks.Domain.Base;

namespace Bcommerce.Modules.Coupons.Domain.Entities;

public class CouponReservation : Entity<Guid>
{
    public Guid CouponId { get; private set; }
    public Guid UserId { get; private set; }
    public DateTime ExpiresAt { get; private set; }

    private CouponReservation() { }

    public CouponReservation(Guid couponId, Guid userId, DateTime expiresAt)
    {
        Id = Guid.NewGuid();
        CouponId = couponId;
        UserId = userId;
        ExpiresAt = expiresAt;
    }
    
    public bool IsExpired(DateTime now) => now > ExpiresAt;
}
