using Bcommerce.Modules.Payments.Domain.Entities;
using Bcommerce.Modules.Payments.Domain.Enums;
using Bcommerce.Modules.Payments.Domain.ValueObjects;
using FluentAssertions;
using Xunit;

namespace Bcommerce.Modules.Payments.Domain.Tests;

public class PaymentTests
{
    [Fact]
    public void Create_ShouldInitializePaymentWithPendingStatus()
    {
        // Arrange
        var orderId = Guid.NewGuid();
        var amount = new PaymentAmount(100m);
        
        // Act
        var payment = Payment.Create(orderId, null, amount, PaymentMethodType.CreditCard);

        // Assert
        payment.Status.Should().Be(PaymentStatus.Pending);
        payment.Amount.Value.Should().Be(100m);
    }

    [Fact]
    public void MarkAsAuthorized_ShouldChangeStatusToAuthorized()
    {
        // Arrange
        var payment = Payment.Create(Guid.NewGuid(), null, new PaymentAmount(50), PaymentMethodType.CreditCard);

        // Act
        payment.MarkAsAuthorized();

        // Assert
        payment.Status.Should().Be(PaymentStatus.Authorized);
    }
}
