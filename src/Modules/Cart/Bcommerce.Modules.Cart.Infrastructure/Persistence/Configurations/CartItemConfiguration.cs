using Bcommerce.Modules.Cart.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Bcommerce.Modules.Cart.Infrastructure.Persistence.Configurations;

public class CartItemConfiguration : IEntityTypeConfiguration<CartItem>
{
    public void Configure(EntityTypeBuilder<CartItem> builder)
    {
        builder.ToTable("CartItems");

        builder.HasKey(i => i.Id);

        builder.OwnsOne(i => i.Product, product =>
        {
            product.Property(p => p.ProductId).HasColumnName("ProductId").IsRequired();
            product.Property(p => p.Name).HasColumnName("ProductName").IsRequired().HasMaxLength(200);
            product.Property(p => p.Sku).HasColumnName("ProductSku").IsRequired().HasMaxLength(50);
            product.Property(p => p.Price).HasColumnName("ProductPrice").IsRequired();
            product.Property(p => p.ImageUrl).HasColumnName("ProductImageUrl").HasMaxLength(500);
        });

        builder.Property(i => i.Quantity)
            .IsRequired();
            
        builder.Ignore(i => i.TotalPrice);
    }
}
