using Catalog.Core.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Catalog.Infrastructure.Persistence.Configurations;

/// <summary>
/// Configuração EF Core para a entidade UserFavorite.
/// Corresponde à tabela catalog.user_favorites.
/// </summary>
public class UserFavoriteConfiguration : IEntityTypeConfiguration<UserFavorite>
{
    public void Configure(EntityTypeBuilder<UserFavorite> builder)
    {
        builder.ToTable("user_favorites", "catalog");

        builder.HasKey(f => f.Id);

        builder.Property(f => f.Id)
            .HasColumnName("id")
            .IsRequired();

        builder.Property(f => f.UserId)
            .HasColumnName("user_id")
            .IsRequired();

        builder.Property(f => f.ProductId)
            .HasColumnName("product_id")
            .IsRequired();

        builder.Property(f => f.ProductSnapshot)
            .HasColumnName("product_snapshot")
            .HasColumnType("jsonb");

        builder.Property(f => f.CreatedAt)
            .HasColumnName("created_at")
            .IsRequired();

        // Relacionamento
        builder.HasOne(f => f.Product)
            .WithMany()
            .HasForeignKey(f => f.ProductId)
            .OnDelete(DeleteBehavior.Cascade);

        // Índices
        builder.HasIndex(f => f.UserId)
            .HasDatabaseName("ix_user_favorites_user_id");

        builder.HasIndex(f => f.ProductId)
            .HasDatabaseName("ix_user_favorites_product_id");

        builder.HasIndex(f => new { f.UserId, f.ProductId })
            .HasDatabaseName("ix_user_favorites_unique")
            .IsUnique();

        builder.Ignore(f => f.DomainEvents);
    }
}
