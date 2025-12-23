using Bcommerce.Modules.Coupons.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Bcommerce.Modules.Coupons.Infrastructure.Persistence.Configurations;

public class CouponConfiguration : IEntityTypeConfiguration<Coupon>
{
    public void Configure(EntityTypeBuilder<Coupon> builder)
    {
        builder.ToTable("Coupons");
        builder.HasKey(c => c.Id);

        builder.OwnsOne(c => c.Code, code =>
        {
            code.Property(c => c.Value).HasColumnName("Code").HasMaxLength(20).IsRequired();
            code.HasIndex(c => c.Value).IsUnique();
        });

        builder.Property(c => c.Description).HasMaxLength(200).IsRequired();

        builder.OwnsOne(c => c.Discount, discount =>
        {
            discount.Property(d => d.Amount).HasColumnName("DiscountAmount").HasPrecision(18, 2).IsRequired();
            discount.Property(d => d.Type).HasColumnName("DiscountType").HasConversion<int>().IsRequired();
        });

        builder.OwnsOne(c => c.Validity, validity =>
        {
            validity.Property(v => v.StartsAt).HasColumnName("StartsAt");
            validity.Property(v => v.EndsAt).HasColumnName("EndsAt");
        });

        builder.Property(c => c.Status).HasConversion<int>().IsRequired();

        builder.HasMany(c => c.Usages)
            .WithOne()
            .HasForeignKey(u => u.CouponId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(c => c.Eligibilities)
            .WithOne()
            .HasForeignKey(e => e.CouponId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
