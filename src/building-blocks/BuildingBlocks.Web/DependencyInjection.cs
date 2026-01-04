using BuildingBlocks.Web.Filters;
using BuildingBlocks.Web.Middlewares;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;

namespace BuildingBlocks.Web;

/// <summary>
/// Extensões para configurar os serviços web compartilhados
/// </summary>
public static class DependencyInjection
{
    /// <summary>
    /// Adiciona os serviços web compartilhados ao container de DI
    /// Registra controllers, filtros, CORS, e configurações de API
    /// </summary>
    /// <param name="services">Coleção de serviços</param>
    /// <param name="configureControllers">Ação opcional para configurar controllers adicionais</param>
    /// <returns>Coleção de serviços para encadeamento</returns>
    public static IServiceCollection AddWebServices(
        this IServiceCollection services,
        Action<IMvcBuilder>? configureControllers = null)
    {
        // Configura Controllers com filtros globais
        var mvcBuilder = services.AddControllers(options =>
        {
            // Adiciona filtro de validação global
            // Este filtro intercepta requisições e valida ModelState automaticamente
            options.Filters.Add<ValidationFilter>();
        })
        .ConfigureApiBehaviorOptions(options =>
        {
            // Desabilita a resposta automática de validação do ASP.NET Core
            // Nosso ValidationFilter irá lidar com isso de forma padronizada
            options.SuppressModelStateInvalidFilter = true;
        });

        // Permite configuração adicional de controllers (ex: adicionar JSON options)
        configureControllers?.Invoke(mvcBuilder);

        // Registra middleware de tratamento de exceções
        services.AddExceptionHandler<GlobalExceptionHandler>();

        // Adiciona ProblemDetails para respostas padronizadas RFC 7807
        services.AddProblemDetails();

        // Configura CORS de forma permissiva para desenvolvimento
        // IMPORTANTE: Ajustar para produção com políticas específicas
        services.AddCors(options =>
        {
            options.AddDefaultPolicy(policy =>
            {
                policy
                    .AllowAnyOrigin()      // Permite qualquer origem (ajustar em produção)
                    .AllowAnyMethod()      // Permite qualquer método HTTP
                    .AllowAnyHeader();     // Permite qualquer cabeçalho
            });

            // Política CORS restrita para produção (exemplo)
            options.AddPolicy("Production", policy =>
            {
                policy
                    .WithOrigins("https://seudominio.com.br")
                    .WithMethods("GET", "POST", "PUT", "DELETE", "PATCH")
                    .WithHeaders("Content-Type", "Authorization")
                    .AllowCredentials();
            });
        });

        return services;
    }

    /// <summary>
    /// Configura o pipeline de middlewares web
    /// </summary>
    /// <param name="app">Application builder</param>
    /// <returns>Application builder para encadeamento</returns>
    public static IApplicationBuilder UseWebServices(this IApplicationBuilder app)
    {
        // Middleware de tratamento de exceções
        // Deve ser um dos primeiros para capturar erros de todos os middlewares seguintes
        app.UseExceptionHandler();

        // CORS - Deve vir antes de Authorization
        app.UseCors();

        // Autenticação e Autorização
        app.UseAuthentication();
        app.UseAuthorization();

        return app;
    }
}
