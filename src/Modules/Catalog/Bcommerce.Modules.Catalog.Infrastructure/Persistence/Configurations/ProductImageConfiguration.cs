using Bcommerce.Modules.Catalog.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Bcommerce.Modules.Catalog.Infrastructure.Persistence.Configurations;

public class ProductImageConfiguration : IEntityTypeConfiguration<ProductImage>
{
    public void Configure(EntityTypeBuilder<ProductImage> builder)
    {
        builder.ToTable("ProductImages");

        builder.HasKey(p => p.Id);

        builder.Property(p => p.Url)
            .HasMaxLength(500)
            .IsRequired();

        builder.Property(p => p.IsPrimary)
            .IsRequired();
            
        builder.Property(p => p.SortOrder)
            .IsRequired();
    }
}
