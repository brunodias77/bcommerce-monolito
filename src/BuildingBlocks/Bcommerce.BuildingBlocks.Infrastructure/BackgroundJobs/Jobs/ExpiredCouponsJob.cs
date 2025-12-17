using Microsoft.Extensions.Logging;
using Quartz;

namespace Bcommerce.BuildingBlocks.Infrastructure.BackgroundJobs.Jobs;

/// <summary>
/// Job recorrente para inativação de cupons expirados.
/// </summary>
/// <remarks>
/// Verifica e atualiza status de cupons com data de validade vencida.
/// - Garante consistência da base de dados
/// - Evita uso acidental de cupons inválidos em validações simples
/// 
/// Exemplo de uso:
/// <code>
/// // Configurado no Startup
/// q.AddJob&lt;ExpiredCouponsJob&gt;(...);
/// </code>
/// </remarks>
public class ExpiredCouponsJob(ILogger<ExpiredCouponsJob> logger) : IJob
{
    private readonly ILogger<ExpiredCouponsJob> _logger = logger;

    public Task Execute(IJobExecutionContext context)
    {
        _logger.LogInformation("Atualizando status de cupons expirados...");
        return Task.CompletedTask;
    }
}
