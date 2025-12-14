using BuildingBlocks.Application.Behaviors;

namespace Bcommerce.Api.Configurations;

public static class ApplicationDependencyInjection
{
    public static void AddApplication(this IServiceCollection services, IConfiguration configuration)
    {
        // Configuração do MediatR
        // Registra todos os handlers (Commands, Queries, Domain Events) de todos os módulos
        services.AddMediatR(config =>
        {
            // Registra handlers de todos os assemblies dos módulos
            // config.RegisterServicesFromAssembly(typeof(Users.Application.AssemblyReference).Assembly);
            // config.RegisterServicesFromAssembly(typeof(Catalog.Application.AssemblyReference).Assembly);
            // config.RegisterServicesFromAssembly(typeof(Orders.Application.AssemblyReference).Assembly);
            // config.RegisterServicesFromAssembly(typeof(Payments.Application.AssemblyReference).Assembly);
            // config.RegisterServicesFromAssembly(typeof(Coupons.Application.AssemblyReference).Assembly);
            // config.RegisterServicesFromAssembly(typeof(Cart.Application.AssemblyReference).Assembly);
        });

        // Configuração dos Behaviors do MediatR (Pipeline)
        // A ORDEM importa! Os behaviors são executados na ordem de registro

        // 1. Logging Behavior - Loga todas as requisições (início, fim, duração, erros)
        services.AddLoggingBehavior();

        // 2. Performance Logging Behavior - Alerta se requisição demorar mais que o threshold (500ms default)
        // Descomentar apenas se necessário monitorar performance
        // services.AddPerformanceLoggingBehavior(slowRequestThresholdMs: 500);

        // 3. Validation Behavior - Valida comandos/queries usando FluentValidation
        // Será implementado quando adicionar FluentValidation
        // services.AddScoped(typeof(IPipelineBehavior<,>), typeof(ValidationBehavior<,>));

        // 4. Transaction Behavior - Gerencia transações automaticamente para Commands
        // IMPORTANTE: Cada módulo terá seu próprio UnitOfWork registrado
        // Este behavior resolve o IUnitOfWork apropriado para cada command
        services.AddTransactionBehavior();

        // Alternativa para cenários multi-módulo (use com cautela, geralmente evite)
        // services.AddMultiModuleTransactionBehavior();

        // Detailed Logging Behavior - Apenas para desenvolvimento/debug (verbose)
        // NÃO use em produção (impacta performance e loga dados sensíveis)
        // services.AddDetailedLoggingBehavior();
    }
}