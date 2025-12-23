using Bcommerce.Host.WebApi.Configuration;
using Bcommerce.Host.WebApi.Extensions;
using Serilog;

namespace Bcommerce.Host.WebApi;

public class Startup(IConfiguration configuration)
{
    public IConfiguration Configuration { get; } = configuration;

    public void ConfigureServices(IServiceCollection services)
    {
        // Add services to the container.
        services.AddControllers();
        services.AddEndpointsApiExplorer();
        services.AddSwaggerDocumentation();
        
        // Modules
        services.AddModules(Configuration);
        
        // Database
        services.AddDatabaseConfiguration(Configuration);
        
        // MassTransit
        services.AddMassTransitConfiguration(Configuration); // Assuming this method exists in MassTransitConfiguration
        
        // Background Services
        services.AddHostedService<BackgroundServices.OutboxProcessorService>();
        services.AddHostedService<BackgroundServices.InboxProcessorService>();
        services.AddHostedService<BackgroundServices.StockReservationCleanupService>();
        
        // Host Extensions
        // services.AddHostServices(); // If using IHostApplicationBuilder, otherwise register manually here
    }

    public void Configure(WebApplication app, IWebHostEnvironment env)
    {
        // Configure the HTTP request pipeline.
        if (env.IsDevelopment())
        {
            app.UseSwagger();
            app.UseSwaggerUI();
        }

        app.UseHttpsRedirection();

        app.UseAuthentication();
        app.UseAuthorization();
        
        app.UseHostPipeline();

        app.MapControllers();
    }
}
