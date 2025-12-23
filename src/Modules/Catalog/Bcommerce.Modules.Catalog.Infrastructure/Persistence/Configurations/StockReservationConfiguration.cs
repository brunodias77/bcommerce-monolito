using Bcommerce.Modules.Catalog.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Bcommerce.Modules.Catalog.Infrastructure.Persistence.Configurations;

public class StockReservationConfiguration : IEntityTypeConfiguration<StockReservation>
{
    public void Configure(EntityTypeBuilder<StockReservation> builder)
    {
        builder.ToTable("StockReservations");

        builder.HasKey(s => s.Id);

        builder.Property(s => s.Quantity)
            .IsRequired();

        builder.Property(s => s.ExpiresAt)
            .IsRequired();

        builder.Property(s => s.ReferenceType)
            .HasMaxLength(50)
            .IsRequired();
            
        builder.Property(s => s.ReferenceId)
            .IsRequired();
            
        builder.HasIndex(s => new { s.ReferenceId, s.ReferenceType });
    }
}
