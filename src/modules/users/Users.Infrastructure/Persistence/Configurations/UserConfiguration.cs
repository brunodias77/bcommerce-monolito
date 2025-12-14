using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Users.Core.Entities;

namespace Users.Infrastructure.Persistence.Configurations;

/// <summary>
/// Configuração do EF Core para a entidade User (ASP.NET Identity).
/// Mapeia para a tabela users.asp_net_users.
/// </summary>
public class UserConfiguration : IEntityTypeConfiguration<User>
{
    public void Configure(EntityTypeBuilder<User> builder)
    {
        builder.ToTable("asp_net_users", "users");

        builder.HasKey(u => u.Id);

        builder.Property(u => u.Id)
            .HasColumnName("id")
            .IsRequired();

        builder.Property(u => u.UserName)
            .HasColumnName("user_name")
            .HasMaxLength(256);

        builder.Property(u => u.NormalizedUserName)
            .HasColumnName("normalized_user_name")
            .HasMaxLength(256);

        builder.Property(u => u.Email)
            .HasColumnName("email")
            .HasMaxLength(256);

        builder.Property(u => u.NormalizedEmail)
            .HasColumnName("normalized_email")
            .HasMaxLength(256);

        builder.Property(u => u.EmailConfirmed)
            .HasColumnName("email_confirmed")
            .IsRequired()
            .HasDefaultValue(false);

        builder.Property(u => u.PasswordHash)
            .HasColumnName("password_hash");

        builder.Property(u => u.SecurityStamp)
            .HasColumnName("security_stamp");

        builder.Property(u => u.ConcurrencyStamp)
            .HasColumnName("concurrency_stamp");

        builder.Property(u => u.PhoneNumber)
            .HasColumnName("phone_number")
            .HasMaxLength(50);

        builder.Property(u => u.PhoneNumberConfirmed)
            .HasColumnName("phone_number_confirmed")
            .IsRequired()
            .HasDefaultValue(false);

        builder.Property(u => u.TwoFactorEnabled)
            .HasColumnName("two_factor_enabled")
            .IsRequired()
            .HasDefaultValue(false);

        builder.Property(u => u.LockoutEnd)
            .HasColumnName("lockout_end");

        builder.Property(u => u.LockoutEnabled)
            .HasColumnName("lockout_enabled")
            .IsRequired()
            .HasDefaultValue(true);

        builder.Property(u => u.AccessFailedCount)
            .HasColumnName("access_failed_count")
            .IsRequired()
            .HasDefaultValue(0);

        builder.Property(u => u.CreatedAt)
            .HasColumnName("created_at")
            .IsRequired();

        builder.Property(u => u.UpdatedAt)
            .HasColumnName("updated_at")
            .IsRequired();

        // Relacionamentos
        builder.HasOne(u => u.Profile)
            .WithOne(p => p.User)
            .HasForeignKey<Profile>(p => p.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(u => u.Addresses)
            .WithOne(a => a.User)
            .HasForeignKey(a => a.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(u => u.Sessions)
            .WithOne(s => s.User)
            .HasForeignKey(s => s.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(u => u.Notifications)
            .WithOne(n => n.User)
            .HasForeignKey(n => n.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(u => u.LoginHistories)
            .WithOne(l => l.User)
            .HasForeignKey(l => l.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(u => u.NotificationPreferences)
            .WithOne(np => np.User)
            .HasForeignKey<NotificationPreferences>(np => np.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        // Índices (conforme schema.sql)
        builder.HasIndex(u => u.NormalizedUserName)
            .HasDatabaseName("user_name_index")
            .IsUnique()
            .HasFilter("normalized_user_name IS NOT NULL");

        builder.HasIndex(u => u.NormalizedEmail)
            .HasDatabaseName("email_index");
    }
}
