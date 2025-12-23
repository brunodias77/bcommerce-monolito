# Abstrações do Domínio (Building Blocks)

Este diretório contém os contratos fundamentais (Interfaces) que formam a base para a modelagem do Domínio Rico (DDD). Eles garantem consistência estrutural e comportamental para todas as entidades e objetos de valor do sistema.

## `IAggregateRoot`
**Responsabilidade:** Marcar uma Entidade como a "Raiz" de um Agregado e gerenciar seus Eventos de Domínio.
**Por que existe:** Para impor a regra do DDD de que apenas a Raiz do Agregado pode ser acessada diretamente por repositórios ou serviços externos. Também centraliza a publicação de eventos que ocorreram dentro daquele limite transacional.
**Em que situação usar:** Em toda classe que representa o principal objeto de um fluxo de negócio (ex: `Pedido`, `Produto`, `Cliente`).
**O que pode dar errado:** Tratar toda tabela do banco como Aggregate Root. Isso gera problemas de transação e inconsistência. Apenas os objetos principais devem implementar essa interface; os "filhos" (ex: `ItemPedido`) devem ser apenas `Entity`.
**Exemplo real de uso:**
```csharp
// Repositórios só aceitam IAggregateRoot
public interface IPedidoRepository : IRepository<Pedido> { }

public class Pedido : AggregateRoot<Guid> {
    public void Finalizar() {
        Status = StatusPedido.Finalizado;
        AddDomainEvent(new PedidoFinalizadoEvent(Id));
    }
}
```

---

## `IDomainEvent`
**Responsabilidade:** Representar um fato imutável que ocorreu no passado dentro do domínio.
**Por que existe:** Para desacoplar efeitos colaterais. Em vez de o `Pedido` chamar o `Estoque` diretamente, ele emite um evento "PedidoCriado" e o `Estoque` reage a ele.
**Em que situação usar:** Sempre que uma mudança de estado no domínio precisar notificar outras partes do sistema (dentro ou fora do mesmo microsserviço).
**O que pode dar errado:** Colocar lógica de negócio dentro do evento ou torná-lo mutável. Eventos são apenas DTOs de transporte de dados passados.
**Exemplo real de uso:**
```csharp
public record PedidoPagoEvent(Guid PedidoId, DateTime DataPagamento) : IDomainEvent;
```

---

## `IEntity`
**Responsabilidade:** Definir o contrato base para objetos que possuem identidade única (ID) e ciclo de vida (Created/UpdatedAt).
**Por que existe:** Para garantir que todas as entidades do sistema sigam o mesmo padrão de rastreabilidade e identificação, facilitando a criação de Repositórios Genéricos.
**Em que situação usar:** Em qualquer objeto de domínio que precise ser distinguido por seu ID e não por seus atributos (ex: `ItemPedido`, `EnderecoEntrega` se for editável).
**O que pode dar errado:** Esquecer de atualizar o `UpdatedAt`. Idealmente, isso é tratado automaticamente pelo `DbContext` ao salvar.
**Exemplo real de uso:**
```csharp
public class Categoria : Entity<Guid> {
    public string Nome { get; private set; }
}
```

---

## `ISoftDeletable`
**Responsabilidade:** Adicionar capacidade de "exclusão lógica" a uma entidade.
**Por que existe:** Para atender requisitos de auditoria onde dados nunca podem ser removidos fisicamente do banco de dados, apenas marcados como inativos.
**Em que situação usar:** Em entidades críticas como `Pedido`, `TransacaoFinanceira`, `Usuario`.
**O que pode dar errado:** Esquecer de configurar o filtro global no Entity Framework (`QueryFilter`). Se isso acontecer, consultas normais (`GetById`) trarão registros excluídos, causando bugs lógicos graves.
**Exemplo real de uso:**
```csharp
public class Usuario : Entity<Guid>, ISoftDeletable {
    public bool IsDeleted { get; set; }
    public DateTime? DeletedAt { get; set; }
}
// O EF Core filtrará automaticamente onde IsDeleted == false
```

---

## `IValueObject`
**Responsabilidade:** Marcar objetos que são definidos por seus atributos e não por identidade (Imutáveis).
**Por que existe:** Para modelar conceitos como `Dinheiro`, `Endereco`, `CPF`, `Email`. Se dois `CPFs` têm o mesmo número, eles são o mesmo CPF.
**Em que situação usar:** Sempre que possível. Value Objects são mais seguros e fáceis de testar que entidades, pois não têm estado mutável complexo nem ciclo de vida.
**O que pode dar errado:** Adicionar um `Id` ou `Setters` públicos em um Value Object, quebrando o princípio de imutabilidade e igualdade estrutural.
**Exemplo real de uso:**
```csharp
public record Endereco(string Rua, string Cidade, string CEP) : IValueObject;

// Comparação automática por valor:
var end1 = new Endereco("Rua A", "SP", "000");
var end2 = new Endereco("Rua A", "SP", "000");
// end1 == end2 é TRUE
```

---

## `IVersionable`
**Responsabilidade:** Habilitar suporte a Concorrência Otimista (Optimistic Concurrency).
**Por que existe:** Para evitar o problema de "Lost Update" (Último a salvar vence), onde dois usuários editando o mesmo registro sobrescrevem o trabalho um do outro.
**Em que situação usar:** Em entidades com alta concorrência de edição, como `EstoqueProduto` ou `SaldoConta`.
**O que pode dar errado:** Não tratar a exceção `DbUpdateConcurrencyException` na camada de aplicação. Se o conflito ocorrer e não for tratado, o usuário receberá um erro 500 feio em vez de um aviso amigável "O registro foi alterado por outro usuário".
**Exemplo real de uso:**
```csharp
public class Produto : AggregateRoot<Guid>, IVersionable {
    public int Version { get; set; } // Mapeado como Concurrency Token no EF
}
```
