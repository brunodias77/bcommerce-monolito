using Bcommerce.BuildingBlocks.Infrastructure.Data.Configurations;
using Bcommerce.Modules.Users.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Bcommerce.Modules.Users.Infrastructure.Persistence.Configurations;

public class ProfileConfiguration : AggregateRootConfiguration<Profile>
{
    public override void Configure(EntityTypeBuilder<Profile> builder)
    {
        base.Configure(builder);

        builder.ToTable("profiles");

        builder.Property(p => p.FirstName).HasMaxLength(100);
        builder.Property(p => p.LastName).HasMaxLength(100);
        builder.Property(p => p.DisplayName).HasMaxLength(100);
        
        builder.OwnsOne(p => p.Cpf, cpf =>
        {
            cpf.Property(c => c.Value).HasColumnName("cpf").HasMaxLength(14);
        });

        builder.Property(p => p.PreferredLanguage).HasMaxLength(5).HasDefaultValue("pt-BR");
        builder.Property(p => p.PreferredCurrency).HasMaxLength(3).HasDefaultValue("BRL");

        builder.HasOne<ApplicationUser>()
            .WithOne()
            .HasForeignKey<Profile>(p => p.UserId)
            .IsRequired()
            .OnDelete(DeleteBehavior.Cascade);
    }
}
