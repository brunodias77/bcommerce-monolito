using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Bcommerce.Host.WebApi.Configuration;

public static class DatabaseConfiguration
{
    public static IServiceCollection AddDatabaseConfiguration(this IServiceCollection services, IConfiguration configuration)
    {
        // Database configuration logic here (e.g., Global connection string setup if needed, 
        // though typically handled by modules individually).
        // This might be where we apply migrations or ensure DB creation.
        
        return services;
    }
}
