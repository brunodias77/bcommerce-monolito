# AuditLog Infrastructure

Este diretório contém a infraestrutura responsável pelo rastreamento e persistência de logs de auditoria (quem fez o quê e quando) no banco de dados.

## `Models/AuditLog.cs`
**Responsabilidade:** Representar a entidade de tabela de Log, mapeando exatamente o que será salvo no banco (snapshot das mudanças).
**Por que existe:** Para ter um modelo de dados estruturado que armazene os valores antigos e novos (`OldValues`/`NewValues`) em formato JSON, permitindo auditoria detalhada de Update e Delete.
**Em que situação usar:** Instanciado automaticamente pelo `AuditInterceptor` (EntityFramework) ao detectar mudanças.
**O que pode dar errado:** Tentar serializar grafos circulares ou objetos muito complexos nas propriedades JSON, o que pode falhar ou ocupar muito espaço.
**Exemplo real de uso:**
```csharp
var log = new AuditLog {
    TableName = "Pedidos",
    Type = "Update",
    OldValues = JsonSerializer.Serialize(oldValues),
    NewValues = JsonSerializer.Serialize(newValues)
};
```

---

## `Repositories/IAuditLogRepository.cs`
**Responsabilidade:** Definir o contrato para salvar os logs.
**Por que existe:** Para garantir que o serviço de auditoria (ou interceptor) não dependa diretamente do Entity Framework, permitindo trocar o armazenamento no futuro (ex: salvar logs no ElasticSearch ou Mongo).
**Em que situação usar:** Injetado dentro de `AuditLogService` ou Interceptors.
**O que pode dar errado:** Se a implementação for síncrona ou bloquear a thread principal, pode deixar todas as transações do sistema lentas, pois a auditoria roda junto com o SaveChanges.
**Exemplo real de uso:**
```csharp
public async Task SaveChangesAsync() {
    // ...
    await _auditRepository.AddAsync(log); // Apenas adiciona ao contexto
}
```

---

## `Repositories/AuditLogRepository.cs`
**Responsabilidade:** Implementar a persistência usando o `BaseDbContext`.
**Por que existe:** Para integrar a auditoria na mesma transação de banco de dados da operação principal (se usando o mesmo Contexto), ou persistir em tabela dedicada.
**Em que situação usar:** Configurado no container de DI como a implementação de `IAuditLogRepository`.
**O que pode dar errado:** Como ele usa `_dbContext.Set<AuditLogModel>().AddAsync`, se o `DbContext` injetado for o mesmo que está disparando a auditoria, deve-se tomar cuidado para não criar loop infinito de eventos. Normalmente a auditoria é salva *antes* do commit efetivo.
**Exemplo real de uso:**
```csharp
// Configuração DI
services.AddScoped<IAuditLogRepository, AuditLogRepository>();
```

---

## `Services/IAuditLogService.cs`
**Responsabilidade:** Fachada (Facade) para operações de auditoria na camada de aplicação/infra.
**Por que existe:** Para centralizar qualquer regra extra que precise ocorrer antes de salvar o log (ex: enriquecer com IP do usuário, filtrar campos sensíveis).
**Em que situação usar:** Ponto de entrada principal para quem quer logar algo manualmente.
**O que pode dar errado:** Usar para logar debugs de código (ex: "Entrou no método X"). Auditoria é para mudanças de dados de negócio, não tracing de aplicação.
**Exemplo real de uso:**
```csharp
public interface IAuditLogService {
    Task LogAsync(AuditLog log);
}
```

---

## `Services/AuditLogService.cs`
**Responsabilidade:** Repassar a chamada para o repositório, servindo como orquestrador.
**Por que existe:** Mantém o padrão de arquitetura (Service -> Repository) e permite interceptação ou decoração futura.
**Em que situação usar:** Implementação padrão usada pelo sistema.
**O que pode dar errado:** Em sistemas de alta volumetria, escrever no banco relacional (SQL) para cada mudança pode gargalar o I/O. Nestes casos, este serviço poderia ser alterado para jogar em uma fila (RabbitMQ) sem quebrar o contrato.
**Exemplo real de uso:**
```csharp
public async Task LogAsync(AuditLog auditLog) {
    // Poderia anonimizar dados aqui antes de salvar
    await _repository.AddAsync(auditLog);
}
```
