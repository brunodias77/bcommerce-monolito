using Cart.Core.Entities;
using Cart.Core.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Cart.Infrastructure.Persistence.Configurations;

/// <summary>
/// Configuração EF Core para a entidade Cart.
/// </summary>
public class CartConfiguration : IEntityTypeConfiguration<Core.Entities.Cart>
{
    public void Configure(EntityTypeBuilder<Core.Entities.Cart> builder)
    {
        builder.ToTable("carts");

        builder.HasKey(c => c.Id);

        builder.Property(c => c.Id)
            .HasColumnName("id");

        builder.Property(c => c.UserId)
            .HasColumnName("user_id");

        builder.Property(c => c.SessionId)
            .HasColumnName("session_id")
            .HasMaxLength(100);

        builder.Property(c => c.CouponId)
            .HasColumnName("coupon_id");

        builder.Property(c => c.CouponCode)
            .HasColumnName("coupon_code")
            .HasMaxLength(50);

        builder.Property(c => c.DiscountAmount)
            .HasColumnName("discount_amount")
            .HasPrecision(10, 2)
            .HasDefaultValue(0);

        builder.Property(c => c.Status)
            .HasColumnName("status")
            .HasColumnType("shared.cart_status")
            .HasConversion(
                v => v.ToString().ToLower(),
                v => Enum.Parse<CartStatus>(v, true))
            .IsRequired();

        builder.Property(c => c.IpAddress)
            .HasColumnName("ip_address")
            .HasMaxLength(45);

        builder.Property(c => c.UserAgent)
            .HasColumnName("user_agent");

        builder.Property(c => c.Version)
            .HasColumnName("version")
            .HasDefaultValue(1)
            .IsConcurrencyToken();

        builder.Property(c => c.ExpiresAt)
            .HasColumnName("expires_at");

        builder.Property(c => c.ConvertedAt)
            .HasColumnName("converted_at");

        builder.Property(c => c.CreatedAt)
            .HasColumnName("created_at")
            .IsRequired();

        builder.Property(c => c.UpdatedAt)
            .HasColumnName("updated_at")
            .IsRequired();

        // Relacionamentos
        builder.HasMany(c => c.Items)
            .WithOne()
            .HasForeignKey(i => i.CartId)
            .OnDelete(DeleteBehavior.Cascade);

        // Índices
        builder.HasIndex(c => c.UserId);
        builder.HasIndex(c => c.SessionId);
        builder.HasIndex(c => c.Status);
    }
}
