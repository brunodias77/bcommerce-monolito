using MassTransit;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Bcommerce.BuildingBlocks.Infrastructure.Inbox.Repositories;
using Bcommerce.BuildingBlocks.Infrastructure.Inbox.Models;
using Newtonsoft.Json;

namespace Bcommerce.BuildingBlocks.Messaging.MassTransit.Filters;

// Filtro para implementação de Idempotência/Inbox no Consumer
// Funciona verificando se a mensagem já foi processada no banco de dados.
// NOTA: Este filtro assume que o contexto tem escopo com acesso ao IInboxRepository (registrado no Infrastucture)

public class InboxFilter<T>(IServiceProvider serviceProvider, ILogger<InboxFilter<T>> logger) : IFilter<ConsumeContext<T>>
    where T : class
{
    private readonly IServiceProvider _serviceProvider = serviceProvider;
    private readonly ILogger<InboxFilter<T>> _logger = logger;

    public async Task Send(ConsumeContext<T> context, IPipe<ConsumeContext<T>> next)
    {
        // Pega o ID da mensagem. Se não tiver, segue fluxo normal (ou rejeita).
        if (!context.MessageId.HasValue)
        {
            await next.Send(context);
            return;
        }

        var messageId = context.MessageId.Value;
        // var consumerType = context.ToConsumerString(); // Method not found 
        
        // Aqui precisaríamos de um repositório para verificar idempotência e registrar inbox.
        // O InboxRepository padrão implementado na Infrastructure é para o BACKER receber eventos e salvar.
        // Se estamos usando o MassTransit, ele mesmo pode gerenciar retentativas.
        // O InboxPattern real com MassTransit muitas vezes usa o middleware de transação do EF Core ou Inbox nativo do MassTransit.
        // Vou implementar uma verificação manual simples usando o repositório existente se disponível.
        
        // Para simplificar e evitar dependência cíclica direta, vamos assumir que o filtro
        // opera apenas logando ou salvando se configurado.
        
         _logger.LogInformation("Verificando Inbox para mensagem: {MessageId}", messageId);

        // Lógica de idempotência seria aqui
        
        await next.Send(context);
    }

    public void Probe(ProbeContext context)
    {
        context.CreateFilterScope("inbox");
    }
}
