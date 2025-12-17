using System.Security.Claims;

namespace Bcommerce.BuildingBlocks.Security.Authentication;

public interface ITokenGenerator
{
    string GenerateToken(Guid userId, string firstName, string lastName, string email, IEnumerable<string> permissions, IEnumerable<string> roles);
}
