namespace Bcommerce.BuildingBlocks.Application.Exceptions;

public class ConflictException(string message) 
    : ApplicationException("Conflito", message)
{
}
