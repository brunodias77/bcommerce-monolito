using Bcommerce.Modules.Catalog.Domain.Entities;
using Bcommerce.Modules.Catalog.Domain.Enums;
using Bcommerce.Modules.Catalog.Domain.ValueObjects;
using FluentAssertions;
using Xunit;

namespace Bcommerce.Modules.Catalog.Domain.Tests;

public class ProductTests
{
    [Fact]
    public void Create_ShouldCreateProduct_WhenValidDataProvided()
    {
        // Arrange
        var name = "Laptop";
        var description = "High end laptop";
        var price = new Money(1500, "USD");
        var sku = Sku.Create("LAP-123");
        var categoryId = Guid.NewGuid();

        // Act
        var product = Product.Create(name, description, price, sku, categoryId);

        // Assert
        product.Should().NotBeNull();
        product.Id.Should().NotBeEmpty();
        product.Name.Should().Be(name);
        product.Status.Should().Be(ProductStatus.Draft);
    }

    [Fact]
    public void Activate_ShouldChangeStatusToActive_WhenProductIsDraft()
    {
        // Arrange
        var product = Product.Create("P", "D", new Money(10, "BRL"), Sku.Create("SKU-1"), Guid.NewGuid());
        
        // Act
        product.Activate();

        // Assert
        product.Status.Should().Be(ProductStatus.Active);
    }
}
