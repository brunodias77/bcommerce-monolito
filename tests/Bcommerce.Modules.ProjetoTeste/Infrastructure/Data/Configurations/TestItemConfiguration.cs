
using Bcommerce.BuildingBlocks.Infrastructure.Data.Configurations;
using Bcommerce.Modules.ProjetoTeste.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Bcommerce.Modules.ProjetoTeste.Infrastructure.Data.Configurations;

public class TestItemConfiguration : AggregateRootConfiguration<TestItem>
{
    public override void Configure(EntityTypeBuilder<TestItem> builder)
    {
        base.Configure(builder); // Configures Id, DomainEvents ignored, etc.

        builder.Property(x => x.Name).HasMaxLength(100).IsRequired();
        builder.Property(x => x.Description).HasMaxLength(500);
        builder.Property(x => x.Value).HasColumnType("decimal(18,2)");
    }
}
