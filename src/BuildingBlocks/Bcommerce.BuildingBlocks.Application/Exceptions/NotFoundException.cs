namespace Bcommerce.BuildingBlocks.Application.Exceptions;

public class NotFoundException(string name, object key) 
    : ApplicationException("Não Encontrado", $"A entidade \"{name}\" ({key}) não foi encontrada.")
{
}
