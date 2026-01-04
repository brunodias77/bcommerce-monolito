using BuildingBlocks.Application.Models;
using FluentValidation;
using MediatR;

namespace BuildingBlocks.Application.Behaviors;

/// <summary>
/// Pipeline behavior para validação automática de comandos e queries usando FluentValidation
///
/// Intercepta todas as requisições (comandos/queries) antes da execução
/// Executa validadores registrados no container de DI
/// Retorna erro de validação se houver falhas
///
/// Exemplos de validações baseadas no schema SQL:
///
/// CriarProdutoCommand:
/// - Nome não pode ser vazio (chk_products_name)
/// - Preço deve ser >= 0 (chk_products_price)
/// - Estoque deve ser >= 0 (chk_products_stock)
/// - SKU é obrigatório e único
///
/// CriarPedidoCommand:
/// - Endereço de entrega é obrigatório
/// - Pelo menos um item no pedido
/// - Total do pedido >= 0 (chk_orders_total)
/// - Desconto <= subtotal (chk_orders_discount)
///
/// ProcessarPagamentoCommand:
/// - Valor > 0 (chk_payments_amount)
/// - Parcelas entre 1 e 24 (chk_payments_installments)
/// - Método de pagamento válido
///
/// CriarCupomCommand:
/// - Código entre 3 e 50 caracteres (chk_coupon_code_format)
/// - Desconto percentual <= 100 (chk_coupon_percentage)
/// - Data fim > data início (chk_coupon_validity)
///
/// AdicionarItemCarrinhoCommand:
/// - Quantidade > 0 (chk_cart_items_quantity)
/// - Preço unitário >= 0 (chk_cart_items_price)
/// - Produto existe e está ativo
/// </summary>
/// <typeparam name="TRequest">Tipo da requisição</typeparam>
/// <typeparam name="TResponse">Tipo da resposta</typeparam>
public sealed class ValidationBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
    where TRequest : IRequest<TResponse>
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
        // Se não há validadores registrados, continua o pipeline
        if (!_validators.Any())
        {
            return await next();
        }

        // Cria o contexto de validação
        var context = new ValidationContext<TRequest>(request);

        // Executa todos os validadores em paralelo
        var validationResults = await Task.WhenAll(
            _validators.Select(v => v.ValidateAsync(context, cancellationToken)));

        // Coleta todas as falhas de validação
        var failures = validationResults
            .Where(r => !r.IsValid)
            .SelectMany(r => r.Errors)
            .ToList();

        // Se há falhas, retorna erro de validação
        if (failures.Any())
        {
            return CriarResultadoDeValidacao(failures);
        }

        // Se validação passou, continua o pipeline
        return await next();
    }

    /// <summary>
    /// Cria um Result de falha com os erros de validação
    /// </summary>
    private static TResponse CriarResultadoDeValidacao(
        IEnumerable<FluentValidation.Results.ValidationFailure> failures)
    {
        // Agrupa os erros em uma única mensagem
        var erros = failures
            .Select(f => $"{f.PropertyName}: {f.ErrorMessage}")
            .ToList();

        var mensagemErro = string.Join("; ", erros);

        var error = Error.Validation(
            "ERRO_VALIDACAO",
            $"Falha na validação: {mensagemErro}");

        // Cria uma instância de Result usando reflexão
        // Isso permite que funcione tanto para Result quanto Result<T>
        return (TResponse)(object)Result.Failure(error);
    }
}
