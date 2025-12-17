using System.Security.Claims;

namespace Bcommerce.BuildingBlocks.Security.Services;

public interface ICurrentUserService
{
    Guid UserId { get; }
    ClaimsPrincipal? User { get; }
}
