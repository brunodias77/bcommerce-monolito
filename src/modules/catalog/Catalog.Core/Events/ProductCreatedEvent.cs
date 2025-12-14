using BuildingBlocks.Domain.Events;

namespace Catalog.Core.Events;

/// <summary>
/// Evento levantado quando um novo produto é criado.
/// </summary>
[AggregateType("Product")]
public class ProductCreatedEvent : DomainEvent
{
    public Guid ProductId { get; }
    public string Sku { get; }
    public string Name { get; }
    public decimal Price { get; }

    public override Guid AggregateId => ProductId;

    public ProductCreatedEvent(Guid productId, string sku, string name, decimal price)
    {
        ProductId = productId;
        Sku = sku;
        Name = name;
        Price = price;
    }
}
