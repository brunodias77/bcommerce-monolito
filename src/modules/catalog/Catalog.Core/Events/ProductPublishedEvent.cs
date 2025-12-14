using BuildingBlocks.Domain.Events;

namespace Catalog.Core.Events;

/// <summary>
/// Evento levantado quando um produto é publicado.
/// </summary>
[AggregateType("Product")]
public class ProductPublishedEvent : DomainEvent
{
    public Guid ProductId { get; }
    public string Name { get; }

    public override Guid AggregateId => ProductId;

    public ProductPublishedEvent(Guid productId, string name)
    {
        ProductId = productId;
        Name = name;
    }
}
