using Bcommerce.BuildingBlocks.Domain.Base;
using Bcommerce.Modules.Cart.Domain.ValueObjects;

namespace Bcommerce.Modules.Cart.Domain.Entities;

public class CartItem : Entity<Guid>
{
    public Guid CartId { get; private set; }
    public ProductSnapshot Product { get; private set; }
    public int Quantity { get; private set; }
    public decimal TotalPrice => Product.Price * Quantity;

    private CartItem() { } // EF Core

    public CartItem(Guid cartId, ProductSnapshot product, int quantity)
    {
        Id = Guid.NewGuid();
        CartId = cartId;
        Product = product;
        SetQuantity(quantity);
    }

    public void SetQuantity(int quantity)
    {
        if (quantity <= 0)
        {
            throw new ArgumentException("Quantity must be greater than zero.", nameof(quantity));
        }
        Quantity = quantity;
    }

    public void AddQuantity(int quantity)
    {
        SetQuantity(Quantity + quantity);
    }
}
