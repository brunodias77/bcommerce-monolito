###CMD-01: RegisterUserCommand**Descrição Técnica**
Este comando é responsável por orquestrar o fluxo de escrita para o registro de um novo usuário no módulo de Identity. Ele encapsula a lógica de validação de domínio, segurança (hashing), persistência e o disparo de eventos de integração para garantir a consistência eventual com outros módulos (ex: criação de carrinho).

####Request (Input)**Estrutura de Dados**

| Nome        | Tipo   | Obrigatório | Descrição                                                                     |
| ----------- | ------ | ----------- | ----------------------------------------------------------------------------- |
| `Email`     | String | Sim         | Endereço de e-mail do usuário. Deve ser um formato válido e único no sistema. |
| `Password`  | String | Sim         | Senha em texto plano para autenticação. Sujeita a regras de complexidade.     |
| `FirstName` | String | Não         | Primeiro nome do usuário para compor o perfil.                                |
| `LastName`  | String | Não         | Sobrenome do usuário para compor o perfil.                                    |

**Exemplo de Requisição (JSON)**

```json
{
  "email": "usuario@exemplo.com.br",
  "password": "SenhaForte123!",
  "firstName": "João",
  "lastName": "Silva"
}
```

####Regras de Negócio (Business Rules)\* **RN-01 (Validação de Email):** O e-mail fornecido deve respeitar o formato padrão (RFC 5322).

- **RN-02 (Unicidade de Conta):** Não é permitido registrar mais de um usuário com o mesmo endereço de e-mail. Se o e-mail já existir, o processo deve ser interrompido.
- **RN-03 (Complexidade de Senha):** A senha deve conter no mínimo 8 caracteres, incluindo pelo menos uma letra maiúscula e um número.
- **RN-04 (Segurança de Credenciais):** A senha nunca deve ser persistida em texto plano. Deve-se utilizar o algoritmo de hash BCrypt antes da persistência.
- **RN-05 (Criação de Perfil):** O registro do usuário deve disparar a criação das entidades agregadas básicas (User e Profile).
- **RN-06 (Integração de Carrinho):** A criação de um usuário deve garantir a disponibilidade de um carrinho de compras vazio para o mesmo, através de comunicação assíncrona.

####Fluxo de Processamento (Workflow)1. **Validação de Contrato (Fail-Fast):** O `ValidationBehavior` intercepta o comando e valida os campos obrigatórios e formatos (Email e Regras de Senha) utilizando FluentValidation. Retorna `400 Bad Request` se inválido. 2. **Verificação de Existência:** O Handler consulta o `IUserRepository` para verificar se o e-mail já está cadastrado. Se existir, retorna `Result.Fail` (conflito). 3. **Hashing de Senha:** O serviço de criptografia gera o hash da senha utilizando BCrypt. 4. **Construção do Agregado:**

- A entidade `User` é instanciada com os dados fornecidos.
- Um evento de domínio `UserCreatedEvent` é adicionado à lista de eventos da entidade.

5. **Persistência (Unit of Work):**

- O `IUserRepository` adiciona o novo usuário ao contexto.
- O `UnitOfWork.SaveChangesAsync` é invocado.
- O `PublishDomainEventsInterceptor` intercepta a transação, extrai o `UserCreatedEvent` e o converte/salva na tabela de Outbox (`shared.domain_events`).

6. **Publicação de Evento de Integração:** O sistema publica o `UserCreatedIntegrationEvent` no barramento (Outbox) para consumo do módulo _Cart_.
7. **Notificação:** O `IEmailService` é acionado para enviar o e-mail de boas-vindas/confirmação.
8. **Retorno:** O ID do usuário criado (`Guid`) é retornado.

####Response (Output)**Sucesso (201 Created)**

```json
{
  "userId": "d290f1ee-6c54-4b01-90e6-d701748f0851"
}
```

**Erro (400 Bad Request - Validação)**

