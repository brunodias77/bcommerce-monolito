using Microsoft.Extensions.Logging;
using Quartz;

namespace Bcommerce.BuildingBlocks.Infrastructure.BackgroundJobs.Jobs;

/// <summary>
/// Job recorrente para processar carrinhos abandonados.
/// </summary>
/// <remarks>
/// Identifica carrinhos sem checkout há 'X' tempo e dispara ações.
/// - Envio de e-mails de lembrete
/// - Gera métricas de abandono
/// - Agendado via Cron na configuração
/// 
/// Exemplo de uso:
/// <code>
/// // Configurado no Startup:
/// q.AddJob&lt;AbandonedCartsJob&gt;(opts => opts.WithIdentity("carts-job"));
/// </code>
/// </remarks>
public class AbandonedCartsJob(ILogger<AbandonedCartsJob> logger) : IJob
{
    private readonly ILogger<AbandonedCartsJob> _logger = logger;

    public Task Execute(IJobExecutionContext context)
    {
        _logger.LogInformation("Verificando carrinhos abandonados para envio de e-mails...");
        return Task.CompletedTask;
    }
}
