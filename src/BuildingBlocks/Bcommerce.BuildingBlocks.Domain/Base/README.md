# Base Classes do Domínio

Este diretório contém as implementações base (Abstract Classes) para os conceitos centrais do DDD. Diferente das abstrações (interfaces), estas classes fornecem comportamentos padrão prontos para uso, como comparação de igualdade, gerenciamento de eventos e timestamps.

## `AggregateRoot.cs`
**Responsabilidade:** Implementação base para Raízes de Agregado. Gerencia a lista interna de eventos de domínio (`_domainEvents`) e expõe métodos protegidos para adicioná-los.
**Por que existe:** Para evitar que cada Entidade tenha que reimplementar a lógica de coleção e despacho de eventos manualmente.
**Em que situação usar:** Herde desta classe em toda entidade que for uma Raiz de Agregado.
**O que pode dar errado:** Tentar manipular a lista de eventos externamente (ela é exposta como `IReadOnlyCollection`). Apenas a própria classe deve adicionar eventos a si mesma.
**Exemplo real de uso:**
```csharp
public class Pedido : AggregateRoot<Guid> {
    public void Cancelar() {
        // Lógica de negócio...
        AddDomainEvent(new PedidoCanceladoEvent(Id));
    }
}
```

---

## `DomainEvent.cs`
**Responsabilidade:** Classe base para eventos de domínio que já fornece ID único e data de ocorrência (`OccurredOn`) automáticos.
**Por que existe:** Para reduzir o boilerplate na criação de eventos, garantindo que todo evento tenha metadados de rastreabilidade padronizados.
**Em que situação usar:** Herde desta classe para criar seus eventos de domínio (fatos passados).
**O que pode dar errado:** Esquecer de que eventos devem ser imutáveis. Como é uma classe (e não `record`), tecnicamente permite setters, mas eles devem ser evitados.
**Exemplo real de uso:**
```csharp
public class ProdutoCriadoEvent : DomainEvent {
    public string Nome { get; }
    public ProdutoCriadoEvent(string nome) => Nome = nome;
}
```

---

## `Entity.cs`
**Responsabilidade:** Implementação base para Entidades. Sobrescreve `Equals` e `GetHashCode` para garantir que duas entidades sejam iguais se tiverem o mesmo ID (Identity Equality), não a mesma referência de memória.
**Por que existe:** O comportamento padrão do C# compara referências de memória. No DDD, se você carregar o mesmo cliente duas vezes do banco (instâncias diferentes), eles devem ser considerados iguais (`a == b`) se tiverem o mesmo ID.
**Em que situação usar:** Herde desta classe para todas as entidades (que não sejam Aggregate Roots).
**O que pode dar errado:** Usar entidades como chaves de Dicionário (`Dictionary<Entity, ...>`) se o ID for mutável ou nulo (ex: antes de salvar no banco se o ID for gerado lá). O HashCode muda e você perde o objeto no Dictionary.
**Exemplo real de uso:**
```csharp
public class ItemPedido : Entity<Guid> {
    // Comportamento de igualdade por ID já vem pronto da base
}
```

---

## `Enumeration.cs`
**Responsabilidade:** Implementar o padrão **Smart Enum**. Permite criar enums que são classes de verdade, podendo ter métodos e propriedades.
**Por que existe:** Os `enum` padrão do C# são apenas inteiros mascarados. Eles não permitem validação de regras nem comportamentos associados.
**Em que situação usar:** Quando você tem uma lista fixa de opções (ex: Status, TipoCartao) mas precisa de lógica atrelada a cada opção.
**O que pode dar errado:** Tentar usar switch-case tradicional (com constantes). Como são objetos, o switch deve ser feito por Pattern Matching ou `when`.
**Exemplo real de uso:**
```csharp
public class CartaoCredito : Enumeration {
    public static CartaoCredito Visa = new(1, "Visa");
    // Exemplo de comportamento que um enum comum não teria:
    public bool AceitaParcelamento => Id == 1; 

    private CartaoCredito(int id, string nome) : base(id, nome) {}
}
```

---

## `ValueObject.cs`
**Responsabilidade:** Implementação base para Objetos de Valor. Sobrescreve `Equals` e `GetHashCode` para comparar objetos baseando-se em **todos** os seus atributos (Structural Equality).
**Por que existe:** Para evitar ter que escrever manualmente a lógica de comparação propriedade-por-propriedade em cada VO.
**Em que situação usar:** Herde desta classe para criar Value Objects. Obrigatório implementar `GetEqualityComponents()`.
**O que pode dar errado:** Esquecer de incluir uma propriedade nova no `GetEqualityComponents()`. Se isso acontecer, dois objetos diferentes serão considerados iguais se as outras propriedades coincidirem.
**Exemplo real de uso:**
```csharp
public class NomeCompleto : ValueObject {
    public string Nome { get; }
    public string Sobrenome { get; }

    protected override IEnumerable<object> GetEqualityComponents() {
        yield return Nome; // A ordem importa!
        yield return Sobrenome;
    }
}
```
