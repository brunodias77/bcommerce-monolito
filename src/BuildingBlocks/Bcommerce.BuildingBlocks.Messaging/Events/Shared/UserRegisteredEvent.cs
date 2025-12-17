namespace Bcommerce.BuildingBlocks.Messaging.Events.Shared;

/// <summary>
/// Evento disparado quando um novo usuário se registra no sistema.
/// </summary>
/// <remarks>
/// Notifica a criação de uma nova conta de cliente.
/// - Gatilho para envio de email de boas-vindas
/// - Criação de perfis em outros serviços (Fidelidade, etc)
/// 
/// Exemplo de uso:
/// <code>
/// new UserRegisteredEvent(user.Id, user.Email, user.Name, user.Surname);
/// </code>
/// </remarks>
public record UserRegisteredEvent(Guid UserId, string Email, string FirstName, string LastName) 
    : IntegrationEvent(Guid.NewGuid(), DateTime.UtcNow);
