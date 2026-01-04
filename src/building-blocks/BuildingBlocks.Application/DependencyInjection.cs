using System.Reflection;
using BuildingBlocks.Application.Behaviors;
using FluentValidation;
using MediatR;
using Microsoft.Extensions.DependencyInjection;

namespace BuildingBlocks.Application;

/// <summary>
/// Extensões para configurar os serviços de aplicação compartilhados
/// </summary>
public static class DependencyInjection
{
    /// <summary>
    /// Adiciona os serviços de aplicação compartilhados ao container de DI
    /// Registra MediatR, FluentValidation e Behaviors do pipeline
    /// </summary>
    /// <param name="services">Coleção de serviços</param>
    /// <param name="assemblies">Assemblies que contém handlers, validadores, etc.</param>
    /// <returns>Coleção de serviços para encadeamento</returns>
    public static IServiceCollection AddApplicationServices(
        this IServiceCollection services,
        params Assembly[] assemblies)
    {
        // Registra MediatR (CQRS)
        services.AddMediatR(config =>
        {
            // Registra handlers dos assemblies fornecidos
            config.RegisterServicesFromAssemblies(assemblies);

            // Registra os pipeline behaviors na ordem correta:
            // 1. Logging (registra início da operação)
            // 2. Validation (valida o request)
            // 3. Transaction (gerencia transação do banco)

            config.AddOpenBehavior(typeof(LoggingBehavior<,>));
            config.AddOpenBehavior(typeof(ValidationBehavior<,>));
            config.AddOpenBehavior(typeof(TransactionBehavior<,>));
        });

        // Registra FluentValidation
        // Busca todos os validadores nos assemblies fornecidos
        services.AddValidatorsFromAssemblies(
            assemblies,
            includeInternalTypes: false);

        return services;
    }

    /// <summary>
    /// Adiciona os serviços de aplicação de um módulo específico
    /// Versão conveniente que aceita um único assembly
    /// </summary>
    /// <param name="services">Coleção de serviços</param>
    /// <param name="assembly">Assembly do módulo</param>
    /// <returns>Coleção de serviços para encadeamento</returns>
    public static IServiceCollection AddModuleApplicationServices(
        this IServiceCollection services,
        Assembly assembly)
    {
        return services.AddApplicationServices(assembly);
    }

    /// <summary>
    /// Adiciona os serviços de aplicação de um módulo usando um tipo marcador
    /// </summary>
    /// <typeparam name="TMarker">Tipo do assembly do módulo</typeparam>
    /// <param name="services">Coleção de serviços</param>
    /// <returns>Coleção de serviços para encadeamento</returns>
    public static IServiceCollection AddModuleApplicationServices<TMarker>(
        this IServiceCollection services)
    {
        return services.AddApplicationServices(typeof(TMarker).Assembly);
    }
}
