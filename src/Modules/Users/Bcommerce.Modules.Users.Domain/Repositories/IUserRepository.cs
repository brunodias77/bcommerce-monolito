using Bcommerce.BuildingBlocks.Application.Abstractions.Data;
using Bcommerce.Modules.Users.Domain.Entities;

namespace Bcommerce.Modules.Users.Domain.Repositories;

public interface IUserRepository : IRepository<ApplicationUser>
{
    Task<ApplicationUser?> GetByEmailAsync(string email, CancellationToken cancellationToken = default);
    Task<bool> IsEmailUniqueAsync(string email, CancellationToken cancellationToken = default);
}
