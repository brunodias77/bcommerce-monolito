# Security Extensions

Este diretório contém extensões para simplificar a configuração de segurança e o acesso a dados do usuário.

## `ClaimsPrincipalExtensions.cs`
**Responsabilidade:** Fornecer métodos de conveniência para extrair dados fortemente tipados do `ClaimsPrincipal` (o usuário logado).
**Por que existe:** O acesso padrão a claims (ex: `User.FindFirst("sub")?.Value`) é verboso, propenso a erros (strings mágicas) e retorna strings que precisam de conversão (parsing).
**Em que situação usar:** Em Controllers ou Services onde é necessário obter o ID do usuário atual a partir do token.
**O que pode dar errado:** Se o token não contiver a claim "sub" ou se ela não for um GUID válido, o método retorna `Guid.Empty`. O chamador deve tratar esse caso (ex: retornar 401).
**Exemplo real de uso:**
```csharp
[HttpGet]
public IActionResult GetMyProfile()
{
    Guid userId = HttpContext.User.GetUserId();
    if (userId == Guid.Empty) return Unauthorized();
    // ...
}
```

---

## `ServiceCollectionExtensions.cs`
**Responsabilidade:** Centralizar a injeção de dependência de todo o módulo de segurança (Autenticação JWT, Handlers de Autorização, Serviços de Hash).
**Por que existe:** A configuração do JWT Bearer envolve muitas opções (validação de emissor, chave de assinatura, tempo de vida). Colocar isso diretamente no `Program.cs` polui o startup.
**Em que situação usar:** No `Program.cs` da API.
**O que pode dar errado:** 
- Esquecer de chamar `app.UseAuthentication()` e `app.UseAuthorization()` no pipeline de middleware (no `Program.cs`) fará com que esta configuração de serviços seja inútil.
- Configuração incorreta no `appsettings.json` (seção `JwtSettings`) causará falha na validação de todos os tokens.
**Exemplo real de uso:**
```csharp
// No Program.cs
builder.Services.AddSecurityServices(builder.Configuration);
```
