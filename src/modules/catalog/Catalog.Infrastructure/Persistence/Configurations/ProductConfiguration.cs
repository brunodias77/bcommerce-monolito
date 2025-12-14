using Catalog.Core.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Catalog.Infrastructure.Persistence.Configurations;

/// <summary>
/// Configuração EF Core para a entidade Product.
/// Corresponde à tabela catalog.products.
/// </summary>
public class ProductConfiguration : IEntityTypeConfiguration<Product>
{
    public void Configure(EntityTypeBuilder<Product> builder)
    {
        builder.ToTable("products", "catalog");

        builder.HasKey(p => p.Id);

        builder.Property(p => p.Id)
            .HasColumnName("id")
            .IsRequired();

        // Identificação
        builder.Property(p => p.Sku)
            .HasColumnName("sku")
            .HasMaxLength(100)
            .IsRequired();

        builder.Property(p => p.Slug)
            .HasColumnName("slug")
            .HasMaxLength(200)
            .IsRequired();

        builder.Property(p => p.Barcode)
            .HasColumnName("barcode")
            .HasMaxLength(50);

        // Dados do produto
        builder.Property(p => p.Name)
            .HasColumnName("name")
            .HasMaxLength(150)
            .IsRequired();

        builder.Property(p => p.ShortDescription)
            .HasColumnName("short_description")
            .HasMaxLength(500);

        builder.Property(p => p.Description)
            .HasColumnName("description");

        // Preços
        builder.Property(p => p.Price)
            .HasColumnName("price")
            .HasPrecision(10, 2)
            .IsRequired();

        builder.Property(p => p.CompareAtPrice)
            .HasColumnName("compare_at_price")
            .HasPrecision(10, 2);

        builder.Property(p => p.CostPrice)
            .HasColumnName("cost_price")
            .HasPrecision(10, 2);

        // Estoque
        builder.Property(p => p.Stock)
            .HasColumnName("stock")
            .IsRequired();

        builder.Property(p => p.ReservedStock)
            .HasColumnName("reserved_stock")
            .IsRequired();

        builder.Property(p => p.LowStockThreshold)
            .HasColumnName("low_stock_threshold")
            .HasDefaultValue(10);

        // Dimensões
        builder.Property(p => p.WeightGrams)
            .HasColumnName("weight_grams");

        builder.Property(p => p.HeightCm)
            .HasColumnName("height_cm")
            .HasPrecision(6, 2);

        builder.Property(p => p.WidthCm)
            .HasColumnName("width_cm")
            .HasPrecision(6, 2);

        builder.Property(p => p.LengthCm)
            .HasColumnName("length_cm")
            .HasPrecision(6, 2);

        // SEO
        builder.Property(p => p.MetaTitle)
            .HasColumnName("meta_title")
            .HasMaxLength(70);

        builder.Property(p => p.MetaDescription)
            .HasColumnName("meta_description")
            .HasMaxLength(160);

        // Status e flags
        builder.Property(p => p.Status)
            .HasColumnName("status")
            .HasConversion<string>()
            .HasMaxLength(20)
            .IsRequired();

        builder.Property(p => p.IsFeatured)
            .HasColumnName("is_featured")
            .HasDefaultValue(false);

        builder.Property(p => p.IsDigital)
            .HasColumnName("is_digital")
            .HasDefaultValue(false);

        builder.Property(p => p.RequiresShipping)
            .HasColumnName("requires_shipping")
            .HasDefaultValue(true);

        // JSON
        builder.Property(p => p.Attributes)
            .HasColumnName("attributes")
            .HasColumnType("jsonb");

        builder.Property(p => p.Tags)
            .HasColumnName("tags");

        // Versionamento
        builder.Property(p => p.Version)
            .HasColumnName("version")
            .IsRequired()
            .IsConcurrencyToken();

        // Timestamps
        builder.Property(p => p.PublishedAt)
            .HasColumnName("published_at");

        builder.Property(p => p.CreatedAt)
            .HasColumnName("created_at")
            .IsRequired();

        builder.Property(p => p.UpdatedAt)
            .HasColumnName("updated_at")
            .IsRequired();

        builder.Property(p => p.DeletedAt)
            .HasColumnName("deleted_at");

        // Relacionamentos
        builder.Property(p => p.CategoryId)
            .HasColumnName("category_id");

        builder.Property(p => p.BrandId)
            .HasColumnName("brand_id");

        builder.HasOne(p => p.Category)
            .WithMany(c => c.Products)
            .HasForeignKey(p => p.CategoryId)
            .OnDelete(DeleteBehavior.SetNull);

        builder.HasOne(p => p.Brand)
            .WithMany(b => b.Products)
            .HasForeignKey(p => p.BrandId)
            .OnDelete(DeleteBehavior.SetNull);

        builder.HasMany(p => p.Images)
            .WithOne(i => i.Product)
            .HasForeignKey(i => i.ProductId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(p => p.StockMovements)
            .WithOne(m => m.Product)
            .HasForeignKey(m => m.ProductId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(p => p.StockReservations)
            .WithOne(r => r.Product)
            .HasForeignKey(r => r.ProductId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(p => p.Reviews)
            .WithOne(r => r.Product)
            .HasForeignKey(r => r.ProductId)
            .OnDelete(DeleteBehavior.Cascade);

        // Índices
        builder.HasIndex(p => p.Sku)
            .HasDatabaseName("ix_products_sku")
            .IsUnique();

        builder.HasIndex(p => p.Slug)
            .HasDatabaseName("ix_products_slug")
            .IsUnique();

        builder.HasIndex(p => p.CategoryId)
            .HasDatabaseName("ix_products_category_id");

        builder.HasIndex(p => p.BrandId)
            .HasDatabaseName("ix_products_brand_id");

        builder.HasIndex(p => p.Status)
            .HasDatabaseName("ix_products_status");

        builder.HasIndex(p => new { p.Status, p.DeletedAt })
            .HasDatabaseName("ix_products_active")
            .HasFilter("deleted_at IS NULL");

        // Ignorar DomainEvents (não persiste no banco)
        builder.Ignore(p => p.DomainEvents);
    }
}
