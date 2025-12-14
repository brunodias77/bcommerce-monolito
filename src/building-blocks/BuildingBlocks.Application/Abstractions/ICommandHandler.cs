using BuildingBlocks.Application.Results;
using MediatR;

namespace BuildingBlocks.Application.Abstractions;

/// <summary>
/// Handler para comandos sem retorno de valor.
/// </summary>
/// <remarks>
/// O Handler é o coração da Camada de Aplicação (Application Layer).
/// Ele orquestra a execução do comando:
/// 1. Recebe o comando validado
/// 2. Carrega entidades via Repositórios
/// 3. Executa métodos de negócio nas entidades
/// 4. Persiste as mudanças via Repositórios/UnitOfWork
/// </remarks>
public interface ICommandHandler<in TCommand> : IRequestHandler<TCommand, Result>
    where TCommand : ICommand
{
}

/// <summary>
/// Handler para comandos com retorno de valor.
/// </summary>
/// <remarks>
/// Similar ao ICommandHandler, mas retorna dados (ex: Id gerado).
/// Útil quando a UI precisa de informações imediatas sobre o recurso criado.
/// </remarks>
public interface ICommandHandler<in TCommand, TResponse> : IRequestHandler<TCommand, Result<TResponse>>
    where TCommand : ICommand<TResponse>
{
}