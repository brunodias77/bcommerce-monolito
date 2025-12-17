using Bcommerce.BuildingBlocks.Infrastructure.Data;
using Bcommerce.Modules.Catalog.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using System.Reflection;

namespace Bcommerce.Modules.Catalog.Infrastructure.Persistence;

public class CatalogDbContext : BaseDbContext
{
    public DbSet<Product> Products => Set<Product>();
    public DbSet<Category> Categories => Set<Category>();
    public DbSet<Brand> Brands => Set<Brand>();
    public DbSet<StockReservation> StockReservations => Set<StockReservation>();
    public DbSet<ProductReview> ProductReviews => Set<ProductReview>();
    public DbSet<ProductImage> ProductImages => Set<ProductImage>();

    public CatalogDbContext(
        DbContextOptions<CatalogDbContext> options,
        Bcommerce.BuildingBlocks.Infrastructure.Data.Interceptors.AuditableEntityInterceptor auditableEntityInterceptor,
        Bcommerce.BuildingBlocks.Infrastructure.Data.Interceptors.SoftDeleteInterceptor softDeleteInterceptor,
        Bcommerce.BuildingBlocks.Infrastructure.Data.Interceptors.DomainEventInterceptor domainEventInterceptor,
        Bcommerce.BuildingBlocks.Infrastructure.Data.Interceptors.OptimisticLockInterceptor optimisticLockInterceptor)
        : base(options, auditableEntityInterceptor, softDeleteInterceptor, domainEventInterceptor, optimisticLockInterceptor)
    {
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.HasDefaultSchema("catalog");
        modelBuilder.ApplyConfigurationsFromAssembly(Assembly.GetExecutingAssembly());
        base.OnModelCreating(modelBuilder);
    }
}
