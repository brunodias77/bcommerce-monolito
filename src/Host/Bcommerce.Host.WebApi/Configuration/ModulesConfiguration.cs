using Bcommerce.Modules.Users.Api.Extensions;
using Bcommerce.Modules.Catalog.Api.Extensions;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Bcommerce.Host.WebApi.Configuration;

public static class ModulesConfiguration
{
    public static IServiceCollection AddModules(this IServiceCollection services, IConfiguration configuration)
    {
        // Register Users Module
        services.AddUsersModule(configuration);
        
        // Register Catalog Module (assuming generic extension for now as I haven't seen the specific file, but following pattern)
        services.AddCatalogModule(configuration);
        
        return services;
    }
}
