using Bcommerce.BuildingBlocks.Domain.Base;

namespace Bcommerce.Modules.Coupons.Domain.ValueObjects;

public class CouponCode : ValueObject
{
    public string Value { get; }

    public CouponCode(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            throw new ArgumentException("Coupon code cannot be empty.", nameof(value));
        }

        if (value.Length < 3 || value.Length > 20)
        {
             throw new ArgumentException("Coupon code length must be between 3 and 20 characters.", nameof(value));
        }

        Value = value.ToUpperInvariant();
    }

    public static implicit operator string(CouponCode code) => code.Value;

    protected override IEnumerable<object> GetEqualityComponents()
    {
        yield return Value;
    }
}
