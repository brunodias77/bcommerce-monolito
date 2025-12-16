using Bcommerce.BuildingBlocks.Infrastructure.Inbox.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Bcommerce.BuildingBlocks.Infrastructure.Inbox.Configuration;

public class InboxConfiguration : IEntityTypeConfiguration<InboxMessage>
{
    public void Configure(EntityTypeBuilder<InboxMessage> builder)
    {
        builder.ToTable("InboxMessages");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Content).IsRequired();
        builder.Property(x => x.Type).IsRequired();
    }
}
