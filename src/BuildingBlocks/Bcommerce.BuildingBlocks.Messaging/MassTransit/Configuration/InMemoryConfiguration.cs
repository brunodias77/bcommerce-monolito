using MassTransit;

namespace Bcommerce.BuildingBlocks.Messaging.MassTransit.Configuration;

public static class InMemoryConfiguration
{
    public static void ConfigureInMemory(IBusRegistrationContext context, IInMemoryBusFactoryConfigurator cfg)
    {
        cfg.ConfigureEndpoints(context);
    }
}
