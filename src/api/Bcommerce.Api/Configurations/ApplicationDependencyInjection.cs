using BuildingBlocks.Application.Behaviors;
using FluentValidation;
using MediatR;
using Users.Application.Commands.RegisterUser;

namespace Bcommerce.Api.Configurations;

/// <summary>
/// Configuração de Dependency Injection para a camada Application.
/// </summary>
/// <remarks>
/// Registra:
/// - MediatR com handlers de todos os assemblies
/// - Pipeline behaviors na ordem correta: Logging → Validation → Transaction
/// - FluentValidation validators
/// </remarks>
public static class ApplicationDependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services, IConfiguration configuration)
    {
        // ===============================================================
        // MediatR
        // ===============================================================
        // Registra handlers de todos os assemblies de módulos
        services.AddMediatR(cfg =>
        {
            cfg.RegisterServicesFromAssembly(typeof(ApplicationDependencyInjection).Assembly);
            
            // Users Module
            cfg.RegisterServicesFromAssembly(typeof(RegisterUserCommand).Assembly);
            
            // Adicione aqui os assemblies dos outros módulos quando implementados:
            // cfg.RegisterServicesFromAssembly(typeof(Catalog.Application.AssemblyReference).Assembly);
            // cfg.RegisterServicesFromAssembly(typeof(Orders.Application.AssemblyReference).Assembly);
        });

        // ===============================================================
        // MediatR Pipeline Behaviors (ordem importa!)
        // ===============================================================
        // 1. Logging: Mais externo - captura tudo incluindo exceções
        services.AddLoggingBehavior();

        // 2. Validation: Antes da transação - evita abrir transação para request inválido
        services.AddValidationBehavior();

        // 3. Transaction: Mais interno - envolve apenas o handler
        services.AddTransactionBehavior();

        // Opcional: Performance logging para detectar requests lentos
        // services.AddPerformanceLoggingBehavior(slowRequestThresholdMs: 500);

        // ===============================================================
        // FluentValidation
        // ===============================================================
        // Registra validators do módulo Users
        services.AddValidatorsFromAssembly(typeof(RegisterUserCommand).Assembly);

        return services;
    }
}