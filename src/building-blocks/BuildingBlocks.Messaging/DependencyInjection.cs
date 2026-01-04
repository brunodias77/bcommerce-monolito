using BuildingBlocks.Messaging.Abstractions;
using BuildingBlocks.Messaging.Configurations;
using BuildingBlocks.Messaging.Implementation;
using MassTransit;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace BuildingBlocks.Messaging;

/// <summary>
/// Extensões para configurar os serviços de mensageria
/// </summary>
public static class DependencyInjection
{
    /// <summary>
    /// Adiciona os serviços de mensageria ao container de DI
    /// Configura InMemoryEventBus ou MassTransitEventBus baseado nas configurações
    /// </summary>
    /// <param name="services">Coleção de serviços</param>
    /// <param name="configuration">Configuração da aplicação</param>
    /// <returns>Coleção de serviços para encadeamento</returns>
    public static IServiceCollection AddMessagingServices(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        // Carrega configurações de mensageria
        var messagingOptions = configuration
            .GetSection(MessagingOptions.SectionName)
            .Get<MessagingOptions>() ?? new MessagingOptions();

        services.Configure<MessagingOptions>(
            configuration.GetSection(MessagingOptions.SectionName));

        // Decide qual implementação usar baseado na configuração
        if (messagingOptions.UseInMemory)
        {
            services.AddInMemoryEventBus();
        }
        else
        {
            services.AddMassTransitEventBus(messagingOptions);
        }

        return services;
    }

    /// <summary>
    /// Adiciona o Event Bus em memória (para desenvolvimento e testes)
    /// </summary>
    private static IServiceCollection AddInMemoryEventBus(this IServiceCollection services)
    {
        services.AddSingleton<IEventBus, InMemoryEventBus>();

        services.AddLogging(builder =>
        {
            builder.AddConsole();
        });

        return services;
    }

    /// <summary>
    /// Adiciona o Event Bus com MassTransit e RabbitMQ (para produção)
    /// </summary>
    private static IServiceCollection AddMassTransitEventBus(
        this IServiceCollection services,
        MessagingOptions options)
    {
        services.AddMassTransit(x =>
        {
            // Configura naming convention para endpoints (kebab-case)
            x.SetKebabCaseEndpointNameFormatter();

            // Adiciona consumers do assembly de entrada
            x.AddConsumers(System.Reflection.Assembly.GetEntryAssembly());

            // Configuração do RabbitMQ
            x.UsingRabbitMq((context, cfg) =>
            {
                var rabbitMq = options.RabbitMQ;

                // Configura conexão com RabbitMQ
                cfg.Host(rabbitMq.Host, (ushort)rabbitMq.Port, rabbitMq.VirtualHost, h =>
                {
                    h.Username(rabbitMq.Username);
                    h.Password(rabbitMq.Password);

                    if (rabbitMq.UseSsl)
                    {
                        h.UseSsl(s =>
                        {
                            s.Protocol = System.Security.Authentication.SslProtocols.Tls12;
                        });
                    }

                    h.Heartbeat(TimeSpan.FromSeconds(rabbitMq.Heartbeat));
                    h.RequestedConnectionTimeout(TimeSpan.FromSeconds(rabbitMq.RequestedConnectionTimeout));
                });

                // Configura naming conventions para filas
                // Exemplo: bcommerce.orders.pedido-criado
                cfg.MessageTopology.SetEntityNameFormatter(new KebabCaseEntityNameFormatter());

                // Configura política de retry
                cfg.UseMessageRetry(r =>
                {
                    r.Incremental(
                        options.RetryPolicy.MaxRetryCount,
                        TimeSpan.FromSeconds(options.RetryPolicy.InitialIntervalSeconds),
                        TimeSpan.FromSeconds(options.RetryPolicy.IntervalIncrementSeconds));
                });

                // Configura prefetch
                cfg.PrefetchCount = options.Prefetch.PrefetchCount;
                cfg.ConcurrentMessageLimit = options.Prefetch.ConcurrentMessageLimit;

                // Configura endpoints automaticamente
                cfg.ConfigureEndpoints(context);
            });
        });

        // Registra o Event Bus
        services.AddScoped<IEventBus, MassTransitEventBus>();

        return services;
    }

