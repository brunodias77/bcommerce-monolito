using BuildingBlocks.Presentation.Middleware;

namespace Bcommerce.Api.Configurations;

/// <summary>
/// Configuração do pipeline HTTP (Middlewares).
/// </summary>
public static class MiddlewarePipelineConfiguration
{
    /// <summary>
    /// Configura o pipeline de middlewares da aplicação.
    /// </summary>
    public static WebApplication UseMiddlewarePipeline(this WebApplication app)
    {
        // ===============================================================
        // Exception Handling (captura todas as exceções)
        // ===============================================================
        app.UseExceptionHandlingMiddleware();

        // ===============================================================
        // Request Logging (logging de todas as requisições)
        // ===============================================================
        app.UseRequestLoggingMiddleware(options =>
        {
            options.SlowRequestThresholdMs = 3000;
            options.IgnoredPaths.Add("/health");
        });

        // ===============================================================
        // Swagger (apenas em desenvolvimento)
        // ===============================================================
        if (app.Environment.IsDevelopment())
        {
            app.UseSwagger();
            app.UseSwaggerUI(options =>
            {
                options.SwaggerEndpoint("/swagger/v1/swagger.json", "BCommerce API v1");
                options.RoutePrefix = string.Empty; // Swagger na raiz
            });
        }

        // ===============================================================
        // HTTPS e Segurança
        // ===============================================================
        app.UseHttpsRedirection();

        // ===============================================================
        // Authentication & Authorization
        // ===============================================================
        // Será configurado quando implementar autenticação
        // app.UseAuthentication();
        app.UseAuthorization();

        // ===============================================================
        // Endpoints
        // ===============================================================
        app.MapControllers();

        return app;
    }
}
