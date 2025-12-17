# Security Authorization

Este diretório contém a implementação do sistema de autorização baseado em Políticas (Policy-Based Authorization) do ASP.NET Core. Ele define como as permissões são verificadas e impostas.

## Requirements

### `PermissionRequirement.cs`
**Responsabilidade:** Representar um requisito de autorização que exige que o usuário tenha uma permissão específica.
**Por que existe:** O mecanismo de Policies do .NET precisa de um objeto que carregue os dados da necessidade (neste caso, "qual permissão é necessária?").
**Em que situação usar:** Instanciado dinamicamente quando uma Policy baseada em permissão é construída.
**Exemplo real de uso:**
```csharp
// Exige permissão "catalog.products.read"
new PermissionRequirement("catalog.products.read");
```

### `ModuleAccessRequirement.cs`
**Responsabilidade:** Representar um requisito que exige acesso a um módulo funcional inteiro (ex: "Admin", "Storefront").
**Por que existe:** Para permitir bloqueios broad-level (nível macro), impedindo que um usuário comum acesse endpoints administrativos mesmo que tenha permissões individuais.
**Em que situação usar:** Em policies que protegem controllers inteiros ou áreas da aplicação.
**Exemplo real de uso:**
```csharp
new ModuleAccessRequirement("BackOffice");
```

---

## Handlers

### `PermissionHandler.cs`
**Responsabilidade:** Lógica que verifica se o `PermissionRequirement` é satisfeito pelo usuário atual.
**Por que existe:** É onde a "mágica" acontece. Ele inspeciona o `User.Claims` procurando por uma claim do tipo `permissions` que bata com o exigido.
**Em que situação usar:** Registrado no DI como `IAuthorizationHandler`. O framework invoca automaticamente.
**O que pode dar errado:** Se o nome da claim no token JWT for diferente de "permissions" (ex: "scope" ou "p"), a validação falhará silenciosamente (acesso negado).
**Exemplo real de uso:**
```csharp
// Validação interna:
if (user.HasClaim("permissions", requirement.Permission)) Succeed();
```

### `ModuleAccessHandler.cs`
**Responsabilidade:** Lógica que verifica se o usuário tem a claim necessária para acessar o módulo solicitado.
**Por que existe:** Separa a lógica de validação de módulo da validação de permissão fina, mantendo SRP.
**Em que situação usar:** Registrado no DI.
**Exemplo real de uso:**
```csharp
// Validação interna:
if (user.HasClaim("modules", requirement.ModuleName)) Succeed();
```

---

## Policies

### `Permissions.cs`
**Responsabilidade:** Centralizar strings constantes (Magic Strings) que representam as ações/recursos do sistema.
**Por que existe:** Para evitar erros de digitação (`"write"` vs `"Write"`) espalhados pelo código. Serve como catálogo de todas as permissões disponíveis.
**Em que situação usar:** Ao definir atributos `[Authorize]` ou cadastrar Seeds de permissões no banco.
**O que pode dar errado:** Alterar o valor de uma constante aqui sem migrar os dados no banco de dados deixará usuários sem acesso (pois a string no banco não baterá mais com o código).
**Exemplo real de uso:**
```csharp
[Authorize(Policy = Permissions.Write)]
public IActionResult Create() { ... }
```

### `PolicyNames.cs`
**Responsabilidade:** Centralizar os nomes das Políticas de Autorização registradas no Startup.
**Por que existe:** As Policies são registradas com um nome (string) e consumidas com esse mesmo nome.
**Em que situação usar:** Na configuração do `AddAuthorization` e nos atributos `[Authorize(Policy = ...)]`.
**Exemplo real de uso:**
```csharp
[Authorize(Policy = PolicyNames.ModuleAccess)]
```