```json
{
  "type": "https://tools.ietf.org/html/rfc7231#section-6.5.1",
  "title": "Validation Error",
  "status": 400,
  "detail": "Password: A senha deve conter pelo menos uma letra maiúscula.",
  "instance": "/api/auth/register",
  "errorCode": "VALIDATION_ERROR",
  "traceId": "00-98236a8d..."
}
```

**Erro (409 Conflict - Regra de Negócio)**

```json
{
  "type": "https://tools.ietf.org/html/rfc7231#section-6.5.8",
  "title": "Conflict",
  "status": 409,
  "detail": "O e-mail informado já está cadastrado no sistema.",
  "instance": "/api/auth/register",
  "errorCode": "EMAIL_ALREADY_EXISTS",
  "traceId": "00-b1236a8d..."
}
```

===============================================================================================================

Aqui está a documentação técnica detalhada para o Command `ConfirmEmailCommand`, seguindo estritamente a estrutura e as diretrizes solicitadas.

---

###CMD-11: Confirmar Email (ConfirmEmailCommand)Este command é responsável por finalizar o fluxo de verificação de identidade do usuário, validando a posse do endereço de e-mail fornecido no registro. Ele utiliza os mecanismos de segurança do ASP.NET Core Identity para validar tokens criptográficos temporários.

###Request (Input)A requisição deve conter o identificador único do usuário e o token de verificação recebido (geralmente via link no e-mail).

**Estrutura de Dados**

| Nome   | Tipo      | Obrigatório | Descrição                                                                                       |
| ------ | --------- | ----------- | ----------------------------------------------------------------------------------------------- |
| userId | UUID/Guid | Sim         | Identificador único do usuário no sistema (Identity User ID).                                   |
| token  | String    | Sim         | Token de confirmação gerado pelo Identity Service. Deve ser enviado decodificado (URL decoded). |

**Exemplo de JSON (Request)**

```json
{
  "userId": "d290f1ee-6c54-4b01-90e6-d701748f0851",
  "token": "CfDJ8N5...[token_hash_longo]...Vd"
}
```

###Regras de Negócio (Business Rules)\* **RN-01 (Validação de Existência):** O processo deve ser interrompido se o `userId` informado não corresponder a nenhum usuário cadastrado na base de dados.

- **RN-02 (Validação de Token):** O token fornecido deve ser válido, corresponder ao usuário específico e não estar expirado, conforme validação interna do ASP.NET Identity provider.
- **RN-03 (Idempotência de Confirmação):** Se o e-mail do usuário já estiver marcado como confirmado (`EmailConfirmed = true`), o sistema deve tratar a requisição com sucesso ou informar que já foi realizado, sem gerar erros de processamento, desde que o token seja válido ou o estado já seja consistente.
- **RN-04 (Eventos de Domínio):** A confirmação de e-mail deve disparar um evento de integração/domínio para notificar outros módulos (ex: liberar funcionalidades restritas no Catálogo ou Carrinho).

###Fluxo de Processamento (Workflow)1. **Validação de Contrato (Pipeline Behavior):**

- O `ValidationBehavior` verifica se `userId` e `token` não são nulos ou vazios via FluentValidation.

2. **Recuperação do Usuário:**

- O Handler invoca `_userManager.FindByIdAsync(request.UserId)`.
- Se o retorno for nulo, lança-se uma `NotFoundException` (ou retorna erro de domínio específico).

3. **Processamento de Confirmação (Identity):**

- O sistema invoca o método: `await _userManager.ConfirmEmailAsync(user, request.Token)`.
- Este método decodifica o token, verifica a assinatura, checa a expiração e, se válido, atualiza a flag `EmailConfirmed` na tabela `AspNetUsers`.

4. **Verificação de Resultado:**

- O resultado da operação (`IdentityResult`) é analisado.
- Se `result.Succeeded` for `false`, extraem-se os erros (ex: "Invalid token") e retorna-se uma `DomainException` ou resultado de falha.

