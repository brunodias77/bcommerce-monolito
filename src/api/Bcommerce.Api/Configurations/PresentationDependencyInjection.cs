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
        // Módulos de Apresentação
        // ===============================================================
        
        // Users Module
        mvcBuilder.AddUsersPresentationModule();

        // Adicione aqui os módulos de apresentação quando implementados:
        // mvcBuilder.AddCatalogPresentationModule();
        // mvcBuilder.AddOrdersPresentationModule();
        // mvcBuilder.AddPaymentsPresentationModule();
        // mvcBuilder.AddShippingPresentationModule();
        // mvcBuilder.AddReviewsPresentationModule();

        return mvcBuilder;
    }
}
