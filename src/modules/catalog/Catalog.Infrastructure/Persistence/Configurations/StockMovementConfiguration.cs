using Catalog.Core.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Catalog.Infrastructure.Persistence.Configurations;

/// <summary>
/// Configuração EF Core para a entidade StockMovement.
/// Corresponde à tabela catalog.stock_movements.
/// </summary>
public class StockMovementConfiguration : IEntityTypeConfiguration<StockMovement>
{
    public void Configure(EntityTypeBuilder<StockMovement> builder)
    {
        builder.ToTable("stock_movements", "catalog");

        builder.HasKey(m => m.Id);

        builder.Property(m => m.Id)
            .HasColumnName("id")
            .IsRequired();

        builder.Property(m => m.ProductId)
            .HasColumnName("product_id")
            .IsRequired();

        builder.Property(m => m.MovementType)
            .HasColumnName("movement_type")
            .HasConversion<string>()
            .HasMaxLength(20)
            .IsRequired();

        builder.Property(m => m.Quantity)
            .HasColumnName("quantity")
            .IsRequired();

        builder.Property(m => m.ReferenceType)
            .HasColumnName("reference_type")
            .HasMaxLength(50);

        builder.Property(m => m.ReferenceId)
            .HasColumnName("reference_id");

        builder.Property(m => m.StockBefore)
            .HasColumnName("stock_before")
            .IsRequired();

        builder.Property(m => m.StockAfter)
            .HasColumnName("stock_after")
            .IsRequired();

        builder.Property(m => m.Reason)
            .HasColumnName("reason");

        builder.Property(m => m.PerformedBy)
            .HasColumnName("performed_by");

        builder.Property(m => m.CreatedAt)
            .HasColumnName("created_at")
            .IsRequired();

        // Índices
        builder.HasIndex(m => m.ProductId)
            .HasDatabaseName("ix_stock_movements_product_id");

        builder.HasIndex(m => m.CreatedAt)
            .HasDatabaseName("ix_stock_movements_created_at");

        builder.HasIndex(m => new { m.ReferenceType, m.ReferenceId })
            .HasDatabaseName("ix_stock_movements_reference");

        builder.Ignore(m => m.DomainEvents);
    }
}