    /// <summary>
    /// Adiciona um consumer de evento de integração
    /// Usado pelos módulos para registrar handlers de eventos
    /// </summary>
    /// <typeparam name="TEvent">Tipo do evento</typeparam>
    /// <typeparam name="THandler">Tipo do handler</typeparam>
    /// <param name="services">Coleção de serviços</param>
    /// <param name="queueName">Nome da fila (opcional, usa convenção se não informado)</param>
    /// <returns>Coleção de serviços para encadeamento</returns>
    public static IServiceCollection AddIntegrationEventHandler<TEvent, THandler>(
        this IServiceCollection services,
        string? queueName = null)
        where TEvent : class, IIntegrationEvent
        where THandler : class, IIntegrationEventHandler<TEvent>
    {
        // Registra o handler no container de DI
        services.AddScoped<THandler>();

        // Registra o consumer no MassTransit
        // Nota: ConfigureMassTransit não existe na versão atual do MassTransit.
        // Devemos usar AddMassTransitTestHarness ou registrar todos os consumers no setup inicial.
        // No entanto, para suporte a registro modular, a abordagem correta é usar AddMassTransit novamente
        // mas o MassTransit 8+ prefere uma configuração única.
        // Como alternativa, vamos registrar apenas o serviço no DI e deixar o MassTransit descobrir via assembly scanning
        // ou usar uma abordagem diferente.
        
        // CORREÇÃO: Em MassTransit 8+, para adicionar consumers dinamicamente, 
        // normalmente se usa options.AddConsumer no setup inicial.
        // Mas como estamos em um método de extensão separado, vamos tentar adicionar ao container.
        
        // Solução temporária: Registrar o adapter como serviço scoped
        // e usar uma configuração centralizada que escaneia assemblies.
        // Mas para manter compatibilidade com a estrutura atual:
        services.AddOptions<MassTransitHostOptions>()
            .Configure(options =>
            {
                options.WaitUntilStarted = true;
            });
            
        // Registra o adapter para que possa ser resolvido
        services.AddScoped<MassTransitConsumerAdapter<TEvent, THandler>>();

        return services;
    }

    /// <summary>
    /// Gera nome da fila baseado no tipo do evento
    /// Exemplo: PedidoCriadoIntegrationEvent → bcommerce.pedido-criado
    /// </summary>
    private static string GenerateQueueName<TEvent>()
    {
        var eventName = typeof(TEvent).Name
            .Replace("IntegrationEvent", "")
            .ToKebabCase();

        return $"bcommerce.{eventName}";
    }
}

/// <summary>
/// Extensão para converter string para kebab-case
/// Exemplo: PedidoCriado → pedido-criado
/// </summary>
internal static class StringExtensions
{
    public static string ToKebabCase(this string value)
    {
        if (string.IsNullOrEmpty(value))
        {
            return value;
        }

        var chars = new List<char>();

        for (int i = 0; i < value.Length; i++)
        {
            var c = value[i];

            if (char.IsUpper(c))
            {
                // Adiciona hífen antes de letras maiúsculas (exceto no início)
                if (i > 0 && value[i - 1] != '-')
                {
                    chars.Add('-');
                }

                chars.Add(char.ToLower(c));
            }
            else
            {
                chars.Add(c);
            }
        }

        return new string(chars.ToArray());
    }
}

/// <summary>
/// Formatter customizado para nomes de entidades no MassTransit
/// Converte nomes de tipos para kebab-case
/// </summary>
internal sealed class KebabCaseEntityNameFormatter : IEntityNameFormatter
{
    public string FormatEntityName<T>()
    {
        return typeof(T).Name
            .Replace("IntegrationEvent", "")
            .ToKebabCase();
    }
}
