# Domain Specifications

Este diretório contém a implementação do padrão **Specification**, utilizado para encapsular regras de negócio e critérios de consulta de forma reutilizável e combinável. Ele permite desacoplar a lógica de filtragem da implementação do repositório.

## `CompositeSpecification.cs`
**Responsabilidade:** Servir como classe de compatibilidade estrutural.
**Por que existe:** Historicamente usada para classes que compõem outras especificações, mas atualmente a lógica de composição (`AndSpecification`, `OrSpecification`) foi internalizada na classe base `Specification<T>` para simplificar o uso.
**Em que situação usar:** Não é recomendado para novas implementações. Use os métodos `.And()`, `.Or()` diretamente fluentes na classe base.
**O que pode dar errado:** Herdar desta classe desnecessariamente adiciona uma camada de complexidade sem benefício real, já que ela não adiciona comportamento novo além do que `Specification<T>` já tem.
**Exemplo real de uso:**
```csharp
// Abordagem antiga (evitar):
// public class MeuFiltro : CompositeSpecification<T> ...

// Abordagem recomendada (uso fluente):
var spec = new AtivoSpec().And(new EmEstoqueSpec());
```

---

## `ISpecification.cs`
**Responsabilidade:** Definir o contrato para qualquer regra de especificação. Garante que ela possa ser validada em memória (`IsSatisfiedBy`) ou convertida para expressão SQL (`ToExpression`).
**Por que existe:** Para permitir que Repositórios aceitem qualquer critério de filtro genericamente (`GetListAsync(ISpecification<T> spec)`), sem saber quais regras específicas estão sendo aplicadas.
**Em que situação usar:** Quando precisar injetar uma especificação ou criar um mock para testes.
**O que pode dar errado:** Criar implementações que funcionam em memória (`IsSatisfiedBy`) mas falham ao serem convertidas para SQL (Expression) porque usam métodos do .NET não suportados pelo Entity Framework (ex: `DateTime.Parse`).
**Exemplo real de uso:**
```csharp
public interface IProdutoRepository {
    Task<List<Produto>> ListarAsync(ISpecification<Produto> spec);
}
```

---

## `Specification.cs`
**Responsabilidade:** Classe base abstrata que implementa a lógica de combinação booleana (AND, OR, NOT) usando árvores de expressão (`Expression Trees`).
**Por que existe:** Para permitir a criação de regras complexas a partir de regras simples ("Building Blocks"). Em vez de escrever uma query gigante, você combina `AtivoSpec` + `VencidoSpec` + `CategoriaSpec`.
**Em que situação usar:** Herdar desta classe para criar qualquer regra de negócio ou filtro de consulta.
**O que pode dar errado:** Combinar muitas especificações complexas pode gerar um Query SQL resultante muito aninhado e performaticamente ruim. Se a query ficar muito lenta, prefira escrever uma query SQL/LINQ dedicada no repositório (Dapper/EF) em vez de Specification.
**Exemplo real de uso:**
```csharp
public class ProdutoElegivelPromocaoSpec : Specification<Produto> {
    public override Expression<Func<Produto, bool>> ToExpression() {
        // Regra simples, mas reutilizável
        return p => p.Ativo && p.Estoque > 0 && p.Preco > 10;
    }
}

// Uso combinado:
var spec = new ProdutoElegivelPromocaoSpec().And(new CategoriaEletronicosSpec());
```
