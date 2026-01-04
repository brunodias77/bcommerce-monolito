using System.Reflection;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.OpenApi.Models;

namespace BuildingBlocks.Web.Extensions;

/// <summary>
/// Extensões para configuração padronizada do Swagger/OpenAPI
/// Fornece documentação interativa da API REST
///
/// O Swagger gera documentação automática baseada nos controllers,
/// models e atributos aplicados aos endpoints
///
/// Recursos configurados:
/// - Documentação de versão da API
/// - Suporte a autenticação JWT Bearer
/// - Comentários XML para descrições detalhadas
/// - Exemplos de requisição e resposta
/// - Agrupamento por módulos (tags)
/// </summary>
public static class SwaggerExtensions
{
    /// <summary>
    /// Adiciona e configura o Swagger/OpenAPI
    /// </summary>
    /// <param name="services">Coleção de serviços</param>
    /// <param name="applicationName">Nome da aplicação</param>
    /// <param name="version">Versão da API (padrão: v1)</param>
    /// <param name="description">Descrição da API</param>
    /// <param name="xmlDocAssemblies">Assemblies que contém comentários XML</param>
    /// <returns>Coleção de serviços para encadeamento</returns>
    public static IServiceCollection AddSwaggerDocumentation(
        this IServiceCollection services,
        string applicationName,
        string version = "v1",
        string? description = null,
        params Assembly[] xmlDocAssemblies)
    {
        services.AddEndpointsApiExplorer();

        services.AddSwaggerGen(options =>
        {
            // Informações básicas da API
            options.SwaggerDoc(version, new OpenApiInfo
            {
                Title = applicationName,
                Version = version,
                Description = description ?? $"API REST do {applicationName}",
                Contact = new OpenApiContact
                {
                    Name = "Equipe de Desenvolvimento",
                    Email = "dev@exemplo.com.br"
                },
                License = new OpenApiLicense
                {
                    Name = "MIT",
                    Url = new Uri("https://opensource.org/licenses/MIT")
                }
            });

            // Configuração de autenticação JWT Bearer
            options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
            {
                Name = "Authorization",
                Type = SecuritySchemeType.Http,
                Scheme = "Bearer",
                BearerFormat = "JWT",
                In = ParameterLocation.Header,
                Description = @"Autenticação JWT usando o esquema Bearer.

                Digite 'Bearer' seguido de espaço e então seu token JWT.

                Exemplo: 'Bearer eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...'",
            });

            // Requisito de segurança global (aplica a todos os endpoints)
            options.AddSecurityRequirement(new OpenApiSecurityRequirement
            {
                {
                    new OpenApiSecurityScheme
                    {
                        Reference = new OpenApiReference
                        {
                            Type = ReferenceType.SecurityScheme,
                            Id = "Bearer"
                        }
                    },
                    Array.Empty<string>()
                }
            });

            // Adiciona comentários XML para documentação detalhada
            foreach (var assembly in xmlDocAssemblies)
            {
                var xmlFile = $"{assembly.GetName().Name}.xml";
                var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);

                if (File.Exists(xmlPath))
                {
                    options.IncludeXmlComments(xmlPath);
                }
            }

            // Ordena endpoints por tag (módulo) e depois por método HTTP
            options.OrderActionsBy(apiDesc =>
                $"{apiDesc.ActionDescriptor.RouteValues["controller"]}_{apiDesc.HttpMethod}");

            // Usa nomes amigáveis para SchemaIds (evita conflitos)
            options.CustomSchemaIds(type =>
            {
                var name = type.Name;

                // Remove sufixos comuns de DTOs
                if (name.EndsWith("Dto"))
                    name = name[..^3];
                else if (name.EndsWith("Request"))
                    name = name[..^7];
                else if (name.EndsWith("Response"))
                    name = name[..^8];

                return name;
            });

            // Configura exemplos para enums
            options.UseInlineDefinitionsForEnums();

            // Configura descrições para status codes
            options.DescribeAllParametersInCamelCase();
        });

        return services;
    }

    /// <summary>
    /// Configura o middleware do Swagger UI
    /// </summary>
    /// <param name="app">Application builder</param>
    /// <param name="version">Versão da API (padrão: v1)</param>
    /// <param name="routePrefix">Prefixo da rota do Swagger (padrão: swagger)</param>
    /// <returns>Application builder para encadeamento</returns>
    public static IApplicationBuilder UseSwaggerDocumentation(
        this IApplicationBuilder app,
        string version = "v1",
        string routePrefix = "swagger")
    {
        // Habilita middleware do Swagger (gera JSON)
        app.UseSwagger();

        // Habilita Swagger UI (interface web interativa)
        // Requer pacote Swashbuckle.AspNetCore.SwaggerUI
        app.UseSwaggerUI(options =>
        {
            // Configura endpoint do Swagger JSON
            options.SwaggerEndpoint($"/swagger/{version}/swagger.json", $"API {version}");

            // Define rota raiz do Swagger UI
            options.RoutePrefix = routePrefix;

            // Expande métodos por padrão
            options.DocExpansion(Swashbuckle.AspNetCore.SwaggerUI.DocExpansion.List);

            // Exibe duração das requisições
            options.DisplayRequestDuration();

            // Habilita filtro de pesquisa
            options.EnableFilter();

            // Habilita validação do Swagger spec
            options.EnableValidator();

            // Configura tema
            options.DefaultModelsExpandDepth(2);
            options.DefaultModelExpandDepth(2);

            // Persiste autorização entre recarregamentos
            options.EnablePersistAuthorization();
        });

        return app;
    }

    /// <summary>
    /// Adiciona e configura Swagger com valores padrão para o e-commerce
    /// Configuração conveniente para uso rápido
    /// </summary>
    /// <param name="services">Coleção de serviços</param>
    /// <param name="xmlDocAssemblies">Assemblies que contém comentários XML</param>
    /// <returns>Coleção de serviços para encadeamento</returns>
    public static IServiceCollection AddECommerceSwagger(
        this IServiceCollection services,
        params Assembly[] xmlDocAssemblies)
    {
        return services.AddSwaggerDocumentation(
            applicationName: "BCommerce API",
            version: "v1",
            description: @"API REST do BCommerce - E-commerce Modular Monolith

Módulos disponíveis:
- **Catálogo**: Produtos, categorias, marcas, estoque e avaliações
- **Carrinho**: Gerenciamento de carrinhos de compras
- **Pedidos**: Processamento de pedidos, rastreamento e notas fiscais
- **Pagamentos**: Gateway de pagamentos, métodos salvos e transações
- **Cupons**: Cupons de desconto e promoções
- **Usuários**: Autenticação, perfis e endereços

Para usar a API:
1. Faça login através do endpoint POST /api/auth/login
2. Copie o token JWT retornado
3. Clique no botão 'Authorize' acima
4. Cole o token no formato: Bearer {seu_token}
5. Todos os endpoints protegidos estarão acessíveis",
            xmlDocAssemblies: xmlDocAssemblies);
    }
}
