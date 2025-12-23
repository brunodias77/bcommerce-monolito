using Bcommerce.Modules.Orders.Domain.Entities;
using Bcommerce.Modules.Orders.Domain.Enums;
using Bcommerce.Modules.Orders.Domain.ValueObjects;
using FluentAssertions;
using Xunit;

namespace Bcommerce.Modules.Orders.Domain.Tests;

public class OrderTests
{
    [Fact]
    public void Create_ShouldInitializeOrderWithPendingPaymentStatus()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var shippingAddress = new ShippingAddress("Street", "1", null, "Hood", "City", "ST", "00000000", "Country");
        var items = new List<OrderItem>
        {
            new OrderItem(Guid.NewGuid(), "Prod", "SKU", "url", 100m, 1)
        };
        
        // Act
        var order = Order.Create(userId, shippingAddress, items, ShippingMethod.Standard, 10m, 0m);

        // Assert
        order.Status.Should().Be(OrderStatus.PendingPayment);
        order.Total.Total.Should().Be(110m); // 100 + 10 shipping
    }

    [Fact]
    public void MarkAsPaid_ShouldChangeStatusToPaid()
    {
         // Arrange
        var userId = Guid.NewGuid();
        var shippingAddress = new ShippingAddress("Street", "1", null, "Hood", "City", "ST", "00000000", "Country");
        var items = new List<OrderItem> { new OrderItem(Guid.NewGuid(), "P", "S", "u", 10, 1) };
        var order = Order.Create(userId, shippingAddress, items, ShippingMethod.Standard, 5, 0);

        // Act
        order.MarkAsPaid();

        // Assert
        order.Status.Should().Be(OrderStatus.Paid);
    }
}
