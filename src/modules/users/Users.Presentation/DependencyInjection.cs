using Microsoft.Extensions.DependencyInjection;

namespace Users.Presentation;

/// <summary>
/// Extensão para registrar os serviços de apresentação do módulo Users.
/// </summary>
public static class DependencyInjection
{
    public static IMvcBuilder AddUsersPresentationModule(this IMvcBuilder mvcBuilder)
    {
        // Registrar controllers do assembly
        mvcBuilder.AddApplicationPart(typeof(DependencyInjection).Assembly);

        return mvcBuilder;
    }
}
