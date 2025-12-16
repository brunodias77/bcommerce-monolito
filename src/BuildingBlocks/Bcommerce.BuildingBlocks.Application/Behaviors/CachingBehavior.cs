using MediatR;

namespace Bcommerce.BuildingBlocks.Application.Behaviors;

public class CachingBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
    where TRequest : notnull
{
    public async Task<TResponse> Handle(TRequest request, RequestHandlerDelegate<TResponse> next, CancellationToken cancellationToken)
    {
        // Implementação de cache (Redis/Memory)
        return await next();
    }
}
