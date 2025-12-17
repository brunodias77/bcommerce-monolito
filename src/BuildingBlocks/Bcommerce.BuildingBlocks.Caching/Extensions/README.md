# Caching Extensions

Este diretório contém extensões para o `IServiceCollection` que facilitam a configuração e injeção de dependência dos serviços de cache.

## `ServiceCollectionExtensions.cs`
**Responsabilidade:** Centralizar a lógica de escolha e registro do provedor de cache (Redis vs Memory) e suas configurações.
**Por que existe:** Para abstrair a complexidade de configuração do StackExchange.Redis e fornecer um "fallback" gracioso para cache em memória em ambientes de desenvolvimento onde o Redis pode não estar disponível.
**Em que situação usar:** No `Program.cs` de cada microserviço, para habilitar o sistema de cache com uma única linha de código.
**O que pode dar errado:** Se a configuração `RedisSettings:ConnectionString` estiver presente mas apontar para um servidor inacessível, a aplicação pode falhar ao iniciar ou sofrer timeouts (dependendo da configuração de retry), pois ele tentará registrar o Redis.
**Exemplo real de uso:**
```csharp
// No Program.cs:
builder.Services.AddCachingServices(builder.Configuration);

/* Comporta-se de forma híbrida:
   1. Se appsettings.json tiver "RedisSettings:ConnectionString", usa Redis.
   2. Caso contrário, usa MemoryCache (In-Process) automaticamente.
*/
```
