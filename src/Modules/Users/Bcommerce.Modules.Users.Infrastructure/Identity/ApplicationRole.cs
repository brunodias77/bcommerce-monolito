using Microsoft.AspNetCore.Identity;

namespace Bcommerce.Modules.Users.Infrastructure.Identity;

public class ApplicationRole : IdentityRole<Guid>
{
    public ApplicationRole(string roleName) : base(roleName)
    {
    }
    
    public ApplicationRole() { }
}
