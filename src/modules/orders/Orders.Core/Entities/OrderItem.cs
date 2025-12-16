using BuildingBlocks.Domain.Entities;

namespace Orders.Core.Entities;

public class OrderItem : Entity
{
    public Guid OrderId { get; private set; }
    public Guid ProductId { get; private set; }
    public string ProductSnapshot { get; private set; } // JSONB
    public decimal UnitPrice { get; private set; }
    public int Quantity { get; private set; }
    public decimal DiscountAmount { get; private set; }
    public decimal Subtotal { get; private set; }

    // Ef Core Constructor
    protected OrderItem() { }

    public OrderItem(Guid orderId, Guid productId, string productSnapshot, decimal unitPrice, int quantity, decimal discountAmount)
    {
        Id = Guid.NewGuid();
        OrderId = orderId;
        ProductId = productId;
        ProductSnapshot = productSnapshot;
        UnitPrice = unitPrice;
        Quantity = quantity;
        DiscountAmount = discountAmount;
        Subtotal = (unitPrice * quantity) - discountAmount;
        CreatedAt = DateTime.UtcNow;
    }

    public DateTime CreatedAt { get; private set; }
}
