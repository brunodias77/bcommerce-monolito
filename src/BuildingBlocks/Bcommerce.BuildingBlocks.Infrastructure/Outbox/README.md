# Outbox Pattern Infrastructure

Este diretório contém a implementação do padrão **Transactional Outbox**, fundamental para garantir consistência eventual em sistemas distribuídos. Ele assegura que eventos de domínio sejam publicados se, e somente se, a transação de banco de dados for bem sucedida.

## `Models/OutboxMessage.cs`
**Responsabilidade:** Representar um evento que precisa ser enviado para fora do serviço.
**Por que existe:** Para garantir a atomicidade. Se salvarmos o `Pedido` e a `OutboxMessage` na mesma transação, ou ambos são salvos ou nenhum. Isso elimina o risco de salvar o pedido mas falhar ao enviar a mensagem para o RabbitMQ.
**Em que situação usar:** Criado automaticamente por interceptors ou serviços ao disparar um evento.
**O que pode dar errado:** Se o clock do servidor estiver errado, a ordem de eventos (`OccurredOnUtc`) pode ficar inconsistente ao serem consumidos.
**Exemplo real de uso:**
```csharp
var msg = new OutboxMessage {
    Type = domainEvent.GetType().AssemblyQualifiedName,
    Content = JsonConvert.SerializeObject(domainEvent)
};
```

---

## `Configuration/OutboxConfiguration.cs`
**Responsabilidade:** Mapeamento ORM da tabela de Outbox.
**Por que existe:** Para persistir as mensagens na tabela `OutboxMessages`.
**Em que situação usar:** Aplicado automaticamente pelo `BaseDbContext`.
**O que pode dar errado:** O tamanho da coluna `Content` deve ser suficiente para armazenar o JSON do maior evento possível do sistema.
**Exemplo real de uso:**
```csharp
builder.ToTable("OutboxMessages");
```

---

## `Repositories/IOutboxRepository.cs`
**Responsabilidade:** Contrato para adicionar mensagens ao Outbox.
**Por que existe:** Para que componentes de infraestrutura (como o `DomainEventInterceptor` ou `OutboxPublisher`) possam enfileirar mensagens sem depender do EF Core direto.
**Em que situação usar:** Injetado em classes que precisam garantir o envio futuro de uma mensagem.
**O que pode dar errado:** Usar este repositório fora de uma transação ativa. A mensagem será salva, mas perde-se o propósito de atomicidade com a operação de negócio principal.
**Exemplo real de uso:**
```csharp
await _outboxRepository.AddAsync(msg);
```

---

## `Repositories/OutboxRepository.cs`
**Responsabilidade:** Adicionar a mensagem ao `DbSet` do contexto atual.
**Por que existe:** Implementação concreta do acesso a dados.
**Em que situação usar:** Registrado no Container de DI.
**O que pode dar errado:** Similar ao Inbox, ele não chama `SaveChanges`. O commit deve vir do UnitOfWork principal.
**Exemplo real de uso:**
```csharp
services.AddScoped<IOutboxRepository, OutboxRepository>();
```

---

## `Processors/IOutboxPublisher.cs`
**Responsabilidade:** Interface marcadora/placeholder para o publicador do Outbox.
**Por que existe:** Para abstrair a lógica de conversão de `DomainEvent` -> `OutboxMessage`.
**Em que situação usar:** Em cenários onde se deseja publicar explicitamente para o outbox manualmente, em vez de usar interceptors automáticos.
**Exemplo real de uso:**
```csharp
public interface IOutboxPublisher { ... }
```

---

## `Processors/OutboxProcessor.cs`
**Responsabilidade:** Ler as mensagens pendentes do banco e enviá-las para o Message Broker (via `IPublisher` do MediatR ou direto para o Bus).
**Por que existe:** É o componente ativo que esvazia a fila. Sem ele, a tabela `OutboxMessages` apenas cresceria infinitamente e nenhum evento seria realmente enviado.
**Em que situação usar:** Invocado periodicamente pelo Job.
**O que pode dar errado:**
- **Duplicidade:** Se o processor falhar logo após enviar para o broker, mas antes de marcar como processado no banco, a mensagem será enviada novamente na próxima execução. ( consumidores devem ser idempotentes).
- **Ordem:** O processamento em paralelo ou em lote pode não garantir estritamente a ordem de envio FIFO se não houver um ordenamento rigoroso na query.
**Exemplo real de uso:**
```csharp
// Lê 20 msgs, publica e atualiza ProcessedOn
await _processor.ProcessAsync(ct);
```

---

## `BackgroundJobs/OutboxProcessorJob.cs`
**Responsabilidade:** Gatilho temporal (Timer) para rodar o processador.
**Por que existe:** Para transformar o processamento em uma tarefa contínua em background (Polling).
**Em que situação usar:** Configurado no Quartz.
**O que pode dar errado:** Intervalo de execução muito longo gera latência na entrega de eventos. Muito curto pode gerar overhead no banco (polling excessivo) se a tabela estiver vazia.
**Exemplo real de uso:**
```csharp
q.AddJob<OutboxProcessorJob>(...)
 .WithSimpleSchedule(x => x.WithIntervalInSeconds(10));
```
