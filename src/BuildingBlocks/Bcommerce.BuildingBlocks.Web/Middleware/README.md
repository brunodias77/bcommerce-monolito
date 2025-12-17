# Web Middleware

Este diretório contém os componentes de middleware que compõem o pipeline de requisição HTTP (The Pipeline) da aplicação. Eles são responsáveis por processar a requisição antes e depois de passar pelos Controllers.

## `ExceptionHandlingMiddleware.cs`
**Responsabilidade:** Envolver todo o processamento da requisição em um bloco `try/catch` global.
**Por que existe:** Para capturar exceções lançadas em qualquer lugar da aplicação (Domain, Application, Infra) e convertê-las em respostas HTTP JSON (ProblemDetails) inteligíveis, evitando vazamento de stacktrace e erros 500 genéricos.
**Em que situação usar:** Deve ser um dos primeiros middlewares configurados no `Program.cs`.
**O que pode dar errado:**
- Se posicionado *depois* do middleware que lançou o erro, não capturará a exceção.
- Se a exceção não tiver um mapeamento no `switch` interno, retornará 500 Internal Server Error (o que é o comportamento seguro default).
**Exemplo real de uso:**
```csharp
throw new NotFoundException("Produto não existe"); 
// Middleware converte para:
// HTTP 404 { "code": "Resource.NotFound", "message": "Produto não existe" }
```

---

## `CorrelationIdMiddleware.cs`
**Responsabilidade:** Gerenciar o `X-Correlation-Id`, garantindo que cada requisição tenha um ID único para rastreabilidade.
**Por que existe:** Em sistemas distribuídos, é vital rastrear uma operação através de vários serviços. Este middleware lê o header de entrada (se existir) ou cria um novo, e garante que ele esteja presente no header da resposta.
**Em que situação usar:** Configurar logo no início do pipeline, antes do Logging.
**O que pode dar errado:** Se for esquecido, logs de diferentes requisições simultâneas ficam misturados e impossíveis de desenrolar.
**Exemplo real de uso:**
```http
// Request
GET /api/orders
X-Correlation-Id: 12345

// Response
HTTP 200 OK
X-Correlation-Id: 12345
```

---

## `RequestLoggingMiddleware.cs`
**Responsabilidade:** Logar o início e o fim de cada requisição HTTP, incluindo método, rota, status code e latência.
**Por que existe:** O log padrão do ASP.NET Core pode ser muito verboso ou insuficiente. Este middleware cria um log conciso "entrada/saída".
**Em que situação usar:** Após o `CorrelationIdMiddleware` para que os logs já tenham o ID de correlação anexado.
**O que pode dar errado:** Logar dados sensíveis (senhas, cartões) se o middleware fosse expandido para logar o *Body* da requisição (atualmente loga apenas Path e Method, o que é seguro).
**Exemplo real de uso:**
```text
[INFO] Recebendo requisição: GET /api/products
[INFO] Finalizando requisição: GET /api/products - Status: 200 - Duração: 45ms
```

---

## `PerformanceMonitoringMiddleware.cs`
**Responsabilidade:** Monitorar o tempo de resposta da API e emitir alertas se exceder um limite (ex: 500ms).
**Por que existe:** Para detecção proativa de endpoints lentos (Slow Logs) sem depender exclusivamente de ferramentas de APM externas.
**Em que situação usar:** Útil em ambientes de produção para identificar gargalos.
**Exemplo real de uso:**
```text
[WARN] Requisição lenta detectada: GET /api/relatorios/pesado levou 1250ms
```

---

## `TenantResolutionMiddleware.cs`
**Responsabilidade:** Identificar o cliente (Tenant) dono dos dados da requisição atual.
**Por que existe:** Para suportar arquitetura Multi-tenant, onde o mesmo serviço atende vários clientes isolados logicamente.
**Em que situação usar:** Quando a aplicação atende múltiplos clientes.
**O que pode dar errado:** Confiar cegamente no header `X-Tenant-Id` enviado pelo frontend é um risco de segurança (Insecure Direct Object Reference). Em produção, o Tenant deve ser resolvido a partir do Token JWT ou subdomínio validado.
**Exemplo real de uso:**
```http
GET /api/orders
X-Tenant-Id: cliente-a
```
