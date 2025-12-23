using Bcommerce.BuildingBlocks.Domain.Base;

namespace Bcommerce.Modules.Orders.Domain.ValueObjects;

public class OrderNumber : ValueObject
{
    public string Value { get; }

    public OrderNumber(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            throw new ArgumentException("Order number cannot be empty", nameof(value));
        }

        Value = value;
    }
    
    public static implicit operator string(OrderNumber number) => number.Value;
    public static implicit operator OrderNumber(string value) => new(value);

    // Initial basic implementation - can be enhanced with generation logic
    public static OrderNumber Generate() => new($"{DateTime.UtcNow.Year}{DateTime.UtcNow.DayOfYear}-{Guid.NewGuid().ToString().Substring(0, 8).ToUpper()}");

    protected override IEnumerable<object> GetEqualityComponents()
    {
        yield return Value;
    }
}
