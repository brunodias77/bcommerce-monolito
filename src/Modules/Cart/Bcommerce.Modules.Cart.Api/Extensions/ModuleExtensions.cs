using Bcommerce.Modules.Cart.Infrastructure.Extensions;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Bcommerce.Modules.Cart.Api.Extensions;

public static class ModuleExtensions
{
    public static IServiceCollection AddCartModule(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddCartInfrastructure(configuration);
        
        return services;
    }
}
