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
======================================================================================================================================
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













====================================================================================
Com base na análise do repositório `bcommerce-monolito` e no algoritmo fornecido, segue a documentação técnica detalhada para o `AddAddressCommand`.

---

#CMD-07: Adicionar Endereço (AddAddressCommand)**Descrição:**
Este comando é responsável por registrar um novo endereço de entrega vinculado a um usuário específico no sistema. Ele encapsula a lógica de criação da entidade de endereço, validação de Value Objects (como CEP), e a regra de negócio para manutenção de unicidade do endereço padrão (default). O fluxo segue o padrão CQRS, garantindo a consistência através de transações e disparo de eventos de domínio.

##1. Request (Input)A requisição deve conter os dados necessários para a composição do endereço e identificação do usuário proprietário.

| Campo | Tipo | Obrigatório | Descrição |
| --- | --- | --- | --- |
| `UserId` | `Guid` | Sim | Identificador único do usuário (Owner). Geralmente extraído do contexto de autenticação ou passado explicitamente. |
| `Label` | `String` | Sim | Identificador amigável do endereço (ex: "Casa", "Trabalho"). |
| `RecipientName` | `String` | Sim | Nome da pessoa responsável por receber a encomenda. |
| `PostalCode` | `String` | Sim | Código postal (CEP). Deve seguir o formato válido (ex: 12345-678). |
| `Street` | `String` | Sim | Logradouro (Rua, Avenida, etc.). |
| `Number` | `String` | Sim | Número do endereço. |
| `Complement` | `String` | Não | Complemento do endereço (ex: "Apto 101"). |
| `Neighborhood` | `String` | Sim | Bairro. |
| `City` | `String` | Sim | Cidade. |
| `State` | `String` | Sim | Sigla da Unidade Federativa (UF). Deve conter 2 caracteres. |
| `IsDefault` | `Boolean` | Sim | Indica se este será o endereço principal do usuário. |

###Exemplo de JSON (Request)```json
{
  "userId": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
  "label": "Casa",
  "recipientName": "Bruno Dias",
  "postalCode": "17500-000",
  "street": "Rua Exemplo",
  "number": "123",
  "complement": "Sobrado",
  "neighborhood": "Centro",
  "city": "Marília",
  "state": "SP",
  "isDefault": true
}

```

##2. Regras de Negócio (Business Rules)As seguintes regras devem ser garantidas durante a execução do comando:

* **RN-01 (Validação de CEP):** O campo `PostalCode` deve ser validado conforme as regras do Value Object `PostalCode` (formato XXXXX-XXX ou 8 dígitos numéricos). O sistema não deve aceitar CEPs mal formatados.
* **RN-02 (Validação de UF):** O campo `State` deve conter estritamente 2 letras maiúsculas correspondentes a uma UF válida.
* **RN-03 (Unicidade de Endereço Padrão):** Um usuário só pode ter um endereço marcado como `IsDefault = true`. Se o novo endereço for marcado como padrão, o sistema deve buscar qualquer endereço padrão existente para este usuário e remover a flag `IsDefault` do mesmo antes de persistir o novo.
* **RN-04 (Vínculo de Usuário):** É obrigatório que o `UserId` corresponda a um usuário existente e ativo na base de dados (validado via repositório ou serviço de domínio).

##3. Fluxo de Processamento (Workflow)1. **Validação Sintática:** O `AddAddressCommandValidator` (FluentValidation) verifica se todos os campos obrigatórios estão preenchidos e se respeitam os limites de caracteres.
2. **Verificação de Existência do Usuário:** O Handler consulta o `IUserRepository` para garantir que o `UserId` informado existe. Caso contrário, retorna erro `UserNotFound`.
3. **Instanciação e Validação de Domínio:**
* Criação do Value Object `PostalCode`. Se inválido, lança exceção ou retorna erro de domínio.
* Instanciação da entidade `Address`.


4. **Gestão de Endereço Padrão (Regra RN-03):**
* Verifica se `IsDefault` é `true`.
* Se sim, consulta o `IAddressRepository` buscando o endereço atual marcado como padrão para este `UserId`.
* Se existir um endereço padrão anterior, atualiza a entidade antiga definindo `IsDefault = false`.


5. **Adição da Entidade:** Adiciona o novo objeto `Address` ao contexto através do `IAddressRepository.Add()`.
6. **Registro de Evento:** Adiciona o evento de domínio `AddressAddedEvent` à lista de eventos da entidade ou publica via `IMediator` (dependendo da estratégia de consistência eventual vs transacional).
7. **Persistência:** Invoca o `IUnitOfWork.CommitAsync()` para persistir as alterações (novo endereço e atualização do antigo padrão, se houver) em uma única transação atômica.
8. **Retorno:** Retorna o `Id` (Guid) do endereço recém-criado envolto em um objeto `Result`.

##4. Response (Output)###Sucesso (HTTP 200/201)Retorna o ID do recurso criado.

```json
{
  "value": "a1b2c3d4-e5f6-7890-1234-567890abcdef",
  "isSuccess": true,
  "isFailure": false,
  "error": {
    "code": "None",
    "message": ""
  }
}

```

###Erro (HTTP 400 - Bad Request)Exemplo de erro de validação de domínio (CEP Inválido).

```json
{
  "type": "https://tools.ietf.org/html/rfc7231#section-6.5.1",
  "title": "One or more validation errors occurred.",
  "status": 400,
  "errors": {
    "PostalCode": [
      "O formato do CEP é inválido."
    ]
  },
  "traceId": "00-9823654..."
}

```

###Erro (HTTP 404 - Not Found)Exemplo caso o usuário não seja encontrado.

```json
{
  "value": null,
  "isSuccess": false,
  "isFailure": true,
  "error": {
    "code": "User.NotFound",
    "message": "O usuário informado não foi encontrado."
  }
}

```


====================================================================================