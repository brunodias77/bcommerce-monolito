namespace BuildingBlocks.Domain.Exceptions;

/// <summary>
/// Exceção base para erros de regra de negócio.
/// </summary>
/// <remarks>
/// Use DomainException quando uma regra de domínio for violada:
/// - Estoque insuficiente
/// - Cupom expirado
/// - Status de pedido inválido para operação
/// - CPF inválido
/// - Preço negativo
/// 
/// Não use para:
/// - Erros de infraestrutura (banco de dados, rede, etc.)
/// - Erros de validação de entrada (use FluentValidation)
/// - Erros de autorização
/// 
/// Exemplo de uso:
/// <code>
/// public class Product : AggregateRoot
/// {
///     public void ReserveStock(int quantity)
///     {
///         if (quantity &lt;= 0)
///             throw new DomainException("Quantity must be positive");
///             
///         var availableStock = Stock - ReservedStock;
///         if (availableStock &lt; quantity)
///             throw new InsufficientStockException(
///                 $"Insufficient stock. Available: {availableStock}, Requested: {quantity}");
///         
///         ReservedStock += quantity;
///     }
/// }
/// 
/// public class InsufficientStockException : DomainException
/// {
///     public InsufficientStockException(string message) : base(message) { }
/// }
/// </code>
/// </remarks>
public class DomainException : Exception
{
    /// <summary>
    /// Código de erro (opcional, útil para internacionalização).
    /// </summary>
    public string? ErrorCode { get; }

    public DomainException()
    {
    }

    public DomainException(string message) : base(message)
    {
    }

    public DomainException(string message, Exception innerException)
        : base(message, innerException)
    {
    }

    public DomainException(string message, string errorCode)
        : base(message)
    {
        ErrorCode = errorCode;
    }

    public DomainException(string message, string errorCode, Exception innerException)
        : base(message, innerException)
    {
        ErrorCode = errorCode;
    }
}
