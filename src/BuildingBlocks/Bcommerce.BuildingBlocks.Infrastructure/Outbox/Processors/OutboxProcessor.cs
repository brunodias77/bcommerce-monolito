using Bcommerce.BuildingBlocks.Application.Abstractions.Services;
using Bcommerce.BuildingBlocks.Infrastructure.Data;
using Bcommerce.BuildingBlocks.Infrastructure.Outbox.Models;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

namespace Bcommerce.BuildingBlocks.Infrastructure.Outbox.Processors;

public class OutboxProcessor(
    BaseDbContext dbContext, // Acesso direto ao DbContext para processar em lote
    IPublisher publisher,
    IDateTimeProvider dateTimeProvider,
    ILogger<OutboxProcessor> logger)
{
    private readonly BaseDbContext _dbContext = dbContext;
    private readonly IPublisher _publisher = publisher;
    private readonly IDateTimeProvider _dateTimeProvider = dateTimeProvider;
    private readonly ILogger<OutboxProcessor> _logger = logger;

    public async Task ProcessAsync(CancellationToken cancellationToken = default)
    {
        var messages = await _dbContext.Set<OutboxMessage>()
            .Where(m => m.ProcessedOnUtc == null)
            .Take(20) // Lote de 20
            .ToListAsync(cancellationToken);

        if (messages.Count == 0) return;

        foreach (var message in messages)
        {
            try
            {
                // Deserializar e publicar
                // Nota: Precisamos de uma maneira de converter Type string -> Type real
                // Isso geralmente requer um Assembly scanner ou registro de tipos.
                // Para simplificar, vou assumir DomainEvent base, mas o ideal é Type.GetType(message.Type)
                
                var eventType = Type.GetType(message.Type);
                if (eventType == null)
                {
                    _logger.LogWarning("Tipo de evento desconhecido: {EventType}", message.Type);
                    message.Error = $"Tipo desconhecido: {message.Type}";
                    message.ProcessedOnUtc = _dateTimeProvider.UtcNow;
                    continue;
                }

                var domainEvent = JsonConvert.DeserializeObject(message.Content, eventType);
                if (domainEvent != null)
                {
                    await _publisher.Publish(domainEvent, cancellationToken);
                }

                message.ProcessedOnUtc = _dateTimeProvider.UtcNow;
            }
            catch (Exception ex)
            {
                message.Error = ex.Message;
                // Não marcamos como processado (ou usamos um campo RetryCount para tentar novamente depois)
                // Se marcarmos ProcessedOnUtc, ele sai da fila. Se não, tenta de novo. 
                // Seguiremos a estratégia de marcar processado mas com erro para não travar a fila.
                message.ProcessedOnUtc = _dateTimeProvider.UtcNow;
                _logger.LogError(ex, "Erro ao processar mensagem outbox: {MessageId}", message.Id);
            }
        }

        await _dbContext.SaveChangesAsync(cancellationToken);
    }
}
