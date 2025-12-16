using Bcommerce.BuildingBlocks.Application.Abstractions.Services;
using MediatR;
using Microsoft.Extensions.Logging;

namespace Bcommerce.BuildingBlocks.Application.Behaviors;

public class LoggingBehavior<TRequest, TResponse>(
    ILogger<LoggingBehavior<TRequest, TResponse>> logger,
    ICurrentUserService currentUserService)
    : IPipelineBehavior<TRequest, TResponse>
    where TRequest : notnull
{
    private readonly ILogger<LoggingBehavior<TRequest, TResponse>> _logger = logger;
    private readonly ICurrentUserService _currentUserService = currentUserService;

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
