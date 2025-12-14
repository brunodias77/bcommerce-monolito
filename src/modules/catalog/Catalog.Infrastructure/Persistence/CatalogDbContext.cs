using BuildingBlocks.Domain.Repositories;
using BuildingBlocks.Infrastructure.Persistence;
using Catalog.Core.Entities;
using Microsoft.EntityFrameworkCore;
using System.Reflection;

namespace Catalog.Infrastructure.Persistence;

/// <summary>
/// DbContext para o módulo de catálogo.
/// Implementa IUnitOfWork para suporte ao padrão Unit of Work.
/// </summary>
/// <remarks>
/// Corresponde ao schema 'catalog' no banco de dados PostgreSQL.
/// Tabelas:
/// - catalog.categories
/// - catalog.brands
/// - catalog.products
/// - catalog.product_images
/// - catalog.stock_movements
/// - catalog.stock_reservations
/// - catalog.product_reviews
/// - catalog.user_favorites
/// </remarks>
public class CatalogDbContext : DbContext, IUnitOfWork
{
    public CatalogDbContext(DbContextOptions<CatalogDbContext> options)
        : base(options)
    {
    }

    // ========================================
    // DBSETS
    // ========================================

    public DbSet<Product> Products => Set<Product>();
    public DbSet<Category> Categories => Set<Category>();
    public DbSet<Brand> Brands => Set<Brand>();
    public DbSet<ProductImage> ProductImages => Set<ProductImage>();
    public DbSet<StockMovement> StockMovements => Set<StockMovement>();
    public DbSet<StockReservation> StockReservations => Set<StockReservation>();
    public DbSet<ProductReview> ProductReviews => Set<ProductReview>();
    public DbSet<UserFavorite> UserFavorites => Set<UserFavorite>();

    // ========================================
    // CONFIGURAÇÃO DO MODELO
    // ========================================

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Define o schema padrão como 'catalog'
        modelBuilder.HasDefaultSchema("catalog");

        // Aplica todas as configurações de entidades do assembly
        modelBuilder.ApplyConfigurationsFromAssembly(Assembly.GetExecutingAssembly());
    }

    // ========================================
    // IMPLEMENTAÇÃO DE IUnitOfWork
    // ========================================

    async Task<int> IUnitOfWork.SaveChangesAsync(CancellationToken cancellationToken)
    {
        return await base.SaveChangesAsync(cancellationToken);
    }

    async Task<bool> IUnitOfWork.SaveEntitiesAsync(CancellationToken cancellationToken)
    {
        return await this.SaveEntitiesAsync(cancellationToken);
    }
}
