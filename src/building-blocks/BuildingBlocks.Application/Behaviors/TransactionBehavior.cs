using System.Transactions;
using BuildingBlocks.Application.CQRS;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace BuildingBlocks.Application.Behaviors;

/// <summary>
/// Pipeline behavior para gerenciamento automático de transações (Unit of Work)
///
/// Garante que comandos sejam executados dentro de uma transação de banco de dados
/// Se o comando falhar, a transação é revertida (rollback)
/// Se o comando for bem-sucedido, a transação é confirmada (commit)
///
/// Importante: Apenas COMANDOS são executados em transação
/// Queries não modificam dados e não precisam de transação
///
/// Benefícios:
/// - Atomicidade: Todas as operações ocorrem ou nenhuma ocorre
/// - Consistência: O banco sempre fica em estado consistente
/// - Integração com Outbox Pattern: Eventos são salvos na mesma transação
///
/// Exemplos de uso baseados no schema SQL:
///
/// CriarPedidoCommand (operação complexa):
/// 1. Cria registro em orders.orders
/// 2. Cria múltiplos registros em orders.items
/// 3. Atualiza cart.carts (status = CONVERTED)
/// 4. Cria reservas em catalog.stock_reservations
/// 5. Salva eventos de domínio em shared.domain_events
/// → Tudo na mesma transação, garante consistência
///
/// ProcessarPagamentoCommand:
/// 1. Cria registro em payments.payments
/// 2. Cria transação em payments.transactions
/// 3. Atualiza orders.orders (status, paid_at)
/// 4. Salva eventos de domínio em shared.domain_events
/// → Se o gateway falhar, nada é salvo
///
/// AtualizarEstoqueCommand:
/// 1. Atualiza catalog.products (stock, reserved_stock)
/// 2. Cria registro em catalog.stock_movements
/// 3. Salva eventos de domínio
/// → Garante histórico consistente
///
/// CancelarPedidoCommand:
/// 1. Atualiza orders.orders (status, cancelled_at)
/// 2. Cria histórico em orders.status_history
/// 3. Libera reservas em catalog.stock_reservations
/// 4. Atualiza estoque em catalog.products
/// 5. Cria reembolso em payments.refunds (se aplicável)
/// → Rollback automático se qualquer passo falhar
/// </summary>
/// <typeparam name="TRequest">Tipo da requisição (comando)</typeparam>
/// <typeparam name="TResponse">Tipo da resposta</typeparam>
public sealed class TransactionBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
    where TRequest : IRequest<TResponse>
{
    private readonly DbContext _dbContext;
    private readonly ILogger<TransactionBehavior<TRequest, TResponse>> _logger;

    public TransactionBehavior(
        DbContext dbContext,
        ILogger<TransactionBehavior<TRequest, TResponse>> logger)
    {
        _dbContext = dbContext;
        _logger = logger;
    }

    public async Task<TResponse> Handle(
        TRequest request,
        RequestHandlerDelegate<TResponse> next,
        CancellationToken cancellationToken)
    {
        // Apenas comandos devem ser executados em transação
        // Queries são somente leitura e não precisam de transação
        if (request is not ICommand)
        {
            return await next();
        }

        var requestName = typeof(TRequest).Name;

        // Se já existe uma transação ativa, reutiliza
        if (_dbContext.Database.CurrentTransaction is not null)
        {
            return await next();
        }

        _logger.LogDebug(
            "Iniciando transação para {RequestName}",
            requestName);

        // Cria estratégia de execução para retry automático em caso de falha transitória
        var strategy = _dbContext.Database.CreateExecutionStrategy();

        return await strategy.ExecuteAsync(async () =>
        {
            // Inicia transação com nível de isolamento READ COMMITTED
            // (padrão do PostgreSQL, bom balanço entre performance e consistência)
            await using var transaction = await _dbContext.Database.BeginTransactionAsync(
                System.Data.IsolationLevel.ReadCommitted,
                cancellationToken);

            try
            {
                _logger.LogDebug(
                    "Transação iniciada para {RequestName}",
                    requestName);

                // Executa o comando
                var response = await next();

                // Salva mudanças no banco de dados
                await _dbContext.SaveChangesAsync(cancellationToken);

                // Confirma a transação
                await transaction.CommitAsync(cancellationToken);

                _logger.LogDebug(
                    "Transação confirmada com sucesso para {RequestName}",
                    requestName);

                return response;
            }
            catch (Exception ex)
            {
                _logger.LogError(
                    ex,
                    "Erro ao executar {RequestName}, revertendo transação",
                    requestName);

                // Reverte a transação em caso de erro
                await transaction.RollbackAsync(cancellationToken);

                throw;
            }
        });
    }
}
