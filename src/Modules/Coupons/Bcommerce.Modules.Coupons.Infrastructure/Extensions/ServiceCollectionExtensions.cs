using Bcommerce.BuildingBlocks.Infrastructure.Data;
using Bcommerce.Modules.Coupons.Domain.Repositories;
using Bcommerce.Modules.Coupons.Infrastructure.Persistence;
using Bcommerce.Modules.Coupons.Infrastructure.Persistence.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Bcommerce.Modules.Coupons.Infrastructure.Extensions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddCouponsInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("DefaultConnection");

        services.AddDbContext<CouponsDbContext>((sp, options) =>
        {
            options.UseSqlServer(connectionString);
            options.AddInterceptors(sp.GetServices<Microsoft.EntityFrameworkCore.Diagnostics.IInterceptor>());
        });

        services.AddScoped<ICouponRepository, CouponRepository>();

        return services;
    }
}
