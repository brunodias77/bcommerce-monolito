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

public static class ServiceCollectionExtensions
{
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
