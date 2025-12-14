using BuildingBlocks.Domain.Events;

namespace Catalog.Core.Events;

/// <summary>
/// Evento levantado quando estoque reservado é liberado.
/// </summary>
[AggregateType("Product")]
public class StockReleasedEvent : DomainEvent
{
    public Guid ProductId { get; }
    public int Quantity { get; }
    public string ReferenceType { get; }
    public Guid ReferenceId { get; }

    public override Guid AggregateId => ProductId;

    public StockReleasedEvent(Guid productId, int quantity, string referenceType, Guid referenceId)
    {
        ProductId = productId;
        Quantity = quantity;
        ReferenceType = referenceType;
        ReferenceId = referenceId;
    }
}
