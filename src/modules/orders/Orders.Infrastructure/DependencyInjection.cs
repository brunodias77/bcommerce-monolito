using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Orders.Core.Repositories;
using Orders.Infrastructure.Persistence;
using Orders.Infrastructure.Repositories;
using Orders.Infrastructure.Services;

namespace Orders.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddOrdersInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddDbContext<OrdersDbContext>(options =>
            options.UseNpgsql(configuration.GetConnectionString("DefaultConnection")));

        services.AddScoped<IOrderRepository, OrderRepository>();
        services.AddScoped<ShippingService>();

        return services;
    }
}
