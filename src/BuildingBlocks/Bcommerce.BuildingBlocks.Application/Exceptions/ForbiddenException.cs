namespace Bcommerce.BuildingBlocks.Application.Exceptions;

public class ForbiddenException(string message) 
    : ApplicationException("Proibido", message)
{
}
