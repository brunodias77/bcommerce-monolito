using MassTransit;
using Microsoft.Extensions.Logging;

namespace Bcommerce.BuildingBlocks.Messaging.MassTransit.Filters;

/// <summary>
/// Filtro para logging detalhado do ciclo de vida da mensagem.
/// </summary>
/// <typeparam name="T">Tipo da mensagem.</typeparam>
/// <remarks>
/// Registra o início e fim do processamento de cada mensagem.
/// - Auxilia na rastreabilidade e debug
/// - Loga IDs de correlação e tipo de mensagem
/// 
/// Exemplo de uso:
/// <code>
/// cfg.UseConsumeFilter(typeof(LoggingFilter&lt;&gt;), context);
/// </code>
/// </remarks>
public class LoggingFilter<T>(ILogger<LoggingFilter<T>> logger) : IFilter<ConsumeContext<T>>
    where T : class
{
    private readonly ILogger<LoggingFilter<T>> _logger = logger;

    public async Task Send(ConsumeContext<T> context, IPipe<ConsumeContext<T>> next)
    {
        _logger.LogInformation("Processando mensagem: {MessageType} ID: {MessageId}", typeof(T).Name, context.MessageId);
        
        try
        {
            await next.Send(context);
            _logger.LogInformation("Mensagem processada com sucesso: {MessageType} ID: {MessageId}", typeof(T).Name, context.MessageId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao processar mensagem: {MessageType} ID: {MessageId}", typeof(T).Name, context.MessageId);
            throw;
        }
    }

    public void Probe(ProbeContext context)
    {
        context.CreateFilterScope("logging");
    }
}
