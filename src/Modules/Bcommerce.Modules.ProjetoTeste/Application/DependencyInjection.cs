
using FluentValidation;
using Microsoft.Extensions.DependencyInjection;
using System.Reflection;

namespace Bcommerce.Modules.ProjetoTeste.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddTestProjectApplication(this IServiceCollection services)
    {
        services.AddValidatorsFromAssembly(Assembly.GetExecutingAssembly());
        
        services.AddMediatR(cfg => {
            cfg.RegisterServicesFromAssembly(Assembly.GetExecutingAssembly());
            // pipeline behaviors are usually registered globally in BuildingBlocks or here if specific
        });

        return services;
    }
}
