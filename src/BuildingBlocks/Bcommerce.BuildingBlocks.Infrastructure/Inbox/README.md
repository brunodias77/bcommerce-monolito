# Inbox Pattern Infrastructure

Este diretório contém a implementação do padrão **Inbox**, projetado para garantir a idempotência e o processamento confiável de mensagens de integração (Integration Events) recebidas de outros serviços/módulos.

## `Models/InboxMessage.cs`
**Responsabilidade:** Representar a mensagem (evento) recebida que precisa ser processada.
**Por que existe:** Serve como um buffer persistente para garantir que o evento não seja perdido se o processamento falhar imediatamente. Armazena o JSON do evento e metadados de controle.
**Em que situação usar:** Instanciado automaticamente pelos Consumers (RabbitMQ/Kafka) para salvar a mensagem antes de processá-la.
**O que pode dar errado:** Se o JSON do conteúdo for muito grande para a coluna do banco, o insert falhará.
**Exemplo real de uso:**
```csharp
var msg = new InboxMessage {
    Id = evento.Id, // ID original do evento para idempotência
    Type = evento.GetType().AssemblyQualifiedName,
    Content = JsonConvert.SerializeObject(evento)
};
```

---

## `Configuration/InboxConfiguration.cs`
**Responsabilidade:** Mapear a entidade `InboxMessage` para a tabela correta no banco de dados via EF Core.
**Por que existe:** Para definir detalhes de armazenamento, como nome da tabela ("InboxMessages"), chaves e índices que otimizam a busca por mensagens não processadas.
**Em que situação usar:** Aplicado automaticamente via Reflection no `BaseDbContext`.
**O que pode dar errado:** Alterar o nome da tabela sem criar uma migration correspondente quebrará o acesso ao banco.
**Exemplo real de uso:**
```csharp
builder.ToTable("InboxMessages");
builder.HasKey(x => x.Id);
```

---

## `Repositories/IInboxRepository.cs`
**Responsabilidade:** Definir o contrato para salvar mensagens no Inbox.
**Por que existe:** Para desacoplar os Consumers da implementação de persistência. O Consumer sabe que precisa salvar no Inbox, mas não *como*.
**Em que situação usar:** Injetado nos `IntegrationEventHandlers` ou Consumers de mensageria.
**O que pode dar errado:** Se a implementação não for transacional, pode haver risco de a mensagem ser salva mas o Ack não ser enviado para a fila (ou vice-versa), gerando duplicidade ou perda em cenários extremos.
**Exemplo real de uso:**
```csharp
await _inboxRepository.AddAsync(msg);
```

---

## `Repositories/InboxRepository.cs`
**Responsabilidade:** Implementação EF Core para adicionar mensagens ao `BaseDbContext`.
**Por que existe:** Para efetivar a gravação no banco de dados relacional.
**Em que situação usar:** Registrado no Container de DI.
**O que pode dar errado:** Ele usa `AddAsync` mas não chama `SaveChanges`. Quem chama deve garantir o commit da transação.
**Exemplo real de uso:**
```csharp
services.AddScoped<IInboxRepository, InboxRepository>();
```

---

## `Processors/InboxProcessor.cs`
**Responsabilidade:** Ler as mensagens pendentes na tabela, desserializá-las e despachá-las internamente para os handlers de domínio.
**Por que existe:** Para separar o recebimento (rápido) do processamento (lento/complexo). Isso evita que o Consumer fique travado processando regra de negócio. Também garante Retry automático em caso de falha.
**Em que situação usar:** Invocado periodicamente pelo `InboxProcessorJob`.
**O que pode dar errado:** 
- **Veneno (Poison Message):** Se uma mensagem sempre causar erro (ex: bug no código), ela pode bloquear ou atrasar o processamento das outras se não houver lógica de Dead Letter Queue ou limite de tentativas.
- **Tipo Desconhecido:** Se a aplicação mudar o nome da classe do evento (Refactor) e houver mensagens antigas salvas com o nome antigo, a desserialização falhará.
**Exemplo real de uso:**
```csharp
// Busca 20 msgs pendentes, processa e marca como concluídas
await _processor.ProcessAsync(cancellationToken);
```

---

## `BackgroundJobs/InboxProcessorJob.cs`
**Responsabilidade:** Job do Quartz para acionar o `InboxProcessor` em intervalos regulares.
**Por que existe:** Para garantir que o processamento do Inbox rode em background continuamente, independente de requisições HTTP.
**Em que situação usar:** Configurado no Startup (via `ServiceCollectionExtensions`) para rodar a cada X segundos.
**O que pode dar errado:** Se o job travar ou parar (ex: OOM Kill), as mensagens vão acumular no banco e o sistema parecerá não estar reagindo a eventos externos.
**Exemplo real de uso:**
```csharp
// Configuração Quartz
q.AddJob<InboxProcessorJob>(...)
 .WithSimpleSchedule(x => x.WithIntervalInSeconds(10));
```
