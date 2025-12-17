using Bcommerce.BuildingBlocks.Domain.Base;

namespace Bcommerce.Modules.Cart.Domain.ValueObjects;

public class CartId : ValueObject
{
    public Guid Value { get; }

    public CartId(Guid value)
    {
        if (value == Guid.Empty)
        {
            throw new ArgumentException("CartId cannot be empty", nameof(value));
        }

        Value = value;
    }

    public static CartId New() => new(Guid.NewGuid());

    protected override IEnumerable<object> GetEqualityComponents()
    {
        yield return Value;
    }
}
