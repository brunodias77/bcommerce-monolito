using Bcommerce.Modules.Coupons.Domain.Entities;
using Bcommerce.Modules.Coupons.Domain.Enums;
using Bcommerce.Modules.Coupons.Domain.ValueObjects;
using FluentAssertions;
using Xunit;

namespace Bcommerce.Modules.Coupons.Domain.Tests;

public class CouponTests
{
    [Fact]
    public void Create_ShouldInitializeDraftCoupon()
    {
        // Arrange
        var discount = new DiscountValue(10, CouponType.Percentage);
        var validity = new ValidityPeriod(DateTime.UtcNow, DateTime.UtcNow.AddDays(10));
        
        // Act
        var coupon = Coupon.Create("TEST10", "Test Coupon", discount, validity, 100, 1, 50);

        // Assert
        coupon.Status.Should().Be(CouponStatus.Draft);
        coupon.Code.Value.Should().Be("TEST10");
    }

    [Fact]
    public void Activate_ShouldChangeStatusToActive()
    {
         // Arrange
        var discount = new DiscountValue(10, CouponType.Percentage);
        var validity = new ValidityPeriod(DateTime.UtcNow, DateTime.UtcNow.AddDays(10));
        var coupon = Coupon.Create("TEST10", "Test", discount, validity, 100, 1, 50);

        // Act
        coupon.Activate();

        // Assert
        coupon.Status.Should().Be(CouponStatus.Active);
    }

    [Fact]
    public void MarkAsUsed_ShouldIncrementUsageCount()
    {
        // Arrange
        var discount = new DiscountValue(10, CouponType.Percentage);
        var validity = new ValidityPeriod(DateTime.UtcNow, DateTime.UtcNow.AddDays(10));
        var coupon = Coupon.Create("TEST10", "Test", discount, validity, 100, 1, 50);
        coupon.Activate();

        // Act
        coupon.MarkAsUsed(Guid.NewGuid(), Guid.NewGuid(), 10m);

        // Assert
        coupon.UsageCount.Should().Be(1);
    }
}
