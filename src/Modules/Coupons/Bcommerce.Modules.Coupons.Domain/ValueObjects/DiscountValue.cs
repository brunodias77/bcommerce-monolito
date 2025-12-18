using Bcommerce.BuildingBlocks.Domain.Base;
using Bcommerce.Modules.Coupons.Domain.Enums;

namespace Bcommerce.Modules.Coupons.Domain.ValueObjects;

public class DiscountValue : ValueObject
{
    public decimal Amount { get; }
    public CouponType Type { get; }

    public DiscountValue(decimal amount, CouponType type)
    {
        if (amount <= 0)
        {
             throw new ArgumentException("Discount amount must be greater than zero.", nameof(amount));
        }

        if (type == CouponType.Percentage && amount > 100)
        {
            throw new ArgumentException("Percentage discount cannot exceed 100%.", nameof(amount));
        }

        Amount = amount;
        Type = type;
    }

    public decimal CalculateDiscount(decimal totalAmount)
    {
        if (Type == CouponType.Percentage)
        {
            return totalAmount * (Amount / 100);
        }
        
        if (Type == CouponType.FixedAmount)
        {
            return Amount > totalAmount ? totalAmount : Amount;
        }

        return 0; // FreeShipping handled separately typically
    }

    protected override IEnumerable<object> GetEqualityComponents()
    {
        yield return Amount;
        yield return Type;
    }
}
