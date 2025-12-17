using Bcommerce.BuildingBlocks.Domain.Base;

namespace Bcommerce.Modules.Orders.Domain.ValueObjects;

public class OrderTotal : ValueObject
{
    public decimal ItemsTotal { get; }
    public decimal ShippingFee { get; }
    public decimal Discount { get; }
    public decimal Total => ItemsTotal + ShippingFee - Discount;

    public OrderTotal(decimal itemsTotal, decimal shippingFee, decimal discount)
    {
        ItemsTotal = itemsTotal;
        ShippingFee = shippingFee;
        Discount = discount;
    }

    protected override IEnumerable<object> GetEqualityComponents()
    {
        yield return ItemsTotal;
        yield return ShippingFee;
        yield return Discount;
    }
}
