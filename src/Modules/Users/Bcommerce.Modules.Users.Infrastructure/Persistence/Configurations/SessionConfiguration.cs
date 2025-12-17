using Bcommerce.BuildingBlocks.Infrastructure.Data.Configurations;
using Bcommerce.Modules.Users.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Bcommerce.Modules.Users.Infrastructure.Persistence.Configurations;

public class SessionConfiguration : AggregateRootConfiguration<Session>
{
    public override void Configure(EntityTypeBuilder<Session> builder)
    {
        base.Configure(builder);

        builder.ToTable("sessions");

        builder.Property(s => s.RefreshTokenHash).IsRequired().HasMaxLength(512);
        
        builder.OwnsOne(s => s.DeviceInfo, d =>
        {
            d.Property(x => x.DeviceId).HasColumnName("device_id").HasMaxLength(100);
            d.Property(x => x.DeviceName).HasColumnName("device_name").HasMaxLength(100);
            d.Property(x => x.DeviceType).HasColumnName("device_type").HasMaxLength(20);
            d.Ignore(x => x.OsVersion); // Simplificação, mapear se necessário
            d.Ignore(x => x.Browser);
        });

        builder.HasOne<ApplicationUser>()
            .WithMany()
            .HasForeignKey(s => s.UserId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
