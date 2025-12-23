using Bcommerce.BuildingBlocks.Domain.Base;

namespace Bcommerce.Modules.Catalog.Domain.Events;

public class ProductCreatedEvent : DomainEvent
{
    public Guid ProductId { get; }

    public ProductCreatedEvent(Guid productId)
    {
        ProductId = productId;
    }
}

public class ProductPublishedEvent : DomainEvent
{
    public Guid ProductId { get; }

    public ProductPublishedEvent(Guid productId)
    {
        ProductId = productId;
    }
}

public class StockReservedEvent : DomainEvent
{
    public Guid ProductId { get; }
    public int Quantity { get; }
    public Guid ReservationId { get; }

    public StockReservedEvent(Guid productId, int quantity, Guid reservationId)
    {
        ProductId = productId;
        Quantity = quantity;
        ReservationId = reservationId;
    }
}

public class StockReleasedEvent : DomainEvent
{
    public Guid ProductId { get; }
    public int Quantity { get; }
    public Guid ReservationId { get; }

    public StockReleasedEvent(Guid productId, int quantity, Guid reservationId)
    {
        ProductId = productId;
        Quantity = quantity;
        ReservationId = reservationId;
    }
}

public class ReviewAddedEvent : DomainEvent
{
    public Guid ProductId { get; }
    public Guid ReviewId { get; }

    public ReviewAddedEvent(Guid productId, Guid reviewId)
    {
        ProductId = productId;
        ReviewId = reviewId;
    }
}
