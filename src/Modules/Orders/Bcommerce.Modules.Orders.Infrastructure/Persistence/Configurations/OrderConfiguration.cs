using Bcommerce.Modules.Orders.Domain.Entities;
using Bcommerce.Modules.Orders.Domain.ValueObjects;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Bcommerce.Modules.Orders.Infrastructure.Persistence.Configurations;

public class OrderConfiguration : IEntityTypeConfiguration<Order>
{
    public void Configure(EntityTypeBuilder<Order> builder)
    {
        builder.ToTable("Orders");

        builder.HasKey(o => o.Id);

        builder.OwnsOne(o => o.OrderNumber, number =>
        {
            number.Property(x => x.Value).HasColumnName("OrderNumber").IsRequired().HasMaxLength(50);
            number.HasIndex(x => x.Value).IsUnique();
        });

        builder.OwnsOne(o => o.ShippingAddress, address =>
        {
            address.Property(a => a.Street).HasColumnName("ShippingStreet").HasMaxLength(200);
            address.Property(a => a.Number).HasColumnName("ShippingNumber").HasMaxLength(20);
            address.Property(a => a.Complement).HasColumnName("ShippingComplement").HasMaxLength(100);
            address.Property(a => a.Neighborhood).HasColumnName("ShippingNeighborhood").HasMaxLength(100);
            address.Property(a => a.City).HasColumnName("ShippingCity").HasMaxLength(100);
            address.Property(a => a.State).HasColumnName("ShippingState").HasMaxLength(50);
            address.Property(a => a.ZipCode).HasColumnName("ShippingZipCode").HasMaxLength(20);
            address.Property(a => a.Country).HasColumnName("ShippingCountry").HasMaxLength(50);
        });

        builder.OwnsOne(o => o.Total, total =>
        {
            total.Property(t => t.ItemsTotal).HasColumnName("ItemsTotal").HasPrecision(18, 2);
            total.Property(t => t.ShippingFee).HasColumnName("ShippingFee").HasPrecision(18, 2);
            total.Property(t => t.Discount).HasColumnName("Discount").HasPrecision(18, 2);
            total.Ignore(t => t.Total); // Calculated property
        });

        builder.OwnsOne(o => o.TrackingCode, code =>
        {
            code.Property(c => c.Value).HasColumnName("TrackingCode").HasMaxLength(100);
        });

        builder.Property(o => o.Status).HasConversion<int>().IsRequired();
        builder.Property(o => o.ShippingMethod).HasConversion<int>().IsRequired();
        builder.Property(o => o.CancellationReason).HasConversion<int>();

        builder.HasMany(o => o.Items)
            .WithOne()
            .HasForeignKey(i => i.OrderId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(o => o.StatusHistory)
            .WithOne()
            .HasForeignKey(h => h.OrderId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(o => o.TrackingEvents)
            .WithOne()
            .HasForeignKey(e => e.OrderId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(o => o.Invoice)
            .WithOne()
            .HasForeignKey<Invoice>(i => i.OrderId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(o => o.Refund)
            .WithOne()
            .HasForeignKey<OrderRefund>(r => r.OrderId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
