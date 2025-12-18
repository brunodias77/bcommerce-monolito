using Bcommerce.BuildingBlocks.Infrastructure.Data;
using Bcommerce.Modules.Payments.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using System.Reflection;

namespace Bcommerce.Modules.Payments.Infrastructure.Persistence;

public class PaymentsDbContext : BaseDbContext
{
    public DbSet<Payment> Payments => Set<Payment>();
    public DbSet<PaymentMethod> PaymentMethods => Set<PaymentMethod>();
    public DbSet<PaymentTransaction> Transactions => Set<PaymentTransaction>();
    public DbSet<PaymentRefund> Refunds => Set<PaymentRefund>();
    public DbSet<Chargeback> Chargebacks => Set<Chargeback>();

    public PaymentsDbContext(
        DbContextOptions<PaymentsDbContext> options,
        Bcommerce.BuildingBlocks.Infrastructure.Data.Interceptors.AuditableEntityInterceptor auditableEntityInterceptor,
        Bcommerce.BuildingBlocks.Infrastructure.Data.Interceptors.SoftDeleteInterceptor softDeleteInterceptor,
        Bcommerce.BuildingBlocks.Infrastructure.Data.Interceptors.DomainEventInterceptor domainEventInterceptor,
        Bcommerce.BuildingBlocks.Infrastructure.Data.Interceptors.OptimisticLockInterceptor optimisticLockInterceptor)
        : base(options, auditableEntityInterceptor, softDeleteInterceptor, domainEventInterceptor, optimisticLockInterceptor)
    {
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.HasDefaultSchema("payments");
        modelBuilder.ApplyConfigurationsFromAssembly(Assembly.GetExecutingAssembly());
        base.OnModelCreating(modelBuilder);
    }
}
