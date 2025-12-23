using Bcommerce.BuildingBlocks.Infrastructure.Data;
using Bcommerce.Modules.Catalog.Domain.Repositories;
using Bcommerce.Modules.Catalog.Domain.Services;
using Bcommerce.Modules.Catalog.Infrastructure.Persistence;
using Bcommerce.Modules.Catalog.Infrastructure.Persistence.Repositories;
using Bcommerce.Modules.Catalog.Infrastructure.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Bcommerce.Modules.Catalog.Infrastructure.Extensions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddCatalogInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("DefaultConnection");

        services.AddDbContext<CatalogDbContext>((sp, options) =>
        {
            options.UseSqlServer(connectionString);
            options.AddInterceptors(sp.GetServices<Microsoft.EntityFrameworkCore.Diagnostics.IInterceptor>());
        });

        services.AddScoped<IProductRepository, ProductRepository>();
        services.AddScoped<ICategoryRepository, CategoryRepository>();
        services.AddScoped<IBrandRepository, BrandRepository>();
        services.AddScoped<IStockReservationRepository, StockReservationRepository>();

        services.AddScoped<ISlugGenerator, SlugGenerator>();
        services.AddScoped<ImageStorageService>();

        return services;
    }
}
