# Domain Events Utilities

Este diretório contém classes e interfaces auxiliares para o funcionamento do mecanismo de Eventos de Domínio. Enquanto as abstrações definem *o que* é um evento, estas classes definem *como* eles são processados e entregues.

## `DomainEventBase.cs`
**Responsabilidade:** Fornecer uma implementação base alternativa para Eventos de Domínio.
**Por que existe:** Principalmente para manter compatibilidade ou oferecer uma opção onde o `EventId` e `OccurredOn` podem ter setters protegidos (diferente da implementação em `Base/DomainEvent.cs` que pode ser mais restritiva).
**Em que situação usar:** Ao criar novos eventos de domínio, prefira herdar desta classe ou de `Base/DomainEvent`. Ambas cumprem o mesmo papel de fornecer metadados automáticos.
**O que pode dar errado:** Misturar implementações. Se metade dos eventos herda de `DomainEventBase` e a outra metade de `Base/DomainEvent`, pode haver confusão. Escolha um padrão e siga.
**Exemplo real de uso:**
```csharp
public class PedidoCanceladoEvent : DomainEventBase {
    // EventId e OccurredOn já vêm preenchidos
    public Guid PedidoId { get; init; }
}
```

---

## `DomainEventDispatcher.cs`
**Responsabilidade:** Coletar todos os eventos de domínio acumulados nas Entidades rastreadas pelo EF Core e publicá-los via MediatR.
**Por que existe:** O DDD prega "Side Effects at the Edge". Não queremos disparar eventos *durante* a regra de negócio (antes de salvar), pois se o `SaveChanges` falhar, o evento já foi disparado (ex: e-mail enviado). O Dispatcher garante que os eventos só saiam após o sucesso da transação (ou imediatamente antes, dependendo da configuração do UnitOfWork).
**Em que situação usar:** Injetado dentro do método `SaveChangesAsync` do seu `DbContext`.
**O que pode dar errado:** 
- **Loop Infinito:** Se um Event Handler alterar outra entidade, que gera outro evento, que altera outra entidade... O Dispatcher pode entrar em loop se for chamado recursivamente.
- **Esquecer de limpar:** Se não chamar `ClearDomainEvents()`, os mesmos eventos serão disparados de novo na próxima vez que a entidade for salva na mesma requisição.
**Exemplo real de uso:**
```csharp
// Dentro do DbContext:
await base.SaveChangesAsync();
await _dispatcher.DispatchAndClearEvents(changeTracker.Entries());
```

---

## `IDomainEventHandler.cs`
**Responsabilidade:** Definir o contrato para classes que vão reagir a eventos de domínio. É um wrapper semântico sobre `INotificationHandler<T>` do MediatR.
**Por que existe:** Para diferenciar visualmente e semanticamente os handlers de eventos de domínio (side-effects internos) de outros tipos de notificações (ex: notificações de erro ou integração).
**Em que situação usar:** Sempre que precisar executar uma ação colateral (ex: Enviar Email, Atualizar Cache, Logar) em resposta a algo que aconteceu no domínio.
**O que pode dar errado:** Executar lógica bloqueante ou lenta (ex: chamada HTTP síncrona de 30s) dentro do handler. Como o dispatcher geralmente roda na mesma thread da requisição HTTP original, isso vai travar a resposta para o usuário. Use filas (Background Jobs) para tarefas pesadas.
**Exemplo real de uso:**
```csharp
public class NotificarClienteHandler : IDomainEventHandler<PedidoEnviadoEvent> {
    public async Task Handle(PedidoEnviadoEvent args, CancellationToken ct) {
        await _emailService.Enviar(args.ClienteId, "Seu pedido saiu!");
    }
}
```
