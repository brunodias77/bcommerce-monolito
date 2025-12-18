using Bcommerce.Modules.Payments.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Bcommerce.Modules.Payments.Infrastructure.Persistence.Configurations;

public class PaymentConfiguration : IEntityTypeConfiguration<Payment>
{
    public void Configure(EntityTypeBuilder<Payment> builder)
    {
        builder.ToTable("Payments");
        builder.HasKey(p => p.Id);

        builder.OwnsOne(p => p.Amount, amount =>
        {
            amount.Property(a => a.Value).HasColumnName("Amount").HasPrecision(18, 2).IsRequired();
            amount.Property(a => a.Currency).HasColumnName("Currency").HasMaxLength(3).IsRequired();
        });

        builder.OwnsOne(p => p.PixData, pix =>
        {
            pix.Property(x => x.QrCode).HasColumnName("PixQrCode");
            pix.Property(x => x.QrCodeUrl).HasColumnName("PixQrCodeUrl");
            pix.Property(x => x.ExpiresAt).HasColumnName("PixExpiresAt");
        });

        builder.OwnsOne(p => p.BoletoData, boleto =>
        {
            boleto.Property(x => x.BarCode).HasColumnName("BoletoBarCode");
            boleto.Property(x => x.DigitableLine).HasColumnName("BoletoDigitableLine");
            boleto.Property(x => x.Url).HasColumnName("BoletoUrl");
            boleto.Property(x => x.DueDate).HasColumnName("BoletoDueDate");
        });

        builder.Property(p => p.Status).HasConversion<int>().IsRequired();
        builder.Property(p => p.MethodType).HasConversion<int>().IsRequired();

        builder.HasMany(p => p.Transactions)
            .WithOne()
            .HasForeignKey(t => t.PaymentId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
