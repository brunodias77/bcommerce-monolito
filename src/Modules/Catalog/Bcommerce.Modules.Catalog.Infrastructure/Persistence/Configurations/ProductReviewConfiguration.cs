using Bcommerce.Modules.Catalog.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Bcommerce.Modules.Catalog.Infrastructure.Persistence.Configurations;

public class ProductReviewConfiguration : IEntityTypeConfiguration<ProductReview>
{
    public void Configure(EntityTypeBuilder<ProductReview> builder)
    {
        builder.ToTable("ProductReviews");

        builder.HasKey(r => r.Id);

        builder.OwnsOne(r => r.Rating, rating =>
        {
            rating.Property(p => p.Value).HasColumnName("Rating").IsRequired();
        });

        builder.Property(r => r.Comment)
            .HasMaxLength(1000);
            
        builder.Property(r => r.UserId)
            .IsRequired();
    }
}
