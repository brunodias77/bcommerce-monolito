using Bcommerce.Modules.Catalog.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Bcommerce.Modules.Catalog.Infrastructure.Persistence.Configurations;

public class CategoryConfiguration : IEntityTypeConfiguration<Category>
{
    public void Configure(EntityTypeBuilder<Category> builder)
    {
        builder.ToTable("Categories");

        builder.HasKey(c => c.Id);

        builder.Property(c => c.Name)
            .HasMaxLength(100)
            .IsRequired();

        builder.OwnsOne(c => c.Slug, slug =>
        {
            slug.Property(p => p.Value).HasColumnName("Slug").HasMaxLength(150).IsRequired();
            slug.HasIndex(p => p.Value).IsUnique();
        });

        builder.Property(c => c.Description)
            .HasMaxLength(1000);

        builder.HasOne(c => c.Parent)
            .WithMany(c => c.SubCategories)
            .HasForeignKey(c => c.ParentId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
