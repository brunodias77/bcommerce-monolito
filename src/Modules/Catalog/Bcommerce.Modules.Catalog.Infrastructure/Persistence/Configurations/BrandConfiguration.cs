using Bcommerce.Modules.Catalog.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Bcommerce.Modules.Catalog.Infrastructure.Persistence.Configurations;

public class BrandConfiguration : IEntityTypeConfiguration<Brand>
{
    public void Configure(EntityTypeBuilder<Brand> builder)
    {
        builder.ToTable("Brands");

        builder.HasKey(b => b.Id);

        builder.Property(b => b.Name)
            .HasMaxLength(100)
            .IsRequired();

        builder.OwnsOne(b => b.Slug, slug =>
        {
            slug.Property(p => p.Value).HasColumnName("Slug").HasMaxLength(150).IsRequired();
            slug.HasIndex(p => p.Value).IsUnique();
        });

        builder.Property(b => b.LogoUrl)
            .HasMaxLength(255);
    }
}
