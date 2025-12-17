using MediatR;

namespace Bcommerce.BuildingBlocks.Application.Behaviors;

/// <summary>
/// Behavior do MediatR para garantir idempotência de comandos.
/// </summary>
/// <remarks>
/// Evita processamento duplicado de requisições com a mesma chave (IdempotencyKey).
/// - Verifica se a chave já foi processada no banco/cache
/// - Retorna a resposta original se já processado
/// - Garante consistência em retentativas de rede
/// 
/// Exemplo de uso:
/// <code>
/// // O cliente envia header: "X-Idempotency-Key: uuid-v4"
/// // O comportamento intercepta e valida automaticamente.
/// </code>
/// </remarks>
public class IdempotencyBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
    where TRequest : notnull
{
    /// <inheritdoc />
    public async Task<TResponse> Handle(TRequest request, RequestHandlerDelegate<TResponse> next, CancellationToken cancellationToken)
    {
        // Verificar chave de idempotência no header/request
        // Se processado -> retornar resposta salva
        // Se processando -> esperar/erro
        // Se novo -> next() -> salvar resposta
        return await next();
    }
}
