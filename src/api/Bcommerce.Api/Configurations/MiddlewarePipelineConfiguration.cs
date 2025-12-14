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
        // EXCEPTION HANDLING (Camada de Segurança)
        // ===============================================================
        // Deve ser o PRIMEIRO middleware para capturar exceções de todo o pipeline.
        // Retorna ProblemDetails padronizados (RFC 7807) para o cliente.
        app.UseExceptionHandlingMiddleware();

        // ===============================================================
        // REQUEST LOGGING (Auditoria)
        // ===============================================================
        // Loga informações sobre a requisição (Método, Path, Status Code, Latência).
        // Útil para monitoramento e debugging.
        app.UseRequestLoggingMiddleware(options =>
        {
            options.SlowRequestThresholdMs = 3000; // Alerta se demorar mais de 3s
            options.IgnoredPaths.Add("/health");   // Não poluir log com health checks
        });

        // ===============================================================
        // SWAGGER (Documentação)
        // ===============================================================
        // Disponível apenas em ambiente de desenvolvimento.
        // Expõe a documentação da API e interface interativa (/index.html).
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
        // HTTPS & SEGURANÇA
        // ===============================================================
        // Força redirecionamento para HTTPS.
        app.UseHttpsRedirection();

        // ===============================================================
        // AUTENTICAÇÃO & AUTORIZAÇÃO
        // ===============================================================
        // Authentication: Identifica QUEM é o usuário (valida token JWT).
        // Authorization: Verifica se tem PERMISSÃO para acessar o recurso.
        // Ordem: AuthN -> AuthZ
        // app.UseAuthentication();
        app.UseAuthorization();

        // ===============================================================
        // ENDPOINTS (Execução Final)
        // ===============================================================
        // Mapeia as rotas dos Controllers para os endpoints.
        // É aqui que o código da sua API (Controllers) é executado.
        app.MapControllers();

        return app;
    }
}
