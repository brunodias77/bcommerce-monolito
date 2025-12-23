using Bcommerce.BuildingBlocks.Infrastructure.Data.Configurations;
using Bcommerce.Modules.Users.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Bcommerce.Modules.Users.Infrastructure.Persistence.Configurations;

public class NotificationConfiguration : AggregateRootConfiguration<Notification>
{
    public override void Configure(EntityTypeBuilder<Notification> builder)
    {
        base.Configure(builder);

        builder.ToTable("notifications");

        builder.Property(n => n.Title).IsRequired().HasMaxLength(200);
        builder.Property(n => n.Message).IsRequired();
        builder.Property(n => n.NotificationType).IsRequired().HasMaxLength(50);
        builder.Property(n => n.ReferenceType).HasMaxLength(50);

        builder.HasOne<ApplicationUser>()
            .WithMany()
            .HasForeignKey(n => n.UserId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
