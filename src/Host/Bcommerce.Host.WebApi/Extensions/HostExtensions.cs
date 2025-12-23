using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Serilog;

namespace Bcommerce.Host.WebApi.Extensions;

public static class HostExtensions
{
    public static void AddHostServices(this IHostApplicationBuilder builder)
    {
        // Example: Centralized service registration
        // builder.Services.AddHealthChecks();
    }

    public static void UseHostPipeline(this WebApplication app)
    {
        // Example: Centralized pipeline configuration
        // app.UseHealthChecks("/health");
        
        if (app.Environment.IsDevelopment())
        {
            app.UseDeveloperExceptionPage();
        }
    }
}
