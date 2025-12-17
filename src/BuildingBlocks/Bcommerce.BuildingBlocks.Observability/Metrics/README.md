# Observability Metrics

Este diretório contém a configuração e wrappers para instrumentação de métricas da aplicação usando **OpenTelemetry**.

## `MetricsConfiguration.cs`
**Responsabilidade:** Centralizar a configuração do provedor de métricas (MeterProvider).
**Por que existe:** Para garantir que todos os serviços exponham métricas padrão (Runtime, HTTP, ASP.NET) e customizadas de forma consistente.
**Em que situação usar:** Utilizado internamente pelas extensões de DI durante o startup.
**O que pode dar errado:** Se o nome da aplicação (`applicationName`) não for único ou padronizado, os dashboards do Prometheus/Grafana podem misturar dados de serviços diferentes.
**Exemplo real de uso:**
```csharp
MetricsConfiguration.ConfigureMetrics(builder, "OrderApi");
```

---

## Extensions

### `ServiceCollectionExtensions.cs`
**Responsabilidade:** Registrar o OpenTelemetry e as classes auxiliares de métricas no container de DI.
**Por que existe:** Facilita a configuração no `Program.cs` com uma única linha, garantindo que `BusinessMetrics` e `PerformanceMetrics` sejam singletons disponíveis para injeção.
**Em que situação usar:** No startup da aplicação.
**Exemplo real de uso:**
```csharp
builder.Services.AddMetricsServices("OrderApi");
```

---

## CustomMetrics

### `BusinessMetrics.cs`
**Responsabilidade:** Wrapper para criação facilitada de métricas de negócio (KPIs).
**Por que existe:** Abstrai a complexidade do objeto `Meter` nativo e centraliza a nomenclatura das métricas de domínio.
**Em que situação usar:** Injetado em Services ou Handlers para incrementar contadores (ex: "vendas_total", "erros_pagamento").
**O que pode dar errado:** Criar métricas com cardinalidade infinita (ex: usar UserID ou OrderID como label/tag da métrica), o que pode derrubar o sistema de monitoramento ppr excesso de memória.
**Exemplo real de uso:**
```csharp
public class OrderService(BusinessMetrics metrics) {
    public void Place() {
        metrics.CreateCounter("orders_placed").Add(1);
    }
}
```

### `PerformanceMetrics.cs`
**Responsabilidade:** Wrapper para criação de métricas técnicas e de performance não cobertas automaticamente.
**Por que existe:** Para instrumentar trechos de código específicos, como tempo de execução de um algoritmo complexo ou tamanho de filas internas.
**Em que situação usar:** Injetado onde é necessário medir latência fina ou uso de recursos específicos.
**Exemplo real de uso:**
```csharp
// Exemplo teórico (classe pode estar vazia inicialmente para expansão futura)
_perfMetrics.CreateHistogram("image_processing_seconds").Record(2.5);
```
