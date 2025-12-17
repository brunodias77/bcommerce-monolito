using Bcommerce.BuildingBlocks.Domain.Base;

namespace Bcommerce.Modules.Catalog.Domain.ValueObjects;

public class Slug : ValueObject
{
    public string Value { get; private set; }

    public Slug(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            throw new ArgumentException("Slug cannot be empty", nameof(value));
        }

        Value = value.ToLowerInvariant();
    }
    
    public static implicit operator string(Slug slug) => slug.Value;

    protected override IEnumerable<object> GetEqualityComponents()
    {
        yield return Value;
    }
}
