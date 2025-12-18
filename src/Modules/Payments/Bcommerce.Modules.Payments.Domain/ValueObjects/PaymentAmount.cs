using Bcommerce.BuildingBlocks.Domain.Base;

namespace Bcommerce.Modules.Payments.Domain.ValueObjects;

public class PaymentAmount : ValueObject
{
    public decimal Value { get; }
    public string Currency { get; } = "BRL"; // Defaulting to BRL as implied context

    public PaymentAmount(decimal value, string currency = "BRL")
    {
        if (value < 0)
        {
            throw new ArgumentException("Payment amount cannot be negative", nameof(value));
        }

        Value = value;
        Currency = currency;
    }

    protected override IEnumerable<object> GetEqualityComponents()
    {
        yield return Value;
        yield return Currency;
    }
}
