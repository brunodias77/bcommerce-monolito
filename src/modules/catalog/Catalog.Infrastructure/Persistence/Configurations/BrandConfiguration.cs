using Catalog.Core.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Catalog.Infrastructure.Persistence.Configurations;

/// <summary>
/// Configuração EF Core para a entidade Brand.
/// Corresponde à tabela catalog.brands.
/// </summary>
public class BrandConfiguration : IEntityTypeConfiguration<Brand>
{
    public void Configure(EntityTypeBuilder<Brand> builder)
    {
        builder.ToTable("brands", "catalog");

        builder.HasKey(b => b.Id);

        builder.Property(b => b.Id)
            .HasColumnName("id")
            .IsRequired();

        builder.Property(b => b.Name)
            .HasColumnName("name")
            .HasMaxLength(100)
            .IsRequired();

        builder.Property(b => b.Slug)
            .HasColumnName("slug")
            .HasMaxLength(120)
            .IsRequired();

        builder.Property(b => b.Description)
            .HasColumnName("description");

        builder.Property(b => b.LogoUrl)
            .HasColumnName("logo_url");

        builder.Property(b => b.WebsiteUrl)
            .HasColumnName("website_url");

        builder.Property(b => b.IsActive)
            .HasColumnName("is_active")
            .IsRequired();

        builder.Property(b => b.SortOrder)
            .HasColumnName("sort_order")
            .HasDefaultValue(0);

        // Timestamps
        builder.Property(b => b.CreatedAt)
            .HasColumnName("created_at")
            .IsRequired();

        builder.Property(b => b.UpdatedAt)
            .HasColumnName("updated_at")
            .IsRequired();

        builder.Property(b => b.DeletedAt)
            .HasColumnName("deleted_at");

        // Versionamento
        builder.Property(b => b.Version)
            .HasColumnName("version")
            .IsRequired()
            .IsConcurrencyToken();

        // Índices
        builder.HasIndex(b => b.Slug)
            .HasDatabaseName("ix_brands_slug")
            .IsUnique();

        builder.HasIndex(b => new { b.IsActive, b.DeletedAt })
            .HasDatabaseName("ix_brands_active")
            .HasFilter("deleted_at IS NULL");

        builder.Ignore(b => b.DomainEvents);
    }
}
