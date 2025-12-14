using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Users.Core.Entities;

namespace Users.Infrastructure.Persistence.Configurations;

/// <summary>
/// Configuração do EF Core para a entidade Notification.
/// Mapeia para a tabela users.notifications.
/// </summary>
public class NotificationConfiguration : IEntityTypeConfiguration<Notification>
{
    public void Configure(EntityTypeBuilder<Notification> builder)
    {
        builder.ToTable("notifications", "users");

        builder.HasKey(n => n.Id);

        builder.Property(n => n.Id)
            .HasColumnName("id")
            .IsRequired();

        builder.Property(n => n.UserId)
            .HasColumnName("user_id")
            .IsRequired();

        builder.Property(n => n.Title)
            .HasColumnName("title")
            .HasMaxLength(200)
            .IsRequired();

        builder.Property(n => n.Message)
            .HasColumnName("message")
            .IsRequired();

        builder.Property(n => n.NotificationType)
            .HasColumnName("notification_type")
            .HasMaxLength(50)
            .IsRequired();

        builder.Property(n => n.ReferenceType)
            .HasColumnName("reference_type")
            .HasMaxLength(50);

        builder.Property(n => n.ReferenceId)
            .HasColumnName("reference_id");

        builder.Property(n => n.ActionUrl)
            .HasColumnName("action_url");

        builder.Property(n => n.ReadAt)
            .HasColumnName("read_at");

        builder.Property(n => n.CreatedAt)
            .HasColumnName("created_at")
            .IsRequired();

        // Índices (conforme schema.sql)
        builder.HasIndex(n => n.UserId)
            .HasDatabaseName("idx_notifications_user_id");

        builder.HasIndex(n => new { n.UserId, n.CreatedAt })
            .HasDatabaseName("idx_notifications_unread")
            .HasFilter("read_at IS NULL");
    }
}
