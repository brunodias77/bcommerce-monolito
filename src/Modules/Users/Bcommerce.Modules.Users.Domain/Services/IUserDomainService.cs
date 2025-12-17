using Bcommerce.Modules.Users.Domain.Entities;

namespace Bcommerce.Modules.Users.Domain.Services;

public interface IUserDomainService
{
    // Exemplo de serviço de domínio: validação complexa de cadastro que envolve regras de unicidade e políticas
    Task<bool> IsEmailUniqueAsync(string email, CancellationToken cancellationToken = default);
    Task RegisterUserAsync(ApplicationUser user, string password, CancellationToken cancellationToken = default);
}
