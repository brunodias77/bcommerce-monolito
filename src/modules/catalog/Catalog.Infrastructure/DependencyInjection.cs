using BuildingBlocks.Domain.Repositories;
using Catalog.Core.Repositories;
using Catalog.Infrastructure.Persistence;
using Catalog.Infrastructure.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Catalog.Infrastructure;

/// <summary>
/// Extensão para registrar os serviços do módulo Catalog.
/// </summary>
public static class DependencyInjection
{
    /// <summary>
    /// Adiciona os serviços de infraestrutura do módulo Catalog.
    /// </summary>
    public static IServiceCollection AddCatalogModule(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        // ========================================
        // DbContext
        // ========================================
        services.AddDbContext<CatalogDbContext>(options =>
            options.UseNpgsql(
                configuration.GetConnectionString("DefaultConnection"),
                builder => builder.MigrationsHistoryTable("__EFMigrationsHistory", "catalog")));

        // ========================================
        // UnitOfWork
        // ========================================
        services.AddScoped<IUnitOfWork>(provider =>
            provider.GetRequiredService<CatalogDbContext>());

        // ========================================
        // Repositories
        // ========================================
        services.AddScoped<IProductRepository, ProductRepository>();
        services.AddScoped<ICategoryRepository, CategoryRepository>();
        services.AddScoped<IBrandRepository, BrandRepository>();
        services.AddScoped<IProductReviewRepository, ProductReviewRepository>();

        return services;
    }
}
