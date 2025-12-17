namespace Bcommerce.BuildingBlocks.Domain.Exceptions;

/// <summary>
/// Exceção lançada quando um Value Object recebe valor inválido.
/// </summary>
/// <remarks>
/// Usada na criação de Value Objects para validar invariantes.
/// - Mensagem prefixada com "Valor inválido para o objeto de valor: "
/// - Garante que Value Objects são sempre válidos
/// - Lançada no construtor do Value Object
/// 
/// Exemplo de uso:
/// <code>
/// public class Email : ValueObject
/// {
///     public string Valor { get; }
///     
///     public Email(string valor)
///     {
///         if (!valor.Contains("@"))
///             throw new InvalidValueObjectException("Email inválido");
///         Valor = valor;
///     }
/// }
/// </code>
/// </remarks>
public class InvalidValueObjectException : DomainException
{
    public InvalidValueObjectException(string message) : base($"Valor inválido para o objeto de valor: {message}")
    {
    }

    public InvalidValueObjectException(string message, Exception innerException) 
        : base($"Valor inválido para o objeto de valor: {message}", innerException)
    {
    }
}
