namespace BuildingBlocks.Infrastructure.Services;

/// <summary>
/// Interface para acessar informações do usuário autenticado atual.
/// </summary>
/// <remarks>
/// Este serviço abstrai o acesso ao usuário atual do HttpContext,
/// facilitando testes e mantendo a lógica de negócio desacoplada do ASP.NET Core.
///
/// Uso típico:
/// <code>
/// public class CreateOrderCommandHandler : ICommandHandler&lt;CreateOrderCommand, Guid&gt;
/// {
///     private readonly ICurrentUserService _currentUser;
///
///     public async Task&lt;Result&lt;Guid&gt;&gt; Handle(CreateOrderCommand command, CancellationToken ct)
///     {
///         if (!_currentUser.IsAuthenticated)
///             return Result.Fail&lt;Guid&gt;(Error.Unauthorized());
///
///         var order = Order.Create(_currentUser.UserId!.Value, command.Items);
///         // ...
///     }
/// }
/// </code>
///
/// Implementação em ASP.NET Core:
/// - Obtém dados do HttpContext.User (Claims)
/// - UserId vem do claim "sub" ou "userId"
/// - Email vem do claim "email"
/// - IsAuthenticated vem de HttpContext.User.Identity.IsAuthenticated
///
/// Implementação para testes:
/// - FakeCurrentUserService permite simular diferentes usuários
/// </remarks>
public interface ICurrentUserService
{
    /// <summary>
    /// ID do usuário autenticado (null se não autenticado).
    /// </summary>
    Guid? UserId { get; }

    /// <summary>
    /// Email do usuário autenticado (null se não autenticado).
    /// </summary>
    string? Email { get; }

    /// <summary>
    /// Indica se há um usuário autenticado.
    /// </summary>
    bool IsAuthenticated { get; }
}
