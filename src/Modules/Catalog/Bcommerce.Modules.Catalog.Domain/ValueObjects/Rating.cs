using Bcommerce.BuildingBlocks.Domain.Base;

namespace Bcommerce.Modules.Catalog.Domain.ValueObjects;

public class Rating : ValueObject
{
    public int Value { get; private set; }

    public Rating(int value)
    {
        if (value < 1 || value > 5)
        {
            throw new ArgumentOutOfRangeException(nameof(value), "Rating must be between 1 and 5");
        }

        Value = value;
    }

    public static implicit operator int(Rating rating) => rating.Value;
    public static implicit operator Rating(int value) => new(value);

    protected override IEnumerable<object> GetEqualityComponents()
    {
        yield return Value;
    }
}
