# Observability Tracing

Este diretório contém a configuração para Rastreamento Distribuído (Distributed Tracing) utilizando **OpenTelemetry**.

## `TracingConfiguration.cs`
**Responsabilidade:** Definir as fontes de telemetria e instrumentações ativas para o rastreio.
**Por que existe:** Para garantir que todos os serviços rastreiem requisições HTTP (entrada e saída) e propaguem o contexto (W3C Trace Context) corretamente.
**Em que situação usar:** Chamado internamente pelas extensões de DI.
**O que pode dar errado:** Se a instrumentação de `HttpClient` for esquecida, a cadeia de rastreio quebrará ao chamar outro microsserviço (o TraceId não será enviado no header).
**Exemplo real de uso:**
```csharp
TracingConfiguration.ConfigureTracing(builder, "PaymentApi");
```

---

## `ActivityExtensions.cs`
**Responsabilidade:** Fornecer métodos auxiliares para enriquecer o `Activity` (Span) atual com tags, de forma segura.
**Por que existe:** O `System.Diagnostics.Activity` é a API nativa do .NET para Tracing. Este método evita exceções de *NullReference* ou verificações repetitivas ao tentar adicionar tags em atividades que podem ser nulas (se o tracing estiver desligado).
**Em que situação usar:** Dentro de regras de negócio ou handlers onde se deseja adicionar contexto extra ao trace (ex: OrderId, UserId).
**O que pode dar errado:** Adicionar dados sensíveis (PII) ou objetos gigantes como tags, sujando o trace e violando privacidade.
**Exemplo real de uso:**
```csharp
// Adiciona o ID do pedido ao trace atual para facilitar busca no Jaeger/Zipkin
Activity.Current?.SetTagIfPresent("order.id", request.Id);
```

---

## Extensions

### `ServiceCollectionExtensions.cs`
**Responsabilidade:** Registrar o OpenTelemetry Tracing no container de DI.
**Por que existe:** Simplifica a inicialização do tracing no `Program.cs`.
**Em que situação usar:** No startup da aplicação.
**Exemplo real de uso:**
```csharp
builder.Services.AddTracingServices("PaymentApi");
```
