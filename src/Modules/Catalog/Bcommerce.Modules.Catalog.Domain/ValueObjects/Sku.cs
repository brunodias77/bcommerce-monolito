using Bcommerce.BuildingBlocks.Domain.Base;

namespace Bcommerce.Modules.Catalog.Domain.ValueObjects;

public class Sku : ValueObject
{
    public string Value { get; private set; }

    public Sku(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            throw new ArgumentException("Sku cannot be empty", nameof(value));
        }

        Value = value;
    }

    public static implicit operator string(Sku sku) => sku.Value;
    public static implicit operator Sku(string value) => new(value);

    protected override IEnumerable<object> GetEqualityComponents()
    {
        yield return Value;
    }
}
