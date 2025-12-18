using Bcommerce.Modules.Orders.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Bcommerce.Modules.Orders.Infrastructure.Persistence.Configurations;

public class OrderItemConfiguration : IEntityTypeConfiguration<OrderItem>
{
    public void Configure(EntityTypeBuilder<OrderItem> builder)
    {
        builder.ToTable("OrderItems");
        builder.HasKey(i => i.Id);

        builder.Property(i => i.ProductName).IsRequired().HasMaxLength(200);
        builder.Property(i => i.ProductSku).IsRequired().HasMaxLength(50);
        builder.Property(i => i.ProductImageUrl).HasMaxLength(500);
        builder.Property(i => i.UnitPrice).HasPrecision(18, 2);
        
        builder.Ignore(i => i.TotalPrice);
    }
}
