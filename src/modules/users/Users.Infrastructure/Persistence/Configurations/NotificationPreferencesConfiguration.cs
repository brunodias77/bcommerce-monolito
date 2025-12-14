using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Users.Core.Entities;

namespace Users.Infrastructure.Persistence.Configurations;

/// <summary>
/// Configuração do EF Core para a entidade NotificationPreferences.
/// Mapeia para a tabela users.notification_preferences.
/// </summary>
public class NotificationPreferencesConfiguration : IEntityTypeConfiguration<NotificationPreferences>
{
    public void Configure(EntityTypeBuilder<NotificationPreferences> builder)
    {
        builder.ToTable("notification_preferences", "users");

        builder.HasKey(np => np.Id);

        builder.Property(np => np.Id)
            .HasColumnName("id")
            .IsRequired();

        builder.Property(np => np.UserId)
            .HasColumnName("user_id")
            .IsRequired();

        builder.Property(np => np.EmailEnabled)
            .HasColumnName("email_enabled")
            .HasDefaultValue(true);

        builder.Property(np => np.PushEnabled)
            .HasColumnName("push_enabled")
            .HasDefaultValue(true);

        builder.Property(np => np.SmsEnabled)
            .HasColumnName("sms_enabled")
            .HasDefaultValue(false);

        builder.Property(np => np.OrderUpdates)
            .HasColumnName("order_updates")
            .HasDefaultValue(true);

        builder.Property(np => np.Promotions)
            .HasColumnName("promotions")
            .HasDefaultValue(true);

        builder.Property(np => np.PriceDrops)
            .HasColumnName("price_drops")
            .HasDefaultValue(true);

        builder.Property(np => np.BackInStock)
            .HasColumnName("back_in_stock")
            .HasDefaultValue(true);

        builder.Property(np => np.Newsletter)
            .HasColumnName("newsletter")
            .HasDefaultValue(false);

        builder.Property(np => np.CreatedAt)
            .HasColumnName("created_at")
            .IsRequired();

        builder.Property(np => np.UpdatedAt)
            .HasColumnName("updated_at")
            .IsRequired();
    }
}
