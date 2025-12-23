using Bcommerce.BuildingBlocks.Infrastructure.Data;
using Bcommerce.Modules.Cart.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using System.Reflection;

namespace Bcommerce.Modules.Cart.Infrastructure.Persistence;

public class CartDbContext : BaseDbContext
{
    public DbSet<ShoppingCart> ShoppingCarts => Set<ShoppingCart>();
    public DbSet<CartItem> CartItems => Set<CartItem>();
    public DbSet<SavedCart> SavedCarts => Set<SavedCart>();

    public CartDbContext(
        DbContextOptions<CartDbContext> options,
        Bcommerce.BuildingBlocks.Infrastructure.Data.Interceptors.AuditableEntityInterceptor auditableEntityInterceptor,
        Bcommerce.BuildingBlocks.Infrastructure.Data.Interceptors.SoftDeleteInterceptor softDeleteInterceptor,
        Bcommerce.BuildingBlocks.Infrastructure.Data.Interceptors.DomainEventInterceptor domainEventInterceptor,
        Bcommerce.BuildingBlocks.Infrastructure.Data.Interceptors.OptimisticLockInterceptor optimisticLockInterceptor)
        : base(options, auditableEntityInterceptor, softDeleteInterceptor, domainEventInterceptor, optimisticLockInterceptor)
    {
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.HasDefaultSchema("cart");
        modelBuilder.ApplyConfigurationsFromAssembly(Assembly.GetExecutingAssembly());
        base.OnModelCreating(modelBuilder);
    }
}
