using Catalog.Core.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Catalog.Infrastructure.Persistence.Configurations;

/// <summary>
/// Configuração EF Core para a entidade StockReservation.
/// Corresponde à tabela catalog.stock_reservations.
/// </summary>
public class StockReservationConfiguration : IEntityTypeConfiguration<StockReservation>
{
    public void Configure(EntityTypeBuilder<StockReservation> builder)
    {
        builder.ToTable("stock_reservations", "catalog");

        builder.HasKey(r => r.Id);

        builder.Property(r => r.Id)
            .HasColumnName("id")
            .IsRequired();

        builder.Property(r => r.ProductId)
            .HasColumnName("product_id")
            .IsRequired();

        builder.Property(r => r.ReferenceType)
            .HasColumnName("reference_type")
            .HasMaxLength(50)
            .IsRequired();

        builder.Property(r => r.ReferenceId)
            .HasColumnName("reference_id")
            .IsRequired();

        builder.Property(r => r.Quantity)
            .HasColumnName("quantity")
            .IsRequired();

        builder.Property(r => r.ExpiresAt)
            .HasColumnName("expires_at")
            .IsRequired();

        builder.Property(r => r.ReleasedAt)
            .HasColumnName("released_at");

        builder.Property(r => r.CreatedAt)
            .HasColumnName("created_at")
            .IsRequired();

        // Índices
        builder.HasIndex(r => r.ProductId)
            .HasDatabaseName("ix_stock_reservations_product_id");

        builder.HasIndex(r => new { r.ProductId, r.ReferenceType, r.ReferenceId })
            .HasDatabaseName("ix_stock_reservations_unique")
            .IsUnique();

        builder.HasIndex(r => r.ExpiresAt)
            .HasDatabaseName("ix_stock_reservations_expires_at")
            .HasFilter("released_at IS NULL");

        builder.Ignore(r => r.DomainEvents);
    }
}
