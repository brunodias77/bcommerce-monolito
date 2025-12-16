namespace Bcommerce.BuildingBlocks.Domain.Exceptions;

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
