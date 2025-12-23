using Bcommerce.BuildingBlocks.Infrastructure.Data.Configurations;
using Bcommerce.Modules.Users.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Bcommerce.Modules.Users.Infrastructure.Persistence.Configurations;

public class ApplicationUserConfiguration : IEntityTypeConfiguration<ApplicationUser>
{
    public void Configure(EntityTypeBuilder<ApplicationUser> builder)
    {
        // IdentityUser confs are handled by IdentityDbContext, here we configure custom props
        builder.Ignore(u => u.DomainEvents);
        
        builder.Property(u => u.CreatedAt).HasDefaultValueSql("NOW()");
        builder.Property(u => u.UpdatedAt).HasDefaultValueSql("NOW()");
    }
}
