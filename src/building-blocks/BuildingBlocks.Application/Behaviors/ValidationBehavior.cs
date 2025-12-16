using BuildingBlocks.Application.Results;
using FluentValidation;
using MediatR;
using Microsoft.Extensions.DependencyInjection;

namespace BuildingBlocks.Application.Behaviors;


/// <summary>
/// Behavior do MediatR para validação automática de requests usando FluentValidation.
/// </summary>
/// <remarks>
/// Este behavior:
/// 1. Injeta todos os validators registrados para o tipo de request
/// 2. Executa validação de todos os validators
/// 3. Se falhar: retorna Result.Fail com erros de validação
/// 4. Se passar: continua para o próximo behavior/handler
///
/// ## Ordem de Registro
///
/// IMPORTANTE: Este behavior deve ser registrado ANTES do TransactionBehavior
/// para evitar abrir transação para requests inválidos.
///
/// <code>
/// // Ordem correta:
/// services.AddLoggingBehavior();      // 1. Logging (mais externo)
/// services.AddValidationBehavior();   // 2. Validação (antes da transação)
/// services.AddTransactionBehavior();  // 3. Transação (mais interno)
/// </code>
///
/// ## Criando Validators
///
/// Use FluentValidation para criar validators:
/// <code>
/// public class CreateOrderCommandValidator : AbstractValidator&lt;CreateOrderCommand&gt;
/// {
///     public CreateOrderCommandValidator()
///     {
///         RuleFor(x => x.CustomerId)
///             .NotEmpty()
///             .WithMessage("Customer ID is required");
///     }
/// }
/// </code>
/// </remarks>
public class ValidationBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
    where TRequest : notnull
    where TResponse : Result
{
    private readonly IEnumerable<IValidator<TRequest>> _validators;

    public ValidationBehavior(IEnumerable<IValidator<TRequest>> validators)
    {
        _validators = validators;
    }

    public async Task<TResponse> Handle(
        TRequest request,
        RequestHandlerDelegate<TResponse> next,
        CancellationToken cancellationToken)
    {
        // Se não houver validators, prossegue
        if (!_validators.Any())
        {
            return await next();
        }

        // Cria contexto de validação
        var context = new ValidationContext<TRequest>(request);

        // Executa todos os validators em paralelo
        var validationResults = await Task.WhenAll(
            _validators.Select(v => v.ValidateAsync(context, cancellationToken)));

        // Coleta todos os erros
        var failures = validationResults
            .Where(r => !r.IsValid)
            .SelectMany(r => r.Errors)
            .Where(f => f != null)
            .ToList();

        // Se houver erros, retorna Result.Fail
        if (failures.Any())
        {
            var errorMessages = failures
                .Select(f => $"{f.PropertyName}: {f.ErrorMessage}")
                .ToList();

            var error = Error.Validation(
                "VALIDATION_ERROR",
                string.Join("; ", errorMessages));

            // Cria Result do tipo correto
            return CreateValidationErrorResult(error);
        }

        // Validação passou, continua para o próximo behavior/handler
        return await next();
    }


    /// <summary>
    /// Cria um Result de erro de validação do tipo correto (Result ou Result&lt;T&gt;).
    /// </summary>
    private static TResponse CreateValidationErrorResult(Error error)
    {
        // Se TResponse é Result<T>, cria Result<T>.Fail
        if (typeof(TResponse).IsGenericType &&
            typeof(TResponse).GetGenericTypeDefinition() == typeof(Result<>))
        {
            var valueType = typeof(TResponse).GetGenericArguments()[0];
            var failMethod = typeof(Result)
                .GetMethod(nameof(Result.Fail), 1, new[] { typeof(Error) })!
                .MakeGenericMethod(valueType);

            return (TResponse)failMethod.Invoke(null, new object[] { error })!;
        }

        // Caso contrário, é Result simples
        return (TResponse)(object)Result.Fail(error);
    }
}


/// <summary>
/// Extensões para facilitar registro do ValidationBehavior.
/// </summary>
public static class ValidationBehaviorExtensions
{
    /// <summary>
    /// Registra ValidationBehavior no pipeline do MediatR.
    /// </summary>
    /// <remarks>
    /// IMPORTANTE: Registre ANTES do TransactionBehavior.
    /// </remarks>
    public static IServiceCollection AddValidationBehavior(this IServiceCollection services)
    {
        services.AddScoped(typeof(IPipelineBehavior<,>), typeof(ValidationBehavior<,>));
        return services;
    }

    /// <summary>
    /// Registra ValidationBehavior e validators de um assembly.
    /// </summary>
    /// <param name="services">Service collection</param>
    /// <param name="assembly">Assembly contendo os validators</param>
    /// <param name="lifetime">Lifetime dos validators (default: Scoped)</param>
    public static IServiceCollection AddValidationBehavior(
        this IServiceCollection services,
        System.Reflection.Assembly assembly,
        ServiceLifetime lifetime = ServiceLifetime.Scoped)
    {
        // Registra o behavior
        services.AddScoped(typeof(IPipelineBehavior<,>), typeof(ValidationBehavior<,>));

        // Registra validators do assembly
        services.AddValidatorsFromAssembly(assembly, lifetime);

        return services;
    }
}
