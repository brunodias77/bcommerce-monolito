using BuildingBlocks.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Payments.Core.Entities;

namespace Payments.Infrastructure.Persistence;

public class PaymentsDbContext : UnitOfWork
{
    public PaymentsDbContext(DbContextOptions<PaymentsDbContext> options) : base(options)
    {
    }

    public DbSet<Payment> Payments { get; set; }
    public DbSet<PaymentTransaction> Transactions { get; set; }
    public DbSet<UserPaymentMethod> UserPaymentMethods { get; set; }
    public DbSet<PaymentRefund> Refunds { get; set; }
    public DbSet<Chargeback> Chargebacks { get; set; }
    public DbSet<PaymentWebhook> Webhooks { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(PaymentsDbContext).Assembly);
        
        // Ensure schemas are created if not using default public
        modelBuilder.HasDefaultSchema("payments");

        base.OnModelCreating(modelBuilder);
    }
}
