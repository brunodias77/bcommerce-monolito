namespace Bcommerce.BuildingBlocks.Domain.Exceptions;

/// <summary>
/// Exceção lançada quando uma regra de negócio é violada.
/// </summary>
/// <remarks>
/// Especialização de DomainException para regras de negócio.
/// - Mensagem automaticamente prefixada com "Regra de negócio violada: "
/// - Indica erro de lógica de negócio, não de infraestrutura
/// - Deve resultar em HTTP 400 ou 422
/// 
/// Exemplo de uso:
/// <code>
/// public class Pedido
/// {
///     public void Cancelar()
///     {
///         if (Status == StatusPedido.Enviado)
///             throw new BusinessRuleException("Não é possível cancelar pedido já enviado");
///         
///         Status = StatusPedido.Cancelado;
///     }
/// }
/// </code>
/// </remarks>
public class BusinessRuleException : DomainException
{
    public BusinessRuleException(string message) : base($"Regra de negócio violada: {message}")
    {
    }

    public BusinessRuleException(string message, Exception innerException) 
        : base($"Regra de negócio violada: {message}", innerException)
    {
    }
}
