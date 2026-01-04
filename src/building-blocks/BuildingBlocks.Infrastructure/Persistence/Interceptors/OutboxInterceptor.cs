using System.Text.Json;
using BuildingBlocks.Domain.Events;
using BuildingBlocks.Domain.Models;
using BuildingBlocks.Infrastructure.Persistence.Outbox;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;

namespace BuildingBlocks.Infrastructure.Persistence.Interceptors;

/// <summary>
/// Interceptor que converte eventos de domínio em mensagens de outbox durante o SaveChanges
/// Implementa o padrão Transactional Outbox para garantir que eventos sejam publicados de forma consistente
/// </summary>
public sealed class OutboxInterceptor : SaveChangesInterceptor
{
    private readonly string _moduleName;

    /// <summary>
    /// Cria uma nova instância do OutboxInterceptor
    /// </summary>
    /// <param name="moduleName">Nome do módulo (users, catalog, cart, orders, payments, coupons)</param>
    public OutboxInterceptor(string moduleName = "shared")
    {
        _moduleName = moduleName;
    }

    public override async ValueTask<InterceptionResult<int>> SavingChangesAsync(
        DbContextEventData eventData,
        InterceptionResult<int> result,
        CancellationToken cancellationToken = default)
    {
        if (eventData.Context is not null)
        {
            await ConverterEventosEmMensagensOutboxAsync(eventData.Context, cancellationToken);
        }

        return await base.SavingChangesAsync(eventData, result, cancellationToken);
    }

    public override InterceptionResult<int> SavingChanges(
        DbContextEventData eventData,
        InterceptionResult<int> result)
    {
        if (eventData.Context is not null)
        {
            ConverterEventosEmMensagensOutbox(eventData.Context);
        }

        return base.SavingChanges(eventData, result);
    }

    /// <summary>
    /// Converte eventos de domínio em mensagens de outbox (versão assíncrona)
    /// </summary>
    private async Task ConverterEventosEmMensagensOutboxAsync(
        DbContext context,
        CancellationToken cancellationToken)
    {
        // Obtém todas as entidades com eventos de domínio
        var entidadesComEventos = context.ChangeTracker
            .Entries<AggregateRoot>()
            .Where(entry => entry.Entity.DomainEvents.Any())
            .Select(entry => entry.Entity)
            .ToList();

        if (!entidadesComEventos.Any())
        {
            return;
        }

        // Converte cada evento em uma mensagem de outbox
        var mensagensOutbox = new List<OutboxMessage>();

        foreach (var entidade in entidadesComEventos)
        {
            foreach (var evento in entidade.DomainEvents)
            {
                var mensagem = CriarMensagemOutbox(entidade, evento);
                mensagensOutbox.Add(mensagem);
            }

            // Limpa os eventos da entidade após converter
            entidade.ClearDomainEvents();
        }

        // Adiciona as mensagens no contexto
        await context.Set<OutboxMessage>().AddRangeAsync(mensagensOutbox, cancellationToken);
    }

    /// <summary>
    /// Converte eventos de domínio em mensagens de outbox (versão síncrona)
    /// </summary>
    private void ConverterEventosEmMensagensOutbox(DbContext context)
    {
        // Obtém todas as entidades com eventos de domínio
        var entidadesComEventos = context.ChangeTracker
            .Entries<AggregateRoot>()
            .Where(entry => entry.Entity.DomainEvents.Any())
            .Select(entry => entry.Entity)
            .ToList();

        if (!entidadesComEventos.Any())
        {
            return;
        }

        // Converte cada evento em uma mensagem de outbox
        var mensagensOutbox = new List<OutboxMessage>();

        foreach (var entidade in entidadesComEventos)
        {
            foreach (var evento in entidade.DomainEvents)
            {
                var mensagem = CriarMensagemOutbox(entidade, evento);
                mensagensOutbox.Add(mensagem);
            }

            // Limpa os eventos da entidade após converter
            entidade.ClearDomainEvents();
        }

        // Adiciona as mensagens no contexto
        context.Set<OutboxMessage>().AddRange(mensagensOutbox);
    }

    /// <summary>
    /// Cria uma mensagem de outbox a partir de um evento de domínio
    /// </summary>
    private OutboxMessage CriarMensagemOutbox(AggregateRoot entidade, IDomainEvent evento)
    {
        var tipoEvento = evento.GetType();
        var tipoAgregado = entidade.GetType().Name;
        var agregadoId = ObterIdDoAgregado(entidade);

        // Serializa o evento para JSON
        var payload = JsonSerializer.Serialize(evento, new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            WriteIndented = false
        });

        return new OutboxMessage(
            module: _moduleName,
            aggregateType: tipoAgregado,
            aggregateId: agregadoId,
            eventType: tipoEvento.FullName ?? tipoEvento.Name,
            payload: payload
        );
    }

    /// <summary>
    /// Obtém o ID do agregado usando reflexão
    /// </summary>
    private Guid ObterIdDoAgregado(AggregateRoot entidade)
    {
        var propriedadeId = entidade.GetType().GetProperty("Id");

        if (propriedadeId == null)
        {
            throw new InvalidOperationException(
                $"A entidade {entidade.GetType().Name} não possui uma propriedade 'Id'");
        }

        var valor = propriedadeId.GetValue(entidade);

        return valor switch
        {
            Guid guidId => guidId,
            string stringId when Guid.TryParse(stringId, out var parsedGuid) => parsedGuid,
            _ => throw new InvalidOperationException(
                $"O tipo do ID da entidade {entidade.GetType().Name} não é Guid ou conversível para Guid")
        };
    }
}