5. **Geração de Evento:**

- Com o sucesso da confirmação, a entidade de domínio `User` (se separada do IdentityUser) ou um wrapper deve instanciar o evento `EmailConfirmedEvent`.
- O evento é enfileirado no contexto do EF Core ou disparado para o `OutboxEventBus`.

6. **Persistência (Unit of Work):**

- Caso haja alterações em entidades complementares (como `Profile`) ou persistência do evento na tabela `Outbox`, invoca-se `await _unitOfWork.CommitAsync()`.
- _Nota: O método `ConfirmEmailAsync` do Identity já persiste a flag de e-mail, mas o commit manual pode ser necessário para garantir a transacionalidade do evento de domínio no padrão Outbox._

7. **Retorno:**

- Retorna status de sucesso para a API.

###Response (Output)**Exemplo de JSON (Sucesso - HTTP 200 OK)**

```json
{
  "success": true,
  "message": "E-mail confirmado com sucesso.",
  "data": null
}
```

**Exemplo de JSON (Erro - HTTP 400 Bad Request)**

```json
{
  "type": "https://bcommerce.api/errors/invalid-token",
  "title": "Falha na confirmação de e-mail",
  "status": 400,
  "detail": "O token fornecido é inválido ou expirou.",
  "errors": {
    "Token": ["Invalid token."]
  }
}
```

Aqui está a documentação técnica detalhada para o Command `LoginCommand`, seguindo a estrutura e o algoritmo solicitados.

---

###CMD-02: Autenticar Usuário (LoginCommand)Este command é responsável pelo processo de autenticação de credenciais, gestão de segurança (bloqueio de contas) e estabelecimento de sessão. Ele orquestra a validação de identidade e a emissão de tokens de segurança (JWT e Refresh Token) necessários para o acesso aos recursos protegidos da API, seguindo uma abordagem híbrida de autenticação stateless (JWT) com controle de sessão stateful (Refresh Token no banco).

###Request (Input)A requisição deve conter as credenciais do usuário e informações sobre o dispositivo para fins de auditoria e segurança da sessão.

**Estrutura de Dados**

| Nome                  | Tipo   | Obrigatório | Descrição                                                    |
| --------------------- | ------ | ----------- | ------------------------------------------------------------ |
| email                 | String | Sim         | Endereço de e-mail do usuário. Deve estar em formato válido. |
| password              | String | Sim         | Senha do usuário em texto plano (será comparada com o hash). |
| deviceInfo            | Object | Sim         | Objeto contendo metadados do dispositivo de origem.          |
| deviceInfo.ipAddress  | String | Sim         | Endereço IP do cliente.                                      |
| deviceInfo.userAgent  | String | Sim         | Identificação do navegador/cliente e sistema operacional.    |
| deviceInfo.deviceName | String | Não         | Nome amigável do dispositivo (ex: "iPhone de Bruno").        |

**Exemplo de JSON (Request)**

```json
{
  "email": "cliente@exemplo.com",
  "password": "SenhaSegura123!",
  "deviceInfo": {
    "ipAddress": "203.0.113.195",
    "userAgent": "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36...",
    "deviceName": "Chrome Desktop"
  }
}
```

###Regras de Negócio (Business Rules)\* **RN-01 (Proteção contra Enumeração):** O sistema deve retornar uma mensagem de erro genérica ("Credenciais inválidas") tanto para usuário inexistente quanto para senha incorreta, evitando a enumeração de e-mails cadastrados.

