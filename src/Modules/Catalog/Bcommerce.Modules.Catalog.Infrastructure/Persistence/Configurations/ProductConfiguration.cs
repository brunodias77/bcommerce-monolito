using Bcommerce.Modules.Catalog.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Bcommerce.Modules.Catalog.Infrastructure.Persistence.Configurations;

public class ProductConfiguration : IEntityTypeConfiguration<Product>
{
    public void Configure(EntityTypeBuilder<Product> builder)
    {
        builder.ToTable("Products");

        builder.HasKey(p => p.Id);

        builder.Property(p => p.Name)
            .HasMaxLength(200)
            .IsRequired();

        builder.Property(p => p.Description)
            .HasMaxLength(2000);

        builder.OwnsOne(p => p.Price, price =>
        {
            price.Property(p => p.Amount).HasColumnName("PriceAmount").HasColumnType("decimal(18,2)").IsRequired();
            price.Property(p => p.Currency).HasColumnName("PriceCurrency").HasMaxLength(3).IsRequired();
        });

        builder.OwnsOne(p => p.Stock, stock =>
        {
            stock.Property(p => p.Quantity).HasColumnName("StockQuantity").IsRequired();
            stock.Property(p => p.Reserved).HasColumnName("StockReserved").IsRequired();
            stock.Ignore(p => p.Available);
        });

        builder.OwnsOne(p => p.Dimensions, dimensions =>
        {
            dimensions.Property(p => p.Height).HasColumnName("Height").HasColumnType("decimal(10,2)");
            dimensions.Property(p => p.Width).HasColumnName("Width").HasColumnType("decimal(10,2)");
            dimensions.Property(p => p.Depth).HasColumnName("Depth").HasColumnType("decimal(10,2)");
            dimensions.Property(p => p.Weight).HasColumnName("Weight").HasColumnType("decimal(10,2)");
        });

        builder.OwnsOne(p => p.Sku, sku =>
        {
            sku.Property(p => p.Value).HasColumnName("Sku").HasMaxLength(50).IsRequired();
            sku.HasIndex(p => p.Value).IsUnique();
        });

        builder.OwnsOne(p => p.Slug, slug =>
        {
            slug.Property(p => p.Value).HasColumnName("Slug").HasMaxLength(200).IsRequired();
            slug.HasIndex(p => p.Value).IsUnique();
        });

        builder.Property(p => p.Status)
            .HasConversion<int>()
            .IsRequired();

        builder.HasOne(p => p.Category)
            .WithMany()
            .HasForeignKey(p => p.CategoryId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(p => p.Brand)
            .WithMany()
            .HasForeignKey(p => p.BrandId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasMany(p => p.Images)
            .WithOne()
            .HasForeignKey("ProductId")
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(p => p.Reviews)
            .WithOne()
            .HasForeignKey("ProductId")
            .OnDelete(DeleteBehavior.Cascade);
    }
}
