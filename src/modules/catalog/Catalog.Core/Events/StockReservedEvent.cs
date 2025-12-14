using BuildingBlocks.Domain.Events;

namespace Catalog.Core.Events;

/// <summary>
/// Evento levantado quando estoque é reservado.
/// </summary>
[AggregateType("Product")]
public class StockReservedEvent : DomainEvent
{
    public Guid ProductId { get; }
    public int Quantity { get; }
    public string ReferenceType { get; }
    public Guid ReferenceId { get; }

    public override Guid AggregateId => ProductId;

    public StockReservedEvent(Guid productId, int quantity, string referenceType, Guid referenceId)
    {
        ProductId = productId;
        Quantity = quantity;
        ReferenceType = referenceType;
        ReferenceId = referenceId;
    }
}
