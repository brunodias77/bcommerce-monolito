using Bcommerce.BuildingBlocks.Domain.Base;
using Bcommerce.Modules.Cart.Domain.Enums;
using Bcommerce.Modules.Cart.Domain.Events;
using Bcommerce.Modules.Cart.Domain.ValueObjects;

namespace Bcommerce.Modules.Cart.Domain.Entities;

public class ShoppingCart : AggregateRoot<Guid>
{
    private readonly List<CartItem> _items = new();

    public Guid? UserId { get; private set; }
    public SessionId? SessionId { get; private set; } // For guest carts
    public CartStatus Status { get; private set; }
    public IReadOnlyCollection<CartItem> Items => _items.AsReadOnly();
    public decimal TotalAmount => _items.Sum(i => i.TotalPrice);
    public DateTime? ExpiresAt { get; private set; }

    private ShoppingCart() { }

    // Create Cart for User
    public static ShoppingCart CreateForUser(Guid userId)
    {
        var cart = new ShoppingCart
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            Status = CartStatus.Active,
            CreatedAt = DateTime.UtcNow
        };
        cart.AddDomainEvent(new CartCreatedEvent(cart.Id, userId, null));
        return cart;
    }

    // Create Cart for Guest (Session)
    public static ShoppingCart CreateForSession(SessionId sessionId)
    {
        var cart = new ShoppingCart
        {
            Id = Guid.NewGuid(),
            SessionId = sessionId,
            Status = CartStatus.Active,
            CreatedAt = DateTime.UtcNow
        };
        cart.AddDomainEvent(new CartCreatedEvent(cart.Id, null, sessionId.Value));
        return cart;
    }

    public void AddItem(ProductSnapshot product, int quantity)
    {
        if (Status != CartStatus.Active)
        {
            throw new InvalidOperationException("Cannot add items to a non-active cart.");
        }

        var existingItem = _items.FirstOrDefault(i => i.Product.ProductId == product.ProductId);
        if (existingItem != null)
        {
            existingItem.AddQuantity(quantity);
        }
        else
        {
            var newItem = new CartItem(Id, product, quantity);
            _items.Add(newItem);
        }
        
        UpdatedAt = DateTime.UtcNow;
        AddDomainEvent(new ItemAddedToCartEvent(Id, product.ProductId, quantity));
    }

    public void RemoveItem(Guid productId)
    {
        var item = _items.FirstOrDefault(i => i.Product.ProductId == productId);
        if (item == null) return;

        _items.Remove(item);
        UpdatedAt = DateTime.UtcNow;
        AddDomainEvent(new ItemRemovedFromCartEvent(Id, productId));
    }

    public void UpdateItemQuantity(Guid productId, int quantity)
    {
        var item = _items.FirstOrDefault(i => i.Product.ProductId == productId);
        if (item == null) throw new InvalidOperationException("Item not found in cart.");

        item.SetQuantity(quantity);
        UpdatedAt = DateTime.UtcNow;
    }

    public void Clear()
    {
        _items.Clear();
        UpdatedAt = DateTime.UtcNow;
    }
    
    public void MarkAsAbandoned()
    {
        if (Status == CartStatus.Active)
        {
            Status = CartStatus.Abandoned;
            UpdatedAt = DateTime.UtcNow;
            AddDomainEvent(new CartAbandonedEvent(Id));
        }
    }
    
    public void MarkAsConverted()
    {
        Status = CartStatus.Converted;
        UpdatedAt = DateTime.UtcNow;
        AddDomainEvent(new CartConvertedEvent(Id));
    }
}
