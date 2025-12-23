using Bcommerce.BuildingBlocks.Infrastructure.Data;
using Bcommerce.Modules.Cart.Domain.Repositories;
using Bcommerce.Modules.Cart.Infrastructure.Persistence;
using Bcommerce.Modules.Cart.Infrastructure.Persistence.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Bcommerce.Modules.Cart.Infrastructure.Extensions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddCartInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("DefaultConnection");

        services.AddDbContext<CartDbContext>((sp, options) =>
        {
            options.UseSqlServer(connectionString);
            options.AddInterceptors(sp.GetServices<Microsoft.EntityFrameworkCore.Diagnostics.IInterceptor>());
        });

        services.AddScoped<ICartRepository, CartRepository>();

        return services;
    }
}
