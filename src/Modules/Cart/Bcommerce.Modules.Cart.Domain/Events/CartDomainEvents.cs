using Bcommerce.BuildingBlocks.Domain.Base;

namespace Bcommerce.Modules.Cart.Domain.Events;

public class CartCreatedEvent : DomainEvent
{
    public Guid CartId { get; }
    public Guid? UserId { get; }
    public Guid? SessionId { get; }

    public CartCreatedEvent(Guid cartId, Guid? userId, Guid? sessionId)
    {
        CartId = cartId;
        UserId = userId;
        SessionId = sessionId;
    }
}

public class ItemAddedToCartEvent : DomainEvent
{
    public Guid CartId { get; }
    public Guid ProductId { get; }
    public int Quantity { get; }

    public ItemAddedToCartEvent(Guid cartId, Guid productId, int quantity)
    {
        CartId = cartId;
        ProductId = productId;
        Quantity = quantity;
    }
}

public class ItemRemovedFromCartEvent : DomainEvent
{
    public Guid CartId { get; }
    public Guid ProductId { get; }

    public ItemRemovedFromCartEvent(Guid cartId, Guid productId)
    {
        CartId = cartId;
        ProductId = productId;
    }
}

public class CartConvertedEvent : DomainEvent
{
    public Guid CartId { get; }

    public CartConvertedEvent(Guid cartId)
    {
        CartId = cartId;
    }
}

public class CartAbandonedEvent : DomainEvent
{
    public Guid CartId { get; }

    public CartAbandonedEvent(Guid cartId)
    {
        CartId = cartId;
    }
}
