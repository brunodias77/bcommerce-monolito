using Bcommerce.Modules.Cart.Domain.Entities;
using Bcommerce.Modules.Cart.Domain.ValueObjects;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Bcommerce.Modules.Cart.Infrastructure.Persistence.Configurations;

public class ShoppingCartConfiguration : IEntityTypeConfiguration<ShoppingCart>
{
    public void Configure(EntityTypeBuilder<ShoppingCart> builder)
    {
        builder.ToTable("ShoppingCarts");

        builder.HasKey(c => c.Id);

        builder.OwnsOne(c => c.SessionId, sessionId =>
        {
            sessionId.Property(x => x.Value).HasColumnName("SessionId");
            sessionId.HasIndex(x => x.Value);
        });
        
        builder.Property(c => c.UserId)
            .IsRequired(false);
            
        builder.HasIndex(c => c.UserId);

        builder.Property(c => c.Status)
            .HasConversion<int>()
            .IsRequired();

        builder.HasMany(c => c.Items)
            .WithOne()
            .HasForeignKey(i => i.CartId)
            .OnDelete(DeleteBehavior.Cascade);
            
        builder.Ignore(c => c.TotalAmount);
    }
}