- **RN-02 (Política de Bloqueio - Brute Force):** Após 5 tentativas de login consecutivas falhas, a conta do usuário deve ser temporariamente bloqueada.
- **RN-03 (Bloqueio de Acesso):** Usuários com a flag `LockoutEnabled` ativa e `LockoutEnd` no futuro não podem realizar login, devendo receber a mensagem específica "Conta bloqueada".
- **RN-04 (Ciclo de Vida do Token):** O `AccessToken` (JWT) deve ter expiração curta (15 minutos) e o `RefreshToken` expiração longa (7 dias).
- **RN-05 (Sessão Única por Dispositivo):** Cada login bem-sucedido deve criar uma nova entidade `Session` associada ao `RefreshToken` gerado.
- **RN-06 (Auditoria de Acesso):** Todas as tentativas de login (sucesso ou falha) devem ser registradas na entidade `LoginHistory` para fins de auditoria e segurança.

###Fluxo de Processamento (Workflow)1. **Validação de Contrato (Pipeline Behavior):**

- Verificação sintática dos dados de entrada (formato de e-mail, campos obrigatórios) via FluentValidation.

2. **Recuperação do Agregado User:**

- O Handler consulta o `IUserRepository` buscando pelo e-mail fornecido.
- _Caminho de Falha (Usuário Inexistente):_ Se o usuário não for encontrado, o fluxo é desviado para o registro de log (passo 4.c) simulado e retorno de erro genérico (RN-01).

3. **Verificação de Bloqueio (Lockout Check):**

- Verifica se `user.LockoutEnd` possui uma data futura.
- Se verdadeiro, interrompe o fluxo e retorna `DomainError` ("Conta bloqueada").

4. **Validação de Credenciais (Password Verify):**

- Utiliza o serviço de Hash (ex: `IPasswordHasher`) para comparar a senha fornecida com o hash armazenado.
- **SE Senha Inválida:**

1. Incrementa o contador `user.AccessFailedCount`.
2. Verifica se `AccessFailedCount >= 5`. Se sim, define `user.LockoutEnd` para `DateTime.UtcNow.AddMinutes(LockoutDuration)`.
3. Cria entidade `LoginHistory` com `Success = false` e detalhes da falha.
4. Persiste as alterações (`UnitOfWork.Commit`).
5. Retorna erro genérico "Credenciais inválidas".

6. **Processamento de Sucesso:**

- Reseta `user.AccessFailedCount` para 0.
- Reseta `user.LockoutEnd` para `null` (caso houvesse bloqueio anterior expirado).

6. **Geração de Tokens:**

- Serviço de Token gera o `AccessToken` (JWT) contendo claims (Id, Email, Roles).
- Serviço gera um `RefreshToken` (string aleatória criptograficamente segura).

7. **Criação de Sessão:**

- Instancia uma nova entidade `Session` associada ao `UserId`.
- Define `RefreshToken` e `ExpiresAt` (7 dias).
- Preenche dados do dispositivo (`IpAddress`, `UserAgent`).

8. **Eventos e Auditoria:**

- Adiciona o evento de domínio `SessionCreatedEvent` à entidade User ou Session.
- Cria entidade `LoginHistory` com `Success = true`.

9. **Persistência (Atomicidade):**

- Executa `await _unitOfWork.CommitAsync()`, persistindo o reset de falhas, a nova sessão, o histórico de login e disparando os eventos (Outbox).

10. **Retorno:**

- Retorna DTO contendo os tokens e tempos de expiração.

###Response (Output)**Exemplo de JSON (Sucesso - HTTP 200 OK)**

```json
{
  "accessToken": "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...",
  "refreshToken": "8f9d2a3c-4b5e-6f7g-8h9i-0j1k2l3m4n5o",
  "tokenType": "Bearer",
  "expiresIn": 900,
  "user": {
    "id": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
    "email": "cliente@exemplo.com",
    "name": "Bruno Dias"
  }
}
```

**Exemplo de JSON (Erro - HTTP 401 Unauthorized)**

```json
{
  "type": "https://bcommerce.api/errors/authentication-failed",
  "title": "Falha na Autenticação",
  "status": 401,
  "detail": "Credenciais inválidas.",
  "instance": "/api/users/login"
}
```
