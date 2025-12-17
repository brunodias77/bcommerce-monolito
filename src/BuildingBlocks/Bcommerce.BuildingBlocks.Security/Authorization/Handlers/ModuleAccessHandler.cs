using Bcommerce.BuildingBlocks.Security.Authorization.Requirements;
using Microsoft.AspNetCore.Authorization;

namespace Bcommerce.BuildingBlocks.Security.Authorization.Handlers;

public class ModuleAccessHandler : AuthorizationHandler<ModuleAccessRequirement>
{
    protected override Task HandleRequirementAsync(AuthorizationHandlerContext context, ModuleAccessRequirement requirement)
    {
        // Example implementation: Check if user has a role or claim for the module
        if (context.User.HasClaim(c => c.Type == "modules" && c.Value == requirement.ModuleName))
        {
            context.Succeed(requirement);
        }

        return Task.CompletedTask;
    }
}
