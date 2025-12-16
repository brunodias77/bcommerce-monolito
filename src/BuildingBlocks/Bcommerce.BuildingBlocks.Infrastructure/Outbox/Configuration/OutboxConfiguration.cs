using Bcommerce.BuildingBlocks.Infrastructure.Outbox.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Bcommerce.BuildingBlocks.Infrastructure.Outbox.Configuration;

public class OutboxConfiguration : IEntityTypeConfiguration<OutboxMessage>
{
    public void Configure(EntityTypeBuilder<OutboxMessage> builder)
    {
        builder.ToTable("OutboxMessages");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Content).IsRequired();
        builder.Property(x => x.Type).IsRequired();
    }
}
