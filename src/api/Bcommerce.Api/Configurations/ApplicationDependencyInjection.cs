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
        // MEDIATR (Padrão Mediator)
        // ===============================================================
        // A biblioteca MediatR desacopla o envio de comandos (Commands/Queries) de seus processadores (Handlers).
        // Registramos os handlers escaneando os assemblies de cada módulo.
        services.AddMediatR(cfg =>
        {
            // Scan no assembly atual (API) e BuildingBlocks
            cfg.RegisterServicesFromAssembly(typeof(ApplicationDependencyInjection).Assembly);
            
            // Scan no assembly do módulo Users (onde estão os Commands/Events)
            cfg.RegisterServicesFromAssembly(typeof(RegisterUserCommand).Assembly);
            
            // TODO: Adicione aqui os assemblies dos outros módulos à medida que forem criados:
            // cfg.RegisterServicesFromAssembly(typeof(Catalog.Application.AssemblyReference).Assembly);
        });

        // ===============================================================
        // PIPELINE BEHAVIORS (Cross-Cutting Concerns)
        // ===============================================================
        // Os behaviors envolvem a execução do handler como "bonecas russas".
        // A ordem de registro é CRÍTICA, pois define a ordem de execução do pipeline.
        
        // 1. Logging (Mais Externo):
        // Envolve toda a execução. Captura request/response e mede o tempo total.
        // Se ocorrer erro na validação ou transação, o Log ainda captura.
        services.AddLoggingBehavior();

        // 2. Validation (Intermediário):
        // Verifica se o comando é válido ANTES de abrir qualquer transação ou conexão com banco.
        // Se falhar aqui, retorna erro imediatamente, economizando recursos.
        services.AddValidationBehavior();

        // 3. Transaction (Mais Interno):
        // Envolve apenas a execução do Handler.
        // Abre uma transação de banco de dados e faz Commit se o Handler executar com sucesso.
        // Se o Handler lançar exceção, faz Rollback automático.
        services.AddTransactionBehavior();

        // ===============================================================
        // FLUENT VALIDATION
        // ===============================================================
        // Registra automaticamente todas as classes Validator<T> encontradas nos assemblies.
        // Isso permite que o ValidationBehavior encontre e execute as regras de validação.
        services.AddValidatorsFromAssembly(typeof(RegisterUserCommand).Assembly);

        return services;
    }
}