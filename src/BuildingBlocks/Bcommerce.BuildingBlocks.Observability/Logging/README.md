# Observability Logging

Este diretório contém a configuração e extensão do sistema de logs da aplicação, utilizando **Serilog** como biblioteca principal para log estruturado.

## `LoggingConfiguration.cs`
**Responsabilidade:** Definir o pipeline de logging padrão para todos os microsserviços.
**Por que existe:** Para garantir uniformidade nos logs (formato, sinks, níveis mínimos) sem precisar duplicar configuração em cada `Program.cs`.
**Em que situação usar:** Chamado no startup do Host.
**O que pode dar errado:** Se o `applicationName` não for passado corretamente, ficará difícil distinguir os logs de diferentes serviços em um agregador (Elastic/Seq).
**Exemplo real de uso:**
```csharp
LoggingConfiguration.ConfigureLogging(builder.Host, "Bcommerce.Catalog.API");
```

---

## Extensions

### `ServiceCollectionExtensions.cs`
**Responsabilidade:** Registrar dependências necessárias para o funcionamento dos Enrichers customizados.
**Por que existe:** Enrichers como `UserContextEnricher` precisam de acesso ao `IHttpContextAccessor`. Este método garante que essas dependências estejam no DI.
**Em que situação usar:** No startup da aplicação.
**Exemplo real de uso:**
```csharp
builder.Services.AddLoggingServices();
```

---

## SerilogEnrichers

### `CorrelationIdEnricher.cs`
**Responsabilidade:** Extrair o `TraceIdentifier` do request atual e anexá-lo a cada linha de log como propriedade `CorrelationId`.
**Por que existe:** Para permitir rastrear todo o ciclo de vida de uma requisição, mesmo quando o log ocorre em camadas profundas que não têm acesso ao HttpContext.
**Em que situação usar:** Configurado automaticamente no `LoggingConfiguration`.
**O que pode dar errado:** Se usado fora de um contexto HTTP (ex: Background Job), o `HttpContext` será nulo e a propriedade não será adicionada (o que é o comportamento esperado, mas bom saber).
**Exemplo real de uso:**
```csharp
// Log resultante:
// { "Message": "Erro processando", "CorrelationId": "0HLX123..." }
```

### `UserContextEnricher.cs`
**Responsabilidade:** Adicionar `UserId` e `UserName` ao contexto do log se o usuário estiver autenticado.
**Por que existe:** Fundamental para auditoria e troubleshooting. Permite responder perguntas como "Quais erros o usuário X encontrou hoje?".
**Em que situação usar:** Configurado automaticamente. Funciona melhor com autenticação JWT.
**Exemplo real de uso:**
```csharp
// Log resultante:
// { "Uri": "/checkout", "UserId": "guido-guid", "UserName": "admin@loja.com" }
```
