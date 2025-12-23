using Bcommerce.BuildingBlocks.Infrastructure.Data.Configurations;
using Bcommerce.Modules.Users.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Bcommerce.Modules.Users.Infrastructure.Persistence.Configurations;

public class AddressConfiguration : AggregateRootConfiguration<Address>
{
    public override void Configure(EntityTypeBuilder<Address> builder)
    {
        base.Configure(builder);

        builder.ToTable("addresses");

        builder.Property(a => a.Street).IsRequired().HasMaxLength(255);
        builder.Property(a => a.City).IsRequired().HasMaxLength(100);
        builder.Property(a => a.State).IsRequired().HasMaxLength(2);
        builder.Property(a => a.Country).IsRequired().HasMaxLength(2).HasDefaultValue("BR");
        
        builder.OwnsOne(a => a.PostalCode, pc =>
        {
            pc.Property(c => c.Value).HasColumnName("postal_code").HasMaxLength(9).IsRequired();
            pc.HasIndex(c => c.Value).IsUnique(false); // Schema check only
        });
        
        builder.OwnsOne(a => a.Location, l =>
        {
            l.Property(c => c.Latitude).HasColumnName("latitude").HasColumnType("decimal(10,8)");
            l.Property(c => c.Longitude).HasColumnName("longitude").HasColumnType("decimal(11,8)");
        });

        builder.HasOne<ApplicationUser>() // No navigation property in User
            .WithMany()
            .HasForeignKey(a => a.UserId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
