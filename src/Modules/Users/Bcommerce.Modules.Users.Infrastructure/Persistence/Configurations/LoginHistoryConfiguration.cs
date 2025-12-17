using Bcommerce.BuildingBlocks.Infrastructure.Data.Configurations;
using Bcommerce.Modules.Users.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Bcommerce.Modules.Users.Infrastructure.Persistence.Configurations;

public class LoginHistoryConfiguration : EntityConfiguration<LoginHistory>
{
    public override void Configure(EntityTypeBuilder<LoginHistory> builder)
    {
        base.Configure(builder);

        builder.ToTable("login_history");

        builder.Property(l => l.LoginProvider).IsRequired().HasMaxLength(50);
        builder.Property(l => l.IpAddress).HasMaxLength(45);
        builder.Property(l => l.Country).HasMaxLength(2);
        
        builder.OwnsOne(l => l.DeviceInfo, d =>
        {
            d.Property(x => x.DeviceId).HasColumnName("device_id"); // JSON schema has it as JSONB, here mapped as columns or JSON? 
            // Schema has device_info as JSONB, but here we mapped as VO. Let's map to JSONB for Postgres
            d.ToJson(); 
        });

        builder.HasOne<ApplicationUser>()
            .WithMany()
            .HasForeignKey(l => l.UserId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
