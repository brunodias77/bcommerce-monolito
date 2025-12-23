using Bcommerce.BuildingBlocks.Domain.Base;

namespace Bcommerce.Modules.Orders.Domain.ValueObjects;

public class TrackingCode : ValueObject
{
    public string Value { get; }

    public TrackingCode(string value)
    {
         if (string.IsNullOrWhiteSpace(value))
        {
            throw new ArgumentException("Tracking code cannot be empty", nameof(value));
        }
        Value = value;
    }
    
    public static implicit operator string(TrackingCode code) => code.Value;
    public static implicit operator TrackingCode(string value) => new(value);

    protected override IEnumerable<object> GetEqualityComponents()
    {
        yield return Value;
    }
}
