using BuildingBlocks.Domain.Events;

namespace Catalog.Core.Events;

/// <summary>
/// Evento levantado quando o preço de um produto é alterado.
/// </summary>
[AggregateType("Product")]
public class ProductPriceChangedEvent : DomainEvent
{
    public Guid ProductId { get; }
    public decimal OldPrice { get; }
    public decimal NewPrice { get; }

    public override Guid AggregateId => ProductId;

    public ProductPriceChangedEvent(Guid productId, decimal oldPrice, decimal newPrice)
    {
        ProductId = productId;
        OldPrice = oldPrice;
        NewPrice = newPrice;
    }
}
