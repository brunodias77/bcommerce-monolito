using System.Security.Claims;

namespace Bcommerce.BuildingBlocks.Security.Extensions;

public static class ClaimsPrincipalExtensions
{
    public static Guid GetUserId(this ClaimsPrincipal principal)
    {
        var userId = principal.FindFirst("sub")?.Value;
        return Guid.TryParse(userId, out var result) ? result : Guid.Empty;
    }
}
