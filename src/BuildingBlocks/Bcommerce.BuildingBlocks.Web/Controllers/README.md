# Web Controllers

Este diretório contém os controladores base e utilitários para a camada de apresentação (API) da aplicação.

## `ApiControllerBase.cs`
**Responsabilidade:** Servir como classe pai para todos os Controllers da API, padronizando o envio de comandos e o formato das respostas.
**Por que existe:** Para evitar duplicação de código no tratamento de erros. Em vez de todo controller fazer `if (fail) return BadRequest()`, este controller base traduz os objetos `Result` (Domain Pattern) para `IActionResult` (Http Status Codes) automaticamente.
**Em que situação usar:** Todos os novos controllers devem herdar de `ApiControllerBase`.
**O que pode dar errado:** 
- Tentar retornar `HandleFailure` para um resultado de sucesso lançará uma exceção de `InvalidOperationException` (falha de programação).
- Se novos tipos de erro (`ErrorType`) forem adicionados ao enum mas não mapeados no `switch` deste arquivo, eles retornarão 400 Bad Request por padrão, o que pode mascarar erros de servidor ou segurança.
**Exemplo real de uso:**
```csharp
[HttpPost]
public async Task<IActionResult> Create(CreateOrderCommand command)
{
    var result = await _sender.Send(command);
    
    if (result.IsFailure)
    {
        return HandleFailure(result);
    }

    return CreatedAtAction(nameof(GetById), new { id = result.Value }, result.Value);
}
```
