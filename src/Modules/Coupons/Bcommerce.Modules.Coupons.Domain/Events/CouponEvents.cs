using Bcommerce.BuildingBlocks.Domain.Base;

namespace Bcommerce.Modules.Coupons.Domain.Events;

public class CouponCreatedEvent : DomainEvent
{
    public Guid CouponId { get; }
    public string Code { get; }

    public CouponCreatedEvent(Guid couponId, string code)
    {
        CouponId = couponId;
        Code = code;
    }
}

public class CouponUsedEvent : DomainEvent
{
    public Guid CouponId { get; }
    public Guid UserId { get; }
    public Guid OrderId { get; }
    public decimal DiscountAmount { get; }

    public CouponUsedEvent(Guid couponId, Guid userId, Guid orderId, decimal discountAmount)
    {
        CouponId = couponId;
        UserId = userId;
        OrderId = orderId;
        DiscountAmount = discountAmount;
    }
}

public class CouponExpiredEvent : DomainEvent
{
    public Guid CouponId { get; }

    public CouponExpiredEvent(Guid couponId)
    {
        CouponId = couponId;
    }
}

public class CouponDepletedEvent : DomainEvent
{
    public Guid CouponId { get; }

    public CouponDepletedEvent(Guid couponId)
    {
        CouponId = couponId;
    }
}
