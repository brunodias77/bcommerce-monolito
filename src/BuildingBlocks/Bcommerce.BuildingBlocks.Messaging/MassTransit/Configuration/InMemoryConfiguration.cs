using MassTransit;

namespace Bcommerce.BuildingBlocks.Messaging.MassTransit.Configuration;

/// <summary>
/// Configuração do MassTransit para uso em memória (Testes/Dev).
/// </summary>
/// <remarks>
/// Permite executar o barramento sem dependência de broker externo.
/// - Ideal para testes de integração ou ambiente local
/// - Simula a troca de mensagens em RAM
/// 
/// Exemplo de uso:
/// <code>
/// InMemoryConfiguration.ConfigureInMemory(context, cfg);
/// </code>
/// </remarks>
public static class InMemoryConfiguration
{
    public static void ConfigureInMemory(IBusRegistrationContext context, IInMemoryBusFactoryConfigurator cfg)
    {
        cfg.ConfigureEndpoints(context);
    }
}
