using Bcommerce.BuildingBlocks.Infrastructure.Data;
using Bcommerce.Modules.Orders.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using System.Reflection;

namespace Bcommerce.Modules.Orders.Infrastructure.Persistence;

public class OrdersDbContext : BaseDbContext
{
    public DbSet<Order> Orders => Set<Order>();
    public DbSet<OrderItem> OrderItems => Set<OrderItem>();
    public DbSet<OrderStatusHistory> OrderStatusHistory => Set<OrderStatusHistory>();
    public DbSet<Invoice> Invoices => Set<Invoice>();
    public DbSet<OrderRefund> OrderRefunds => Set<OrderRefund>();

    public OrdersDbContext(
        DbContextOptions<OrdersDbContext> options,
        Bcommerce.BuildingBlocks.Infrastructure.Data.Interceptors.AuditableEntityInterceptor auditableEntityInterceptor,
        Bcommerce.BuildingBlocks.Infrastructure.Data.Interceptors.SoftDeleteInterceptor softDeleteInterceptor,
        Bcommerce.BuildingBlocks.Infrastructure.Data.Interceptors.DomainEventInterceptor domainEventInterceptor,
        Bcommerce.BuildingBlocks.Infrastructure.Data.Interceptors.OptimisticLockInterceptor optimisticLockInterceptor)
        : base(options, auditableEntityInterceptor, softDeleteInterceptor, domainEventInterceptor, optimisticLockInterceptor)
    {
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.HasDefaultSchema("orders");
        modelBuilder.ApplyConfigurationsFromAssembly(Assembly.GetExecutingAssembly());
        base.OnModelCreating(modelBuilder);
    }
}
