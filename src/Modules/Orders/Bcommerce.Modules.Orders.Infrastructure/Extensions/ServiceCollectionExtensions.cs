using Bcommerce.BuildingBlocks.Infrastructure.Data;
using Bcommerce.Modules.Orders.Domain.Repositories;
using Bcommerce.Modules.Orders.Infrastructure.Persistence;
using Bcommerce.Modules.Orders.Infrastructure.Persistence.Repositories;
using Bcommerce.Modules.Orders.Infrastructure.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Bcommerce.Modules.Orders.Infrastructure.Extensions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddOrdersInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("DefaultConnection");

        services.AddDbContext<OrdersDbContext>((sp, options) =>
        {
            options.UseSqlServer(connectionString);
            options.AddInterceptors(sp.GetServices<Microsoft.EntityFrameworkCore.Diagnostics.IInterceptor>());
        });

        services.AddScoped<IOrderRepository, OrderRepository>();
        
        // Registering services as Scoped or Transient as appropriate
        services.AddScoped<ShippingService>();
        services.AddScoped<InvoiceService>();

        return services;
    }
}
