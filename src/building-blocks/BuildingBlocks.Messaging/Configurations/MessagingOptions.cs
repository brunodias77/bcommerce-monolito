namespace BuildingBlocks.Messaging.Configurations;

/// <summary>
/// Configurações do sistema de mensageria
/// </summary>
public sealed class MessagingOptions
{
    public const string SectionName = "Messaging";

    /// <summary>
    /// Define se deve usar o barramento em memória (para testes/dev)
    /// </summary>
    public bool UseInMemory { get; set; } = false;

    /// <summary>
    /// Configurações do RabbitMQ
    /// </summary>
    public RabbitMQOptions RabbitMQ { get; set; } = new();

    /// <summary>
    /// Configurações de política de retry
    /// </summary>
    public RetryPolicyOptions RetryPolicy { get; set; } = new();

    /// <summary>
    /// Configurações de prefetch
    /// </summary>
    public PrefetchOptions Prefetch { get; set; } = new();
}

public sealed class RabbitMQOptions
{
    public string Host { get; set; } = "localhost";
    public int Port { get; set; } = 5672;
    public string VirtualHost { get; set; } = "/";
    public string Username { get; set; } = "guest";
    public string Password { get; set; } = "guest";
    public bool UseSsl { get; set; } = false;
    public int Heartbeat { get; set; } = 60;
    public int RequestedConnectionTimeout { get; set; } = 30;
}

public sealed class RetryPolicyOptions
{
    public int MaxRetryCount { get; set; } = 3;
    public int InitialIntervalSeconds { get; set; } = 5;
    public int IntervalIncrementSeconds { get; set; } = 10;
    public int MaxIntervalSeconds { get; set; } = 60;
}

public sealed class PrefetchOptions
{
    public int PrefetchCount { get; set; } = 16;
    public int ConcurrentMessageLimit { get; set; } = 8;
}