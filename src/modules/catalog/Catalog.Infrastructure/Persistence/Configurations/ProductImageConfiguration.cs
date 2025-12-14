using Catalog.Core.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Catalog.Infrastructure.Persistence.Configurations;

/// <summary>
/// Configuração EF Core para a entidade ProductImage.
/// Corresponde à tabela catalog.product_images.
/// </summary>
public class ProductImageConfiguration : IEntityTypeConfiguration<ProductImage>
{
    public void Configure(EntityTypeBuilder<ProductImage> builder)
    {
        builder.ToTable("product_images", "catalog");

        builder.HasKey(i => i.Id);

        builder.Property(i => i.Id)
            .HasColumnName("id")
            .IsRequired();

        builder.Property(i => i.ProductId)
            .HasColumnName("product_id")
            .IsRequired();

        builder.Property(i => i.Url)
            .HasColumnName("url")
            .IsRequired();

        builder.Property(i => i.AltText)
            .HasColumnName("alt_text")
            .HasMaxLength(255);

        builder.Property(i => i.UrlThumbnail)
            .HasColumnName("url_thumbnail");

        builder.Property(i => i.UrlMedium)
            .HasColumnName("url_medium");

        builder.Property(i => i.UrlLarge)
            .HasColumnName("url_large");

        builder.Property(i => i.IsPrimary)
            .HasColumnName("is_primary")
            .HasDefaultValue(false);

        builder.Property(i => i.SortOrder)
            .HasColumnName("sort_order")
            .HasDefaultValue(0);

        builder.Property(i => i.CreatedAt)
            .HasColumnName("created_at")
            .IsRequired();

        // Índices
        builder.HasIndex(i => i.ProductId)
            .HasDatabaseName("ix_product_images_product_id");

        builder.HasIndex(i => new { i.ProductId, i.IsPrimary })
            .HasDatabaseName("ix_product_images_primary")
            .HasFilter("is_primary = true");

        builder.Ignore(i => i.DomainEvents);
    }
}
