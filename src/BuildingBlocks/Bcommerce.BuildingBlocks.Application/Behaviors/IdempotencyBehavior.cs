using MediatR;

namespace Bcommerce.BuildingBlocks.Application.Behaviors;

public class IdempotencyBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
    where TRequest : notnull
{
    public async Task<TResponse> Handle(TRequest request, RequestHandlerDelegate<TResponse> next, CancellationToken cancellationToken)
    {
        // Verificar chave de idempotência no header/request
        // Se processado -> retornar resposta salva
        // Se processando -> esperar/erro
        // Se novo -> next() -> salvar resposta
        return await next();
    }
}
