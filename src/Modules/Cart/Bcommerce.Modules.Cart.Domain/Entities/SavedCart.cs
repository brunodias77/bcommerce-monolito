using Bcommerce.BuildingBlocks.Domain.Base;
using Bcommerce.Modules.Cart.Domain.ValueObjects;

namespace Bcommerce.Modules.Cart.Domain.Entities;

public class SavedCart : Entity<Guid>
{
    public Guid UserId { get; private set; }
    public string Name { get; private set; }
    private readonly List<CartItem> _items = new();
    public IReadOnlyCollection<CartItem> Items => _items.AsReadOnly();

    private SavedCart() { }

    public SavedCart(Guid userId, string name)
    {
        Id = Guid.NewGuid();
        UserId = userId;
        Name = name;
        CreatedAt = DateTime.UtcNow;
    }

    public void AddItem(ProductSnapshot product, int quantity)
    {
         var newItem = new CartItem(Id, product, quantity);
        _items.Add(newItem);
    }
}
