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
        // CONTROLLERS & FILTERS
        // ===============================================================
        services.AddControllers(options =>
        {
            // Filtro Global de Exceção (Fallback caso o Middleware falhe ou para tratar erros específicos de MVC)
            options.Filters.Add<ExceptionHandlingFilter>();
        })
        .AddPresentation(); // Chama a extensão que registra os controllers dos módulos

        // Registrar ExceptionHandlingFilter no DI para que ele possa injetar ILogger, etc.
        services.AddScoped<ExceptionHandlingFilter>();

        // ===============================================================
        // SWAGGER / OPENAPI
        // ===============================================================
        // Gera a especificação OpenAPI (swagger.json) a partir dos Controllers e Models.
        services.AddEndpointsApiExplorer();
        services.AddSwaggerGen(options =>
        {
            options.SwaggerDoc("v1", new OpenApiInfo
            {
                Title = "BCommerce API",
                Version = "v1",
                Description = "API do BCommerce - Modular Monolith com DDD\n\n" +
                              "Módulos disponíveis:\n" +
                              "- **Users**: Gestão de usuários, auth e perfis.\n" +
                              "- **Catalog**: Produtos, categorias e gestão de estoque.\n" +
                              "- **Cart**: Carrinho de compras e checkout.\n"
            });

            // Permite anotações via XML Comments (/// <summary>) nos controllers
            // var xmlFile = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
            // var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
            // options.IncludeXmlComments(xmlPath);
            
            // TODO: Configurar JWT Bearer Auth no Swagger quando implementar autenticação
        });

        return services;
    }
}
