using Bcommerce.BuildingBlocks.Domain.Base;
using Bcommerce.Modules.Coupons.Domain.Enums;
using Bcommerce.Modules.Coupons.Domain.Events;
using Bcommerce.Modules.Coupons.Domain.ValueObjects;

namespace Bcommerce.Modules.Coupons.Domain.Entities;

public class Coupon : AggregateRoot<Guid>
{
    private readonly List<CouponUsage> _usages = new();
    private readonly List<CouponEligibility> _eligibilities = new();
    
    public CouponCode Code { get; private set; }
    public string Description { get; private set; }
    public DiscountValue Discount { get; private set; }
    public ValidityPeriod Validity { get; private set; }
    public CouponStatus Status { get; private set; }
    
    public int? MaxUsageCount { get; private set; }
    public int UsageCount { get; private set; }
    public int? MaxUsagePerUser { get; private set; }
    public decimal? MinOrderAmount { get; private set; }

    public IReadOnlyCollection<CouponUsage> Usages => _usages.AsReadOnly();
    public IReadOnlyCollection<CouponEligibility> Eligibilities => _eligibilities.AsReadOnly();

    private Coupon() { }

    public static Coupon Create(string code, string description, DiscountValue discount, ValidityPeriod validity, int? maxUsageCount, int? maxUsagePerUser, decimal? minOrderAmount)
    {
        var coupon = new Coupon
        {
            Id = Guid.NewGuid(),
            Code = new CouponCode(code),
            Description = description,
            Discount = discount,
            Validity = validity,
            MaxUsageCount = maxUsageCount,
            MaxUsagePerUser = maxUsagePerUser,
            MinOrderAmount = minOrderAmount,
            Status = CouponStatus.Draft,
            CreatedAt = DateTime.UtcNow
        };
        
        coupon.AddDomainEvent(new CouponCreatedEvent(coupon.Id, code));
        return coupon;
    }

    public void Activate()
    {
        if (Status == CouponStatus.Draft || Status == CouponStatus.Paused)
        {
            Status = CouponStatus.Active;
        }
    }

    public void MarkAsUsed(Guid userId, Guid orderId, decimal discountAmount)
    {
        if (Status != CouponStatus.Active) throw new InvalidOperationException("Coupon is not active.");
        
        // Additional validations (expiry, limits) should be done by domain service or validator before calling this, 
        // OR integrated here. For simplicity, assuming validation passed.
        
        UsageCount++;
        _usages.Add(new CouponUsage(Id, userId, orderId, discountAmount));
        
        AddDomainEvent(new CouponUsedEvent(Id, userId, orderId, discountAmount));

        if (MaxUsageCount.HasValue && UsageCount >= MaxUsageCount.Value)
        {
            Status = CouponStatus.Depleted;
            AddDomainEvent(new CouponDepletedEvent(Id));
        }
    }
    
    public void AddEligibility(CouponEligibility eligibility)
    {
        _eligibilities.Add(eligibility);
    }
}
