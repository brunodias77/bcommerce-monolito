using Bcommerce.Modules.Cart.Domain.Entities;
using Bcommerce.Modules.Cart.Domain.ValueObjects;
using FluentAssertions;
using Xunit;

namespace Bcommerce.Modules.Cart.Domain.Tests;

public class ShoppingCartTests
{
    [Fact]
    public void AddItem_ShouldAddItemToCart_WhenNewItem()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var cart = ShoppingCart.Create(userId);
        var productId = Guid.NewGuid();
        var productSnapshot = new ProductSnapshot(productId, "Product A", "img.jpg", 100m);
        
        // Act
        cart.AddItem(productSnapshot, 2);

        // Assert
        cart.Items.Should().HaveCount(1);
        cart.Items.First().Quantity.Should().Be(2);
        cart.TotalAmount.Should().Be(200m);
    }

    [Fact]
    public void RemoveItem_ShouldRemoveItemFromCart()
    {
        // Arrange
        var cart = ShoppingCart.Create(Guid.NewGuid());
        var prod = new ProductSnapshot(Guid.NewGuid(), "P", "i", 10m);
        cart.AddItem(prod, 1);
        
        // Act
        cart.RemoveItem(prod.ProductId);

        // Assert
        cart.Items.Should().BeEmpty();
        cart.TotalAmount.Should().Be(0);
    }
}
