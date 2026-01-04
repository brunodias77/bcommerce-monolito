namespace BuildingBlocks.Domain.Exceptions;

/// <summary>
/// Exceção lançada quando um recurso não é encontrado
///
/// Exemplos de uso baseados no schema SQL:
///
/// Catálogo:
/// - Produto não encontrado pelo ID ou SKU
/// - Categoria não encontrada
/// - Marca não encontrada
/// - Avaliação de produto não encontrada
///
/// Usuários:
/// - Usuário não encontrado
/// - Perfil não encontrado
/// - Endereço não encontrado
/// - Sessão não encontrada
/// - Notificação não encontrada
///
/// Pedidos:
/// - Pedido não encontrado pelo ID ou número do pedido
/// - Item do pedido não encontrado
/// - Nota fiscal não encontrada
/// - Evento de rastreamento não encontrado
///
/// Pagamentos:
/// - Pagamento não encontrado
/// - Método de pagamento não encontrado
/// - Transação não encontrada
/// - Reembolso não encontrado
///
/// Cupons:
/// - Cupom não encontrado pelo código ou ID
/// - Uso de cupom não encontrado
/// - Reserva de cupom não encontrada
///
/// Carrinho:
/// - Carrinho não encontrado
/// - Item do carrinho não encontrado
/// - Carrinho salvo não encontrado
/// </summary>
public sealed class NotFoundException : DomainException
{
    /// <summary>
    /// Nome do recurso que não foi encontrado
    /// </summary>
    public string ResourceName { get; }

    /// <summary>
    /// Identificador do recurso que não foi encontrado
    /// </summary>
    public object ResourceId { get; }

    /// <summary>
    /// Cria uma exceção de recurso não encontrado
    /// </summary>
    /// <param name="resourceName">Nome do recurso (ex: "Produto", "Pedido", "Usuário")</param>
    /// <param name="resourceId">Identificador do recurso</param>
    public NotFoundException(string resourceName, object resourceId)
        : base($"{resourceName} com identificador '{resourceId}' não foi encontrado.")
    {
        ResourceName = resourceName;
        ResourceId = resourceId;
    }

    /// <summary>
    /// Cria uma exceção de recurso não encontrado com mensagem customizada
    /// </summary>
    /// <param name="resourceName">Nome do recurso</param>
    /// <param name="resourceId">Identificador do recurso</param>
    /// <param name="message">Mensagem customizada</param>
    public NotFoundException(string resourceName, object resourceId, string message)
        : base(message)
    {
        ResourceName = resourceName;
        ResourceId = resourceId;
    }

    /// <summary>
    /// Cria uma exceção de recurso não encontrado com inner exception
    /// </summary>
    /// <param name="resourceName">Nome do recurso</param>
    /// <param name="resourceId">Identificador do recurso</param>
    /// <param name="innerException">Exceção interna</param>
    public NotFoundException(string resourceName, object resourceId, Exception innerException)
        : base($"{resourceName} com identificador '{resourceId}' não foi encontrado.", innerException)
    {
        ResourceName = resourceName;
        ResourceId = resourceId;
    }

    public override string ToString()
    {
        return $"{base.ToString()}\nRecurso: {ResourceName}\nIdentificador: {ResourceId}";
    }
}
