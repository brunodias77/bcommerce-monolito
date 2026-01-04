using BuildingBlocks.Infrastructure.Persistence.Inbox;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;

namespace BuildingBlocks.Infrastructure.Persistence.Interceptors;

/// <summary>
/// Interceptor que verifica se eventos já foram processados antes de permitir o processamento
/// Implementa o padrão Inbox para garantir idempotência no consumo de eventos
/// </summary>
public sealed class InboxInterceptor : SaveChangesInterceptor
{
    private readonly string _moduleName;

    /// <summary>
    /// Cria uma nova instância do InboxInterceptor
    /// </summary>
    /// <param name="moduleName">Nome do módulo (users, catalog, cart, orders, payments, coupons)</param>
    public InboxInterceptor(string moduleName = "shared")
    {
        _moduleName = moduleName;
    }

    /// <summary>
    /// Verifica se um evento já foi processado por este módulo
    /// </summary>
    /// <param name="eventoId">ID do evento a verificar</param>
    /// <param name="context">Contexto do banco de dados</param>
    /// <param name="cancellationToken">Token de cancelamento</param>
    /// <returns>True se o evento já foi processado, False caso contrário</returns>
    public async Task<bool> EventoJaProcessadoAsync(
        Guid eventoId,
        DbContext context,
        CancellationToken cancellationToken = default)
    {
        return await context.Set<InboxMessage>()
            .AnyAsync(m => m.Id == eventoId && m.Module == _moduleName, cancellationToken);
    }

    /// <summary>
    /// Verifica se um evento já foi processado por este módulo (versão síncrona)
    /// </summary>
    /// <param name="eventoId">ID do evento a verificar</param>
    /// <param name="context">Contexto do banco de dados</param>
    /// <returns>True se o evento já foi processado, False caso contrário</returns>
    public bool EventoJaProcessado(Guid eventoId, DbContext context)
    {
        return context.Set<InboxMessage>()
            .Any(m => m.Id == eventoId && m.Module == _moduleName);
    }

    /// <summary>
    /// Marca um evento como processado, adicionando-o na tabela inbox
    /// </summary>
    /// <param name="eventoId">ID do evento</param>
    /// <param name="tipoEvento">Tipo do evento</param>
    /// <param name="context">Contexto do banco de dados</param>
    /// <param name="cancellationToken">Token de cancelamento</param>
    public async Task MarcarEventoComoProcessadoAsync(
        Guid eventoId,
        string tipoEvento,
        DbContext context,
        CancellationToken cancellationToken = default)
    {
        // Verifica se já não foi processado (double-check)
        if (await EventoJaProcessadoAsync(eventoId, context, cancellationToken))
        {
            return;
        }

        var mensagemInbox = new InboxMessage(eventoId, tipoEvento, _moduleName);
        await context.Set<InboxMessage>().AddAsync(mensagemInbox, cancellationToken);
        await context.SaveChangesAsync(cancellationToken);
    }

    /// <summary>
    /// Marca um evento como processado, adicionando-o na tabela inbox (versão síncrona)
    /// </summary>
    /// <param name="eventoId">ID do evento</param>
    /// <param name="tipoEvento">Tipo do evento</param>
    /// <param name="context">Contexto do banco de dados</param>
    public void MarcarEventoComoProcessado(
        Guid eventoId,
        string tipoEvento,
        DbContext context)
    {
        // Verifica se já não foi processado (double-check)
        if (EventoJaProcessado(eventoId, context))
        {
            return;
        }

        var mensagemInbox = new InboxMessage(eventoId, tipoEvento, _moduleName);
        context.Set<InboxMessage>().Add(mensagemInbox);
        context.SaveChanges();
    }

    /// <summary>
    /// Executa uma ação apenas se o evento ainda não foi processado
    /// Garante idempotência automaticamente
    /// </summary>
    /// <param name="eventoId">ID do evento</param>
    /// <param name="tipoEvento">Tipo do evento</param>
    /// <param name="context">Contexto do banco de dados</param>
    /// <param name="acao">Ação a ser executada se o evento ainda não foi processado</param>
    /// <param name="cancellationToken">Token de cancelamento</param>
    /// <returns>True se a ação foi executada, False se o evento já foi processado</returns>
    public async Task<bool> ExecutarSeNaoProcessadoAsync(
        Guid eventoId,
        string tipoEvento,
        DbContext context,
        Func<Task> acao,
        CancellationToken cancellationToken = default)
    {
        // Verifica se já foi processado
        if (await EventoJaProcessadoAsync(eventoId, context, cancellationToken))
        {
            return false;
        }

        // Executa a ação
        await acao();

        // Marca como processado
        await MarcarEventoComoProcessadoAsync(eventoId, tipoEvento, context, cancellationToken);

        return true;
    }

    /// <summary>
    /// Executa uma ação apenas se o evento ainda não foi processado (versão síncrona)
    /// Garante idempotência automaticamente
    /// </summary>
    /// <param name="eventoId">ID do evento</param>
    /// <param name="tipoEvento">Tipo do evento</param>
    /// <param name="context">Contexto do banco de dados</param>
    /// <param name="acao">Ação a ser executada se o evento ainda não foi processado</param>
    /// <returns>True se a ação foi executada, False se o evento já foi processado</returns>
    public bool ExecutarSeNaoProcessado(
        Guid eventoId,
        string tipoEvento,
        DbContext context,
        Action acao)
    {
        // Verifica se já foi processado
        if (EventoJaProcessado(eventoId, context))
        {
            return false;
        }

        // Executa a ação
        acao();

        // Marca como processado
        MarcarEventoComoProcessado(eventoId, tipoEvento, context);

        return true;
    }
}
