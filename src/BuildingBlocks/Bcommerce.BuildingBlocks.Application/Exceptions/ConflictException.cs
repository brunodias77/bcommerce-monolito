namespace Bcommerce.BuildingBlocks.Application.Exceptions;

/// <summary>
/// Exceção para conflitos de estado (HTTP 409).
/// </summary>
/// <remarks>
/// Indica que a operação não pode ser completada devido ao estado atual do recurso.
/// - Violação de chaves únicas (Unique Key)
/// - Concorrência otimista (versão desatualizada)
/// - Regras de negócio impeditivas
/// 
/// Exemplo de uso:
/// <code>
/// if (existeClienteComCpf)
///     throw new ConflictException("CPF já cadastrado.");
/// </code>
/// </remarks>
public class ConflictException(string message) 
    : ApplicationException("Conflito", message)
{
}
