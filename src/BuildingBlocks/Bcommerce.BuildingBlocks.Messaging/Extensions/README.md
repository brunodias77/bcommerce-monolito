# Messaging Extensions

Este diretório contém extensões para facilitar a configuração do sistema de mensageria (Message Broker) na inicialização da aplicação.

## `ServiceCollectionExtensions.cs`
**Responsabilidade:** Registrar e configurar o MassTransit com RabbitMQ no container de Injeção de Dependência.
**Por que existe:** A configuração do MassTransit é verbosa e cheia de detalhes (host, credenciais, formatadores de nome, retry, filtros). Este método encapsula tudo em uma única chamada padrão para todos os serviços.
**Em que situação usar:** No `Program.cs` de qualquer serviço que precise publicar ou consumir mensagens.
**O que pode dar errado:** 
- Se a ConnectionString do RabbitMQ não estiver no `appsettings.json`, ele tentará conectar em `localhost:5672` (o que pode funcionar em dev, mas falhar em prod/docker).
- Se não passar o assembly correto em `consumersAssembly`, os Consumers (Event Handlers) não serão registrados e o serviço ficará "surdo".
**Exemplo real de uso:**
```csharp
// No Program.cs
builder.Services.AddMessageBroker(
    builder.Configuration, 
    typeof(Program).Assembly // Passa o assembly atual para scaneamento de Consumers
);
```
