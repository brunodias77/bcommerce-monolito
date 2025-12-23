# Messaging Events (Shared Contract)

Este diretório define os contratos de mensagens (Eventos de Integração) que representam fatos ocorridos no passado. Estes eventos formam a API pública assíncrona da aplicação monolítica, permitindo comunicação desacoplada entre módulos ou serviços externos.

## Core

### `IntegrationEvent.cs`
**Responsabilidade:** Classe base para todos os eventos de integração.
**Por que existe:** Para impor uma estrutura comum (Envelope) a todas as mensagens, garantindo que tenham ID único, data de ocorrência e tipo.
**Em que situação usar:** Deve ser herdada por qualquer Record que represente um evento de integração.
**O que pode dar errado:** Tentar instanciar diretamente (é abstrata) ou herdar em classes mutáveis (deve ser imutável/Record).
**Exemplo real de uso:**
```csharp
public record MeuEvento(string Dado) : IntegrationEvent(Guid.NewGuid(), DateTime.UtcNow);
```

---

## Shared Events (Contratos Globais)

### `OrderPlacedEvent.cs`
**Responsabilidade:** Sinalizar que um cliente finalizou um pedido com sucesso na loja.
**Por que existe:** É o evento central do E-commerce. Desperta processos de Estoque (Reserva), Pagamento e Notificação.
**Em que situação usar:** Publicado imediatamente após a criação do registro de Order no banco.
**Exemplo real de uso:**
```csharp
// Módulo Vendas -> Módulo Estoque
await _bus.Publish(new OrderPlacedEvent(orderId, userId, 150.00m));
```

### `OrderStatusChangedEvent.cs`
**Responsabilidade:** Notificar mudanças no ciclo de vida do pedido (ex: Pago, Enviado, Cancelado).
**Por que existe:** Para permitir que sistemas de notificação (Email/SMS) e Tracking informem o cliente sem acoplamento direto com a máquina de estados do pedido.
**Exemplo real de uso:**
```csharp
// Módulo Vendas -> Módulo Notificações
// De: AwaitingPayment Para: Paid
await _bus.Publish(new OrderStatusChangedEvent(orderId, "Paid", "AwaitingPayment"));
```

### `PaymentCompletedEvent.cs`
**Responsabilidade:** Confirmar que a transação financeira foi aprovada.
**Por que existe:** Para liberar processos logísticos e contáveis que dependem da garantia do dinheiro.
**Em que situação usar:** Publicado pelo Webhook do Gateway de Pagamento ou pelo serviço de Pagamentos.
**Exemplo real de uso:**
```csharp
// Módulo Pagamentos -> Módulo Vendas
await _bus.Publish(new PaymentCompletedEvent(paymentId, orderId, 150.00m));
```

### `PaymentFailedEvent.cs`
**Responsabilidade:** Informar falha ou recusa no pagamento.
**Por que existe:** Para permitir ações de recuperação (enviar email pedindo outro cartão) ou cancelamento automático de reservas.
**Exemplo real de uso:**
```csharp
// Módulo Pagamentos -> Módulo Vendas
await _bus.Publish(new PaymentFailedEvent(payId, orderId, "Saldo Insuficiente"));
```

### `StockReservedEvent.cs`
**Responsabilidade:** Confirmar que os itens do pedido foram "segurados" no estoque.
**Por que existe:** Para garantir que, se o pagamento passar, o produto estará lá. Evita "Venda de ar".
**Em que situação usar:** Publicado pelo módulo de Estoque após validar disponibilidade e decrementar saldo virtual.
**Exemplo real de uso:**
```csharp
// Módulo Estoque -> Módulo Vendas
await _bus.Publish(new StockReservedEvent(orderId, prodId, 5));
```

### `StockReleasedEvent.cs`
**Responsabilidade:** Devolver itens ao estoque disponível.
**Por que existe:** Para desfazer uma `StockReservedEvent` caso o pedido seja cancelado ou o pagamento falhe (Compensação/Saga).
**Exemplo real de uso:**
```csharp
// Módulo Estoque -> Módulo Vendas
await _bus.Publish(new StockReleasedEvent(orderId, prodId, 5));
```

### `CartConvertedEvent.cs`
**Responsabilidade:** Sinalizar que um carrinho virou pedido (Checkout).
**Por que existe:** Útil para Analytics (Funil de Vendas) e para limpar o carrinho do banco (delete lógico ou físico).
**Exemplo real de uso:**
```csharp
// Módulo Vendas -> Módulo Carrinho
await _bus.Publish(new CartConvertedEvent(cartId, orderId, userId));
```

### `CouponUsedEvent.cs`
**Responsabilidade:** Avisar que um cupom de desconto foi efetivamente consumido.
**Por que existe:** Para atualizar o contador de usos do cupom (ex: "apenas os 100 primeiros") e evitar reutilização fraudulenta.
**Exemplo real de uso:**
```csharp
// Módulo Vendas -> Módulo Descontos
await _bus.Publish(new CouponUsedEvent(couponId, orderId, userId));
```

### `ProductPublishedEvent.cs`
**Responsabilidade:** Replicar dados mestres de produto.
**Por que existe:** Para manter sincronizados os dados de produto em outros serviços (como o serviço de Busca ou Carrinho) que guardam cópias (Cache/Réplica) para performance.
**Exemplo real de uso:**
```csharp
// Módulo Catálogo -> Módulo Busca
await _bus.Publish(new ProductPublishedEvent(p.Id, "iPhone 15", 9999m, "SKU123"));
```

### `UserRegisteredEvent.cs`
**Responsabilidade:** Notificar novo cadastro de cliente.
**Por que existe:** Para criar "perfis de sombra" em outros módulos (Fidelidade, CRM) ou enviar Email de Boas-vindas.
**Exemplo real de uso:**
```csharp
// Módulo Identidade -> Módulo CRM
await _bus.Publish(new UserRegisteredEvent(userId, "email@teste.com", "John", "Doe"));
```
