using Bcommerce.Modules.Payments.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Bcommerce.Modules.Payments.Infrastructure.Persistence.Configurations;

public class PaymentMethodConfiguration : IEntityTypeConfiguration<PaymentMethod>
{
    public void Configure(EntityTypeBuilder<PaymentMethod> builder)
    {
        builder.ToTable("PaymentMethods");
        builder.HasKey(pm => pm.Id);

        builder.Property(pm => pm.Alias).HasMaxLength(100).IsRequired();
        builder.Property(pm => pm.Type).HasConversion<int>().IsRequired();
        builder.Property(pm => pm.CardBrand).HasConversion<int>();
        builder.Property(pm => pm.LastFourDigits).HasMaxLength(4);
        builder.Property(pm => pm.Token).HasMaxLength(255);
    }
}
