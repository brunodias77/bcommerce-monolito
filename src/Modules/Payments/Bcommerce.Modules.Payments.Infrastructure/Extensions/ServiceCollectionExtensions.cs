using Bcommerce.BuildingBlocks.Infrastructure.Data;
using Bcommerce.Modules.Payments.Domain.Repositories;
using Bcommerce.Modules.Payments.Domain.Services;
using Bcommerce.Modules.Payments.Infrastructure.Gateways.MercadoPago;
using Bcommerce.Modules.Payments.Infrastructure.Gateways.Stripe;
using Bcommerce.Modules.Payments.Infrastructure.Persistence;
using Bcommerce.Modules.Payments.Infrastructure.Persistence.Repositories;
using Bcommerce.Modules.Payments.Infrastructure.Webhooks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Bcommerce.Modules.Payments.Infrastructure.Extensions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddPaymentsInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
         var connectionString = configuration.GetConnectionString("DefaultConnection");

        services.AddDbContext<PaymentsDbContext>((sp, options) =>
        {
            options.UseSqlServer(connectionString);
             options.AddInterceptors(sp.GetServices<Microsoft.EntityFrameworkCore.Diagnostics.IInterceptor>());
        });

        services.AddScoped<IPaymentRepository, PaymentRepository>();
        services.AddScoped<IPaymentMethodRepository, PaymentMethodRepository>();
        
        // Registering Strategy or Factory for Gateways would be better, but registering both for now
        // Typically one would be active based on config
        services.AddScoped<StripeGateway>();
        services.AddScoped<MercadoPagoGateway>();
        
        // Default gateway alias for now
        services.AddScoped<IPaymentGateway, StripeGateway>();

        services.AddScoped<IWebhookProcessor, WebhookProcessor>();

        return services;
    }
}
