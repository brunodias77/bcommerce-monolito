using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Users.Core.Entities;

namespace Users.Infrastructure.Persistence.Configurations;

/// <summary>
/// Configuração do EF Core para a entidade Session.
/// Mapeia para a tabela users.sessions.
/// </summary>
public class SessionConfiguration : IEntityTypeConfiguration<Session>
{
    public void Configure(EntityTypeBuilder<Session> builder)
    {
        builder.ToTable("sessions", "users");

        builder.HasKey(s => s.Id);

        builder.Property(s => s.Id)
            .HasColumnName("id")
            .IsRequired();

        builder.Property(s => s.UserId)
            .HasColumnName("user_id")
            .IsRequired();

        builder.Property(s => s.RefreshTokenHash)
            .HasColumnName("refresh_token_hash")
            .HasMaxLength(512)
            .IsRequired();

        builder.Property(s => s.DeviceId)
            .HasColumnName("device_id")
            .HasMaxLength(100);

        builder.Property(s => s.DeviceName)
            .HasColumnName("device_name")
            .HasMaxLength(100);

        builder.Property(s => s.DeviceType)
            .HasColumnName("device_type")
            .HasMaxLength(20);

        builder.Property(s => s.IpAddress)
            .HasColumnName("ip_address")
            .HasMaxLength(45);

        builder.Property(s => s.Country)
            .HasColumnName("country")
            .HasMaxLength(2);

        builder.Property(s => s.City)
            .HasColumnName("city")
            .HasMaxLength(100);

        builder.Property(s => s.IsCurrent)
            .HasColumnName("is_current")
            .HasDefaultValue(false);

        builder.Property(s => s.ExpiresAt)
            .HasColumnName("expires_at")
            .IsRequired();

        builder.Property(s => s.RevokedAt)
            .HasColumnName("revoked_at");

        builder.Property(s => s.RevokedReason)
            .HasColumnName("revoked_reason")
            .HasMaxLength(100);

        builder.Property(s => s.CreatedAt)
            .HasColumnName("created_at")
            .IsRequired();

        builder.Property(s => s.LastActivityAt)
            .HasColumnName("last_activity_at")
            .IsRequired();

        // Índices (conforme schema.sql)
        builder.HasIndex(s => s.UserId)
            .HasDatabaseName("idx_sessions_user_id")
            .HasFilter("revoked_at IS NULL");

        builder.HasIndex(s => s.ExpiresAt)
            .HasDatabaseName("idx_sessions_expires")
            .HasFilter("revoked_at IS NULL");

        builder.HasIndex(s => s.RefreshTokenHash)
            .HasDatabaseName("uq_sessions_refresh_token")
            .IsUnique();
    }
}
