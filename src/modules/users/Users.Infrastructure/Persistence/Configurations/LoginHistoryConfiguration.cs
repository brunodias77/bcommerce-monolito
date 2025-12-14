using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Users.Core.Entities;

namespace Users.Infrastructure.Persistence.Configurations;

/// <summary>
/// Configuração do EF Core para a entidade LoginHistory.
/// Mapeia para a tabela users.login_history.
/// </summary>
public class LoginHistoryConfiguration : IEntityTypeConfiguration<LoginHistory>
{
    public void Configure(EntityTypeBuilder<LoginHistory> builder)
    {
        builder.ToTable("login_history", "users");

        builder.HasKey(lh => lh.Id);

        builder.Property(lh => lh.Id)
            .HasColumnName("id")
            .IsRequired();

        builder.Property(lh => lh.UserId)
            .HasColumnName("user_id")
            .IsRequired();

        builder.Property(lh => lh.LoginProvider)
            .HasColumnName("login_provider")
            .HasMaxLength(50)
            .HasDefaultValue("Local");

        builder.Property(lh => lh.IpAddress)
            .HasColumnName("ip_address")
            .HasMaxLength(45);

        builder.Property(lh => lh.UserAgent)
            .HasColumnName("user_agent");

        builder.Property(lh => lh.Country)
            .HasColumnName("country")
            .HasMaxLength(2);

        builder.Property(lh => lh.City)
            .HasColumnName("city")
            .HasMaxLength(100);

        builder.Property(lh => lh.DeviceType)
            .HasColumnName("device_type")
            .HasMaxLength(20);

        builder.Property(lh => lh.DeviceInfo)
            .HasColumnName("device_info")
            .HasColumnType("jsonb");

        builder.Property(lh => lh.Success)
            .HasColumnName("success")
            .HasDefaultValue(true);

        builder.Property(lh => lh.FailureReason)
            .HasColumnName("failure_reason")
            .HasMaxLength(100);

        builder.Property(lh => lh.CreatedAt)
            .HasColumnName("created_at")
            .IsRequired();

        // Índices (conforme schema.sql)
        builder.HasIndex(lh => lh.UserId)
            .HasDatabaseName("idx_login_history_user_id");

        builder.HasIndex(lh => lh.CreatedAt)
            .HasDatabaseName("idx_login_history_created_at")
            .IsDescending();
    }
}
