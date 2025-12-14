using BuildingBlocks.Presentation.Filters;
using Microsoft.OpenApi.Models;

namespace Bcommerce.Api.Configurations;

/// <summary>
/// Configuração de Dependency Injection para Controllers e Swagger.
/// </summary>
public static class WebApiDependencyInjection
{
    /// <summary>
    /// Adiciona configuração de Web API (Controllers, Swagger, Filters).
    /// </summary>
    public static IServiceCollection AddWebApi(this IServiceCollection services)
    {
        // ===============================================================
        // Controllers
        // ===============================================================
        services.AddControllers(options =>
        {
            options.Filters.Add<ExceptionHandlingFilter>();
        })
        .AddPresentation(); // Registra controllers dos módulos

        // Registrar ExceptionHandlingFilter no DI
        services.AddScoped<ExceptionHandlingFilter>();

        // ===============================================================
        // Swagger/OpenAPI
        // ===============================================================
        services.AddEndpointsApiExplorer();
        services.AddSwaggerGen(options =>
        {
            options.SwaggerDoc("v1", new OpenApiInfo
            {
                Title = "BCommerce API",
                Version = "v1",
                Description = "API do BCommerce - Modular Monolith com DDD"
            });

            // Adicionar XML comments se necessário
            // var xmlFile = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
            // var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
            // options.IncludeXmlComments(xmlPath);
        });

        return services;
    }
}
