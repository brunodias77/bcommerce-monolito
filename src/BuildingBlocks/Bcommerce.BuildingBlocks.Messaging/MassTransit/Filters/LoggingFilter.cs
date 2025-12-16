using MassTransit;
using Microsoft.Extensions.Logging;

namespace Bcommerce.BuildingBlocks.Messaging.MassTransit.Filters;

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
