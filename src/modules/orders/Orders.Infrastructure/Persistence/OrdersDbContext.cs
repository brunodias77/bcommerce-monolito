using BuildingBlocks.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Orders.Core.Entities;

namespace Orders.Infrastructure.Persistence;

public class OrdersDbContext : UnitOfWork
{
    public OrdersDbContext(DbContextOptions<OrdersDbContext> options) : base(options)
    {
    }

    public DbSet<Order> Orders { get; set; }
    public DbSet<OrderItem> OrderItems { get; set; }
    public DbSet<OrderStatusHistory> OrderStatusHistories { get; set; }
    public DbSet<TrackingEvent> TrackingEvents { get; set; }
    public DbSet<Invoice> Invoices { get; set; }
    public DbSet<OrderRefund> Refunds { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(OrdersDbContext).Assembly);
        
        // Ensure schemas are created if not using default public
        modelBuilder.HasDefaultSchema("orders");

        base.OnModelCreating(modelBuilder);
    }
}
