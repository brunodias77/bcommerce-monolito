using Bcommerce.BuildingBlocks.Domain.Base;

namespace Bcommerce.Modules.Catalog.Domain.Entities;

public class StockReservation : AggregateRoot<Guid>
{
    public Guid ProductId { get; private set; }
    public int Quantity { get; private set; }
    public DateTime ExpiresAt { get; private set; }
    
    // Could be OrderId or CartId
    public Guid ReferenceId { get; private set; }
    public string ReferenceType { get; private set; }

    protected StockReservation() { }

    public StockReservation(Guid productId, int quantity, DateTime expiresAt, Guid referenceId, string referenceType)
    {
        Id = Guid.NewGuid();
        ProductId = productId;
        Quantity = quantity;
        ExpiresAt = expiresAt;
        ReferenceId = referenceId;
        ReferenceType = referenceType;
        CreatedAt = DateTime.UtcNow;
    }
}
