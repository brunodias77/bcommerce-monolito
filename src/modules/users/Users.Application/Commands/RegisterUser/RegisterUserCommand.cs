using BuildingBlocks.Application.Abstractions;

namespace Users.Application.Commands.RegisterUser;

/// <summary>
/// Comando para registrar um novo usuário.
/// Segue o padrão CQRS - operação de escrita.
/// </summary>
/// <param name="Email">Endereço de e-mail do usuário. Deve ser único no sistema.</param>
/// <param name="Password">Senha em texto plano. Será hasheada antes da persistência.</param>
/// <param name="FirstName">Primeiro nome do usuário (opcional).</param>
/// <param name="LastName">Sobrenome do usuário (opcional).</param>
public record RegisterUserCommand(
    string Email,
    string Password,
    string? FirstName,
    string? LastName
) : ICommand<Guid>;
