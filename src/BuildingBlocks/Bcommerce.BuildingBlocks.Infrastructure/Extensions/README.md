# Infrastructure Extensions

Este diretório contém métodos de extensão para `IServiceCollection`, responsáveis por centralizar e padronizar a Injeção de Dependência (DI) dos componentes de infraestrutura transversal.

## `ServiceCollectionExtensions.cs`
**Responsabilidade:** Registrar no container de DI todos os serviços essenciais dos Building Blocks, como Interceptors do EF Core, Jobs do Quartz, Repositórios de Auditoria e processadores de Inbox/Outbox.
**Por que existe:** Para manter o `Program.cs` da aplicação limpo e garantir que todos os microserviços/módulos subam a infraestrutura base da mesma forma. Em vez de registrar manualmente 20 serviços em cada API, chama-se apenas este método.
**Em que situação usar:** No startup da aplicação (Program.cs), logo após a configuração do banco de dados e antes do `Build()`.
**O que pode dar errado:** 
- Esquecer de chamar este método fará com que funcionalidades "invisíveis" como Auditoria e Outbox parem de funcionar silenciosamente (ou lancem erros de *Service Not Found*).
- Se a string de conexão passada estiver errada, o Quartz (que pode depender de banco) falhará ao iniciar.
**Exemplo real de uso:**
```csharp
// No Program.cs
var connectionString = builder.Configuration.GetConnectionString("Database");

builder.Services.AddDbContext<CatalogDbContext>(opts => ... );

// Registra infraestrutura transversal (Quartz, Audit, Interceptors)
builder.Services.AddBuildingBlocksInfrastructure(connectionString);
```
