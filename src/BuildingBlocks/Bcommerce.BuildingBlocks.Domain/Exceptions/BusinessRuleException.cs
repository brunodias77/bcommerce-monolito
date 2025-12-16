namespace Bcommerce.BuildingBlocks.Domain.Exceptions;

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
