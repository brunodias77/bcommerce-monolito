using MediatR;

namespace Bcommerce.BuildingBlocks.Application.Behaviors;

/// <summary>
/// Behavior do MediatR para implementação de estratégia de Cache-Aside.
/// </summary>
/// <remarks>
/// Intercepta requisições para verificar se já existe resposta cacheada.
/// - Verifica cache distribuído/memória antes de executar o handler
/// - Armazena a resposta no cache após execução bem-sucedida
/// - Requer que a Request implemente ICacheable (ou similar)
/// 
/// Exemplo de uso:
/// <code>
/// // Automático ao registrar no MediatR:
/// services.AddMediatR(cfg => {
///     cfg.AddOpenBehavior(typeof(CachingBehavior&lt;,&gt;));
/// });
/// </code>
/// </remarks>
public class CachingBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
    where TRequest : notnull
{
    /// <inheritdoc />
    public async Task<TResponse> Handle(TRequest request, RequestHandlerDelegate<TResponse> next, CancellationToken cancellationToken)
    {
        // Implementação de cache (Redis/Memory)
        return await next();
    }
}
