namespace Bcommerce.BuildingBlocks.Domain.Exceptions;

/// <summary>
/// Exceção base para erros de domínio.
/// </summary>
/// <remarks>
/// Lançada quando regras de domínio são violadas.
/// - Base para BusinessRuleException e InvalidValueObjectException
/// - Capturada pelo middleware para retornar erro apropriado
/// - Use para erros irrecuperáveis de domínio
/// 
/// Exemplo de uso:
/// <code>
/// public class Produto
/// {
///     public void AplicarDesconto(decimal percentual)
///     {
///         if (percentual &gt; 50)
///             throw new DomainException("Desconto não pode exceder 50%");
///     }
/// }
/// </code>
/// </remarks>
public class DomainException : Exception
{
    public DomainException(string message) : base(message)
    {
    }

    public DomainException(string message, Exception innerException) : base(message, innerException)
    {
    }
}
