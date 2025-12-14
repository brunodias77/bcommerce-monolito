using Catalog.Core.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Catalog.Infrastructure.Persistence.Configurations;

/// <summary>
/// Configuração EF Core para a entidade ProductReview.
/// Corresponde à tabela catalog.product_reviews.
/// </summary>
public class ProductReviewConfiguration : IEntityTypeConfiguration<ProductReview>
{
    public void Configure(EntityTypeBuilder<ProductReview> builder)
    {
        builder.ToTable("product_reviews", "catalog");

        builder.HasKey(r => r.Id);

        builder.Property(r => r.Id)
            .HasColumnName("id")
            .IsRequired();

        builder.Property(r => r.ProductId)
            .HasColumnName("product_id")
            .IsRequired();

        builder.Property(r => r.UserId)
            .HasColumnName("user_id")
            .IsRequired();

        builder.Property(r => r.OrderId)
            .HasColumnName("order_id");

        builder.Property(r => r.Rating)
            .HasColumnName("rating")
            .IsRequired();

        builder.Property(r => r.Title)
            .HasColumnName("title")
            .HasMaxLength(200);

        builder.Property(r => r.Comment)
            .HasColumnName("comment");

        builder.Property(r => r.IsVerifiedPurchase)
            .HasColumnName("is_verified_purchase")
            .HasDefaultValue(false);

        builder.Property(r => r.IsApproved)
            .HasColumnName("is_approved")
            .HasDefaultValue(false);

        builder.Property(r => r.SellerResponse)
            .HasColumnName("seller_response");

        builder.Property(r => r.SellerRespondedAt)
            .HasColumnName("seller_responded_at");

        // Timestamps
        builder.Property(r => r.CreatedAt)
            .HasColumnName("created_at")
            .IsRequired();

        builder.Property(r => r.UpdatedAt)
            .HasColumnName("updated_at")
            .IsRequired();

        builder.Property(r => r.DeletedAt)
            .HasColumnName("deleted_at");

        // Índices
        builder.HasIndex(r => r.ProductId)
            .HasDatabaseName("ix_product_reviews_product_id");

        builder.HasIndex(r => r.UserId)
            .HasDatabaseName("ix_product_reviews_user_id");

        builder.HasIndex(r => new { r.ProductId, r.UserId })
            .HasDatabaseName("ix_product_reviews_unique")
            .IsUnique();

        builder.HasIndex(r => new { r.ProductId, r.IsApproved })
            .HasDatabaseName("ix_product_reviews_approved")
            .HasFilter("is_approved = true AND deleted_at IS NULL");

        builder.Ignore(r => r.DomainEvents);
    }
}
