using MassTransit;
using Microsoft.Extensions.Logging;

namespace Bcommerce.BuildingBlocks.Messaging.MassTransit.Filters;

public class ExceptionHandlingFilter<T>(ILogger<ExceptionHandlingFilter<T>> logger) : IFilter<ConsumeContext<T>>
    where T : class
{
    private readonly ILogger<ExceptionHandlingFilter<T>> _logger = logger;

    public async Task Send(ConsumeContext<T> context, IPipe<ConsumeContext<T>> next)
    {
        try
        {
            await next.Send(context);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Exceção não tratada no consumidor para mensagem {MessageType}", typeof(T).Name);
            // Re-throw para o MassTransit acionar retry policy ou mover para fila de erro
            throw;
        }
    }

    public void Probe(ProbeContext context)
    {
        context.CreateFilterScope("exceptionHandling");
    }
}
