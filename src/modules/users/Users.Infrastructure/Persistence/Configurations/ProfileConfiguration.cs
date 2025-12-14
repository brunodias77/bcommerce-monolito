using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Users.Core.Entities;

namespace Users.Infrastructure.Persistence.Configurations;

/// <summary>
/// Configuração do EF Core para a entidade Profile.
/// Mapeia para a tabela users.profiles.
/// </summary>
public class ProfileConfiguration : IEntityTypeConfiguration<Profile>
{
    public void Configure(EntityTypeBuilder<Profile> builder)
    {
        builder.ToTable("profiles", "users");

        builder.HasKey(p => p.Id);

        builder.Property(p => p.Id)
            .HasColumnName("id")
            .IsRequired();

        builder.Property(p => p.UserId)
            .HasColumnName("user_id")
            .IsRequired();

        builder.Property(p => p.FirstName)
            .HasColumnName("first_name")
            .HasMaxLength(100);

        builder.Property(p => p.LastName)
            .HasColumnName("last_name")
            .HasMaxLength(100);

        builder.Property(p => p.DisplayName)
            .HasColumnName("display_name")
            .HasMaxLength(100);

        builder.Property(p => p.AvatarUrl)
            .HasColumnName("avatar_url");

        builder.Property(p => p.BirthDate)
            .HasColumnName("birth_date")
            .HasColumnType("date");

        builder.Property(p => p.Gender)
            .HasColumnName("gender")
            .HasMaxLength(20);

        builder.Property(p => p.Cpf)
            .HasColumnName("cpf")
            .HasMaxLength(14)
            .HasConversion(
                cpf => cpf != null ? cpf.Value : null,
                value => value != null ? BuildingBlocks.Domain.Models.Cpf.Create(value) : null);

        builder.Property(p => p.PreferredLanguage)
            .HasColumnName("preferred_language")
            .HasMaxLength(5)
            .HasDefaultValue("pt-BR");

        builder.Property(p => p.PreferredCurrency)
            .HasColumnName("preferred_currency")
            .HasMaxLength(3)
            .HasDefaultValue("BRL");

        builder.Property(p => p.NewsletterSubscribed)
            .HasColumnName("newsletter_subscribed")
            .HasDefaultValue(false);

        builder.Property(p => p.AcceptedTermsAt)
            .HasColumnName("accepted_terms_at");

        builder.Property(p => p.AcceptedPrivacyAt)
            .HasColumnName("accepted_privacy_at");

        builder.Property(p => p.Version)
            .HasColumnName("version")
            .IsRequired()
            .HasDefaultValue(1)
            .IsConcurrencyToken();

        builder.Property(p => p.CreatedAt)
            .HasColumnName("created_at")
            .IsRequired();

        builder.Property(p => p.UpdatedAt)
            .HasColumnName("updated_at")
            .IsRequired();

        builder.Property(p => p.DeletedAt)
            .HasColumnName("deleted_at");

        // Índices (conforme schema.sql)
        builder.HasIndex(p => p.UserId)
            .HasDatabaseName("idx_profiles_user_id")
            .IsUnique()
            .HasFilter("deleted_at IS NULL");

        builder.HasIndex(p => p.Cpf)
            .HasDatabaseName("idx_profiles_cpf")
            .HasFilter("cpf IS NOT NULL AND deleted_at IS NULL");

        // Query Filter para Soft Delete
        builder.HasQueryFilter(p => p.DeletedAt == null);
    }
}
