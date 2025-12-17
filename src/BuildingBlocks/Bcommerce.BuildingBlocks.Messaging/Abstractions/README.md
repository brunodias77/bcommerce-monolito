# Messaging Abstractions

Este diretório contém os contratos fundamentais para o sistema de mensageria da aplicação. Estas interfaces desacoplam o código de negócio da implementação concreta do Message Broker (RabbitMQ, ServiceBus, etc.).

## `IEventBus.cs`
**Responsabilidade:** Definir o contrato principal para comunicação com o barramento de eventos (Message Broker).
**Por que existe:** Para permitir que a aplicação publique mensagens sem conhecer a tecnologia subjacente de transporte.
**Em que situação usar:** Quando for necessário enviar uma mensagem bruta ou genérica para uma fila/tópico.
**O que pode dar errado:** Se a implementação concreta (ex: MassTransit ou RabbitMQ puro) não estiver registrada no DI, a injeção falhará.
**Exemplo real de uso:**
```csharp
await _eventBus.PublishAsync(new { Text = "Ping" }, cancellationToken);
```

---

## `IIntegrationEventHandler.cs`
**Responsabilidade:** Definir o contrato para classes que consomem e processam eventos de integração.
**Por que existe:** Para padronizar a assinatura dos métodos `Handle` e permitir a descoberta automática de consumidores (Auto-registration) via Reflection no Startup.
**Em que situação usar:** Ao criar um Consumer para reagir a um evento de outro microsserviço (ex: `PaymentConfirmedHandler`).
**O que pode dar errado:** Esquecer de implementar a interface fará com que o framework de mensageria não registre a classe como um consumidor, e o evento nunca será recebido.
**Exemplo real de uso:**
```csharp
public class PedidoPagoHandler : IIntegrationEventHandler<PedidoPagoEvent>
{
    public async Task Handle(PedidoPagoEvent @event, CancellationToken ct) { ... }
}
```

---

## `IIntegrationEventPublisher.cs`
**Responsabilidade:** Publicar especificamente eventos que implementam `IIntegrationEvent`.
**Por que existe:** Para diferenciar semanticamente a publicação de eventos internos (Domain Events/MediatR) de eventos externos (Integration Events/Broker).
**Em que situação usar:** Em Application Services ou Event Handlers que precisam notificar outros sistemas sobre uma mudança de estado.
**O que pode dar errado:** Confundir com `IPublisher` do MediatR. Este aqui manda para fora (Network), o do MediatR é in-process. O uso incorreto pode gerar erros de serialização se o objeto não for serializável.
**Exemplo real de uso:**
```csharp
await _integrationPublisher.Publish(new ClienteCadastradoIntegrationEvent(cliente.Id));
```
