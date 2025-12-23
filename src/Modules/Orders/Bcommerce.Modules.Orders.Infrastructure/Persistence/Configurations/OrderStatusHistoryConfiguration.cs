using Bcommerce.Modules.Orders.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Bcommerce.Modules.Orders.Infrastructure.Persistence.Configurations;

public class OrderStatusHistoryConfiguration : IEntityTypeConfiguration<OrderStatusHistory>
{
    public void Configure(EntityTypeBuilder<OrderStatusHistory> builder)
    {
        builder.ToTable("OrderStatusHistory");
        builder.HasKey(h => h.Id);

        builder.Property(h => h.Status).HasConversion<int>().IsRequired();
        builder.Property(h => h.Reason).HasMaxLength(500);
    }
}
