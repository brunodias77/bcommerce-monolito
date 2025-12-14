using BuildingBlocks.Application.Abstractions;
using BuildingBlocks.Application.Results;
using BuildingBlocks.Domain.Repositories;
using MediatR;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace BuildingBlocks.Application.Behaviors;

/// <summary>
/// Behavior do MediatR para gerenciamento automático de transações em Commands.
/// Abre transação, executa handler, e faz commit/rollback automaticamente.
/// </summary>
/// <remarks>
/// Este behavior:
/// 1. Detecta se o request é um Command (ICommand)
/// 2. Abre transação no banco de dados
/// 3. Executa o handler
/// 4. Se sucesso: faz COMMIT
/// 5. Se falha/exceção: faz ROLLBACK
///
/// IMPORTANTE: Apenas Commands abrem transação. Queries NÃO.
///
/// ## Ordem de Registro dos Behaviors
///
/// A ORDEM de registro dos behaviors no DI Container é CRÍTICA!
/// Os behaviors são executados na ordem inversa do registro (o último registrado
/// é o mais interno, mais próximo do handler).
///
/// ### Ordem Recomendada (do mais externo para o mais interno):
///
/// <code>
/// // 1. Logging (mais externo - captura tudo)
/// services.AddScoped(typeof(IPipelineBehavior&lt;,&gt;), typeof(LoggingBehavior&lt;,&gt;));
///
/// // 2. Validação (antes da transação - evita abrir transação para request inválido)
/// services.AddScoped(typeof(IPipelineBehavior&lt;,&gt;), typeof(ValidationBehavior&lt;,&gt;));
///
/// // 3. Transação (mais interno - envolve apenas o handler)
/// services.AddScoped(typeof(IPipelineBehavior&lt;,&gt;), typeof(TransactionBehavior&lt;,&gt;));
/// </code>
///
/// ### Fluxo de Execução:
/// <code>
/// Request
///   → LoggingBehavior (início)
///     → ValidationBehavior (valida)
///       → TransactionBehavior (abre transação)
///         → Handler (executa lógica)
///       ← TransactionBehavior (commit/rollback)
///     ← ValidationBehavior
///   ← LoggingBehavior (fim)
/// Response
/// </code>
///
/// ### Por que esta ordem?
///
/// 1. **LoggingBehavior primeiro**: Captura timing completo, incluindo validação e transação
/// 2. **ValidationBehavior segundo**: Evita abrir transação para requests inválidos
/// 3. **TransactionBehavior terceiro**: Envolve apenas a execução do handler
///
/// ### Usando os métodos de extensão:
///
/// <code>
/// // Forma recomendada (ordem correta garantida):
/// services.AddLoggingBehavior();
/// services.AddValidationBehavior();
/// services.AddTransactionBehavior();
/// </code>
///
/// Exemplo de uso:
/// <code>
/// // Command abre transação automaticamente
/// public record CreateOrderCommand(...) : ICommand&lt;Guid&gt;;
///
/// // Query NÃO abre transação (read-only)
/// public record GetOrderByIdQuery(Guid OrderId) : IQuery&lt;OrderDto&gt;;
/// </code>
///
/// No seu sistema modular monolith:
/// - Cada módulo tem seu próprio DbContext
/// - Transações são locais ao módulo (não distribuídas)
/// - Comunicação entre módulos via Integration Events (Outbox)
/// </remarks>
public class TransactionBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
    where TRequest : notnull
    where TResponse : Result
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<TransactionBehavior<TRequest, TResponse>> _logger;

    public TransactionBehavior(
        IUnitOfWork unitOfWork,
        ILogger<TransactionBehavior<TRequest, TResponse>> logger)
    {
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    public async Task<TResponse> Handle(
        TRequest request,
        RequestHandlerDelegate<TResponse> next,
        CancellationToken cancellationToken)
    {
        // Apenas Commands devem abrir transação
        if (!IsCommand())
        {
            return await next();
        }

        var requestName = typeof(TRequest).Name;

        _logger.LogDebug(
            "Beginning transaction for {RequestName}",
            requestName);

        try
        {
            // Executa handler dentro da transação
            var response = await next();

            // Se falhou, não faz commit
            if (response.IsFailure)
            {
                _logger.LogWarning(
                    "Transaction for {RequestName} not committed due to failure: {ErrorCode}",
                    requestName,
                    response.Error.Code);

                return response;
            }

            // Commit da transação
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            _logger.LogDebug(
                "Transaction committed for {RequestName}",
                requestName);

            return response;
        }
        catch (Exception ex)
        {
            _logger.LogError(
                ex,
                "Transaction rolled back for {RequestName}",
                requestName);

            throw;
        }
    }

    private static bool IsCommand()
    {
        return typeof(TRequest).GetInterfaces()
            .Any(i => i.IsGenericType &&
                     (i.GetGenericTypeDefinition() == typeof(ICommand<>) ||
                      i == typeof(ICommand)));
    }
}

/// <summary>
/// Extensões para facilitar registro de transaction behaviors.
/// </summary>
public static class TransactionBehaviorExtensions
{
    /// <summary>
    /// Registra TransactionBehavior no pipeline do MediatR.
    /// </summary>
    public static IServiceCollection AddTransactionBehavior(this IServiceCollection services)
    {
        services.AddScoped(typeof(IPipelineBehavior<,>), typeof(TransactionBehavior<,>));
        return services;
    }
}