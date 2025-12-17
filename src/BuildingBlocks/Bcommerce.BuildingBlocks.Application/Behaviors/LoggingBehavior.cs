using Bcommerce.BuildingBlocks.Application.Abstractions.Services;
using MediatR;
using Microsoft.Extensions.Logging;

namespace Bcommerce.BuildingBlocks.Application.Behaviors;

/// <summary>
/// Behavior do MediatR para logging estruturado de requisições.
/// </summary>
/// <remarks>
/// Registra automaticamente a entrada e saída de cada requisição.
/// - Loga nome do comando/query e payload (cuidado com dados sensíveis)
/// - Loga ID do usuário e Tenant se disponíveis
/// - Mede tempo total de execução implicitamente pelos timestamps
/// 
/// Exemplo de uso:
/// <code>
/// // Output no console/Seq:
/// // [INFO] Processando requisição: CriarPedidoCommand { UserId: "123" } ...
/// // [INFO] Requisição processada: CriarPedidoCommand
/// </code>
/// </remarks>
public class LoggingBehavior<TRequest, TResponse>(
    ILogger<LoggingBehavior<TRequest, TResponse>> logger,
    ICurrentUserService currentUserService)
    : IPipelineBehavior<TRequest, TResponse>
    where TRequest : notnull
{
    private readonly ILogger<LoggingBehavior<TRequest, TResponse>> _logger = logger;
    private readonly ICurrentUserService _currentUserService = currentUserService;

    /// <inheritdoc />
    public async Task<TResponse> Handle(TRequest request, RequestHandlerDelegate<TResponse> next, CancellationToken cancellationToken)
    {
        var requestName = typeof(TRequest).Name;
        var userId = _currentUserService.UserId;
        var userName = _currentUserService.Email ?? string.Empty;

        _logger.LogInformation("Processando requisição: {RequestName} {@UserId} {@UserName} {@Request}",
            requestName, userId, userName, request);

        var response = await next();

        _logger.LogInformation("Requisição processada: {RequestName}", requestName);

        return response;
    }
}
