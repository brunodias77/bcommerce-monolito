using Microsoft.AspNetCore.Authorization;

namespace Bcommerce.BuildingBlocks.Security.Authorization.Requirements;

public class ModuleAccessRequirement : IAuthorizationRequirement
{
    public string ModuleName { get; }

    public ModuleAccessRequirement(string moduleName)
    {
        ModuleName = moduleName;
    }
}
