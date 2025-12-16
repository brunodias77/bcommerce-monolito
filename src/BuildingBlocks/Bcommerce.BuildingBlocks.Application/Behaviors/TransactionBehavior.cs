// Comentado para implementação futura. 
// Geralmente requer dependência de EF Core ou IDbTransaction que não temos aqui explicitamente
// ou uma abstração IUnitOfWork que dispara SaveChanges.
// Como IUnitOfWork já foi definido, podemos fazer uma implementação básica se o UnitOfWork estiver injetado.
// Mas normalmente o TransactionBehavior é específico da infraestrutura (EF Core).
// Vou criar a classe placeholder conforme estrutura solicitada.

using MediatR;

namespace Bcommerce.BuildingBlocks.Application.Behaviors;

public class TransactionBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
    where TRequest : notnull
{
    public async Task<TResponse> Handle(TRequest request, RequestHandlerDelegate<TResponse> next, CancellationToken cancellationToken)
    {
        // Implementação real depende da estratégia de transação (ex: EF Core Execution Strategy)
        // Aqui seria: begin transaction -> next() -> commit -> rollback on error
        return await next();
    }
}
