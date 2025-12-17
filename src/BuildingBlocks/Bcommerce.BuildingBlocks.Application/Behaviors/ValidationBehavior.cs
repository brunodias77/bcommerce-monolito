using Bcommerce.BuildingBlocks.Application.Abstractions.Messaging;
using FluentValidation;
using MediatR;
using ValidationException = Bcommerce.BuildingBlocks.Application.Exceptions.ValidationException;

namespace Bcommerce.BuildingBlocks.Application.Behaviors;

/// <summary>
/// Behavior do MediatR para validação automática de comandos (Fail-Fast).
/// </summary>
/// <remarks>
/// Executa validadores FluentValidation antes do handler.
/// - Coleta todos os erros de validação
/// - Lança ValidationException se houver erros (interrompe pipeline)
/// - Garante que o handler receba apenas dados válidos
/// 
/// Exemplo de uso:
/// <code>
/// // Defina o validador:
/// public class CriarProdutoValidator : AbstractValidator&lt;CriarProdutoCmd&gt; { ... }
/// 
/// // O Behavior valida automaticamente antes do Handler.
/// </code>
/// </remarks>
public class ValidationBehavior<TRequest, TResponse>(IEnumerable<IValidator<TRequest>> validators)
    : IPipelineBehavior<TRequest, TResponse>
    where TRequest : ICommand // Aplica validação apenas em Commands por padrão
{
    private readonly IEnumerable<IValidator<TRequest>> _validators = validators;

    /// <inheritdoc />
    public async Task<TResponse> Handle(TRequest request, RequestHandlerDelegate<TResponse> next, CancellationToken cancellationToken)
    {
        if (!_validators.Any())
        {
            return await next();
        }

        var context = new ValidationContext<TRequest>(request);

        var validationResults = await Task.WhenAll(
            _validators.Select(v => v.ValidateAsync(context, cancellationToken)));

        var failures = validationResults
            .Where(r => r.Errors.Any())
            .SelectMany(r => r.Errors)
            .ToList();

        if (failures.Any())
        {
            throw new ValidationException(failures);
        }

        return await next();
    }
}
