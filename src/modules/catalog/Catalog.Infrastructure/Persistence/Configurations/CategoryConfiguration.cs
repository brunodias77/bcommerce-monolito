using Catalog.Core.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Catalog.Infrastructure.Persistence.Configurations;

/// <summary>
/// Configuração EF Core para a entidade Category.
/// Corresponde à tabela catalog.categories.
/// </summary>
public class CategoryConfiguration : IEntityTypeConfiguration<Category>
{
    public void Configure(EntityTypeBuilder<Category> builder)
    {
        builder.ToTable("categories", "catalog");

        builder.HasKey(c => c.Id);

        builder.Property(c => c.Id)
            .HasColumnName("id")
            .IsRequired();

        builder.Property(c => c.ParentId)
            .HasColumnName("parent_id");

        builder.Property(c => c.Path)
            .HasColumnName("path");

        builder.Property(c => c.Depth)
            .HasColumnName("depth")
            .IsRequired();

        builder.Property(c => c.Name)
            .HasColumnName("name")
            .HasMaxLength(100)
            .IsRequired();

        builder.Property(c => c.Slug)
            .HasColumnName("slug")
            .HasMaxLength(120)
            .IsRequired();

        builder.Property(c => c.Description)
            .HasColumnName("description");

        builder.Property(c => c.ImageUrl)
            .HasColumnName("image_url");

        // SEO
        builder.Property(c => c.MetaTitle)
            .HasColumnName("meta_title")
            .HasMaxLength(70);

        builder.Property(c => c.MetaDescription)
            .HasColumnName("meta_description")
            .HasMaxLength(160);

        // Controle
        builder.Property(c => c.IsActive)
            .HasColumnName("is_active")
            .IsRequired();

        builder.Property(c => c.SortOrder)
            .HasColumnName("sort_order")
            .HasDefaultValue(0);

        builder.Property(c => c.Version)
            .HasColumnName("version")
            .IsRequired()
            .IsConcurrencyToken();

        // Timestamps
        builder.Property(c => c.CreatedAt)
            .HasColumnName("created_at")
            .IsRequired();

        builder.Property(c => c.UpdatedAt)
            .HasColumnName("updated_at")
            .IsRequired();

        builder.Property(c => c.DeletedAt)
            .HasColumnName("deleted_at");

        // Relacionamento hierárquico
        builder.HasOne(c => c.Parent)
            .WithMany(c => c.Children)
            .HasForeignKey(c => c.ParentId)
            .OnDelete(DeleteBehavior.SetNull);

        // Índices
        builder.HasIndex(c => c.Slug)
            .HasDatabaseName("ix_categories_slug")
            .IsUnique();

        builder.HasIndex(c => c.ParentId)
            .HasDatabaseName("ix_categories_parent_id");

        builder.HasIndex(c => new { c.IsActive, c.DeletedAt })
            .HasDatabaseName("ix_categories_active")
            .HasFilter("deleted_at IS NULL");

        builder.Ignore(c => c.DomainEvents);
    }
}
