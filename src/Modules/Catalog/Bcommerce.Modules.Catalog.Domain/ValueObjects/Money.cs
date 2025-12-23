using Bcommerce.BuildingBlocks.Domain.Base;

namespace Bcommerce.Modules.Catalog.Domain.ValueObjects;

public class Money : ValueObject
{
    public decimal Amount { get; private set; }
    public string Currency { get; private set; }

    public Money(decimal amount, string currency = "BRL")
    {
        if (amount < 0)
        {
            throw new ArgumentException("Amount cannot be negative", nameof(amount));
        }

        Amount = amount;
        Currency = currency;
    }
    
    public static Money Zero(string currency = "BRL") => new(0, currency);

    protected override IEnumerable<object> GetEqualityComponents()
    {
        yield return Amount;
        yield return Currency;
    }
}
