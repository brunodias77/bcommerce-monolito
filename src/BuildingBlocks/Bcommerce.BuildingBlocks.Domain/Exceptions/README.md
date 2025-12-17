# Domain Exceptions

Este diretório contém a hierarquia de exceções utilizadas exclusivamente pela camada de Domínio. Elas servem para proteger as invariantes das entidades e interromper operações que violem regras de negócio fundamentais.

## `BusinessRuleException.cs`
**Responsabilidade:** Indicar que uma operação de negócio não pode prosseguir porque violaria uma regra do domínio (ex: regra de estado, regra de limite).
**Por que existe:** Para diferenciar erros semânticos de negócio (que o usuário pode corrigir corrigindo a ação) de bugs de sistema ou erros de infraestrutura.
**Em que situação usar:** Em métodos de Entidades ou Serviços de Domínio quando uma pré-condição de negócio falha.
**O que pode dar errado:** Usar para validação de formato de entrada (ex: "Email inválido"). Para isso, prefira notificações ou `InvalidValueObjectException`. `BusinessRuleException` é sobre regras de *processo* (ex: "Pedido pago não pode ser cancelado").
**Exemplo real de uso:**
```csharp
public void AdicionarItem(Produto produto) {
    if (this.Itens.Count >= 10) {
        throw new BusinessRuleException("Limite máximo de 10 itens por pedido atingido.");
    }
}
```

---

## `DomainException.cs`
**Responsabilidade:** Servir como classe base (marker) para todas as exceções originadas no Domínio.
**Por que existe:** Para permitir que a camada de Aplicação (Middleware ou Try/Catch) capture genericamente qualquer erro de domínio com `catch (DomainException)` sem precisar conhecer cada subtipo específico.
**Em que situação usar:** Nunca instancie diretamente (prefira as derivadas). Use apenas em blocos `catch` ou herde dela ao criar novas categorias de erro de domínio.
**O que pode dar errado:** Lançar `DomainException` genérica com mensagem string. Isso torna difícil para o frontend saber qual foi exatamente o erro (se foi regra, validação, etc) sem fazer parse da mensagem.
**Exemplo real de uso:**
```csharp
// No Middleware de Tratamento de Erro:
catch (DomainException ex) {
    // Retorna 422 Unprocessable Entity ou 400 Bad Request
    return ProblemGoodRequest(ex.Message);
}
```

---

## `InvalidValueObjectException.cs`
**Responsabilidade:** Proteger a integridade de Objetos de Valor, impedindo que eles sejam criados em um estado inválido.
**Por que existe:** Value Objects devem ser sempre válidos. Não faz sentido existir um objeto `Email` contendo "banana". Esta exceção garante a regra "Fail Fast" no construtor.
**Em que situação usar:** Exclusivamente dentro dos construtores ou factories de classes que herdam de `ValueObject`.
**O que pode dar errado:** Usar para erros complexos que dependem de banco de dados (ex: "Email já existe"). Value Objects não acessam banco. Use apenas para validação de formato/estado interno.
**Exemplo real de uso:**
```csharp
public class CPF : ValueObject {
    public string Valor { get; }
    public CPF(string valor) {
        if (!ValidarFormato(valor)) {
            throw new InvalidValueObjectException("CPF com formato inválido.");
        }
        Valor = valor;
    }
}
```
