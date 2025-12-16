using Users.Presentation;

namespace Bcommerce.Api.Configurations;

/// <summary>
/// Configuração de Dependency Injection para a camada Presentation.
/// </summary>
/// <remarks>
/// Registra:
/// - Controllers dos módulos via AddApplicationPart
/// - Configurações específicas de apresentação
/// </remarks>
public static class PresentationDependencyInjection
{
    public static IMvcBuilder AddPresentation(this IMvcBuilder mvcBuilder)
    {
        // ===============================================================
        // MÓDULOS DE APRESENTAÇÃO
        // ===============================================================
        // Como estamos usando Arquitetura Modular, os Controllers não estão no projeto API,
        // mas sim nos projetos de cada módulo (Presentation).
        // Precisamos registrar explicitamente cada módulo aqui.
        
        // Users Module: Registra Users.Presentation assembly
        mvcBuilder.AddUsersPresentationModule();

        // TODO: Adicione aqui os módulos de apresentação quando implementados:
        // mvcBuilder.AddCatalogPresentationModule();
        // mvcBuilder.AddOrdersPresentationModule();
        
        return mvcBuilder;
    }
}
