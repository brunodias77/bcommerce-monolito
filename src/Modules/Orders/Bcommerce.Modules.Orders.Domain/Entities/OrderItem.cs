using Bcommerce.BuildingBlocks.Domain.Base;

namespace Bcommerce.Modules.Orders.Domain.Entities;

public class OrderItem : Entity<Guid>
{
    public Guid OrderId { get; private set; }
    public Guid ProductId { get; private set; }
    public string ProductName { get; private set; }
    public string ProductSku { get; private set; }
    public string ProductImageUrl { get; private set; }
    public decimal UnitPrice { get; private set; }
    public int Quantity { get; private set; }
    public decimal TotalPrice => UnitPrice * Quantity;

    private OrderItem() { }

    public OrderItem(Guid orderId, Guid productId, string productName, string productSku, string productImageUrl, decimal unitPrice, int quantity)
    {
        Id = Guid.NewGuid();
        OrderId = orderId;
        ProductId = productId;
        ProductName = productName;
        ProductSku = productSku;
        ProductImageUrl = productImageUrl;
        UnitPrice = unitPrice;
        Quantity = quantity;
    }
}
