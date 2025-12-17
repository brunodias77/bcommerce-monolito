using Bcommerce.Modules.Users.Domain.Entities;
using Bcommerce.Modules.Users.Domain.Services;
using Microsoft.AspNetCore.Identity;

namespace Bcommerce.Modules.Users.Infrastructure.Identity;

public class IdentityService : IUserDomainService
{
    private readonly UserManager<ApplicationUser> _userManager;

    public IdentityService(UserManager<ApplicationUser> userManager)
    {
        _userManager = userManager;
    }

    public async Task<bool> IsEmailUniqueAsync(string email, CancellationToken cancellationToken = default)
    {
        return await _userManager.FindByEmailAsync(email) == null;
    }

    public async Task RegisterUserAsync(ApplicationUser user, string password, CancellationToken cancellationToken = default)
    {
        var result = await _userManager.CreateAsync(user, password);
        if (!result.Succeeded)
        {
            var errors = string.Join(", ", result.Errors.Select(e => e.Description));
            throw new ApplicationException($"User registration failed: {errors}");
        }
    }
}
