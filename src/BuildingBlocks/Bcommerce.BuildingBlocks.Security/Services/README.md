# Security Services

Este diretório contém serviços utilitários relacionados à segurança e contexto do usuário na aplicação.

## `ICurrentUserService.cs`
**Responsabilidade:** Contrato para acessar informações do usuário autenticado no contexto atual.
**Por que existe:** Para abstrair a dependência `IHttpContextAccessor` das camadas de Aplicação e Domínio (que não devem saber sobre HTTP). Facilita o Mock em testes unitários.
**Em que situação usar:** Em Application Services que precisam saber "quem" está executando a ação (para auditoria ou validação de regras de negócio como "apenas o dono pode editar").
**O que pode dar errado:** Se for usado em um escopo onde não há usuário logado (ex: Background Job ou rota anônima), as propriedades retornarão valores vazios/nulos.
**Exemplo real de uso:**
```csharp
public class CreateOrderHandler(ICurrentUserService user) {
    if (user.UserId == Guid.Empty) throw new UnauthorizedAccessException();
}
```

---

## `CurrentUserService.cs`
**Responsabilidade:** Implementação concreta que busca os dados do `IHttpContextAccessor`.
**Por que existe:** Para extrair o `UserId` e `ClaimsPrincipal` do token JWT presente no header da requisição HTTP atual.
**Em que situação usar:** Registrado com escopo `Scoped` no container de DI.
**O que pode dar errado:** Depender da claim `sub` (Subject). Se o token gerado usar outro nome para o ID do usuário (ex: "id"), este serviço retornará `Guid.Empty` silenciosamente.
**Exemplo real de uso:**
```csharp
// No Program.cs
services.AddScoped<ICurrentUserService, CurrentUserService>();
```
