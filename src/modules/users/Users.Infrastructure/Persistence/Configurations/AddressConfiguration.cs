using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Users.Core.Entities;

namespace Users.Infrastructure.Persistence.Configurations;

/// <summary>
/// Configuração do EF Core para a entidade Address.
/// Mapeia para a tabela users.addresses.
/// </summary>
public class AddressConfiguration : IEntityTypeConfiguration<Address>
{
    public void Configure(EntityTypeBuilder<Address> builder)
    {
        builder.ToTable("addresses", "users");

        builder.HasKey(a => a.Id);

        builder.Property(a => a.Id)
            .HasColumnName("id")
            .IsRequired();

        builder.Property(a => a.UserId)
            .HasColumnName("user_id")
            .IsRequired();

        builder.Property(a => a.Label)
            .HasColumnName("label")
            .HasMaxLength(50);

        builder.Property(a => a.RecipientName)
            .HasColumnName("recipient_name")
            .HasMaxLength(150);

        builder.Property(a => a.Street)
            .HasColumnName("street")
            .HasMaxLength(255)
            .IsRequired();

        builder.Property(a => a.Number)
            .HasColumnName("number")
            .HasMaxLength(20);

        builder.Property(a => a.Complement)
            .HasColumnName("complement")
            .HasMaxLength(100);

        builder.Property(a => a.Neighborhood)
            .HasColumnName("neighborhood")
            .HasMaxLength(100);

        builder.Property(a => a.City)
            .HasColumnName("city")
            .HasMaxLength(100)
            .IsRequired();

        builder.Property(a => a.State)
            .HasColumnName("state")
            .HasMaxLength(2)
            .IsRequired();

        builder.Property(a => a.PostalCode)
            .HasColumnName("postal_code")
            .HasMaxLength(9)
            .IsRequired()
            .HasConversion(
                postalCode => postalCode.Value,
                value => BuildingBlocks.Domain.Models.PostalCode.Create(value));

        builder.Property(a => a.Country)
            .HasColumnName("country")
            .HasMaxLength(2)
            .HasDefaultValue("BR");

        builder.Property(a => a.Latitude)
            .HasColumnName("latitude")
            .HasColumnType("decimal(10,8)");

        builder.Property(a => a.Longitude)
            .HasColumnName("longitude")
            .HasColumnType("decimal(11,8)");

        builder.Property(a => a.IbgeCode)
            .HasColumnName("ibge_code")
            .HasMaxLength(7);

        builder.Property(a => a.IsDefault)
            .HasColumnName("is_default")
            .HasDefaultValue(false);

        builder.Property(a => a.IsBillingAddress)
            .HasColumnName("is_billing_address")
            .HasDefaultValue(false);

        builder.Property(a => a.CreatedAt)
            .HasColumnName("created_at")
            .IsRequired();

        builder.Property(a => a.UpdatedAt)
            .HasColumnName("updated_at")
            .IsRequired();

        builder.Property(a => a.DeletedAt)
            .HasColumnName("deleted_at");

        // Índices (conforme schema.sql)
        builder.HasIndex(a => a.UserId)
            .HasDatabaseName("idx_addresses_user_id")
            .HasFilter("deleted_at IS NULL");

        builder.HasIndex(a => new { a.UserId, a.IsDefault })
            .HasDatabaseName("uq_addresses_default_per_user")
            .IsUnique()
            .HasFilter("is_default = TRUE AND deleted_at IS NULL");

        // Query Filter para Soft Delete
        builder.HasQueryFilter(a => a.DeletedAt == null);
    }
}
