# Exceções de Aplicação

Este diretório contém a hierarquia de exceções customizadas utilizadas pelo domínio e pela camada de aplicação. Elas servem como um "vocabulário" de erros que permite ao sistema reagir de forma semântica a falhas, em vez de apenas retornar erros 500 genéricos.

## `ApplicationException.cs`
**Responsabilidade:** Servir como classe base abstrata para todas as exceções de negócio da aplicação. Define propriedades padrão como `Title` e `Message`.
**Por que existe:** Para permitir que Middlewares e Filtros de Exceção capturem erros conhecidos (`catch (ApplicationException)`) e os tratem de forma diferente de bugs não esperados (`NullReferenceException`).
**Em que situação usar:** Nunca instancie diretamente (é abstrata). Use para criar novas exceções de domínio específicas se as existentes não atenderem.
**O que pode dar errado:** Tentar usar exceções genéricas (System.Exception) para regras de negócio impede que a API retorne os status codes corretos (400, 404, etc).
**Exemplo real de uso:**
```csharp
// Captura genérica no middleware
try {
    await next(); 
} catch (ApplicationException ex) {
    // Sei que é um erro "controlado", retorno 400/404 com JSON bonitinho
    return Problem(ex.Title, ex.Message);
} catch (Exception ex) {
    // Erro crítico/bug, logo e retorno 500
    _logger.Critical(ex);
}
```

---

## `ConflictException.cs`
**Responsabilidade:** Indicar que a operação falhou devido a um conflito com o estado atual do recurso (mapeia para HTTP 409).
**Por que existe:** Para diferenciar erros de validação (dados inválidos) de erros de estado (dados válidos, mas conflitantes).
**Em que situação usar:** Violações de unicidade (email já cadastrado), concorrência otimista (registro alterado por outro usuário) ou regras de negócio impeditivas.
**O que pode dar errado:** Usar para erros de validação simples (ex: email inválido). Isso confunde o cliente da API, que espera um 400 Bad Request, não um 409 Conflict.
**Exemplo real de uso:**
```csharp
var usuarioExistente = await _repo.GetByEmail(cmd.Email);
if (usuarioExistente != null) {
    throw new ConflictException("O e-mail informado já está em uso.");
}
```

---

## `ForbiddenException.cs`
**Responsabilidade:** Indicar que o usuário está autenticado, mas não tem permissão para realizar a ação (mapeia para HTTP 403).
**Por que existe:** Para proteger recursos sensíveis contra acesso não autorizado de usuários logados.
**Em que situação usar:** Quando o usuário tenta alterar um pedido que não é dele, ou acessar uma área administrativa sem ser Admin.
**O que pode dar errado:** Confundir com `UnauthorizedException`. Se o usuário **não está logado**, é `Unauthorized` (401). Se ele **está logado mas não pode mexer**, é `Forbidden` (403).
**Exemplo real de uso:**
```csharp
if (pedido.ClienteId != usuarioAtual.Id && !usuarioAtual.IsAdmin) {
    throw new ForbiddenException("Você não tem permissão para alterar este pedido.");
}
```

---

## `NotFoundException.cs`
**Responsabilidade:** Indicar que o recurso solicitado não foi encontrado (mapeia para HTTP 404).
**Por que existe:** Para padronizar a resposta de "recurso inexistente" evitando retornar `null` ou erros genéricos.
**Em que situação usar:** Em Queries ou Commands que buscam uma entidade por ID e ela não existe.
**O que pode dar errado:** Deixar o ORM lançar exceção interna (ex: "Sequence contains no elements") em vez desta. Isso expõe detalhes de implementação (stack trace) para o cliente.
**Exemplo real de uso:**
```csharp
var produto = await _repo.GetById(id);
if (produto is null) {
    throw new NotFoundException("Produto", id);
}
// Retorno da API:
// 404 Not Found: "A entidade 'Produto' (123...) não foi encontrada."
```

---

## `UnauthorizedException.cs`
**Responsabilidade:** Indicar falha na autenticação (mapeia para HTTP 401).
**Por que existe:** Principalmente para cenários onde a validação de token falha ou quando o sistema detecta credenciais inválidas em um fluxo de login manual.
**Em que situação usar:** Login com senha errada, Token expirado, Token inválido.
**O que pode dar errado:** Lançar isso quando o usuário já está logado mas sem permissão (use `ForbiddenException` nesse caso).
**Exemplo real de uso:**
```csharp
var valido = _hashService.Verify(senha, usuario.SenhaHash);
if (!valido) {
    throw new UnauthorizedException("Credenciais inválidas.");
}
```

---

## `ValidationException.cs`
**Responsabilidade:** Agrupar múltiplos erros de validação de campos (mapeia para HTTP 400).
**Por que existe:** Para suportar o padrão "Notification/Fail-Fast", onde queremos devolver **todos** os erros de uma vez (ex: "Nome obrigatório" E "Email inválido") em vez de parar no primeiro erro.
**Em que situação usar:** Automaticamente disparado pelo `ValidationBehavior` (FluentValidation) ou manualmente se houver validações complexas de domínio.
**O que pode dar errado:** Retornar apenas uma string de erro simples. O cliente espera um dicionário de erros (`Errors: { "campo": ["erro1"] }`) para pintar os campos vermelhos no formulário.
**Exemplo real de uso:**
```csharp
// Uso manual raro (normalmente é automático via Behavior):
var result = validator.Validate(comando);
if (!result.IsValid) {
    throw new ValidationException(result.Errors);
}
```
