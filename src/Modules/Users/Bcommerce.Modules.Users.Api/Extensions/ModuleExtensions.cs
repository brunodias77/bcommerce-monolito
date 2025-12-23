using Bcommerce.Modules.Users.Infrastructure.Extensions;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Bcommerce.Modules.Users.Api.Extensions;

public static class ModuleExtensions
{
    public static IServiceCollection AddUsersModule(this IServiceCollection services, IConfiguration configuration)
    {
        // Register Infrastructure (Persistence, Identity, Services, etc.)
        // Assuming Infrastructure has an AddInfrastructure method or similar extensions
        services.AddUsersInfrastructure(configuration);

        // Register Application services (MediatR is usually registered centrally, but module specific services go here)
        // services.AddUsersApplication(); 

        return services;
    }
}
