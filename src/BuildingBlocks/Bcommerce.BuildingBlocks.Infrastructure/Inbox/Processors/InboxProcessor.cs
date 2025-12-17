using Bcommerce.BuildingBlocks.Application.Abstractions.Services;
using Bcommerce.BuildingBlocks.Infrastructure.Data;
using Bcommerce.BuildingBlocks.Infrastructure.Inbox.Models;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

namespace Bcommerce.BuildingBlocks.Infrastructure.Inbox.Processors;

/// <summary>
/// Serviço responsável por processar mensagens pendentes na tabela Inbox.
/// </summary>
/// <remarks>
/// Lê mensagens não processadas e despacha os eventos correspondentes internamente.
/// - Desserializa o conteúdo JSON para o tipo de evento correto
/// - Publica o evento via MediatR (IPublisher)
/// - Marca a mensagem como processada ou registra erro
/// 
/// Exemplo de uso:
/// <code>
/// await _processor.ProcessAsync(cancellationToken);
/// </code>
/// </remarks>
public class InboxProcessor(
    BaseDbContext dbContext,
    IPublisher publisher,
    IDateTimeProvider dateTimeProvider,
    ILogger<InboxProcessor> logger)
{
    private readonly BaseDbContext _dbContext = dbContext;
    private readonly IPublisher _publisher = publisher;
    private readonly IDateTimeProvider _dateTimeProvider = dateTimeProvider;
    private readonly ILogger<InboxProcessor> _logger = logger;

    /// <summary>
    /// Executa o ciclo de processamento de mensagens pendentes.
    /// </summary>
    /// <param name="cancellationToken">Token de cancelamento.</param>
    public async Task ProcessAsync(CancellationToken cancellationToken = default)
    {
        var messages = await _dbContext.Set<InboxMessage>()
            .Where(m => m.ProcessedOnUtc == null)
            .Take(20)
            .ToListAsync(cancellationToken);

        if (messages.Count == 0) return;

        foreach (var message in messages)
        {
            try
            {
                var eventType = Type.GetType(message.Type);
                if (eventType == null)
                {
                    _logger.LogWarning("Tipo de evento desconhecido no Inbox: {EventType}", message.Type);
                    message.Error = $"Tipo desconhecido: {message.Type}";
                    message.ProcessedOnUtc = _dateTimeProvider.UtcNow;
                    continue;
                }

                var integrationEvent = JsonConvert.DeserializeObject(message.Content, eventType);
                if (integrationEvent != null)
                {
                    await _publisher.Publish(integrationEvent, cancellationToken);
                }

                message.ProcessedOnUtc = _dateTimeProvider.UtcNow;
            }
            catch (Exception ex)
            {
                message.Error = ex.Message;
                message.ProcessedOnUtc = _dateTimeProvider.UtcNow;
                _logger.LogError(ex, "Erro ao processar mensagem inbox: {MessageId}", message.Id);
            }
        }

        await _dbContext.SaveChangesAsync(cancellationToken);
    }
}
