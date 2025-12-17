using Bcommerce.BuildingBlocks.Application.Abstractions.Data;
using Bcommerce.BuildingBlocks.Application.Abstractions.Services;
using Bcommerce.BuildingBlocks.Infrastructure.AuditLog.Repositories;
using Bcommerce.BuildingBlocks.Infrastructure.AuditLog.Services;
using Bcommerce.BuildingBlocks.Infrastructure.Data;
using Bcommerce.BuildingBlocks.Infrastructure.Data.Interceptors;
using Bcommerce.BuildingBlocks.Infrastructure.Inbox.Processors;
using Bcommerce.BuildingBlocks.Infrastructure.Inbox.Repositories;
using Bcommerce.BuildingBlocks.Infrastructure.Outbox.Processors;
using Bcommerce.BuildingBlocks.Infrastructure.Outbox.Repositories;
using Bcommerce.BuildingBlocks.Infrastructure.Time;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Quartz;

namespace Bcommerce.BuildingBlocks.Infrastructure.Extensions;

/// <summary>
/// Métodos de extensão para configuração de serviços de infraestrutura no DI.
/// </summary>
/// <remarks>
/// Centraliza a injeção de dependência dos Building Blocks.
/// - Registra interceptores do EF Core
/// - Configura Repositórios de Inbox/Outbox e AuditLog
/// - Inicializa e configura o Quartz para Jobs em background
/// 
/// Exemplo de uso:
/// <code>
/// builder.Services.AddBuildingBlocksInfrastructure(configuration.GetConnectionString("Db"));
/// </code>
/// </remarks>
public static class ServiceCollectionExtensions
{
    /// <summary>
    /// Adiciona os serviços de infraestrutura (Interceptors, Jobs, Repositórios Base).
    /// </summary>
    /// <param name="services">Coleção de serviços.</param>
    /// <param name="connectionString">String de conexão (não usada diretamente aqui, mas pode ser útil para futuras extensões).</param>
    /// <returns>A mesma coleção de serviços para encadeamento.</returns>
    public static IServiceCollection AddBuildingBlocksInfrastructure(this IServiceCollection services, string connectionString)
    {
        services.AddScoped<IDateTimeProvider, DateTimeProvider>();

        // Interceptors
        services.AddScoped<AuditableEntityInterceptor>();
        services.AddScoped<SoftDeleteInterceptor>();
        services.AddScoped<DomainEventInterceptor>();
        services.AddScoped<OptimisticLockInterceptor>();

        // Repositories Genéricos podem ser registrados aqui ou nos módulos
        // services.AddScoped(typeof(IRepository<>), typeof(Repository<,>)); // Requer passar Contexto, então geralmente se registra no módulo
        
        // Outbox/Inbox
        services.AddScoped<IOutboxRepository, OutboxRepository>();
        services.AddScoped<IInboxRepository, InboxRepository>();
        services.AddScoped<OutboxProcessor>();
        services.AddScoped<InboxProcessor>();

        // AuditLog
        services.AddScoped<IAuditLogRepository, AuditLogRepository>();
        services.AddScoped<IAuditLogService, AuditLogService>();

        // Quartz
        services.AddQuartz(configure =>
        {
            var jobKey = new JobKey(nameof(Outbox.BackgroundJobs.OutboxProcessorJob));
            configure.AddJob<Outbox.BackgroundJobs.OutboxProcessorJob>(jobKey, c => {})
                .AddTrigger(trigger =>
                    trigger.ForJob(jobKey)
                        .WithSimpleSchedule(schedule =>
                            schedule.WithIntervalInSeconds(10).RepeatForever()));
            
            var inboxJobKey = new JobKey(nameof(Inbox.BackgroundJobs.InboxProcessorJob));
            configure.AddJob<Inbox.BackgroundJobs.InboxProcessorJob>(inboxJobKey, c => {})
                .AddTrigger(trigger =>
                    trigger.ForJob(inboxJobKey)
                        .WithSimpleSchedule(schedule =>
                            schedule.WithIntervalInSeconds(10).RepeatForever()));
        });

        services.AddQuartzHostedService(options => options.WaitForJobsToComplete = true);

        return services;
    }
}
