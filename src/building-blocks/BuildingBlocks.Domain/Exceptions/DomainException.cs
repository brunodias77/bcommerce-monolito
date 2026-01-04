namespace BuildingBlocks.Domain.Exceptions;

/// <summary>
/// Exceção base para todas as exceções de domínio
///
/// Exceções de domínio representam erros que ocorrem na camada de domínio
/// devido a violações de regras de negócio ou estados inválidos
///
/// Exemplos de uso baseados no schema SQL:
///
/// Catálogo:
/// - Produto com SKU duplicado
/// - Categoria com nome duplicado no mesmo nível
/// - Tentativa de definir categoria como seu próprio pai
/// - Estoque insuficiente para reserva
///
/// Pedidos:
/// - Pedido já foi pago e não pode ser cancelado
/// - Transição de status inválida
/// - Total do pedido não corresponde à soma dos itens
///
/// Pagamentos:
/// - Pagamento já foi processado
/// - Valor de reembolso maior que o valor pago
/// - Número de parcelas inválido
///
/// Cupons:
/// - Cupom expirado
/// - Limite de uso atingido
/// - Compra mínima não atingida
/// - Cupom não aplicável aos produtos do carrinho
///
/// Carrinho:
/// - Carrinho já foi convertido em pedido
/// - Item já existe no carrinho
/// - Produto não está mais disponível
/// </summary>
public class DomainException : Exception
{
    /// <summary>
    /// Cria uma nova exceção de domínio
    /// </summary>
    public DomainException()
    {
    }

    /// <summary>
    /// Cria uma nova exceção de domínio com mensagem
    /// </summary>
    /// <param name="message">Mensagem de erro</param>
    public DomainException(string message) : base(message)
    {
    }

    /// <summary>
    /// Cria uma nova exceção de domínio com mensagem e exceção interna
    /// </summary>
    /// <param name="message">Mensagem de erro</param>
    /// <param name="innerException">Exceção interna</param>
    public DomainException(string message, Exception innerException)
        : base(message, innerException)
    {
    }
}