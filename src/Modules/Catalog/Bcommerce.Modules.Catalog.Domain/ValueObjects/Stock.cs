using Bcommerce.BuildingBlocks.Domain.Base;

namespace Bcommerce.Modules.Catalog.Domain.ValueObjects;

public class Stock : ValueObject
{
    public int Quantity { get; private set; }
    public int Reserved { get; private set; }
    public int Available => Quantity - Reserved;

    public Stock(int quantity, int reserved = 0)
    {
        if (quantity < 0) throw new ArgumentException("Stock quantity cannot be negative", nameof(quantity));
        if (reserved < 0) throw new ArgumentException("Reserved stock cannot be negative", nameof(reserved));

        Quantity = quantity;
        Reserved = reserved;
    }

    public Stock Reserve(int amount)
    {
        if (amount <= 0) throw new ArgumentException("Amount must be greater than zero", nameof(amount));
        if (Available < amount) throw new InvalidOperationException("Not enough stock available");

        return new Stock(Quantity, Reserved + amount);
    }

    public Stock Release(int amount)
    {
        if (amount <= 0) throw new ArgumentException("Amount must be greater than zero", nameof(amount));
        if (Reserved < amount) throw new InvalidOperationException("Cannot release more than reserved");

        return new Stock(Quantity, Reserved - amount);
    }

    public Stock Add(int amount)
    {
        if (amount <= 0) throw new ArgumentException("Amount must be greater than zero", nameof(amount));
        return new Stock(Quantity + amount, Reserved);
    }

    public Stock Remove(int amount)
    {
        if (amount <= 0) throw new ArgumentException("Amount must be greater than zero", nameof(amount));
        if (Available < amount) throw new InvalidOperationException("Not enough stock available to remove");
        
        return new Stock(Quantity - amount, Reserved);
    }

    protected override IEnumerable<object> GetEqualityComponents()
    {
        yield return Quantity;
        yield return Reserved;
    }
}
