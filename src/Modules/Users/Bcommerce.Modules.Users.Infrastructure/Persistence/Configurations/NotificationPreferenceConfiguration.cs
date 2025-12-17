using Bcommerce.BuildingBlocks.Infrastructure.Data.Configurations;
using Bcommerce.Modules.Users.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Bcommerce.Modules.Users.Infrastructure.Persistence.Configurations;

public class NotificationPreferenceConfiguration : AggregateRootConfiguration<NotificationPreference>
{
    public override void Configure(EntityTypeBuilder<NotificationPreference> builder)
    {
        base.Configure(builder);

        builder.ToTable("notification_preferences");

        builder.HasOne<ApplicationUser>()
            .WithOne()
            .HasForeignKey<NotificationPreference>(np => np.UserId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
