using Bcommerce.BuildingBlocks.Infrastructure.Data;
using Bcommerce.Modules.Coupons.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using System.Reflection;

namespace Bcommerce.Modules.Coupons.Infrastructure.Persistence;

public class CouponsDbContext : BaseDbContext
{
    public DbSet<Coupon> Coupons => Set<Coupon>();
    public DbSet<CouponUsage> CouponUsages => Set<CouponUsage>();
    public DbSet<CouponReservation> CouponReservations => Set<CouponReservation>();
    public DbSet<CouponEligibility> CouponEligibilities => Set<CouponEligibility>();

    public CouponsDbContext(
        DbContextOptions<CouponsDbContext> options,
        Bcommerce.BuildingBlocks.Infrastructure.Data.Interceptors.AuditableEntityInterceptor auditableEntityInterceptor,
        Bcommerce.BuildingBlocks.Infrastructure.Data.Interceptors.SoftDeleteInterceptor softDeleteInterceptor,
        Bcommerce.BuildingBlocks.Infrastructure.Data.Interceptors.DomainEventInterceptor domainEventInterceptor,
        Bcommerce.BuildingBlocks.Infrastructure.Data.Interceptors.OptimisticLockInterceptor optimisticLockInterceptor)
        : base(options, auditableEntityInterceptor, softDeleteInterceptor, domainEventInterceptor, optimisticLockInterceptor)
    {
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.HasDefaultSchema("coupons");
        modelBuilder.ApplyConfigurationsFromAssembly(Assembly.GetExecutingAssembly());
        base.OnModelCreating(modelBuilder);
    }
}
