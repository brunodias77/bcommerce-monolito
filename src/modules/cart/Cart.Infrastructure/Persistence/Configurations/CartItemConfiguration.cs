using Cart.Core.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Cart.Infrastructure.Persistence.Configurations;

/// <summary>
/// Configuração EF Core para a entidade CartItem.
/// </summary>
public class CartItemConfiguration : IEntityTypeConfiguration<CartItem>
{
    public void Configure(EntityTypeBuilder<CartItem> builder)
    {
        builder.ToTable("items");

        builder.HasKey(i => i.Id);

        builder.Property(i => i.Id)
            .HasColumnName("id");

        builder.Property(i => i.CartId)
            .HasColumnName("cart_id")
            .IsRequired();

        builder.Property(i => i.ProductId)
            .HasColumnName("product_id")
            .IsRequired();

        builder.Property(i => i.ProductSnapshot)
            .HasColumnName("product_snapshot")
            .HasColumnType("jsonb")
            .IsRequired();

        builder.Property(i => i.Quantity)
            .HasColumnName("quantity")
            .IsRequired();

        builder.Property(i => i.UnitPrice)
            .HasColumnName("unit_price")
            .HasPrecision(10, 2)
            .IsRequired();

        builder.Property(i => i.CurrentPrice)
            .HasColumnName("current_price")
            .HasPrecision(10, 2);

        builder.Property(i => i.PriceChangedAt)
            .HasColumnName("price_changed_at");

        builder.Property(i => i.StockReserved)
            .HasColumnName("stock_reserved")
            .HasDefaultValue(false);

        builder.Property(i => i.StockReservationId)
            .HasColumnName("stock_reservation_id");

        builder.Property(i => i.AddedAt)
            .HasColumnName("added_at")
            .IsRequired();

        builder.Property(i => i.UpdatedAt)
            .HasColumnName("updated_at")
            .IsRequired();

        builder.Property(i => i.RemovedAt)
            .HasColumnName("removed_at");

        // Índices
        builder.HasIndex(i => new { i.CartId, i.ProductId })
            .IsUnique();

        builder.HasIndex(i => i.ProductId);
    }
}
