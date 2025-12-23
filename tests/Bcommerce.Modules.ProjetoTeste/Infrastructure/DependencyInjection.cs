
using Bcommerce.BuildingBlocks.Infrastructure.Extensions; // ServiceCollectionExtensions
using Bcommerce.Modules.ProjetoTeste.Application.Repositories;
using Bcommerce.Modules.ProjetoTeste.Infrastructure.Data;
using Bcommerce.Modules.ProjetoTeste.Infrastructure.Jobs;
using Bcommerce.Modules.ProjetoTeste.Infrastructure.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Quartz;

namespace Bcommerce.Modules.ProjetoTeste.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddTestProjectInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        // Add DbContext with Interceptors (Auditable, etc are usually added by BuildingBlocks base or extensions)
        // Assuming AddInfrastructureServices or similar from BuildingBlocks handles common stuff? 
        // Or we assume standard AddDbContext
        
        var connectionString = configuration.GetConnectionString("DefaultConnection");

        services.AddDbContext<TestDbContext>((sp, options) =>
        {
             options.UseSqlServer(connectionString);
             options.AddInterceptors(sp.GetServices<Microsoft.EntityFrameworkCore.Diagnostics.IInterceptor>());
        });

        services.AddScoped<ITestItemRepository, TestItemRepository>();

        // Register Quartz Job
        // Using BuildingBlocks extension if available or standard Quartz
        // Assuming we need to register it manually here as simple service or via Quartz config
        
        // Example registration for building blocks background job runner if applicable
        // services.AddTransient<SampleJob>(); 

        return services;
    }
}
