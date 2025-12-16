namespace Bcommerce.BuildingBlocks.Application.Exceptions;

public class UnauthorizedException(string message) 
    : ApplicationException("Não Autorizado", message)
{
}
