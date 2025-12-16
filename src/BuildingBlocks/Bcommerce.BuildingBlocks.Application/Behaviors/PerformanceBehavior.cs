using System.Diagnostics;
using Bcommerce.BuildingBlocks.Application.Abstractions.Services;
using MediatR;
using Microsoft.Extensions.Logging;

namespace Bcommerce.BuildingBlocks.Application.Behaviors;

public class PerformanceBehavior<TRequest, TResponse>(
    ILogger<PerformanceBehavior<TRequest, TResponse>> logger,
    ICurrentUserService currentUserService)
    : IPipelineBehavior<TRequest, TResponse>
    where TRequest : notnull
{
    private readonly ILogger<PerformanceBehavior<TRequest, TResponse>> _logger = logger;
    private readonly ICurrentUserService _currentUserService = currentUserService;
    private readonly Stopwatch _timer = new();

    public async Task<TResponse> Handle(TRequest request, RequestHandlerDelegate<TResponse> next, CancellationToken cancellationToken)
    {
        _timer.Start();

        var response = await next();

        _timer.Stop();

        var elapsedMilliseconds = _timer.ElapsedMilliseconds;

        if (elapsedMilliseconds > 500) // Log se demorar mais que 500ms
        {
            var requestName = typeof(TRequest).Name;
            var userId = _currentUserService.UserId;
            var userName = _currentUserService.Email;

            _logger.LogWarning("Longa Duração na Requisição: {Name} ({ElapsedMilliseconds} milissegundos) {@UserId} {@UserName} {@Request}",
                requestName, elapsedMilliseconds, userId, userName, request);
        }

        return response;
    }
}
