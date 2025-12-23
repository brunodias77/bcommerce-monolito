==============================================================================================================================================================
Com base na análise do código fonte (especialmente os módulos `Users` e `BuildingBlocks`) e do arquivo `schema.sql`, segue a documentação técnica detalhada para o comando `RegisterUserCommand`.

---

### **CMD-01: Registrar Usuário (RegisterUserCommand)**

**Descrição Técnica:**
Este comando é responsável por orquestrar o processo de onboarding de um novo usuário na plataforma. Ele encapsula a criação do Agregado `ApplicationUser`, a geração de credenciais seguras, a criação do perfil associado (`Profile`) e a garantia da consistência transacional. O fluxo segue o padrão CQRS, separando a escrita da leitura, e dispara eventos de domínio para integração assíncrona com outros módulos (ex: criação de carrinho no módulo `Cart`).

#### **1. Request (Input)**

A requisição deve conter os dados fundamentais para identificação e autenticação do usuário, além dos dados básicos de perfil.

| Campo | Tipo | Obrigatório | Descrição |
| --- | --- | --- | --- |
| `firstName` | String | Sim | Primeiro nome do usuário para o perfil. |
| `lastName` | String | Sim | Sobrenome do usuário para o perfil. |
| `email` | String | Sim | Endereço de e-mail válido. Será utilizado como identificador de login. |
| `cpf` | String | Sim | Documento CPF (apenas números ou formatado). Deve ser válido e único. |
| `password` | String | Sim | Senha em texto plano. Deve atender aos critérios de complexidade. |
| `phoneNumber` | String | Não | Número de telefone para contato (opcional). |

**Exemplo de JSON (Request):**

```json
{
  "firstName": "Bruno",
  "lastName": "Dias",
  "email": "bruno.dias@exemplo.com",
  "cpf": "123.456.789-00",
  "password": "Password@123!",
  "phoneNumber": "11999998888"
}

```

#### **2. Regras de Negócio (Business Rules)**

As seguintes regras devem ser validadas durante a execução do handler:

* **RN-01 (Formato de Value Objects):** O `Email` deve seguir o padrão RFC 5322 e o `Cpf` deve respeitar o algoritmo de dígitos verificadores (encapsulados nos Value Objects do Domínio).
* **RN-02 (Unicidade de E-mail):** Não é permitido cadastrar um usuário com um e-mail já existente na base de dados (`users.users`).
* **RN-03 (Unicidade de CPF):** Não é permitido cadastrar um usuário com um CPF já existente na base de dados.
* **RN-04 (Segurança da Senha):** A senha deve ter no mínimo 8 caracteres, contendo letras maiúsculas, minúsculas, números e caracteres especiais.
* **RN-05 (Integridade do Agregado):** Todo usuário criado (`ApplicationUser`) deve, obrigatoriamente, ter um registro correspondente na tabela de perfis (`Profile`) dentro da mesma transação.
* **RN-06 (Hashing):** A senha jamais deve ser persistida em texto plano. Deve-se utilizar o `IPasswordHasher` para gerar um hash seguro.

#### **3. Fluxo de Processamento (Workflow)**

1. **Validação de Sintaxe (Fail-Fast):** O `ValidationBehavior` intercepta o comando e executa as regras do FluentValidation (campos nulos, formatos básicos). Caso falhe, retorna `ValidationException`.
2. **Verificação de Unicidade:** O Handler consulta o `IUserRepository` para verificar a existência do `Email` ou `Cpf`. Se encontrado, retorna um erro de conflito (`Error.Conflict`).
3. **Instanciação de Value Objects:** Criação das instâncias de `Email` e `Cpf`. Falhas na validação interna destes objetos lançam `DomainException`.
4. **Hashing de Senha:** O serviço `IPasswordHasher` é invocado para transformar a senha recebida em um hash criptográfico.
5. **Construção do Agregado:**
* Criação da entidade `ApplicationUser` com os dados de credencial.
* Criação da entidade `Profile` vinculada ao ID do usuário.


6. **Persistência (Unit of Work):**
* O repositório adiciona o novo usuário ao contexto.
* O mecanismo de `UnitOfWork` prepara a transação.


7. **Geração de Eventos:** O método `ApplicationUser.Create` (ou similar factory) adiciona o evento de domínio `UserRegisteredEvent` à lista de eventos da entidade.
8. **Commit e Outbox:** Ao executar `SaveChanges`, os dados são persistidos nas tabelas `users.users` e `users.profiles`, e o evento é salvo na tabela `outbox_messages` (garantindo atomicidade).
9. **Retorno:** Retorna o `Guid` do usuário recém-criado encapsulado em um objeto `Result`.

#### **4. Response (Output)**

O retorno segue o padrão envelope `Result<T>`.

**Exemplo de JSON (Sucesso - HTTP 200 OK):**

```json
{
  "isSuccess": true,
  "isFailure": false,
  "value": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
  "error": {
    "code": "None",
    "message": "",
    "type": 0
  }
}

```

**Exemplo de JSON (Erro - HTTP 409 Conflict):**

```json
{
  "isSuccess": false,
  "isFailure": true,
  "value": null,
  "error": {
    "code": "User.EmailAlreadyExists",
    "message": "O e-mail fornecido já está em uso.",
    "type": 2
  }
}

```

==============================================================================================================================================================
Com base na análise do módulo `Users` e nos padrões de segurança e infraestrutura identificados no projeto `bcommerce-monolito`, segue a documentação técnica detalhada para o comando `LoginCommand`.

---

### **CMD-02: Realizar Login (LoginCommand)**

**Descrição Técnica:**
Este comando é responsável por autenticar um usuário no sistema, garantindo a verificação segura de credenciais e o gerenciamento de sessões. Seguindo o fluxo CQRS, ele não apenas valida o acesso, mas também orquestra a emissão de tokens JWT (Access e Refresh), registra a telemetria da sessão (Dispositivo/IP) e dispara eventos de integração cruciais para a consistência entre módulos, especificamente a fusão de carrinhos de compras (Anonymous-to-Logged-in Merge) no módulo de Vendas/Cart.

#### **1. Request (Input)**

A requisição exige credenciais válidas e informações de contexto do cliente para auditoria e segurança da sessão.

| Campo | Tipo | Obrigatório | Descrição |
| --- | --- | --- | --- |
| `email` | String | Sim | E-mail cadastrado do usuário. Utilizado como chave de busca principal. |
| `password` | String | Sim | Senha do usuário. Será comparada com o hash armazenado. |
| `deviceInfo` | String | Não | Informações do dispositivo/navegador (User-Agent) para rastreio de sessão. |
| `ipAddress` | String | Não | Endereço IP do cliente. Geralmente injetado pelo controlador via contexto HTTP. |

**Exemplo de JSON (Request):**

```json
{
  "email": "bruno.dias@exemplo.com",
  "password": "Password@123!",
  "deviceInfo": "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36...",
  "ipAddress": "201.12.34.56"
}

```

#### **2. Regras de Negócio (Business Rules)**

As seguintes regras garantem a segurança e integridade do processo de autenticação:

* **RN-01 (Validação de Credenciais):** O sistema deve verificar se o e-mail existe e se o hash da senha fornecida corresponde ao hash armazenado, utilizando `IPasswordHasher`. Falhas aqui devem retornar mensagem genérica ("Credenciais inválidas") para evitar enumeração de usuários.
* **RN-02 (Bloqueio de Conta):** Deve-se verificar se a conta do usuário está ativa e não bloqueada (ex: excesso de tentativas falhas ou banimento administrativo).
* **RN-03 (Auditoria de Login):** Toda tentativa de login (sucesso ou falha) deve ser registrada na entidade `LoginHistory` para fins de auditoria e detecção de ataques de força bruta.
* **RN-04 (Gerenciamento de Sessão):** Um login bem-sucedido deve criar e persistir uma nova entidade `Session`, contendo o Refresh Token gerado, data de expiração, IP e Device Info.
* **RN-05 (Emissão de Tokens):** Devem ser gerados um `AccessToken` (curta duração, contendo Claims e Roles) e um `RefreshToken` (longa duração, opaco) via `ITokenGenerator`.
* **RN-06 (Integração de Carrinho):** A autenticação deve disparar o evento `UserLoggedInIntegrationEvent`. O módulo `Cart` deve assinar este evento para migrar itens de um carrinho anônimo (baseado em cookie/session temporária) para o carrinho persistente do usuário.

#### **3. Fluxo de Processamento (Workflow)**

1. **Validação Sintática:** O `ValidationBehavior` verifica se e-mail e senha foram fornecidos e atendem aos formatos básicos.
2. **Busca de Usuário:** O Handler consulta o `IUserRepository` buscando pelo e-mail fornecido.
3. **Verificação de Senha:**
* Se usuário não encontrado: Retorna erro genérico (RN-01).
* Se encontrado: Utiliza `IPasswordHasher.Verify()` para validar a senha.


4. **Registro de Histórico:**
* Se senha inválida: Adiciona registro de falha em `LoginHistory`, incrementa contador de falhas e retorna erro.
* Se senha válida: Adiciona registro de sucesso em `LoginHistory` e zera contadores de falha.


5. **Geração de Tokens:** O `ITokenGenerator` cria o Access Token (JWT) e o Refresh Token (GUID/Random String).
6. **Persistência da Sessão:**
* Instancia a entidade `Session` vinculada ao `UserId`.
* Define `RefreshToken`, `ExpiresAt`, `IpAddress` e `DeviceInfo`.
* Salva a sessão no `ISessionRepository`.


7. **Commit e Eventos:**
* O `UnitOfWork` efetiva a transação no banco de dados.
* O evento `UserLoggedInEvent` é despachado para o Outbox.


8. **Retorno:** Retorna o DTO contendo os tokens e tempo de expiração.

#### **4. Response (Output)**

O retorno contém os artefatos necessários para o cliente manter a autenticação nas requisições subsequentes.

**Exemplo de JSON (Sucesso - HTTP 200 OK):**

```json
{
  "isSuccess": true,
  "isFailure": false,
  "value": {
    "accessToken": "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJzdWIiOiIxMjM0NTY3ODkwIiwibmFtZ...",
    "refreshToken": "8f902b74-3d23-448a-b450-13247963b2f5",
    "expiresIn": 3600,
    "tokenType": "Bearer",
    "userId": "3fa85f64-5717-4562-b3fc-2c963f66afa6"
  },
  "error": {
    "code": "None",
    "message": "",
    "type": 0
  }
}

```

**Exemplo de JSON (Erro - HTTP 400 Bad Request):**

```json
{
  "isSuccess": false,
  "isFailure": true,
  "value": null,
  "error": {
    "code": "Auth.InvalidCredentials",
    "message": "E-mail ou senha inválidos.",
    "type": 4
  }
}

```

---



==============================================================================================================================================================
Com base na estrutura de segurança (`Bcommerce.BuildingBlocks.Security`) e no gerenciamento de sessões do módulo de Usuários (`Bcommerce.Modules.Users`), segue a documentação técnica para o `RefreshTokenCommand`.

---

### **CMD-03: Atualizar Token (RefreshTokenCommand)**

**Descrição Técnica:**
Este comando implementa o padrão de segurança **Refresh Token Rotation**. Ele permite que um cliente obtenha um novo `AccessToken` (curta duração) e um novo `RefreshToken` (longa duração) utilizando um token de atualização válido. O objetivo é manter a sessão do usuário ativa sem exigir novo login, garantindo ao mesmo tempo que, caso um Refresh Token vaze, ele só possa ser usado uma vez (devido à rotação), minimizando janelas de ataque.

#### **1. Request (Input)**

A requisição deve conter o token de atualização atual e, opcionalmente, o token de acesso expirado para validação cruzada (dependendo da implementação estrita), além de dados de telemetria.

| Campo | Tipo | Obrigatório | Descrição |
| --- | --- | --- | --- |
| `accessToken` | String | Não | O token JWT expirado. Pode ser usado para extrair claims de identidade se necessário. |
| `refreshToken` | String | Sim | O token opaco (GUID ou hash) emitido anteriormente. |
| `deviceInfo` | String | Não | User-Agent atual para auditoria de mudança de dispositivo. |
| `ipAddress` | String | Não | IP do cliente para detecção de anomalias (via header/contexto). |

**Exemplo de JSON (Request):**

```json
{
  "accessToken": "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...",
  "refreshToken": "8f902b74-3d23-448a-b450-13247963b2f5",
  "deviceInfo": "Mozilla/5.0 (iPhone; CPU iPhone OS 14_0 like Mac OS X)..."
}

```

#### **2. Regras de Negócio (Business Rules)**

As regras garantem que apenas sessões legítimas sejam renovadas.

* **RN-01 (Validade da Sessão):** O `RefreshToken` informado deve existir no banco de dados (`users.sessions`) e estar associado a uma sessão válida.
* **RN-02 (Expiração):** A data de expiração (`ExpiresAt`) da sessão persistida deve ser maior que a data/hora atual (`DateTime.UtcNow`).
* **RN-03 (Revogação Prévia):** O token não pode ter sido marcado como revogado (`IsRevoked`). A tentativa de uso de um token revogado deve ser tratada como um incidente de segurança (Replay Attack).
* **RN-04 (Rotação Obrigatória):** A operação de refresh é de uso único. O `RefreshToken` enviado na requisição deve ser invalidado imediatamente, e um novo par deve ser gerado.
* **RN-05 (Status do Usuário):** O usuário vinculado à sessão deve estar ativo no sistema (`IUserRepository`).

#### **3. Fluxo de Processamento (Workflow)**

1. **Recuperação da Sessão:** O Handler consulta o `ISessionRepository` buscando a entidade `Session` que contém o `RefreshToken` fornecido.
2. **Validação de Existência (RN-01):** Se a sessão não for encontrada, retorna `Error.NotFound` ou `Error.Unauthorized`.
3. **Verificação de Integridade (RN-02, RN-03):**
* Verifica se `session.ExpiresAt < DateTime.UtcNow`.
* Verifica se `session.IsRevoked` é verdadeiro.
* Caso positivo para qualquer um, a requisição é rejeitada.


4. **Revogação do Token Atual (RN-04):**
* Executa o método `session.Revoke()` na entidade recuperada.
* Dispara o evento de domínio `SessionRevokedEvent` para auditoria.


5. **Geração de Novos Tokens:**
* Recupera as informações do usuário (Roles/Claims) via `IUserRepository`.
* Invoca o `ITokenGenerator` para criar um novo `AccessToken` e um novo `RefreshToken`.


6. **Persistência da Nova Sessão:**
* Cria uma nova entidade `Session` com os novos dados (Novo Refresh Token, Nova Expiração).
* Adiciona a nova sessão ao `ISessionRepository`.


7. **Commit Transacional:** O `UnitOfWork` salva as alterações (Revogação da antiga + Criação da nova) atomicamente.
8. **Retorno:** Retorna o DTO com as novas credenciais.

#### **4. Response (Output)**

O retorno deve atualizar as credenciais do cliente.

**Exemplo de JSON (Sucesso - HTTP 200 OK):**

```json
{
  "isSuccess": true,
  "isFailure": false,
  "value": {
    "accessToken": "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.new...",
    "refreshToken": "a1b2c3d4-e5f6-7890-1234-567890abcdef",
    "expiresIn": 3600,
    "tokenType": "Bearer"
  },
  "error": {
    "code": "None",
    "message": "",
    "type": 0
  }
}

```

**Exemplo de JSON (Erro - HTTP 401 Unauthorized):**

```json
{
  "isSuccess": false,
  "isFailure": true,
  "value": null,
  "error": {
    "code": "Auth.InvalidToken",
    "message": "O token de atualização é inválido ou expirou.",
    "type": 4
  }
}

```
==============================================================================================================================================================
Com base na estrutura de segurança e gerenciamento de sessões do projeto, segue a documentação técnica detalhada para o comando `LogoutCommand` (abrangendo a revogação de sessão).

---

### **CMD-04: Realizar Logout (LogoutCommand)**

**Descrição Técnica:**
Este comando é responsável pelo encerramento seguro da sessão de um usuário. No contexto de Clean Architecture e CQRS, ele executa uma operação de escrita que invalida o `RefreshToken` persistido no banco de dados e adiciona o `JTI` (Json Token Identifier) do `AccessToken` atual a uma *blacklist* distribuída (Redis). Isso garante que, mesmo que o token de acesso ainda seja matematicamente válido (não expirado), ele não possa mais ser utilizado para autenticar requisições na API.

#### **1. Request (Input)**

A requisição deve identificar qual token de atualização (Refresh Token) deve ser invalidado. O Token de Acesso (Access Token) é geralmente extraído do cabeçalho `Authorization`, mas o Refresh Token deve ser enviado no corpo ou via cookie seguro.

| Campo | Tipo | Obrigatório | Descrição |
| --- | --- | --- | --- |
| `refreshToken` | String | Sim | O token de atualização opaco associado à sessão que se deseja encerrar. |
| `revokeAll` | Boolean | Não | Flag opcional. Se `true`, revoga **todas** as sessões ativas do usuário, não apenas a atual (útil para "Sair de todos os dispositivos"). |

**Exemplo de JSON (Request):**

```json
{
  "refreshToken": "8f902b74-3d23-448a-b450-13247963b2f5",
  "revokeAll": false
}

```

#### **2. Regras de Negócio (Business Rules)**

As regras focam na invalidação efetiva e na prevenção de reutilização de credenciais.

* **RN-01 (Existência da Sessão):** O `RefreshToken` informado deve corresponder a uma sessão existente no `ISessionRepository`.
* **RN-02 (Idempotência de Revogação):** Se a sessão já estiver marcada como revogada (`IsRevoked == true`), o sistema deve processar o comando com sucesso sem lançar erro, garantindo idempotência.
* **RN-03 (Invalidação de Acesso - Blacklist):** O identificador único (JTI) do JWT atual deve ser adicionado ao cache (Redis) com um tempo de vida (TTL) igual ao tempo restante de expiração do token. Isso impede o uso do token até que ele expire naturalmente.
* **RN-04 (Auditoria):** O encerramento da sessão deve disparar o evento `SessionRevokedEvent` para fins de log de segurança e auditoria.
* **RN-05 (Escopo de Revogação):** Caso a flag `revokeAll` seja verdadeira, o sistema deve iterar sobre todas as sessões ativas do `UserId` corrente e aplicar a revogação em cada uma.

#### **3. Fluxo de Processamento (Workflow)**

1. **Identificação do Contexto:** O Handler recupera o usuário atual e o JTI do token através do `ICurrentUserService` (extraído do HttpContext).
2. **Consulta de Sessão:**
* Se `revokeAll == false`: Consulta o `ISessionRepository` buscando a sessão específica pelo `refreshToken`.
* Se `revokeAll == true`: Consulta todas as sessões vinculadas ao `UserId` que ainda não expiraram.


3. **Processamento de Domínio (Entidade Session):**
* Para cada sessão encontrada, invoca o método `session.Revoke()`.
* Este método define `IsRevoked = true`, registra a data da revogação e adiciona o evento de domínio `SessionRevokedEvent`.


4. **Blacklisting (Cache):**
* Calcula o tempo restante do Access Token (`Exp - Now`).
* Chama o `ICacheService` (Redis) para adicionar a chave `blacklist:{jti}` com o TTL calculado.


5. **Persistência (UnitOfWork):**
* O repositório atualiza o estado das sessões no banco de dados (`users.sessions`).
* O `UnitOfWork.SaveChangesAsync` confirma a transação.


6. **Publicação de Eventos:** O evento de revogação é despachado (Outbox) para que outros serviços (ex: Notificações) possam reagir se necessário (ex: enviar email de aviso de novo login suspeito se fosse o caso, ou apenas logar).
7. **Retorno:** Retorna sucesso (`Result.Success()`) indicando que a solicitação foi processada.

#### **4. Response (Output)**

A resposta padrão para logout não necessita de corpo, apenas a confirmação da operação.

**Exemplo de JSON (Sucesso - HTTP 200 OK ou 204 No Content):**

```json
{
  "isSuccess": true,
  "isFailure": false,
  "value": null,
  "error": {
    "code": "None",
    "message": "",
    "type": 0
  }
}

```

**Exemplo de JSON (Erro - HTTP 400 Bad Request):**
*(Geralmente erros de logout são suprimidos por segurança, mas em caso de validação de formato inválido:)*

```json
{
  "isSuccess": false,
  "isFailure": true,
  "value": null,
  "error": {
    "code": "Auth.InvalidTokenFormat",
    "message": "O formato do refresh token é inválido.",
    "type": 4
  }
}

```
==============================================================================================================================================================
Com base na arquitetura de segurança e eventos do projeto, segue a documentação técnica detalhada para os fluxos de solicitação e execução de redefinição de senha.

---

### **CMD-05-A: Solicitar Redefinição de Senha (RequestPasswordResetCommand)**

**Descrição Técnica:**
Este comando inicia o fluxo de recuperação de conta (Forgot Password). Sua responsabilidade é validar a existência de um usuário ativo e gerar um token seguro de redefinição. Seguindo o padrão de arquitetura distribuída e assíncrona, este comando **não envia o e-mail diretamente**; em vez disso, ele persiste a intenção e dispara um evento de integração (`SendEmailIntegrationEvent`) para que o serviço de mensageria ou notificação processe o envio (SMTP/SendGrid) de forma desacoplada via *Outbox Pattern*.

#### **1. Request (Input)**

A requisição requer apenas o identificador principal do usuário.

| Campo | Tipo | Obrigatório | Descrição |
| --- | --- | --- | --- |
| `email` | String | Sim | O endereço de e-mail associado à conta que solicita a recuperação. |

**Exemplo de JSON (Request):**

```json
{
  "email": "bruno.dias@exemplo.com"
}

```

#### **2. Regras de Negócio (Business Rules)**

* **RN-01 (Segurança por Obscuridade):** Para evitar *User Enumeration Attacks*, o sistema deve retornar sucesso (HTTP 200/202) mesmo se o e-mail não for encontrado na base de dados.
* **RN-02 (Status da Conta):** Se o usuário estiver inativo ou banido, o token não deve ser gerado, mas o retorno da API permanece o mesmo (sucesso genérico).
* **RN-03 (Geração de Token):** O token gerado deve ser criptograficamente seguro, único e ter um tempo de expiração curto (ex: 15 a 60 minutos) gerenciado pelo `IIdentityService`.
* **RN-04 (Desacoplamento):** O envio do e-mail contendo o link/token deve ocorrer estritamente via processamento de evento assíncrono.

#### **3. Fluxo de Processamento (Workflow)**

1. **Validação de Entrada:** O `ValidationBehavior` verifica se o campo de e-mail é válido sintaticamente.
2. **Consulta de Usuário:** O Handler consulta o `IUserRepository` buscando pelo e-mail.
3. **Verificação Condicional (RN-01):**
* Se usuário **não encontrado**: O fluxo é interrompido e retorna `Result.Success` imediatamente (Silent Fail).
* Se usuário **encontrado**: Prossegue para a geração do token.


4. **Geração do Token:** O `IIdentityService` gera o token de redefinição de senha (ex: Provider de Reset do ASP.NET Core Identity ou token proprietário).
5. **Publicação de Evento:**
* Cria-se o evento `SendEmailIntegrationEvent` contendo: Destinatário, TemplateId (ResetPassword) e Payload (Nome, Token/Link).
* O evento é despachado via `IIntegrationEventPublisher` (ou Outbox).


6. **Retorno:** Retorna `Result.Success` com uma mensagem genérica instruindo o usuário a verificar seu e-mail.

#### **4. Response (Output)**

**Exemplo de JSON (Sucesso - HTTP 200 OK):**

```json
{
  "isSuccess": true,
  "isFailure": false,
  "value": null,
  "error": {
    "code": "None",
    "message": "",
    "type": 0
  }
}

```

---

### **CMD-05-B: Redefinir Senha (ResetPasswordCommand)**

**Descrição Técnica:**
Este comando finaliza o processo de recuperação. Ele recebe o token gerado anteriormente e a nova senha desejada. É uma operação crítica que envolve a validação estrita do token, a garantia das políticas de complexidade de senha e a atualização segura da credencial no banco de dados (hashing), invalidando o token utilizado.

#### **1. Request (Input)**

A requisição deve conter a prova de identidade (token) e a nova credencial.

| Campo | Tipo | Obrigatório | Descrição |
| --- | --- | --- | --- |
| `email` | String | Sim | E-mail do usuário (para validação cruzada com o token). |
| `token` | String | Sim | O token de segurança recebido por e-mail (URL Encoded). |
| `newPassword` | String | Sim | A nova senha a ser definida. |

**Exemplo de JSON (Request):**

```json
{
  "email": "bruno.dias@exemplo.com",
  "token": "CfDJ8K5...",
  "newPassword": "NewPassword@2025!"
}

```

#### **2. Regras de Negócio (Business Rules)**

* **RN-01 (Validade do Token):** O token deve ser verificado quanto à sua autenticidade, expiração e correspondência com o usuário informado. Tokens inválidos ou expirados devem bloquear a operação.
* **RN-02 (Complexidade de Senha):** A `newPassword` deve atender aos requisitos mínimos de segurança (tamanho, caracteres especiais, números), validados via FluentValidation ou `IPasswordHasher`.
* **RN-03 (Hashing Seguro):** A nova senha deve ser processada pelo `IPasswordHasher` antes de ser persistida. Nunca salvar em texto plano.
* **RN-04 (Evento de Auditoria):** A alteração bem-sucedida deve disparar o evento `PasswordChangedEvent` para notificar o usuário (segurança) e revogar sessões antigas se necessário.

#### **3. Fluxo de Processamento (Workflow)**

1. **Validação Inicial:** O `ValidationBehavior` verifica campos nulos e força da senha.
2. **Busca de Usuário:** O Handler recupera a entidade `ApplicationUser` via `IUserRepository`. Se não encontrado, retorna erro.
3. **Validação do Token (RN-01):** Invoca o `IIdentityService` para validar o token fornecido contra o usuário. Se falhar, retorna `Error.Validation` ("Token inválido ou expirado").
4. **Hashing (RN-03):** Utiliza `IPasswordHasher.Hash(newPassword)` para gerar o novo hash.
5. **Atualização da Entidade:**
* Chama o método de domínio `user.ChangePassword(passwordHash)`.
* Este método atualiza o hash e adiciona o evento de domínio `PasswordChangedEvent` à entidade.


6. **Persistência (UnitOfWork):**
* O repositório atualiza o registro.
* `UnitOfWork.SaveChangesAsync` persiste o novo hash e salva o evento no Outbox.


7. **Retorno:** Retorna `Result.Success` indicando que a senha foi alterada.

#### **4. Response (Output)**

**Exemplo de JSON (Sucesso - HTTP 200 OK):**

```json
{
  "isSuccess": true,
  "isFailure": false,
  "value": null,
  "error": {
    "code": "None",
    "message": "",
    "type": 0
  }
}

```

**Exemplo de JSON (Erro - HTTP 400 Bad Request):**

```json
{
  "isSuccess": false,
  "isFailure": true,
  "value": null,
  "error": {
    "code": "Auth.InvalidToken",
    "message": "Token inválido ou expirado.",
    "type": 4
  }
}

```
==============================================================================================================================================================
Com base na arquitetura de identidade e eventos do projeto `bcommerce-monolito`, segue a documentação técnica detalhada para os comandos de verificação de e-mail. Estes comandos são vitais para garantir a legitimidade da base de usuários e desbloquear funcionalidades restritas (como finalizar compras).

---

### **CMD-06-A: Solicitar Verificação de E-mail (SendEmailVerificationCommand)**

**Descrição Técnica:**
Este comando inicia o processo de confirmação de titularidade do endereço de e-mail. Assim como o fluxo de recuperação de senha, ele opera de forma assíncrona para não bloquear a resposta da API. O comando valida o estado do usuário e, se elegível, gera um token seguro que é despachado via evento de integração para o serviço de mensageria (Worker de E-mail).

#### **1. Request (Input)**

Geralmente acionado automaticamente após o registro (`RegisterUserCommand`), mas pode ser invocado manualmente caso o e-mail original se perca.

| Campo | Tipo | Obrigatório | Descrição |
| --- | --- | --- | --- |
| `email` | String | Sim | O e-mail do usuário que necessita de verificação. |

**Exemplo de JSON (Request):**

```json
{
  "email": "bruno.dias@exemplo.com"
}

```

#### **2. Regras de Negócio (Business Rules)**

* **RN-01 (Idempotência de Estado):** Se o usuário já estiver marcado como `EmailVerified = true`, o sistema não deve gerar novo token, mas pode reenviar uma notificação informativa ou simplesmente retornar sucesso sem ação.
* **RN-02 (Validação de Existência):** O e-mail deve pertencer a um usuário ativo no sistema (`IUserRepository`).
* **RN-03 (Tokenização):** O token gerado deve ser específico para confirmação de e-mail (diferente de reset de senha) e ter validade definida (ex: 24 horas).
* **RN-04 (Envio Assíncrono):** A responsabilidade de entrega do e-mail é do consumidor do evento `SendEmailIntegrationEvent`, garantindo resiliência contra falhas no servidor SMTP.

#### **3. Fluxo de Processamento (Workflow)**

1. **Validação Inicial:** O `ValidationBehavior` verifica o formato do e-mail.
2. **Consulta de Usuário:** O Handler busca o usuário pelo e-mail no `IUserRepository`.
3. **Verificação de Status (RN-01):**
* Se `user.EmailVerified` for verdadeiro: Retorna `Result.Success` imediatamente.


4. **Geração de Token:** Invoca `IIdentityService.GenerateEmailConfirmationTokenAsync(user)`.
5. **Criação de Evento de Integração:**
* Instancia `SendEmailIntegrationEvent`.
* Define `TemplateId` como "EmailConfirmation".
* Insere o Token e dados do usuário no Payload.


6. **Publicação:** Despacha o evento para o Message Bus (Outbox Pattern).
7. **Retorno:** HTTP 200 OK (Mensagem: "Se o e-mail for válido, as instruções foram enviadas").

#### **4. Response (Output)**

**Exemplo de JSON (Sucesso):**

```json
{
  "isSuccess": true,
  "isFailure": false,
  "value": null,
  "error": {
    "code": "None",
    "message": "",
    "type": 0
  }
}

```

---

### **CMD-06-B: Verificar E-mail (VerifyEmailCommand)**

**Descrição Técnica:**
Este comando processa a confirmação final. Ele recebe o token enviado por e-mail, valida sua assinatura criptográfica e integridade contra o usuário solicitante e, em caso de sucesso, transaciona a mudança de estado da entidade `ApplicationUser` para "Verificado".

#### **1. Request (Input)**

| Campo | Tipo | Obrigatório | Descrição |
| --- | --- | --- | --- |
| `email` | String | Sim | O e-mail do usuário (chave de busca). |
| `token` | String | Sim | O código/token de verificação recebido (URL Encoded). |

**Exemplo de JSON (Request):**

```json
{
  "email": "bruno.dias@exemplo.com",
  "token": "CfDJ8K5x..."
}

```

#### **2. Regras de Negócio (Business Rules)**

* **RN-01 (Validade do Token):** O token deve ser validado estritamente pelo `IIdentityService`. Tokens adulterados, expirados ou pertencentes a outro usuário devem rejeitar a operação.
* **RN-02 (Transição de Estado):** A propriedade `EmailVerified` da entidade `ApplicationUser` deve ser atualizada para `true`.
* **RN-03 (Registro de Auditoria):** A confirmação deve gerar um evento de domínio `UserEmailVerifiedEvent` para que outros módulos saibam que o usuário agora é confiável (ex: Módulo de Risco/Fraude).

#### **3. Fluxo de Processamento (Workflow)**

1. **Recuperação do Usuário:** Busca a entidade `ApplicationUser` no banco de dados. Retorna erro se não existir.
2. **Validação do Token (RN-01):**
* Chama `IIdentityService.ConfirmEmailAsync(user, token)`.
* Se o serviço de identidade retornar falha, o comando retorna `Result.Failure` (Token Inválido).


3. **Atualização de Domínio:**
* Executa o método `user.MarkEmailAsVerified()`.
* Este método define a flag interna e adiciona o evento `UserEmailVerifiedEvent` à lista de eventos de domínio.


4. **Persistência (UnitOfWork):**
* Salva a alteração no banco de dados.
* O evento de domínio é salvo na tabela `outbox_messages`.


5. **Retorno:** HTTP 200 OK ou 204 No Content.

#### **4. Response (Output)**

**Exemplo de JSON (Sucesso):**

```json
{
  "isSuccess": true,
  "isFailure": false,
  "value": null,
  "error": {
    "code": "None",
    "message": "",
    "type": 0
  }
}

```

**Exemplo de JSON (Erro - HTTP 400):**

```json
{
  "isSuccess": false,
  "isFailure": true,
  "value": null,
  "error": {
    "code": "Auth.InvalidToken",
    "message": "O link de verificação é inválido ou expirou.",
    "type": 4
  }
}

```

==============================================================================================================================================================
Com base na infraestrutura de Identidade do ASP.NET Core (`Microsoft.AspNetCore.Identity`) utilizada no projeto `bcommerce-monolito` e nos requisitos de segurança para autenticação de dois fatores, segue a documentação técnica detalhada.

---

### **CMD-07: Ativar MFA (EnableTwoFactorCommand)**

**Descrição Técnica:**
Este comando finaliza o processo de configuração da Autenticação de Dois Fatores (2FA/MFA). Ele pressupõe que o usuário já tenha solicitado a geração do segredo (via Query `GetTwoFactorConfig`) e escaneado o QR Code. O comando recebe o código de verificação (TOTP), valida a sincronia com o segredo gerado e, em caso de sucesso, altera o estado da conta (`TwoFactorEnabled = true`) e gera códigos de recuperação (Backup Codes) para emergências.

#### **1. Request (Input)**

| Campo | Tipo | Obrigatório | Descrição |
| --- | --- | --- | --- |
| `userId` | Guid | Sim | Identificador do usuário (obtido via Token/Contexto). |
| `verificationCode` | String | Sim | Código numérico de 6 dígitos gerado pelo aplicativo autenticador (ex: Google Authenticator). |

**Exemplo de JSON (Request):**

```json
{
  "verificationCode": "123456"
}

```

#### **2. Regras de Negócio (Business Rules)**

* **RN-01 (Validação de TOTP):** O código fornecido deve ser validado contra o segredo compartilhado (`AuthenticatorKey`) armazenado previamente. O sistema deve permitir uma pequena janela de tempo (clock drift) para evitar falhas de sincronismo.
* **RN-02 (Ativação):** A flag `TwoFactorEnabled` da entidade `ApplicationUser` só deve ser definida como `true` após a validação bem-sucedida do código.
* **RN-03 (Backup Codes):** Ao ativar o MFA, o sistema deve obrigatoriamente gerar um novo conjunto de códigos de recuperação (geralmente 10 códigos) e retorná-los ao usuário. Estes códigos devem ser persistidos hashs no banco de dados.
* **RN-04 (Segurança):** O segredo compartilhado não deve ser exposto na resposta deste comando; apenas os códigos de backup.

#### **3. Fluxo de Processamento (Workflow)**

1. **Contexto do Usuário:** O Handler identifica o usuário autenticado através do `ICurrentUserService`.
2. **Recuperação de Chave:** Consulta o `UserManager` para obter a chave de autenticação não formatada.
3. **Validação do Código (RN-01):** Invoca `VerifyTwoFactorTokenAsync(user, verificationCode)`. Se inválido, retorna `Error.Validation`.
4. **Persistência de Estado (RN-02):** Executa `SetTwoFactorEnabledAsync(user, true)` para ativar a proteção na conta.
5. **Geração de Backup (RN-03):** Executa `GenerateNewTwoFactorRecoveryCodesAsync(user, 10)`.
6. **Retorno:** Retorna os códigos de recuperação em texto plano para que o usuário os salve.

#### **4. Response (Output)**

**Exemplo de JSON (Sucesso - HTTP 200 OK):**

```json
{
  "isSuccess": true,
  "isFailure": false,
  "value": {
    "recoveryCodes": [
      "8f902-b743d",
      "23448-ab450",
      "13247-963b2",
      "f5a1b-2c3d4",
      "e5f67-89012"
    ]
  },
  "error": null
}

```

---

### **CMD-08: Verificar MFA no Login (VerifyTwoFactorLoginCommand)**

**Descrição Técnica:**
Este comando é a segunda etapa do processo de login (fluxo 2FA). Ele é invocado após o usuário fornecer credenciais válidas (Email/Senha) e receber uma resposta `RequiresTwoFactor` do sistema. O comando valida o código TOTP ou um código de recuperação e emite o Token JWT final.

#### **1. Request (Input)**

| Campo | Tipo | Obrigatório | Descrição |
| --- | --- | --- | --- |
| `email` | String | Sim | E-mail do usuário tentando logar. |
| `code` | String | Sim | Código TOTP (6 dígitos) ou Código de Recuperação. |
| `isRecoveryCode` | Boolean | Não | Indica se o código fornecido é um código de backup (Default: false). |
| `rememberMachine` | Boolean | Não | Se verdadeiro, define um cookie persistente para pular o 2FA neste dispositivo no futuro. |

**Exemplo de JSON (Request):**

```json
{
  "email": "bruno.dias@exemplo.com",
  "code": "654321",
  "isRecoveryCode": false,
  "rememberMachine": true
}

```

#### **2. Regras de Negócio (Business Rules)**

* **RN-01 (Validação de Login):** Deve utilizar `SignInManager.TwoFactorAuthenticatorSignInAsync` para códigos TOTP ou `TwoFactorRecoveryCodeSignInAsync` para códigos de backup.
* **RN-02 (Bloqueio):** Deve respeitar as políticas de *Lockout* em caso de múltiplas tentativas falhas de 2FA.
* **RN-03 (Consumo de Backup):** Se um código de recuperação for utilizado, ele deve ser invalidado imediatamente (uso único).
* **RN-04 (Emissão de Token):** O JWT (`AccessToken`) só deve ser gerado se a verificação do segundo fator for bem-sucedida.

#### **3. Fluxo de Processamento (Workflow)**

1. **Busca de Usuário:** Recupera o usuário pelo e-mail.
2. **Verificação de Tipo:**
* Se `isRecoveryCode == true`: Chama `TwoFactorRecoveryCodeSignInAsync`.
* Se `isRecoveryCode == false`: Chama `TwoFactorAuthenticatorSignInAsync`.


3. **Tratamento de Resultado:**
* **Sucesso:** Gera o JWT e Refresh Token via `ITokenGenerator` (Idêntico ao Login padrão).
* **Bloqueio:** Retorna erro `Auth.AccountLocked`.
* **Falha:** Retorna erro `Auth.InvalidCode`.


4. **Auditoria:** Registra o evento de login bem-sucedido via MFA no `LoginHistory`.

#### **4. Response (Output)**

**Exemplo de JSON (Sucesso - HTTP 200 OK):**

```json
{
  "isSuccess": true,
  "value": {
    "accessToken": "eyJhbGciOiJIUzI1NiIsInR5...",
    "refreshToken": "8f902b74...",
    "expiresIn": 3600
  }
}

```

**Exemplo de JSON (Erro - HTTP 401 Unauthorized):**

```json
{
  "isSuccess": false,
  "error": {
    "code": "Auth.InvalidTwoFactorCode",
    "message": "Código de verificação inválido."
  }
}

```

---

### **Video Recomendado**

Para visualizar a implementação prática deste fluxo usando ASP.NET Core Identity, este vídeo detalha desde a configuração do serviço até a geração de QR Codes e validação.

[Authentication made easy with ASP.NET Core Identity in .NET 8](https://www.youtube.com/watch?v=S0RSsHKiD6Y)

Este vídeo é relevante pois demonstra a configuração moderna do Identity no .NET 8, alinhada com a stack tecnológica do seu projeto monólito.
==============================================================================================================================================================
Com base na estrutura de domínio do módulo `Users` e na separação de responsabilidades entre Credenciais (Identity) e Perfil (Profile), segue a documentação técnica detalhada para o comando `UpdateUserProfileCommand`.

---

### **CMD-08: Atualizar Perfil de Usuário (UpdateUserProfileCommand)**

**Descrição Técnica:**
Este comando é responsável pela atualização dos dados cadastrais "pessoais" do usuário, dissociados das credenciais de acesso (login/senha). Ele atua sobre a entidade `Profile`, que é um Agregado (ou entidade forte vinculada ao `ApplicationUser`). O comando segue o princípio de encapsulamento, garantindo que a atualização ocorra através de métodos de domínio que validam a integridade dos Value Objects (`PhoneNumber`) e regras de consistência antes da persistência via Entity Framework Core.

#### **1. Request (Input)**

A requisição não necessita do ID do usuário no corpo (body), pois este deve ser inferido do contexto de segurança (Token JWT) para garantir que um usuário não altere o perfil de outro.

| Campo | Tipo | Obrigatório | Descrição |
| --- | --- | --- | --- |
| `displayName` | String | Sim | Nome de exibição do usuário na plataforma. |
| `phoneNumber` | String | Não | Número de telefone/celular. Deve respeitar formato válido para conversão em Value Object. |
| `dateOfBirth` | Date | Não | Data de nascimento do usuário (formato `YYYY-MM-DD`). |

**Exemplo de JSON (Request):**

```json
{
  "displayName": "Bruno Dias",
  "phoneNumber": "11999887766",
  "dateOfBirth": "1990-05-15"
}

```

#### **2. Regras de Negócio (Business Rules)**

As regras asseguram a integridade dos dados demográficos.

* **RN-01 (Integridade do Value Object):** O campo `phoneNumber`, se fornecido, deve ser passível de conversão para o Value Object `PhoneNumber`. Formatos inválidos ou com caracteres não numéricos excessivos devem ser rejeitados pelo domínio.
* **RN-02 (Coerência Temporal):** O campo `dateOfBirth` não pode ser uma data futura. O sistema pode, opcionalmente, impor uma idade mínima (ex: 18 anos) dependendo da política de vendas, mas a regra base é ser uma data no passado.
* **RN-03 (Propriedade do Recurso):** Apenas o próprio usuário autenticado pode alterar seu perfil. O `UserId` utilizado na consulta ao repositório deve ser extraído estritamente do `ICurrentUserService`.
* **RN-04 (Obrigatoriedade Condicional):** Embora `Profile` seja criado no registro, campos como `DisplayName` não podem ser definidos como vazios ou nulos durante a atualização.

#### **3. Fluxo de Processamento (Workflow)**

1. **Identificação de Contexto:** O Handler utiliza o `ICurrentUserService` para obter o `UserId` do contexto da requisição HTTP atual.
2. **Consulta ao Repositório:**
* Invoca `IProfileRepository.GetByUserIdAsync(userId)`.
* Caso o perfil não seja encontrado (inconsistência de dados), retorna `Error.NotFound`.


3. **Instanciação de Value Objects:**
* Tenta criar a instância de `PhoneNumber` a partir da string fornecida. Se o formato for inválido, uma `DomainException` é capturada e convertida em resultado de falha.


4. **Execução de Método de Domínio:**
* Chama o método `profile.UpdateDetails(displayName, phoneNumber, dateOfBirth)`.
* Este método atualiza as propriedades internas e atualiza o campo de auditoria `LastModified`.
* O método adiciona o evento `ProfileUpdatedEvent` à lista de eventos de domínio da entidade.


5. **Persistência (UnitOfWork):**
* O EF Core detecta as mudanças no rastreamento (Change Tracker).
* `UnitOfWork.SaveChangesAsync` é executado, persistindo os dados na tabela `users.profiles`.


6. **Retorno:** Retorna `Result.Success()` (HTTP 200/204).

#### **4. Response (Output)**

**Exemplo de JSON (Sucesso - HTTP 200 OK):**

```json
{
  "isSuccess": true,
  "isFailure": false,
  "value": null,
  "error": {
    "code": "None",
    "message": "",
    "type": 0
  }
}

```

**Exemplo de JSON (Erro - HTTP 400 Bad Request):**

```json
{
  "isSuccess": false,
  "isFailure": true,
  "value": null,
  "error": {
    "code": "Profile.InvalidPhoneNumber",
    "message": "O número de telefone fornecido não é válido.",
    "type": 2
  }
}

```
==============================================================================================================================================================
Com base na análise do domínio `Users` e na necessidade de manipulação de ativos digitais (Blob Storage), segue a documentação técnica detalhada para os comandos de gerenciamento de Avatar.

---

### **CMD-09-A: Realizar Upload de Avatar (UploadUserAvatarCommand)**

**Descrição Técnica:**
Este comando gerencia a atualização da imagem de perfil do usuário. Diferente de comandos puramente transacionais (JSON), este endpoint geralmente consome dados em formato `multipart/form-data` para permitir o envio eficiente de streams binários. O Handler orquestra a validação do arquivo, o upload para um provedor de armazenamento em nuvem (via abstração `IStorageService`) e a atualização da referência (URL) na entidade `Profile`.

#### **1. Request (Input)**

A requisição deve ser enviada preferencialmente como `multipart/form-data`.

| Campo | Tipo | Obrigatório | Descrição |
| --- | --- | --- | --- |
| `file` | Binary (IFormFile) | Sim | O arquivo de imagem a ser processado. |

**Exemplo de Payload (Representação Conceitual):**

```json
// Header: Content-Type: multipart/form-data; boundary=----WebKitFormBoundary...

// Body Part:
{
  "key": "file",
  "filename": "meu-avatar.jpg",
  "contentType": "image/jpeg",
  "content": "<binary_data_stream>"
}

```

#### **2. Regras de Negócio (Business Rules)**

As regras focam na segurança, desempenho e consistência visual.

* **RN-01 (Validação de Formato):** O sistema deve aceitar apenas tipos de imagem web-safe e performáticos: `.jpg`, `.jpeg`, `.png` e `.webp`. Arquivos executáveis ou scripts disfarçados devem ser rejeitados imediatamente.
* **RN-02 (Limite de Tamanho):** O arquivo não deve exceder um limite configurado (ex: 2MB) para evitar consumo excessivo de banda e armazenamento.
* **RN-03 (Limpeza de Órfãos):** Se o usuário já possuir um avatar configurado (`Profile.AvatarUrl != null`), a imagem antiga deve ser excluída do Storage Provider antes ou durante a substituição para evitar arquivos "fantasmas" (custo desnecessário).
* **RN-04 (Sanitização de Nome):** O nome do arquivo salvo no storage deve ser gerado pelo sistema (ex: GUID ou Hash) para evitar conflitos e caracteres inseguros.

#### **3. Fluxo de Processamento (Workflow)**

1. **Identificação de Contexto:** O Handler recupera o `UserId` do usuário autenticado via `ICurrentUserService`.
2. **Validação de Entrada:** O `ValidationBehavior` (ou validador manual no Handler para streams) verifica a extensão (`Path.GetExtension`) e o tamanho (`Length`) do stream. Retorna `ValidationException` em caso de violação.
3. **Recuperação do Perfil:** Consulta o `IProfileRepository` para obter a entidade `Profile` atual.
4. **Processamento de Substituição (RN-03):**
* Verifica se `profile.AvatarUrl` possui valor.
* Se sim, invoca `IStorageService.DeleteAsync(profile.AvatarUrl)` para remover o recurso antigo.


5. **Upload:**
* Gera um nome único (ex: `{UserId}_{Guid}.webp`).
* Invoca `IStorageService.UploadAsync(stream, fileName, contentType)`.
* Recebe como retorno a URL pública/assinada do novo recurso.


6. **Atualização de Domínio:**
* Executa o método `profile.SetAvatarUrl(newUrl)`.
* Este método atualiza a propriedade e o carimbo de tempo `LastModified`.


7. **Persistência (UnitOfWork):** Salva as alterações na tabela `users.profiles`.
8. **Retorno:** Retorna a nova URL da imagem.

#### **4. Response (Output)**

**Exemplo de JSON (Sucesso - HTTP 200 OK):**

```json
{
  "isSuccess": true,
  "isFailure": false,
  "value": {
    "avatarUrl": "https://storage.bcommerce.com/avatars/3fa85f64-5717-4562-b3fc-2c963f66afa6_v1.webp"
  },
  "error": {
    "code": "None",
    "message": "",
    "type": 0
  }
}

```

**Exemplo de JSON (Erro - HTTP 400 Bad Request):**

```json
{
  "isSuccess": false,
  "isFailure": true,
  "value": null,
  "error": {
    "code": "Storage.InvalidFormat",
    "message": "O formato .exe não é permitido. Use JPG, PNG ou WEBP.",
    "type": 2
  }
}

```

---

### **CMD-09-B: Remover Avatar (DeleteUserAvatarCommand)**

**Descrição Técnica:**
Este comando remove a imagem de perfil personalizada do usuário, revertendo para o estado padrão (null). Ele garante a exclusão física do arquivo no provedor de nuvem para liberar espaço e a limpeza da referência no banco de dados.

#### **1. Request (Input)**

A requisição não necessita de corpo, apenas a intenção autenticada.

| Campo | Tipo | Obrigatório | Descrição |
| --- | --- | --- | --- |
| - | - | - | Nenhum parâmetro de corpo necessário. |

**Exemplo de JSON (Request):**

```json
{}

```

#### **2. Regras de Negócio (Business Rules)**

* **RN-01 (Idempotência):** Se o usuário não tiver um avatar (`AvatarUrl` é null), o comando deve retornar sucesso sem realizar operações no Storage, garantindo idempotência.
* **RN-02 (Exclusão Física):** A URL armazenada deve ser utilizada para localizar e deletar o blob correspondente no Storage Service. Falhas na deleção do arquivo (ex: storage indisponível) devem ser tratadas (logadas), mas idealmente não devem impedir a limpeza da referência no banco, ou devem usar um mecanismo de retentativa.

#### **3. Fluxo de Processamento (Workflow)**

1. **Recuperação do Perfil:** O Handler busca a entidade `Profile` via `IProfileRepository` utilizando o ID do usuário logado.
2. **Verificação de Estado (RN-01):**
* Se `profile.AvatarUrl` for nulo ou vazio, retorna `Result.Success()` imediatamente.


3. **Exclusão no Storage:**
* Invoca `IStorageService.DeleteAsync(profile.AvatarUrl)`.


4. **Atualização de Domínio:**
* Executa o método `profile.RemoveAvatar()`.
* Define a propriedade `AvatarUrl` como `null`.


5. **Persistência (UnitOfWork):** Confirma a alteração no banco de dados.
6. **Retorno:** Retorna sucesso (HTTP 204 No Content ou 200 OK).

#### **4. Response (Output)**

**Exemplo de JSON (Sucesso - HTTP 200 OK):**

```json
{
  "isSuccess": true,
  "isFailure": false,
  "value": null,
  "error": {
    "code": "None",
    "message": "",
    "type": 0
  }
}

```
=============================================================================================================================================================
Com base na entidade de domínio `NotificationPreference` e na estrutura do módulo `Users` do projeto `bcommerce-monolito`, segue a documentação técnica detalhada para o comando `UpdateUserPreferencesCommand`.

---

### **CMD-10: Atualizar Preferências do Usuário (UpdateUserPreferencesCommand)**

**Descrição Técnica:**
Este comando permite que o usuário autenticado modifique suas configurações pessoais de sistema e comunicação. Ele atua sobre a entidade `NotificationPreference`, que é um Agregado (ou extensão do Agregado User) responsável por armazenar configurações de opt-in para marketing, idioma da interface (i18n) e tema visual (UI). O comando segue o princípio de responsabilidade única, isolando configurações de preferências de dados cadastrais críticos.

#### **1. Request (Input)**

O `UserId` não trafega no corpo da requisição para evitar *Insecure Direct Object References (IDOR)*; ele deve ser extraído do token de autenticação.

| Campo | Tipo | Obrigatório | Descrição |
| --- | --- | --- | --- |
| `receiveMarketingEmails` | Boolean | Não | Flag para optar por receber ou não e-mails de marketing/newsletter. |
| `receivePromotionalSms` | Boolean | Não | Flag para optar por receber ou não SMS promocionais. |
| `preferredLanguage` | String | Sim | Código de cultura/idioma (ex: "pt-BR", "en-US"). |
| `theme` | String | Sim | Identificador do tema visual (ex: "Light", "Dark", "System"). |

**Exemplo de JSON (Request):**

```json
{
  "receiveMarketingEmails": true,
  "receivePromotionalSms": false,
  "preferredLanguage": "pt-BR",
  "theme": "Dark"
}

```

#### **2. Regras de Negócio (Business Rules)**

* **RN-01 (Validação de Idioma):** O campo `preferredLanguage` deve ser validado contra uma lista branca (whitelist) de culturas suportadas pelo sistema (definidas em configuração ou constante de domínio).
* **RN-02 (Validação de Tema):** O campo `theme` deve corresponder a um valor aceito pelo Enumeration ou conjunto de constantes de UI suportados.
* **RN-03 (Inicialização Tardia - Lazy Creation):** Caso o registro de preferências ainda não exista para o usuário (cenário de migração ou erro na criação do usuário), o sistema deve criar um novo registro com os valores padrão antes de aplicar a atualização, garantindo resiliência.
* **RN-04 (Segurança):** A modificação é restrita estritamente ao usuário autenticado (Contexto `CurrentUser`).

#### **3. Fluxo de Processamento (Workflow)**

1. **Contexto de Segurança:** O Handler obtém o `UserId` através do `ICurrentUserService`.
2. **Consulta de Repositório:**
* Invoca `INotificationPreferenceRepository.GetByUserIdAsync(userId)`.


3. **Verificação de Existência (RN-03):**
* Se o registro retornar nulo: Instancia uma nova entidade `NotificationPreference` vinculada ao usuário com defaults.
* Se existir: Utiliza a entidade rastreada pelo EF Core.


4. **Validação de Domínio (RN-01, RN-02):**
* Verifica se o `preferredLanguage` é suportado.
* Verifica se o `theme` é válido.
* Retorna `DomainException` ou `ValidationException` em caso de falha.


5. **Atualização de Estado:**
* Executa o método de domínio `preferences.UpdateSettings(marketing, sms, language, theme)`.
* A entidade atualiza suas propriedades e o carimbo de tempo `LastModified`.


6. **Persistência (UnitOfWork):**
* Se for novo registro: `Repository.Add(preferences)`.
* Se for atualização: O Change Tracker detecta as mudanças.
* `UnitOfWork.SaveChangesAsync` confirma a transação na tabela `users.notification_preferences`.


7. **Retorno:** `Result.Success()` (HTTP 204 ou 200).

#### **4. Response (Output)**

**Exemplo de JSON (Sucesso - HTTP 200 OK):**

```json
{
  "isSuccess": true,
  "isFailure": false,
  "value": null,
  "error": {
    "code": "None",
    "message": "",
    "type": 0
  }
}

```

**Exemplo de JSON (Erro - HTTP 400 Bad Request):**

```json
{
  "isSuccess": false,
  "isFailure": true,
  "value": null,
  "error": {
    "code": "Preferences.UnsupportedLanguage",
    "message": "O idioma 'fr-FR' não é suportado pelo sistema.",
    "type": 2
  }
}

```
=============================================================================================================================================================

### Comandos de Endereços

Com base na análise do módulo `Users`, especificamente as entidades `Address` e `ApplicationUser`, bem como a necessidade de validação externa de dados geográficos, segue a documentação técnica detalhada para o comando `AddUserAddressCommand`.

---

### **CMD-11: Adicionar Endereço do Usuário (AddUserAddressCommand)**

**Descrição Técnica:**
Este comando é responsável por registrar um novo endereço de entrega ou cobrança vinculado ao perfil do usuário. No fluxo CQRS, ele atua como uma operação de escrita que envolve validações de formato, verificação de existência real do logradouro (via integração com serviços externos de CEP) e a gestão da regra de exclusividade do "Endereço Padrão".

#### **1. Request (Input)**

O `UserId` é obtido implicitamente do contexto de segurança (`ICurrentUserService`). O corpo da requisição deve conter os dados detalhados do logradouro.

| Campo | Tipo | Obrigatório | Descrição |
| --- | --- | --- | --- |
| `street` | String | Sim | Nome da rua, avenida ou logradouro. |
| `number` | String | Sim | Número do imóvel. |
| `complement` | String | Não | Dados complementares (Apto, Bloco, Referência). |
| `neighborhood` | String | Sim | Bairro ou distrito. |
| `city` | String | Sim | Nome da cidade. |
| `state` | String | Sim | Sigla do estado (UF). |
| `country` | String | Sim | País (Default: "BR"). |
| `zipCode` | String | Sim | Código postal (CEP) formatado ou apenas números. |
| `isDefault` | Boolean | Sim | Indica se este será o endereço principal do usuário. |

**Exemplo de JSON (Request):**

```json
{
  "street": "Avenida Paulista",
  "number": "1000",
  "complement": "Apto 501",
  "neighborhood": "Bela Vista",
  "city": "São Paulo",
  "state": "SP",
  "country": "BR",
  "zipCode": "01310-100",
  "isDefault": true
}

```

#### **2. Regras de Negócio (Business Rules)**

* **RN-01 (Validação de CEP):** O `zipCode` fornecido deve ser validado quanto ao formato e existência. O sistema deve consultar um serviço externo (ex: ViaCEP) para garantir que o código postal corresponde à cidade e estado informados.
* **RN-02 (Exclusividade de Padrão):** Um usuário pode ter apenas um endereço marcado como `IsDefault = true`. Se o novo endereço for criado com essa flag, o sistema deve automaticamente remover a marcação de "Padrão" de qualquer outro endereço pré-existente do mesmo usuário.
* **RN-03 (Limite de Endereços):** (Opcional) O sistema pode impor um limite máximo de endereços cadastrados por usuário (ex: 5 ou 10) para evitar abuso, retornando `Error.Validation` caso excedido.
* **RN-04 (Normalização):** O `state` deve ser armazenado preferencialmente em formato de sigla (UF) maiúscula de 2 caracteres.

#### **3. Fluxo de Processamento (Workflow)**

1. **Contexto de Segurança:** O Handler recupera o `UserId` do usuário autenticado.
2. **Validação de Sintaxe:** O `ValidationBehavior` verifica obrigatoriedade de campos e tamanho máximo de strings.
3. **Enriquecimento/Validação Externa (RN-01):**
* O Handler invoca o `IAddressService` (ou Gateway de CEP) para validar o `zipCode`.
* Se o serviço externo retornar inexistente, lança `DomainException` ("CEP não encontrado").
* Opcionalmente, preenche/corrige `Street`, `Neighborhood` e `City` com os dados oficiais retornados.


4. **Gestão de Endereço Padrão (RN-02):**
* Se `request.IsDefault == true`:
* O Handler consulta `IAddressRepository` buscando o endereço atual marcado como padrão para este usuário.
* Se existir, executa o método `existingAddress.UnsetDefault()`.




5. **Construção da Entidade:**
* Instancia a entidade `Address` utilizando o Value Object `PostalCode`.
* Define a propriedade `IsDefault` conforme a requisição.
* Vincula ao `UserId`.


6. **Persistência (UnitOfWork):**
* Adiciona a nova entidade via `IAddressRepository.AddAsync`.
* O `UnitOfWork` rastreia as alterações (inclusão do novo + possível alteração do antigo padrão).
* `SaveChanges` persiste a transação atomicamente.


7. **Eventos:** Dispara o evento de domínio `AddressAddedEvent`.
8. **Retorno:** Retorna o ID do novo endereço.

#### **4. Response (Output)**

**Exemplo de JSON (Sucesso - HTTP 201 Created):**

```json
{
  "isSuccess": true,
  "isFailure": false,
  "value": {
    "addressId": "a1b2c3d4-e5f6-7890-1234-567890abcdef"
  },
  "error": {
    "code": "None",
    "message": "",
    "type": 0
  }
}

```

**Exemplo de JSON (Erro - HTTP 400 Bad Request):**

```json
{
  "isSuccess": false,
  "isFailure": true,
  "value": null,
  "error": {
    "code": "Address.InvalidPostalCode",
    "message": "O CEP informado não foi encontrado na base de dados nacional.",
    "type": 2
  }
}

```
=============================================================================================================================================================
Com base na arquitetura de segurança e persistência do módulo `Users`, segue a documentação técnica detalhada para os fluxos de atualização e remoção de endereços.

---

### **CMD-12-A: Atualizar Endereço do Usuário (UpdateUserAddressCommand)**

**Descrição Técnica:**
Este comando processa a mutação de dados de um endereço existente. Ele implementa verificações de segurança estritas para evitar *Insecure Direct Object References* (IDOR), garantindo que um usuário mal-intencionado não consiga alterar endereços de terceiros, mesmo possuindo o ID do recurso. O fluxo suporta a atualização de dados cadastrais e a redefinição do endereço "Padrão", mantendo a consistência do agregado.

#### **1. Request (Input)**

O ID do endereço deve ser fornecido (via rota ou corpo), e o ID do usuário é inferido do contexto de segurança.

| Campo | Tipo | Obrigatório | Descrição |
| --- | --- | --- | --- |
| `addressId` | Guid | Sim | Identificador único do endereço a ser modificado. |
| `street` | String | Sim | Nome da rua/logradouro atualizado. |
| `number` | String | Sim | Número do imóvel. |
| `complement` | String | Não | Complemento (Apto, Bloco). |
| `neighborhood` | String | Sim | Bairro. |
| `city` | String | Sim | Cidade. |
| `state` | String | Sim | Estado (UF). |
| `zipCode` | String | Sim | CEP atualizado. |
| `isDefault` | Boolean | Sim | Flag indicando se este passará a ser o endereço principal. |

**Exemplo de JSON (Request):**

```json
{
  "addressId": "a1b2c3d4-e5f6-7890-1234-567890abcdef",
  "street": "Rua da Consolação",
  "number": "2000",
  "complement": "",
  "neighborhood": "Consolação",
  "city": "São Paulo",
  "state": "SP",
  "zipCode": "01302-001",
  "isDefault": true
}

```

#### **2. Regras de Negócio (Business Rules)**

* **RN-01 (Propriedade do Recurso - IDOR):** O endereço solicitado (`addressId`) deve existir e a propriedade `UserId` do registro deve corresponder estritamente ao usuário autenticado (`ICurrentUserService`). Caso contrário, deve retornar `Error.NotFound` (para não revelar existência) ou `Error.Forbidden`.
* **RN-02 (Validação de CEP):** Caso o `zipCode` tenha sido alterado, deve-se revalidar sua existência via serviço externo, similar ao processo de criação.
* **RN-03 (Gestão de Endereço Padrão):** Se `isDefault` for alterado de `false` para `true`, o sistema deve buscar qualquer outro endereço do usuário marcado como padrão e desmarcá-lo na mesma transação.
* **RN-04 (Imutabilidade de Histórico):** Endereços vinculados a pedidos já faturados/enviados não devem ser alterados estruturalmente se o sistema utilizar referência por ID nos pedidos. (Nota: No `bcommerce`, os pedidos copiam os dados do endereço (Value Object `ShippingAddress`), permitindo a edição livre no cadastro do usuário sem afetar pedidos históricos).

#### **3. Fluxo de Processamento (Workflow)**

1. **Contexto e Recuperação:** O Handler identifica o `UserId` corrente.
2. **Consulta (Read):** Invoca `IAddressRepository.GetByIdAsync(addressId)`.
3. **Validação de Segurança (RN-01):**
* Verifica se o endereço existe.
* Verifica se `address.UserId == currentUserId`.
* Se falhar, interrompe com erro.


4. **Validação Externa (RN-02):** Se o CEP mudou, consulta o Gateway de CEP.
5. **Atualização de Estado:**
* Executa `address.Update(street, number, ...)` na entidade de domínio.
* Se `request.IsDefault` for verdadeiro, invoca o serviço de domínio ou lógica no Handler para iterar sobre outros endereços do usuário e definir `IsDefault = false` (RN-03).


6. **Persistência (Write):**
* O `UnitOfWork` captura as alterações.
* `SaveChanges` persiste a atualização no banco.


7. **Evento:** Dispara `AddressUpdatedEvent` (auditável).
8. **Retorno:** `Result.Success()`.

#### **4. Response (Output)**

**Exemplo de JSON (Sucesso - HTTP 200 OK):**

```json
{
  "isSuccess": true,
  "isFailure": false,
  "value": null,
  "error": {
    "code": "None",
    "message": "",
    "type": 0
  }
}

```

---

### **CMD-12-B: Remover Endereço do Usuário (DeleteUserAddressCommand)**

**Descrição Técnica:**
Este comando realiza a exclusão lógica (*Soft Delete*) de um endereço. A exclusão física não ocorre imediatamente para manter integridade referencial e histórico de auditoria. O comando garante que o usuário só possa remover seus próprios endereços.

#### **1. Request (Input)**

| Campo | Tipo | Obrigatório | Descrição |
| --- | --- | --- | --- |
| `addressId` | Guid | Sim | Identificador do endereço a ser removido. |

**Exemplo de JSON (Request):**

```json
{
  "addressId": "a1b2c3d4-e5f6-7890-1234-567890abcdef"
}

```

#### **2. Regras de Negócio (Business Rules)**

* **RN-01 (Verificação de Propriedade):** O endereço deve pertencer ao usuário autenticado.
* **RN-02 (Restrição de Endereço Padrão):** (Opcional) O sistema pode impedir a exclusão do endereço marcado como "Padrão" (`IsDefault == true`), exigindo que o usuário defina outro endereço como padrão antes de excluir este, para evitar inconsistências no checkout. Caso implementado, retorna `Error.Validation`.
* **RN-03 (Soft Delete):** A entidade não deve ser removida fisicamente do banco de dados (DELETE SQL). Em vez disso, a propriedade `IsDeleted` deve ser definida como `true` e `DeletedAt` preenchida com o timestamp atual, processo geralmente automatizado pelo `SoftDeleteInterceptor`.

#### **3. Fluxo de Processamento (Workflow)**

1. **Recuperação:** O Handler busca a entidade via `IAddressRepository`.
2. **Validação (RN-01):** Confirma se o registro pertence ao `UserId` do token.
3. **Verificação de Regra (RN-02):** Se o endereço for o padrão, verifica a política de exclusão (Bloquear ou permitir).
4. **Operação de Remoção:**
* Executa `IAddressRepository.Remove(address)`.
* A entidade é marcada como `Deleted` no ChangeTracker do EF Core.


5. **Intercepção (RN-03):**
* Durante o `UnitOfWork.SaveChangesAsync`, o `SoftDeleteInterceptor` intercepta o estado `Deleted`.
* Altera o estado para `Modified`.
* Define `IsDeleted = true`.


6. **Eventos:** Dispara `AddressDeletedEvent`.
7. **Retorno:** Sucesso (HTTP 204 No Content).

#### **4. Response (Output)**

**Exemplo de JSON (Sucesso - HTTP 200/204):**

```json
{
  "isSuccess": true,
  "isFailure": false,
  "value": null,
  "error": {
    "code": "None",
    "message": "",
    "type": 0
  }
}

```

**Exemplo de JSON (Erro - HTTP 400 Bad Request):**

```json
{
  "isSuccess": false,
  "isFailure": true,
  "value": null,
  "error": {
    "code": "Address.CannotDeleteDefault",
    "message": "Não é possível excluir o endereço padrão. Defina outro como padrão antes.",
    "type": 4
  }
}

```
=============================================================================================================================================================
Com base na lógica de gestão de endereços e na necessidade de consistência atômica no agregado de Usuários/Endereços, segue a documentação técnica detalhada para o comando `SetDefaultAddressCommand`.

---

### **CMD-13: Definir Endereço Padrão (SetDefaultAddressCommand)**

**Descrição Técnica:**
Este comando tem a responsabilidade única de alterar a configuração de preferência de entrega/cobrança do usuário, elegendo um endereço específico como "Principal" (`IsDefault = true`). No contexto de CQRS e DDD, esta é uma operação de escrita que exige garantir uma invariante de integridade: **apenas um endereço pode ser padrão por usuário**. O comando deve orquestrar a desativação da flag no endereço padrão anterior (se houver) e a ativação no novo alvo, dentro de uma única transação atômica.

#### **1. Request (Input)**

A requisição necessita apenas do identificador do endereço alvo. O identificador do usuário é obtido via contexto de segurança.

| Campo | Tipo | Obrigatório | Descrição |
| --- | --- | --- | --- |
| `addressId` | Guid | Sim | O UUID do endereço que passará a ser o padrão. |

**Exemplo de JSON (Request):**

```json
{
  "addressId": "a1b2c3d4-e5f6-7890-1234-567890abcdef"
}

```

#### **2. Regras de Negócio (Business Rules)**

* **RN-01 (Verificação de Propriedade - IDOR):** O endereço informado (`addressId`) deve existir no banco de dados e estar vinculado estritamente ao `UserId` do usuário autenticado. Tentativas de manipular endereços de terceiros devem resultar em erro de permissão ou não encontrado.
* **RN-02 (Exclusividade Mútua):** Ao definir um endereço como padrão, o sistema deve garantir que nenhum outro endereço deste usuário permaneça com `IsDefault = true`.
* **RN-03 (Idempotência):** Se o endereço alvo já for o endereço padrão atual, o sistema deve detectar isso e finalizar a operação com sucesso sem realizar alterações no banco de dados (no-op), economizando I/O.
* **RN-04 (Atomicidade):** A atualização (unset do antigo + set do novo) deve ocorrer na mesma transação de banco de dados (`UnitOfWork`) para evitar estados inconsistentes (ex: dois endereços padrão ou nenhum).

#### **3. Fluxo de Processamento (Workflow)**

1. **Identificação de Contexto:** O Handler recupera o `UserId` através do `ICurrentUserService`.
2. **Consulta do Alvo:** Invoca `IAddressRepository.GetByIdAsync(addressId)`.
3. **Validação de Segurança (RN-01):**
* Se endereço não encontrado ou `address.UserId != currentUserId`: Retorna `Error.NotFound`.


4. **Verificação de Estado (RN-03):**
* Se `address.IsDefault == true`: Retorna `Result.Success()` imediatamente.


5. **Gestão da Troca (RN-02):**
* O Handler consulta o repositório buscando o endereço padrão *atual* do usuário (ex: `GetDefaultByUserId(userId)`).
* Se existir um endereço padrão antigo, executa o método de domínio `oldAddress.UnsetDefault()`.
* Executa o método de domínio no novo endereço: `targetAddress.SetDefault()`.


6. **Persistência (RN-04):**
* O `UnitOfWork` detecta as alterações nas entidades rastreadas (uma ou duas entidades modificadas).
* `SaveChangesAsync` executa os comandos SQL de UPDATE.


7. **Evento:** Dispara `AddressUpdatedEvent` (para sincronização de read-models ou cache).
8. **Retorno:** Sucesso (HTTP 204 No Content).

#### **4. Response (Output)**

**Exemplo de JSON (Sucesso - HTTP 200 OK):**

```json
{
  "isSuccess": true,
  "isFailure": false,
  "value": null,
  "error": {
    "code": "None",
    "message": "",
    "type": 0
  }
}

```

**Exemplo de JSON (Erro - HTTP 404 Not Found):**

```json
{
  "isSuccess": false,
  "isFailure": true,
  "value": null,
  "error": {
    "code": "Address.NotFound",
    "message": "O endereço solicitado não foi encontrado.",
    "type": 3
  }
}

```
=============================================================================================================================================================
### Comandos de Administração (Backoffice)

Com base na análise do ciclo de vida do Agregado `ApplicationUser` e nos requisitos de conformidade (como LGPD/GDPR) e integridade transacional do sistema `bcommerce-monolito`, segue a documentação técnica detalhada para os comandos de gestão do estado da conta.

Estes comandos manipulam a propriedade `IsActive` e a interface `ISoftDeletable`, garantindo que o encerramento de uma conta propague efeitos colaterais necessários (como cancelamento de pedidos) através de Eventos de Integração.

---

### **CMD-14-A: Desativar Conta de Usuário (DeactivateUserAccountCommand)**

**Descrição Técnica:**
Este comando coloca a conta do usuário em um estado de suspensão temporária (`Inactive`). Diferente da exclusão, os dados permanecem visíveis para administradores e o usuário pode, futuramente, solicitar reativação. No fluxo CQRS, esta operação de escrita é crítica pois dispara um evento de integração (`UserAccountDeactivatedEvent`) que orquestra o cancelamento preventivo de pedidos não processados no módulo de **Orders**, mitigando riscos de envio de produtos para usuários bloqueados.

#### **1. Request (Input)**

A requisição exige confirmação de segurança para evitar desativações acidentais ou maliciosas (em caso de sessão sequestrada).

| Campo | Tipo | Obrigatório | Descrição |
| --- | --- | --- | --- |
| `password` | String | Sim | Senha atual do usuário para confirmação da operação (Sudo Mode). |
| `reason` | String | Não | Motivo da desativação (para fins de feedback e métricas de churn). |

**Exemplo de JSON (Request):**

```json
{
  "password": "Password@123!",
  "reason": "Não estou utilizando a plataforma com frequência."
}

```

#### **2. Regras de Negócio (Business Rules)**

* **RN-01 (Validação de Segurança):** A senha informada deve corresponder ao hash armazenado para o usuário atual. Falha na verificação deve rejeitar o comando (`Error.Unauthorized`).
* **RN-02 (Transição de Estado):** O usuário não pode ser desativado se já estiver no estado `IsActive = false`.
* **RN-03 (Efeito Colateral - Pedidos):** A desativação deve garantir que novos pedidos não possam ser criados (bloqueio no Login/Auth) e que pedidos em aberto ("Placed" ou "PendingPayment") sejam cancelados assincronamente.
* **RN-04 (Revogação de Sessão):** Todas as sessões ativas (Refresh Tokens) devem ser revogadas imediatamente.

#### **3. Fluxo de Processamento (Workflow)**

1. **Contexto:** Recupera o `UserId` do contexto HTTP.
2. **Consulta:** Obtém a entidade `ApplicationUser` via `IUserRepository`.
3. **Verificação de Senha (RN-01):** Invoca `IPasswordHasher.Verify()`. Retorna erro se inválido.
4. **Alteração de Estado:**
* Executa `user.Deactivate()`.
* Define `IsActive = false`.
* Adiciona o evento de domínio `UserDeactivatedEvent`.


5. **Integração (Outbox):**
* O manipulador do evento de domínio publica um `IntegrationEvent` (ex: `UserAccountDeactivatedIntegrationEvent`).
* O Módulo **Orders** consome este evento para cancelar pedidos pendentes.
* O Módulo **Identity** consome este evento para revogar tokens.


6. **Persistência:** `UnitOfWork.SaveChangesAsync`.
7. **Retorno:** Sucesso (HTTP 200/204).

#### **4. Response (Output)**

**Exemplo de JSON (Sucesso):**

```json
{
  "isSuccess": true,
  "isFailure": false,
  "value": null,
  "error": { "code": "None", "message": "", "type": 0 }
}

```

---

### **CMD-14-B: Reativar Conta de Usuário (ReactivateUserAccountCommand)**

**Descrição Técnica:**
Este comando reverte o estado de suspensão, permitindo que o usuário volte a efetuar login e realizar compras. Geralmente, este comando é executado por um Administrador (Backoffice) ou através de um fluxo específico de recuperação de conta via e-mail seguro.

#### **1. Request (Input)**

Assume-se aqui um cenário administrativo ou fluxo autenticado via token de recuperação.

| Campo | Tipo | Obrigatório | Descrição |
| --- | --- | --- | --- |
| `userId` | Guid | Sim | O ID do usuário a ser reativado. |

**Exemplo de JSON (Request):**

```json
{
  "userId": "3fa85f64-5717-4562-b3fc-2c963f66afa6"
}

```

#### **2. Regras de Negócio (Business Rules)**

* **RN-01 (Estado Prévio):** O usuário deve estar atualmente inativo (`IsActive = false`). Tentar reativar um usuário ativo deve ser idempotente (retornar sucesso) ou informativo.
* **RN-02 (Bloqueio de Excluídos):** Usuários marcados como excluídos (`IsDeleted = true`) não podem ser reativados por este comando.
* **RN-03 (Auditoria):** A ação deve ser logada no `AuditLog` para rastreabilidade de quem realizou o desbloqueio.

#### **3. Fluxo de Processamento (Workflow)**

1. **Consulta:** Busca o usuário no repositório.
2. **Validação (RN-02):** Verifica se o usuário não está Soft Deleted (geralmente filtrado automaticamente pelo QueryFilter do EF Core, a menos que `IgnoreQueryFilters` seja usado).
3. **Alteração de Domínio:**
* Executa `user.Activate()`.
* Define `IsActive = true`.


4. **Persistência:** Salva a alteração no banco de dados.
5. **Retorno:** Sucesso.

#### **4. Response (Output)**

**Exemplo de JSON (Sucesso):**

```json
{
  "isSuccess": true,
  "isFailure": false,
  "value": null,
  "error": { "code": "None", "message": "", "type": 0 }
}

```

---

### **CMD-14-C: Excluir Conta de Usuário (DeleteUserAccountCommand)**

**Descrição Técnica:**
Este comando implementa a exclusão lógica (*Soft Delete*) da conta, atendendo ao "Direito ao Esquecimento" (dentro dos limites legais de retenção de dados fiscais). A entidade implementa `ISoftDeletable`, o que significa que o registro permanece no banco de dados para integridade referencial histórica, mas é invisível para a aplicação através de Global Query Filters e interceptadores do EF Core.

#### **1. Request (Input)**

Requer confirmação de senha obrigatória.

| Campo | Tipo | Obrigatório | Descrição |
| --- | --- | --- | --- |
| `password` | String | Sim | Senha para autorizar a exclusão permanente. |
| `confirmDeletion` | Boolean | Sim | Flag explícita de confirmação de intenção. |

**Exemplo de JSON (Request):**

```json
{
  "password": "Password@123!",
  "confirmDeletion": true
}

```

#### **2. Regras de Negócio (Business Rules)**

* **RN-01 (Confirmação de Identidade):** Validação estrita da senha via `IPasswordHasher`.
* **RN-02 (Soft Delete):** A entidade não deve ser removida fisicamente (`DELETE FROM`). O `SoftDeleteInterceptor` deve interceptar a remoção e configurar `IsDeleted = true` e `DeletedAt = DateTime.UtcNow`.
* **RN-03 (Integridade de Pedidos):** Usuários com pedidos "Em Andamento" (Enviado, Em Processamento) não devem conseguir excluir a conta até a conclusão do ciclo de vida do pedido, para evitar inconsistências de entrega. Retornar `Error.Conflict`.
* **RN-04 (Anonimização - Opcional):** Dependendo da política de privacidade, dados PII (Nome, CPF, Email) podem ser ofuscados/anonimizados no momento do Soft Delete.
* **RN-05 (Evento de Integração):** Disparar `UserDeletedIntegrationEvent` para que outros módulos (Cart, Notifications) limpem dados associados não essenciais.

#### **3. Fluxo de Processamento (Workflow)**

1. **Validação Inicial:** Verifica senha e flag `confirmDeletion`.
2. **Verificação de Pendências (RN-03):** Consulta o Módulo de Orders (ou serviço de domínio) para checar pedidos ativos. Se houver, bloqueia.
3. **Remoção no Repositório:**
* Invoca `IUserRepository.Remove(user)`.
* O EF Core marca o estado como `Deleted`.


4. **Intercepção (Infrastructure):**
* O `SoftDeleteInterceptor` detecta o estado `Deleted`.
* Altera o estado para `Modified`.
* Define a propriedade `IsDeleted` e `DeletedAt`.
* Adiciona o evento de domínio `UserDeletedEvent`.


5. **Persistência e Outbox:** Commit da transação e salvamento do evento.
6. **Retorno:** Sucesso (Logout forçado no cliente).

#### **4. Response (Output)**

**Exemplo de JSON (Sucesso - HTTP 200 OK):**

```json
{
  "isSuccess": true,
  "isFailure": false,
  "value": null,
  "error": { "code": "None", "message": "", "type": 0 }
}

```

**Exemplo de JSON (Erro - HTTP 409 Conflict):**

```json
{
  "isSuccess": false,
  "isFailure": true,
  "value": null,
  "error": {
    "code": "User.CannotDeleteWithActiveOrders",
    "message": "Não é possível excluir a conta enquanto houver pedidos em entrega pendente.",
    "type": 3
  }
}

```

=============================================================================================================================================================
Com base na infraestrutura de `Identity` (`ApplicationRole`, `IdentityService`) e no sistema de permissões baseadas em Claims (`Bcommerce.BuildingBlocks.Security`), segue a documentação técnica para o gerenciamento de Controle de Acesso Baseado em Funções (RBAC).

---

### **CMD-15-A: Criar Nova Função (CreateRoleCommand)**

**Descrição Técnica:**
Este comando é responsável pela definição de novos perfis de acesso (Roles) no sistema. Além de criar a entidade `ApplicationRole`, o comando permite a atribuição inicial de um conjunto de permissões (Claims do tipo `Permission`). A operação interage diretamente com o `RoleManager` do ASP.NET Core Identity e valida as permissões contra o catálogo estático do sistema (`Permissions` constants) para evitar a persistência de strings arbitrárias ou inválidas.

#### **1. Request (Input)**

| Campo | Tipo | Obrigatório | Descrição |
| --- | --- | --- | --- |
| `name` | String | Sim | Nome único da função (ex: "Gerente de Vendas"). |
| `description` | String | Não | Descrição explicativa sobre o escopo da função. |
| `permissions` | List<String> | Não | Lista de chaves de permissão (ex: "Catalog.Read", "Orders.Manage"). |

**Exemplo de JSON (Request):**

```json
{
  "name": "SupportAgent",
  "description": "Nível de acesso para atendimento ao cliente (Leitura de Pedidos).",
  "permissions": [
    "Orders.Read",
    "Customers.Read",
    "Catalog.Read"
  ]
}

```

#### **2. Regras de Negócio (Business Rules)**

* **RN-01 (Unicidade de Nome):** O nome da Role deve ser único em todo o sistema (Case Insensitive).
* **RN-02 (Validade das Permissões):** Todas as strings enviadas na lista `permissions` devem existir na classe estática de referência `Permissions` (BuildingBlocks). Permissões inexistentes devem gerar `Error.Validation`.
* **RN-03 (Nomenclatura):** O nome não deve conter caracteres especiais ou espaços excessivos que dificultem a gestão via API/CLI.

#### **3. Fluxo de Processamento (Workflow)**

1. **Validação de Entrada:** O `ValidationBehavior` verifica se o nome foi fornecido.
2. **Verificação de Existência (RN-01):** Invoca `RoleManager.RoleExistsAsync(name)`. Se `true`, retorna `Error.Conflict`.
3. **Validação de Permissões (RN-02):** Itera sobre a lista de `permissions` comparando com a Reflection/Lista de constantes de permissões do sistema.
4. **Criação da Entidade:**
* Instancia `ApplicationRole(name)`.
* Executa `RoleManager.CreateAsync(role)`.


5. **Atribuição de Claims:**
* Para cada permissão válida, cria um `Claim` (Type: "Permission", Value: "{Permissao}").
* Executa `RoleManager.AddClaimAsync(role, claim)`.


6. **Retorno:** Retorna o ID da Role criada.

#### **4. Response (Output)**

**Exemplo de JSON (Sucesso - HTTP 201 Created):**

```json
{
  "isSuccess": true,
  "isFailure": false,
  "value": {
    "roleId": "e3b0c442-98fc-1c14-9af2-1234567890ab"
  },
  "error": { "code": "None", "message": "", "type": 0 }
}

```

---

### **CMD-15-B: Atualizar Função (UpdateRoleCommand)**

**Descrição Técnica:**
Este comando gerencia a mutação de uma Role existente. A complexidade principal reside na sincronização das permissões (Claims). O comando deve calcular a diferença entre as permissões atuais e as novas, removendo as revogadas e adicionando as novas, garantindo que o estado final da Role reflita exatamente o payload recebido.

#### **1. Request (Input)**

| Campo | Tipo | Obrigatório | Descrição |
| --- | --- | --- | --- |
| `roleId` | Guid | Sim | Identificador da função a ser editada. |
| `name` | String | Sim | Novo nome (ou o atual, caso não mude). |
| `permissions` | List<String> | Sim | Lista completa e atualizada de permissões desejadas. |

**Exemplo de JSON (Request):**

```json
{
  "roleId": "e3b0c442-98fc-1c14-9af2-1234567890ab",
  "name": "SupportAgent_L2",
  "permissions": [
    "Orders.Read",
    "Orders.Manage", 
    "Customers.Read"
  ]
}

```

#### **2. Regras de Negócio (Business Rules)**

* **RN-01 (Imutabilidade de Sistema):** Roles críticas do sistema (ex: "Administrator", "SuperUser") podem ter restrições de edição de nome ou exclusão de certas permissões fundamentais para evitar *lockout*.
* **RN-02 (Sincronização de Claims):** A lista de permissões enviada é autoritativa. Permissões que existiam no banco mas não estão na lista devem ser removidas.
* **RN-03 (Invalidação de Cache):** A alteração de permissões deve sinalizar a invalidação de caches de autorização ou exigir que usuários com esta role façam re-login (ou refresh token) para receberem as novas Claims.

#### **3. Fluxo de Processamento (Workflow)**

1. **Recuperação:** Busca a role via `RoleManager.FindByIdAsync(roleId)`. Retorna `Error.NotFound` se inexistente.
2. **Atualização de Propriedades:**
* Se o nome mudou, verifica unicidade e atualiza via `RoleManager.SetRoleNameAsync`.
* Persiste alterações básicas via `RoleManager.UpdateAsync`.


3. **Gestão de Permissões (Claims):**
* Obtém claims atuais: `RoleManager.GetClaimsAsync(role)`.
* **Remove:** Identifica claims que estão no banco mas não no Request e executa `RemoveClaimAsync`.
* **Adiciona:** Identifica claims que estão no Request mas não no banco e executa `AddClaimAsync`.


4. **Evento:** Dispara `RolePermissionsUpdatedEvent` (útil para invalidar cache distribuído de permissões).
5. **Retorno:** Sucesso.

#### **4. Response (Output)**

**Exemplo de JSON (Sucesso):**

```json
{
  "isSuccess": true,
  "isFailure": false,
  "value": null,
  "error": { "code": "None", "message": "", "type": 0 }
}

```

---

### **CMD-15-C: Excluir Função (DeleteRoleCommand)**

**Descrição Técnica:**
Este comando remove permanentemente uma Role do sistema. A operação exige validação rigorosa de integridade referencial para impedir que usuários fiquem "órfãos" de função ou que Roles sistêmicas sejam apagadas acidentalmente.

#### **1. Request (Input)**

| Campo | Tipo | Obrigatório | Descrição |
| --- | --- | --- | --- |
| `roleId` | Guid | Sim | Identificador da função a ser removida. |

**Exemplo de JSON (Request):**

```json
{
  "roleId": "e3b0c442-98fc-1c14-9af2-1234567890ab"
}

```

#### **2. Regras de Negócio (Business Rules)**

* **RN-01 (Proteção de Uso):** Não é permitido excluir uma Role que esteja atribuída a um ou mais usuários ativos (`Database.UserRoles`). Deve-se desassociar os usuários antes.
* **RN-02 (Roles Protegidas):** As Roles semeadas (Seed) pelo sistema (ex: "Admin", "Customer") são imutáveis e não podem ser excluídas via API. O sistema deve manter uma lista de IDs ou Nomes protegidos.

#### **3. Fluxo de Processamento (Workflow)**

1. **Recuperação:** Busca a role pelo ID.
2. **Validação de Sistema (RN-02):** Verifica se o nome da role consta na lista de protegidas (`IsSystemRole`). Se sim, retorna `Error.Forbidden`.
3. **Validação de Uso (RN-01):**
* Consulta a tabela de junção `AspNetUserRoles` (via `UserManager` ou Query direta).
* Se houver usuários vinculados, retorna `Error.Conflict` ("A função possui usuários associados").


4. **Exclusão:**
* Executa `RoleManager.DeleteAsync(role)`.


5. **Retorno:** Sucesso.

#### **4. Response (Output)**

**Exemplo de JSON (Sucesso):**

```json
{
  "isSuccess": true,
  "isFailure": false,
  "value": null,
  "error": { "code": "None", "message": "", "type": 0 }
}

```

**Exemplo de JSON (Erro - Conflito):**

```json
{
  "isSuccess": false,
  "isFailure": true,
  "value": null,
  "error": {
    "code": "Role.HasAssignedUsers",
    "message": "Não é possível excluir uma função que está em uso por usuários ativos.",
    "type": 3
  }
}

```
=============================================================================================================================================================
Com base na infraestrutura de identidade do ASP.NET Core Identity utilizada no projeto (`Bcommerce.Modules.Users.Infrastructure.Identity`), segue a documentação técnica detalhada para os comandos de associação e dissociação de perfis (Roles).

Estes comandos são fundamentais para a governança de acessos (RBAC - Role Based Access Control), permitindo elevar ou restringir privilégios de usuários dinamicamente.

---

### **CMD-16-A: Atribuir Função ao Usuário (AssignRoleToUserCommand)**

**Descrição Técnica:**
Este comando associa uma `ApplicationRole` existente a um `ApplicationUser`. No fluxo CQRS, trata-se de uma operação de escrita que modifica a tabela de junção `AspNetUserRoles` do Identity. A operação impacta diretamente as Claims que serão geradas no próximo login (ou refresh token) do usuário, concedendo-lhe novas permissões.

#### **1. Request (Input)**

A requisição deve identificar inequivocamente o usuário alvo e o nome da função a ser atribuída.

| Campo | Tipo | Obrigatório | Descrição |
| --- | --- | --- | --- |
| `userId` | Guid | Sim | O Identificador único (UUID) do usuário. |
| `roleName` | String | Sim | O nome da função a ser atribuída (ex: "Manager", "Admin"). |

**Exemplo de JSON (Request):**

```json
{
  "userId": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
  "roleName": "Manager"
}

```

#### **2. Regras de Negócio (Business Rules)**

* **RN-01 (Existência de Entidades):** Tanto o usuário (`userId`) quanto a função (`roleName`) devem existir no banco de dados. Referências inválidas devem retornar `Error.NotFound`.
* **RN-02 (Idempotência):** Se o usuário já possuir a função solicitada, o comando deve ser processado com sucesso sem realizar alterações no banco de dados, evitando exceções de chave duplicada.
* **RN-03 (Nível de Privilégio):** Apenas usuários com permissão elevada (ex: Policy "AdminOnly" ou Claim "Roles.Manage") podem executar este comando.
* **RN-04 (Auditoria):** A alteração de privilégios deve gerar um registro de auditoria (`UserRoleAssignedEvent`) para rastreabilidade de segurança.

#### **3. Fluxo de Processamento (Workflow)**

1. **Validação de Entrada:** O `ValidationBehavior` verifica se os campos ID e Nome não são nulos/vazios.
2. **Busca de Usuário:** O Handler invoca `UserManager.FindByIdAsync(userId)`. Se nulo, retorna erro.
3. **Busca de Função:** O Handler invoca `RoleManager.RoleExistsAsync(roleName)`. Se falso, retorna erro.
4. **Verificação de Vínculo Prévio (RN-02):**
* Invoca `UserManager.IsInRoleAsync(user, roleName)`.
* Se verdadeiro, retorna `Result.Success()` imediatamente.


5. **Persistência:**
* Invoca `UserManager.AddToRoleAsync(user, roleName)`.
* O ASP.NET Identity gerencia a inserção na tabela `AspNetUserRoles`.


6. **Evento de Domínio:** Dispara `UserRoleAssignedEvent` (opcional, dependendo da implementação de auditoria).
7. **Retorno:** HTTP 200 OK ou 204 No Content.

#### **4. Response (Output)**

**Exemplo de JSON (Sucesso):**

```json
{
  "isSuccess": true,
  "isFailure": false,
  "value": null,
  "error": { "code": "None", "message": "", "type": 0 }
}

```

**Exemplo de JSON (Erro - Role Inexistente):**

```json
{
  "isSuccess": false,
  "isFailure": true,
  "value": null,
  "error": {
    "code": "Identity.RoleNotFound",
    "message": "A função 'SuperAdmin' não existe no sistema.",
    "type": 3
  }
}

```

---

### **CMD-16-B: Remover Função do Usuário (RemoveRoleFromUserCommand)**

**Descrição Técnica:**
Este comando revoga privilégios de um usuário removendo a associação com uma Role específica. A operação deve ser imediata na base de dados, embora a efetiva perda de acesso possa depender da expiração do Token JWT atual (ou implementação de *Security Stamp* validation).

#### **1. Request (Input)**

| Campo | Tipo | Obrigatório | Descrição |
| --- | --- | --- | --- |
| `userId` | Guid | Sim | O Identificador único do usuário alvo. |
| `roleName` | String | Sim | O nome da função a ser removida. |

**Exemplo de JSON (Request):**

```json
{
  "userId": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
  "roleName": "Manager"
}

```

#### **2. Regras de Negócio (Business Rules)**

* **RN-01 (Idempotência Segura):** Tentar remover uma função que o usuário não possui não deve gerar erro, mas sim retornar sucesso (o estado final desejado foi atingido).
* **RN-02 (Proteção de Auto-Lockout):** O sistema deve impedir que o último usuário com a role "Admin" remova a própria permissão de administrador, evitando que o sistema fique órfão de gerenciamento (regra opcional, mas recomendada).
* **RN-03 (Roles Básicas):** Dependendo da regra de negócio, pode-se impedir a remoção da role "Customer" se isso for requisito para a existência da conta.

#### **3. Fluxo de Processamento (Workflow)**

1. **Recuperação de Entidades:**
* Busca o usuário via `UserManager`.
* Verifica existência da Role via `RoleManager`.


2. **Verificação de Associação:**
* Verifica se o usuário possui a role (`IsInRoleAsync`).
* Se não possuir, retorna sucesso imediatamente (RN-01).


3. **Execução da Remoção:**
* Invoca `UserManager.RemoveFromRoleAsync(user, roleName)`.


4. **Invalidação de Segurança:**
* Atualiza o `SecurityStamp` do usuário (`UserManager.UpdateSecurityStampAsync`). Isso força a invalidação de cookies/tokens em sistemas que validam o stamp a cada requisição ou na renovação do refresh token.


5. **Persistência:** Commit automático gerenciado pelo Identity Framework.
6. **Retorno:** Sucesso.

#### **4. Response (Output)**

**Exemplo de JSON (Sucesso):**

```json
{
  "isSuccess": true,
  "isFailure": false,
  "value": null,
  "error": { "code": "None", "message": "", "type": 0 }
}

```

**Exemplo de JSON (Erro - Usuário Não Encontrado):**

```json
{
  "isSuccess": false,
  "isFailure": true,
  "value": null,
  "error": {
    "code": "Identity.UserNotFound",
    "message": "Usuário não encontrado.",
    "type": 3
  }
}

```
=============================================================================================================================================================
Com base na arquitetura de leitura do módulo `Users` e nas melhores práticas de performance com Entity Framework Core (Projeções e No-Tracking), segue a documentação técnica detalhada para as consultas de detalhamento de usuário.

---

### **QRY-01-A: Obter Usuário por ID (GetUserByIdQuery)**

**Descrição Técnica:**
Esta query é responsável por recuperar a visão completa dos dados de um usuário específico através de seu identificador único (UUID). Seguindo o padrão CQRS, esta operação é estritamente de leitura, otimizada para performance. Ela projeta os dados agregados das entidades `ApplicationUser` e `Profile` em um objeto de transferência de dados (`UserDetailedDto`), evitando o vazamento de informações sensíveis (como `PasswordHash`) e eliminando o overhead de rastreamento de mudanças do ORM.

#### **1. Request (Input)**

O parâmetro de busca é o identificador primário.

| Campo | Tipo | Obrigatório | Descrição |
| --- | --- | --- | --- |
| `userId` | Guid | Sim | Identificador único do usuário no sistema. |

**Exemplo de JSON (Request - via Query String ou Route):**

```json
// GET /api/users/3fa85f64-5717-4562-b3fc-2c963f66afa6
{
  "userId": "3fa85f64-5717-4562-b3fc-2c963f66afa6"
}

```

#### **2. Regras de Negócio (Business Rules)**

* **RN-01 (Isolamento de Leitura):** A consulta deve ser executada com `AsNoTracking()` para garantir que as entidades não sejam anexadas ao `ChangeTracker` do EF Core, reduzindo consumo de memória e CPU.
* **RN-02 (Validação de Existência):** Caso o ID informado não corresponda a nenhum registro na tabela `users.users`, a query deve retornar uma exceção do tipo `NotFoundException` ou um resultado nulo tratado pelo Controller.
* **RN-03 (Enriquecimento de Dados):** O retorno deve obrigatoriamente realizar o *Join* (Eager Loading ou Projeção) com a tabela `users.profiles` para fornecer nome, avatar e preferências em uma única ida ao banco.
* **RN-04 (Segurança de Dados):** Campos sensíveis como `PasswordHash`, `SecurityStamp` e `ConcurrencyStamp` devem ser estritamente omitidos do DTO de resposta.

#### **3. Fluxo de Processamento (Workflow)**

1. **Validação de Entrada:** Verifica se o `userId` é um GUID válido e não vazio.
2. **Acesso ao Contexto de Dados:** O Handler acessa o `UsersDbContext`.
3. **Construção da Query (LINQ):**
* Seleciona a entidade `ApplicationUser`.
* Inclui a entidade relacionada `Profile` (`.Include(u => u.Profile)`).
* Aplica filtro: `.Where(u => u.Id == request.UserId)`.
* Desabilita rastreamento: `.AsNoTracking()`.


4. **Projeção/Mapeamento:**
* Transforma o resultado da consulta no `UserDetailedDto` (manualmente ou via AutoMapper).
* Combina dados de login (Email, Status, Roles) com dados de perfil (Nome, Avatar).


5. **Verificação (RN-02):** Se o resultado for nulo, retorna erro de recurso não encontrado.
6. **Retorno:** Devolve o DTO preenchido com status HTTP 200.

#### **4. Response (Output)**

**Exemplo de JSON (Sucesso - HTTP 200 OK):**

```json
{
  "isSuccess": true,
  "isFailure": false,
  "value": {
    "id": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
    "email": "bruno.dias@exemplo.com",
    "cpf": "123.456.789-00",
    "firstName": "Bruno",
    "lastName": "Dias",
    "fullName": "Bruno Dias",
    "displayName": "BrunoD",
    "phoneNumber": "11999998888",
    "avatarUrl": "https://storage.bcommerce.com/avatars/bruno.webp",
    "dateOfBirth": "1990-05-15T00:00:00Z",
    "isActive": true,
    "emailVerified": true,
    "joinedAt": "2024-01-10T14:30:00Z",
    "roles": ["Customer", "BetaTester"]
  },
  "error": { "code": "None", "message": "", "type": 0 }
}

```

**Exemplo de JSON (Erro - HTTP 404 Not Found):**

```json
{
  "isSuccess": false,
  "isFailure": true,
  "value": null,
  "error": {
    "code": "User.NotFound",
    "message": "O usuário com o ID fornecido não foi encontrado.",
    "type": 3
  }
}

```

---

### **QRY-01-B: Obter Usuário por E-mail (GetUserByEmailQuery)**

**Descrição Técnica:**
Similar à consulta por ID, esta query permite localizar um usuário utilizando seu endereço de e-mail como chave de busca. É frequentemente utilizada em fluxos administrativos (Backoffice) ou validações de pré-cadastro para verificar duplicidade ou recuperar detalhes de contato.

#### **1. Request (Input)**

| Campo | Tipo | Obrigatório | Descrição |
| --- | --- | --- | --- |
| `email` | String | Sim | O endereço de e-mail exato do usuário. |

**Exemplo de JSON (Request - via Query String):**

```json
// GET /api/users/by-email?email=bruno.dias@exemplo.com
{
  "email": "bruno.dias@exemplo.com"
}

```

#### **2. Regras de Negócio (Business Rules)**

* **RN-01 (Normalização):** A busca deve ser insensível a maiúsculas/minúsculas (Case Insensitive), garantindo que `Bruno.Dias@...` encontre `bruno.dias@...`.
* **RN-02 (Performance):** A coluna de e-mail deve estar indexada no banco de dados para garantir performance em O(1) ou O(log n), evitando *Full Table Scan*.
* **RN-03 (Consistência):** Segue as mesmas regras de `AsNoTracking()` e projeção de DTO da consulta por ID.

#### **3. Fluxo de Processamento (Workflow)**

1. **Validação de Entrada:** Verifica se o formato do e-mail é válido.
2. **Consulta:**
* `_context.Users.AsNoTracking()`
* `.Include(u => u.Profile)`
* `.FirstOrDefaultAsync(u => u.Email == request.Email)`


3. **Projeção:** Mapeia a entidade encontrada para `UserDetailedDto`.
4. **Tratamento de Nulo:** Se não encontrar, retorna `Error.NotFound`.
5. **Retorno:** Devolve o DTO.

#### **4. Response (Output)**

O schema de resposta é idêntico ao **QRY-01-A**.

**Exemplo de JSON (Sucesso - HTTP 200 OK):**

```json
{
  "isSuccess": true,
  "value": {
    "id": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
    "email": "bruno.dias@exemplo.com",
    "fullName": "Bruno Dias",
    "isActive": true,
    // ... outros campos do UserDetailedDto
  }
}

```
=============================================================================================================================================================
Com base na arquitetura de `BuildingBlocks.Security` (para extração de identidade) e `BuildingBlocks.Caching` (para a estratégia de Cache-Aside) do projeto `bcommerce-monolito`, segue a documentação técnica detalhada para a query `GetCurrentUserQuery`.

---

### **QRY-02: Obter Usuário Atual (GetCurrentUserQuery)**

**Descrição Técnica:**
Esta query é responsável por retornar os dados do perfil do usuário autenticado que está realizando a requisição. Diferente da busca por ID genérica, esta operação depende estritamente do contexto de segurança (`ClaimsPrincipal`) para inferir o sujeito da ação. Para otimizar a latência em cenários de alta leitura (ex: carregamento inicial de SPA/Mobile), esta query implementa o padrão **Cache-Aside** utilizando Redis, evitando *round-trips* desnecessários ao banco de dados para dados que raramente mudam.

#### **1. Request (Input)**

Como o identificador do usuário é derivado do Token JWT (Bearer Token) presente no cabeçalho `Authorization`, o corpo da requisição (payload) para esta query é vazio.

| Campo | Tipo | Obrigatório | Descrição |
| --- | --- | --- | --- |
| - | - | - | Nenhum parâmetro de corpo é necessário. O `UserId` é extraído do `HttpContext`. |

**Exemplo de JSON (Request):**

```json
{}

```

*(A requisição deve conter o Header: `Authorization: Bearer <JWT_TOKEN>`)*

#### **2. Regras de Negócio (Business Rules)**

* **RN-01 (Contexto de Segurança Obrigatório):** A execução desta query exige um usuário autenticado. Caso o `ICurrentUserService.UserId` seja nulo ou inválido, deve-se interromper o fluxo com `UnauthorizedException`.
* **RN-02 (Estratégia Cache-Aside):** O sistema deve consultar primeiramente o cache distribuído (Redis). O acesso ao banco de dados (`UsersDbContext`) só deve ocorrer em caso de *Cache Miss*.
* **RN-03 (Chave de Cache e TTL):** A chave utilizada no Redis deve seguir o padrão `user:{UserId}:profile`. O tempo de vida (TTL) do cache deve ser configurado (ex: 15 ou 30 minutos) para garantir eventual consistência.
* **RN-04 (Read-Through/População de Cache):** Ao buscar os dados no banco após um *Cache Miss*, o sistema deve automaticamente persistir o resultado no Redis antes de retornar ao cliente, acelerando leituras subsequentes.
* **RN-05 (Integridade):** Se o usuário existir no Token mas não no banco de dados (ex: conta excluída fisicamente ou banco restaurado), deve retornar `Error.NotFound` ou `Error.Unauthorized`.

#### **3. Fluxo de Processamento (Workflow)**

1. **Resolução de Identidade:** O Handler invoca o `ICurrentUserService` para obter o `UserId` da sessão atual.
2. **Validação de Contexto (RN-01):** Se `UserId` for nulo, lança exceção ou retorna erro de autorização.
3. **Consulta ao Cache (Redis):**
* Gera a chave: `user:{UserId}:profile`.
* Invoca `ICacheService.GetAsync<UserDto>(key)`.
* **Caminho A (Cache Hit):** Se o objeto for retornado, finaliza o fluxo devolvendo o DTO deserializado.


4. **Consulta ao Banco de Dados (Cache Miss):**
* Se o cache retornou nulo, acessa o `IUserRepository`.
* Executa a query com `AsNoTracking()` e `.Include(u => u.Profile)`.


5. **Verificação de Existência (RN-05):** Se a entidade não for encontrada no banco, retorna erro `User.NotFound`.
6. **Mapeamento:** Converte a entidade `ApplicationUser` e `Profile` para `UserDto`.
7. **Hidratação do Cache (RN-04):**
* Serializa o `UserDto`.
* Invoca `ICacheService.SetAsync(key, dto, expiration)`.


8. **Retorno:** Devolve o `UserDto` ao chamador.

#### **4. Response (Output)**

**Exemplo de JSON (Sucesso - HTTP 200 OK):**

```json
{
  "isSuccess": true,
  "isFailure": false,
  "value": {
    "id": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
    "email": "bruno.dias@exemplo.com",
    "firstName": "Bruno",
    "lastName": "Dias",
    "fullName": "Bruno Dias",
    "roles": ["Customer"],
    "permissions": ["Cart.Read", "Order.Create"],
    "avatarUrl": "https://storage.bcommerce.com/avatars/bruno.webp",
    "preferences": {
      "theme": "Dark",
      "language": "pt-BR"
    }
  },
  "error": {
    "code": "None",
    "message": "",
    "type": 0
  }
}

```

**Exemplo de JSON (Erro - HTTP 401 Unauthorized):**

```json
{
  "isSuccess": false,
  "isFailure": true,
  "value": null,
  "error": {
    "code": "Auth.Unauthenticated",
    "message": "Usuário não autenticado ou token inválido.",
    "type": 4
  }
}

```
=============================================================================================================================================================
Com base na entidade `Session` e nos requisitos de segurança e auditoria do módulo `Users`, segue a documentação técnica detalhada para a query `GetUserSessionsQuery`.

---

### **QRY-03: Obter Sessões do Usuário (GetUserSessionsQuery)**

**Descrição Técnica:**
Esta query tem por objetivo fornecer uma visão de segurança e auditoria para o usuário, listando todos os dispositivos e locais onde sua conta está atualmente conectada (ou esteve recentemente). A operação consulta a tabela `users.sessions`, aplicando filtros para garantir que apenas os registros pertencentes ao usuário autenticado sejam retornados. O resultado é projetado em um DTO leve (`UserSessionDto`), ocultando dados sensíveis como o hash do *Refresh Token*, mas expondo metadados cruciais (IP, User-Agent, Datas) para que o usuário possa identificar e revogar sessões suspeitas.

#### **1. Request (Input)**

A requisição não possui corpo (Payload), pois é uma operação GET. O filtro principal (`UserId`) é extraído de forma segura do token de acesso (JWT) presente no cabeçalho da requisição.

| Campo | Tipo | Obrigatório | Descrição |
| --- | --- | --- | --- |
| - | - | - | Nenhum parâmetro de corpo. O contexto é resolvido via `ICurrentUserService`. |

**Exemplo de JSON (Request):**

```json
// GET /api/users/sessions
// Headers: Authorization: Bearer eyJhbGci...
{}

```

#### **2. Regras de Negócio (Business Rules)**

* **RN-01 (Isolamento de Dados - Tenant/User):** A consulta deve filtrar estritamente pelo `UserId` extraído do `ClaimsPrincipal`. É vetado o retorno de sessões de outros usuários, mesmo para administradores (admin deve usar uma query específica de backoffice).
* **RN-02 (Filtragem de Validade):** Por padrão, a query deve retornar apenas sessões consideradas "Ativas". Uma sessão é ativa se:
1. Não foi revogada (`IsRevoked == false`).
2. Não expirou (`ExpiresAt > DateTime.UtcNow`).


* **RN-03 (Mascaramento de Credenciais):** O valor do `RefreshToken` jamais deve ser retornado nesta query. O ID da sessão (`Id`) deve ser exposto para permitir comandos futuros de revogação (`RevokeSessionCommand`).
* **RN-04 (Identificação da Sessão Atual):** O sistema deve, se possível, identificar qual das sessões retornadas corresponde à conexão atual do usuário (flag `IsCurrent`), comparando o ID da sessão ou hash do token se disponível no contexto.

#### **3. Fluxo de Processamento (Workflow)**

1. **Contexto de Segurança:** O Handler invoca `ICurrentUserService` para obter o `UserId` do requisitante.
2. **Acesso aos Dados:**
* Acessa o `UsersDbContext` ou `ISessionRepository`.
* Configura a consulta como `AsNoTracking()` para evitar overhead no Entity Framework.


3. **Filtragem (Querying):**
* Aplica cláusula `.Where(s => s.UserId == currentUserId)`.
* Aplica cláusula `.Where(s => !s.IsRevoked && s.ExpiresAt > DateTime.UtcNow)` (dependendo se a regra pede histórico ou apenas ativas).


4. **Projeção (Mapping):**
* Seleciona os campos para `UserSessionDto`: `Id`, `IpAddress`, `DeviceInfo`, `CreatedAt`, `LastActiveAt`.
* Calcula a propriedade `Location` (se houver serviço de GeoIP integrado, caso contrário, retorna desconhecido ou apenas o IP).


5. **Ordenação:** Ordena descendentemente por `LastActiveAt` ou `CreatedAt` para mostrar a atividade mais recente primeiro.
6. **Retorno:** Retorna a coleção de DTOs.

#### **4. Response (Output)**

**Exemplo de JSON (Sucesso - HTTP 200 OK):**

```json
{
  "isSuccess": true,
  "isFailure": false,
  "value": [
    {
      "sessionId": "a1b2c3d4-e5f6-7890-1234-567890abcdef",
      "ipAddress": "201.12.34.56",
      "deviceInfo": "Chrome 120.0.0 on Windows 10",
      "location": "São Paulo, BR",
      "createdAt": "2024-01-20T10:00:00Z",
      "lastActiveAt": "2024-01-20T14:30:00Z",
      "isCurrent": true
    },
    {
      "sessionId": "f9e8d7c6-b5a4-3210-9876-543210fedcba",
      "ipAddress": "177.55.44.33",
      "deviceInfo": "Safari on iPhone 14",
      "location": "Rio de Janeiro, BR",
      "createdAt": "2024-01-19T20:00:00Z",
      "lastActiveAt": "2024-01-19T22:15:00Z",
      "isCurrent": false
    }
  ],
  "error": {
    "code": "None",
    "message": "",
    "type": 0
  }
}

```

**Exemplo de JSON (Erro - HTTP 401 Unauthorized):**

```json
{
  "isSuccess": false,
  "isFailure": true,
  "value": null,
  "error": {
    "code": "Auth.Unauthorized",
    "message": "Token de acesso ausente ou inválido.",
    "type": 4
  }
}

```
=============================================================================================================================================================
Com base na infraestrutura de segurança implementada em `Bcommerce.BuildingBlocks.Security` (especificamente `JwtSettings` e `JwtTokenGenerator`), segue a documentação técnica detalhada para a query de validação de token.

---

### **QRY-04: Validar Token JWT (ValidateTokenQuery)**

**Descrição Técnica:**
Esta query realiza a validação **offline** (stateless) de um JSON Web Token (JWT). O objetivo é verificar a integridade criptográfica (assinatura), a formatação e a validade temporal do token sem a necessidade de realizar consultas ao banco de dados (IO bound). Esta operação é fundamental para gateways ou middlewares que precisam filtrar requisições inválidas com latência mínima antes de repassá-las aos serviços de domínio.

#### **1. Request (Input)**

A requisição deve conter a string do token e, opcionalmente, o contexto esperado (ex: audiência).

| Campo | Tipo | Obrigatório | Descrição |
| --- | --- | --- | --- |
| `token` | String | Sim | A string JWT completa (Header.Payload.Signature). |
| `validateLifeTime` | Boolean | Não | Permite ignorar a expiração para casos de inspeção (Default: true). |

**Exemplo de JSON (Request):**

```json
{
  "token": "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJzdWIiOiIxMjM0NTY...",
  "validateLifeTime": true
}

```

#### **2. Regras de Negócio (Business Rules)**

* **RN-01 (Integridade Criptográfica):** A assinatura do token deve ser validada utilizando a chave simétrica (`SecretKey`) configurada no `JwtSettings`. Qualquer alteração no payload deve invalidar a assinatura.
* **RN-02 (Validade Temporal):** O claim `exp` (Expiration Time) deve ser posterior ao horário atual (UTC). O sistema deve considerar uma tolerância padrão (Clock Skew) de, no máximo, 5 minutos para compensar diferenças de relógio entre servidores.
* **RN-03 (Validação de Emissor/Audiência):** O token deve conter os claims `iss` (Issuer) e `aud` (Audience) correspondentes aos valores definidos na configuração da aplicação (`appsettings.json`), garantindo que o token foi emitido por esta autoridade para este uso.
* **RN-04 (Abordagem Stateless):** Esta query **não** deve consultar o banco de dados ou cache distribuído para verificar revogação (Blacklist). A verificação é puramente matemática e estrutural.

#### **3. Fluxo de Processamento (Workflow)**

1. **Recuperação de Configuração:** O Handler injeta `IOptions<JwtSettings>` para obter a chave de assinatura e parâmetros de validação.
2. **Configuração de Parâmetros:**
* Instancia `TokenValidationParameters`.
* Define `ValidateIssuerSigningKey = true`.
* Define `IssuerSigningKey` com o array de bytes da chave secreta.
* Define `ValidateLifetime` conforme o input.


3. **Processamento (System.IdentityModel.Tokens.Jwt):**
* Instancia `JwtSecurityTokenHandler`.
* Executa o método `handler.ValidateToken(token, parameters, out validatedToken)`.


4. **Tratamento de Exceções (Fail-Fast):**
* `SecurityTokenExpiredException`: Retorna erro específico informando que o token expirou.
* `SecurityTokenInvalidSignatureException`: Retorna erro de integridade/falsificação.
* `ArgumentException`: Retorna erro de formato (não é um JWT válido).


5. **Extração de Claims:**
* Caso validado com sucesso, extrai os claims principais do `ClaimsPrincipal` resultante (`NameIdentifier`, `Email`, `Role`).


6. **Retorno:** Constrói e retorna um DTO contendo o status de validade e os dados decodificados.

#### **4. Response (Output)**

**Exemplo de JSON (Sucesso - HTTP 200 OK):**

```json
{
  "isSuccess": true,
  "isFailure": false,
  "value": {
    "isValid": true,
    "userId": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
    "email": "bruno.dias@exemplo.com",
    "roles": ["Customer"],
    "issuedAt": "2024-01-20T10:00:00Z",
    "expiresAt": "2024-01-20T11:00:00Z"
  },
  "error": {
    "code": "None",
    "message": "",
    "type": 0
  }
}

```

**Exemplo de JSON (Erro - Token Expirado - HTTP 200 OK com IsValid=False ou 400):**

```json
{
  "isSuccess": true,
  "value": {
    "isValid": false,
    "validationError": "IDX10223: Lifetime validation failed. The token is expired."
  }
}

```

*(Nota: Dependendo da implementação, pode-se retornar um Result.Failure, mas frequentemente em queries de validação retorna-se um objeto com `IsValid: false` para evitar exceções no fluxo de controle do cliente).*
=============================================================================================================================================================
Com base na entidade `Address` e nas necessidades de gestão de entregas do módulo `Users`, segue a documentação técnica detalhada para as queries de consulta de endereços.

---

### **QRY-05-A: Listar Endereços do Usuário (GetUserAddressesQuery)**

**Descrição Técnica:**
Esta query é responsável por recuperar a lista completa de endereços de entrega e cobrança associados ao usuário autenticado. A operação é otimizada para leitura (`AsNoTracking`) e aplica regras de ordenação específicas para facilitar a experiência do usuário no checkout, priorizando o endereço marcado como "Padrão". O retorno é uma coleção de objetos de transferência (`AddressDto`).

#### **1. Request (Input)**

A requisição não necessita de payload no corpo, pois trata-se de uma operação GET onde o contexto do usuário é resolvido via token.

| Campo | Tipo | Obrigatório | Descrição |
| --- | --- | --- | --- |
| - | - | - | Nenhum parâmetro de corpo. O filtro `UserId` é aplicado automaticamente via `ICurrentUserService`. |

**Exemplo de JSON (Request):**

```json
// GET /api/users/addresses
{}

```

#### **2. Regras de Negócio (Business Rules)**

* **RN-01 (Isolamento de Dados):** A consulta deve filtrar estritamente os registros onde `Address.UserId` corresponde ao ID do usuário extraído do Token JWT.
* **RN-02 (Filtragem de Ativos):** Devem ser retornados apenas os endereços que não foram excluídos logicamente (`IsDeleted == false`).
* **RN-03 (Ordenação Prioritária):** A lista deve ser ordenada de forma que o endereço marcado como `IsDefault = true` seja sempre o primeiro elemento (índice 0). Os demais endereços devem ser ordenados secundariamente, por exemplo, pela data de criação decrescente ou ordem alfabética do apelido/rua.
* **RN-04 (Performance):** A consulta ao banco de dados deve utilizar projeção direta para DTO ou `AsNoTracking()` para evitar overhead de gerenciamento de estado no Entity Framework.

#### **3. Fluxo de Processamento (Workflow)**

1. **Identificação de Contexto:** O Handler recupera o `UserId` atual através do serviço de infraestrutura de identidade.
2. **Construção da Query (LINQ):**
* Acessa `IAddressRepository` ou `UsersDbContext`.
* Aplica filtro: `.Where(a => a.UserId == currentUserId && !a.IsDeleted)`.
* Aplica ordenação: `.OrderByDescending(a => a.IsDefault).ThenByDescending(a => a.CreatedAt)`.
* Define modo de rastreamento: `.AsNoTracking()`.


3. **Projeção:** Mapeia as entidades retornadas para `IEnumerable<AddressDto>`.
4. **Retorno:** Retorna a lista (pode ser vazia, mas nunca nula).

#### **4. Response (Output)**

**Exemplo de JSON (Sucesso - HTTP 200 OK):**

```json
{
  "isSuccess": true,
  "isFailure": false,
  "value": [
    {
      "id": "a1b2c3d4-e5f6-7890-1234-567890abcdef",
      "street": "Avenida Paulista",
      "number": "1000",
      "complement": "Apto 501",
      "neighborhood": "Bela Vista",
      "city": "São Paulo",
      "state": "SP",
      "zipCode": "01310-100",
      "country": "BR",
      "isDefault": true
    },
    {
      "id": "f5a1b2c3-d4e5-6789-0123-4567890abcde",
      "street": "Rua das Flores",
      "number": "50",
      "complement": "",
      "neighborhood": "Centro",
      "city": "Campinas",
      "state": "SP",
      "zipCode": "13010-000",
      "country": "BR",
      "isDefault": false
    }
  ],
  "error": { "code": "None", "message": "", "type": 0 }
}

```

---

### **QRY-05-B: Obter Endereço por ID (GetAddressByIdQuery)**

**Descrição Técnica:**
Esta query retorna os detalhes de um endereço específico. É utilizada principalmente em telas de "Edição de Endereço" ou para confirmar dados de entrega antes de finalizar um pedido. A operação inclui validações de segurança para impedir o acesso a endereços de terceiros (IDOR).

#### **1. Request (Input)**

| Campo | Tipo | Obrigatório | Descrição |
| --- | --- | --- | --- |
| `addressId` | Guid | Sim | O identificador único do endereço desejado (geralmente via Rota). |

**Exemplo de JSON (Request):**

```json
// GET /api/users/addresses/a1b2c3d4-e5f6-7890-1234-567890abcdef
{
  "addressId": "a1b2c3d4-e5f6-7890-1234-567890abcdef"
}

```

#### **2. Regras de Negócio (Business Rules)**

* **RN-01 (Validação de Propriedade - IDOR):** O sistema deve verificar se o endereço solicitado pertence ao usuário autenticado (`Address.UserId == CurrentUser.Id`). Se o endereço existir mas pertencer a outro usuário, a query deve retornar `Error.NotFound` (para não expor a existência do recurso) ou `Error.Forbidden`.
* **RN-02 (Validade do Recurso):** Endereços marcados como excluídos (`IsDeleted = true`) não devem ser acessíveis via API pública, devendo retornar `Error.NotFound`.

#### **3. Fluxo de Processamento (Workflow)**

1. **Recuperação de Contexto:** Obtém o `UserId` do token.
2. **Consulta ao Repositório:**
* Executa `FindByIdAsync(addressId)` ou query equivalente.


3. **Verificação de Nulidade e Segurança (RN-01, RN-02):**
* Se `address` for nulo: Retorna `Error.NotFound`.
* Se `address.UserId != userId`: Retorna `Error.NotFound` (Security Best Practice).
* Se `address.IsDeleted`: Retorna `Error.NotFound`.


4. **Mapeamento:** Converte a entidade `Address` encontrada para `AddressDto`.
5. **Retorno:** Devolve o objeto DTO.

#### **4. Response (Output)**

**Exemplo de JSON (Sucesso - HTTP 200 OK):**

```json
{
  "isSuccess": true,
  "isFailure": false,
  "value": {
    "id": "a1b2c3d4-e5f6-7890-1234-567890abcdef",
    "street": "Avenida Paulista",
    "number": "1000",
    "complement": "Apto 501",
    "neighborhood": "Bela Vista",
    "city": "São Paulo",
    "state": "SP",
    "zipCode": "01310-100",
    "country": "BR",
    "isDefault": true
  },
  "error": { "code": "None", "message": "", "type": 0 }
}

```

**Exemplo de JSON (Erro - HTTP 404 Not Found):**

```json
{
  "isSuccess": false,
  "isFailure": true,
  "value": null,
  "error": {
    "code": "Address.NotFound",
    "message": "O endereço solicitado não foi encontrado.",
    "type": 3
  }
}

```
=============================================================================================================================================================
Com base na entidade `NotificationPreference` e na necessidade de personalização da experiência do usuário no front-end, segue a documentação técnica detalhada para a query de obtenção de preferências.

---

### **QRY-06: Obter Preferências do Usuário (GetUserPreferencesQuery)**

**Descrição Técnica:**
Esta query é responsável por recuperar as configurações de personalização do usuário, incluindo opções de notificação (Opt-in de Marketing/SMS) e preferências de interface (Tema, Idioma). A operação implementa um padrão de *Fail-Safe* ou *Lazy Initialization* na leitura: caso o usuário ainda não tenha registrado preferências explicitamente no banco de dados, a query deve retornar um objeto com valores padrão do sistema em vez de um erro `NotFound`, garantindo que a UI sempre tenha uma configuração válida para renderizar.

#### **1. Request (Input)**

A requisição é do tipo GET e não requer corpo. O contexto do usuário é resolvido via token de autenticação.

| Campo | Tipo | Obrigatório | Descrição |
| --- | --- | --- | --- |
| - | - | - | Nenhum parâmetro de corpo. O `UserId` é extraído do `ICurrentUserService`. |

**Exemplo de JSON (Request):**

```json
// GET /api/users/preferences
{}

```

#### **2. Regras de Negócio (Business Rules)**

* **RN-01 (Isolamento de Contexto):** A consulta deve retornar estritamente os dados vinculados ao `UserId` do token JWT atual.
* **RN-02 (Tratamento de Defaults - Fail-Safe):** Se o registro de `NotificationPreference` não for encontrado no banco de dados (ex: usuário recém-criado que nunca acessou a tela de configurações), a query **não** deve retornar 404. Ela deve retornar um DTO preenchido com os valores padrão da aplicação (Ex: `Theme: "System"`, `Marketing: false`).
* **RN-03 (Performance):** A leitura deve ser feita com `AsNoTracking()` para evitar overhead no Entity Framework, visto que é uma operação de alta frequência (pode ser chamada a cada inicialização do App).

#### **3. Fluxo de Processamento (Workflow)**

1. **Identificação de Contexto:** O Handler recupera o `UserId` através do `ICurrentUserService`.
2. **Consulta ao Repositório:**
* Acessa `INotificationPreferenceRepository`.
* Executa a busca: `.FirstOrDefaultAsync(p => p.UserId == currentUserId)`.


3. **Verificação de Existência (RN-02):**
* **Cenário A (Encontrado):** Mapeia a entidade retornada para o `UserPreferencesDto`.
* **Cenário B (Nulo):** Instancia um `UserPreferencesDto` com valores padrão (Hardcoded ou via `IOptions<AppSettings>`).


4. **Retorno:** Devolve o DTO ao chamador.

#### **4. Response (Output)**

**Exemplo de JSON (Sucesso - HTTP 200 OK):**

```json
{
  "isSuccess": true,
  "isFailure": false,
  "value": {
    "receiveMarketingEmails": true,
    "receivePromotionalSms": false,
    "preferredLanguage": "pt-BR",
    "theme": "Dark",
    "lastUpdated": "2024-02-15T10:30:00Z"
  },
  "error": { "code": "None", "message": "", "type": 0 }
}

```

**Exemplo de JSON (Sucesso - Default/Primeiro Acesso - HTTP 200 OK):**

```json
{
  "isSuccess": true,
  "isFailure": false,
  "value": {
    "receiveMarketingEmails": false,
    "receivePromotionalSms": false,
    "preferredLanguage": "pt-BR",
    "theme": "Light",
    "lastUpdated": null
  },
  "error": { "code": "None", "message": "", "type": 0 }
}

```
=============================================================================================================================================================
### Queries Administrativas (Admin & Métricas)

Com base nos padrões de Building Blocks (`PaginatedList`, `PagedRequest`) e na necessidade de performance para listagens administrativas no projeto `bcommerce-monolito`, segue a documentação técnica detalhada para a query de listagem de usuários.

---

### **QRY-07: Listar Usuários (GetUsersQuery)**

**Descrição Técnica:**
Esta query é projetada para o consumo por interfaces administrativas (Backoffice), permitindo a visualização tabular da base de usuários. Ela implementa **Paginação Lógica** (Skip/Take) no banco de dados para minimizar a transferência de dados e o consumo de memória. A query suporta filtragem dinâmica por múltiplos critérios e ordenação, utilizando projeção de dados (`Select`) para retornar apenas as colunas estritamente necessárias para a listagem (evitando carregar colunas pesadas ou metadados de auditoria desnecessários).

#### **1. Request (Input)**

Os parâmetros são geralmente enviados via *Query String*, mas mapeados para um objeto `PagedRequest` no Application Layer.

| Campo | Tipo | Obrigatório | Descrição |
| --- | --- | --- | --- |
| `pageIndex` | Int | Sim | Número da página atual (Base 1 ou 0, conforme configuração padrão). Default: 1. |
| `pageSize` | Int | Sim | Quantidade de registros por página. Default: 10. Max: 100. |
| `searchTerm` | String | Não | Texto livre para busca parcial em Nome, Email ou CPF. |
| `email` | String | Não | Filtro específico por e-mail (Match exato ou parcial). |
| `cpf` | String | Não | Filtro específico por CPF (apenas números). |
| `isActive` | Boolean? | Não | Filtro por status. `null`=Todos, `true`=Ativos, `false`=Inativos. |
| `orderBy` | String | Não | Campo para ordenação (ex: "firstName", "createdAt"). |
| `orderDesc` | Boolean | Não | Define a direção da ordenação (`true` = Descendente). |

**Exemplo de JSON (Representação dos parâmetros):**

```json
{
  "pageIndex": 1,
  "pageSize": 20,
  "searchTerm": "Bruno",
  "isActive": true,
  "orderBy": "createdAt",
  "orderDesc": true
}

```

#### **2. Regras de Negócio (Business Rules)**

* **RN-01 (Autorização Administrativa):** Esta query deve ser restrita a usuários com permissões elevadas (`Roles: Admin, Support`). Usuários comuns não podem listar dados de outros usuários.
* **RN-02 (Projeção Otimizada):** A consulta não deve retornar a entidade `ApplicationUser` completa. Deve ser utilizada uma projeção (`.Select()`) para um `UserSummaryDto` contendo apenas: ID, Nome Completo, Email, CPF, Status e Data de Cadastro.
* **RN-03 (Performance de Busca):** As colunas utilizadas no filtro (Email, CPF, NormalizedName) devem possuir índices no banco de dados para evitar *Full Table Scan* em tabelas volumosas.
* **RN-04 (Sanitização de Filtros):** O `searchTerm` deve ser limpo de caracteres especiais se for utilizado contra campos formatados (como CPF), removendo pontos e traços antes da comparação SQL.

#### **3. Fluxo de Processamento (Workflow)**

1. **Verificação de Permissões:** O pipeline (`AuthorizationBehavior`) valida se o usuário atual possui a *Policy* ou *Role* necessária.
2. **Construção da Query Base:**
* Instancia o `IQueryable<ApplicationUser>` a partir do `UsersDbContext`.
* Aplica `AsNoTracking()` para leitura eficiente.


3. **Aplicação de Filtros (Dinâmico):**
* Se `searchTerm` não for nulo: Adiciona cláusula `OR` buscando em `FirstName`, `LastName` ou `Email`.
* Se `cpf` informado: Remove pontuação e filtra `Cpf.Number == request.Cpf`.
* Se `isActive` informado: Filtra `IsActive == request.IsActive`.


4. **Ordenação:**
* Aplica `OrderBy` ou `OrderByDescending` dinamicamente com base na string `request.OrderBy`.
* Default: `OrderByDescending(u => u.CreatedAt)`.


5. **Contagem Total:** Executa `CountAsync()` sobre a query filtrada (necessário para calcular o total de páginas).
6. **Paginação e Projeção:**
* Aplica `.Skip((PageIndex - 1) * PageSize).Take(PageSize)`.
* Aplica `.Select(u => new UserSummaryDto { ... })`.


7. **Execução:** Executa a consulta (`ToListAsync`) no banco de dados.
8. **Montagem:** Retorna o objeto `PaginatedList<UserSummaryDto>` contendo os itens e metadados de paginação.

#### **4. Response (Output)**

Retorna uma estrutura de resposta paginada padrão.

**Exemplo de JSON (Sucesso - HTTP 200 OK):**

```json
{
  "isSuccess": true,
  "isFailure": false,
  "value": {
    "items": [
      {
        "id": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
        "fullName": "Bruno Dias",
        "email": "bruno.dias@exemplo.com",
        "cpf": "123.456.789-00",
        "isActive": true,
        "createdAt": "2024-01-10T14:30:00Z"
      },
      {
        "id": "a1b2c3d4-e5f6-7890-1234-567890abcdef",
        "fullName": "Maria Silva",
        "email": "maria.silva@exemplo.com",
        "cpf": "987.654.321-99",
        "isActive": true,
        "createdAt": "2024-01-12T09:15:00Z"
      }
    ],
    "pageIndex": 1,
    "totalPages": 5,
    "totalCount": 98,
    "hasPreviousPage": false,
    "hasNextPage": true
  },
  "error": { "code": "None", "message": "", "type": 0 }
}

```

**Exemplo de JSON (Erro - HTTP 403 Forbidden):**

```json
{
  "isSuccess": false,
  "isFailure": true,
  "value": null,
  "error": {
    "code": "Auth.Forbidden",
    "message": "Você não tem permissão para listar usuários.",
    "type": 4
  }
}

```

=============================================================================================================================================================
Com base na infraestrutura de Identity (`UserManager`, `RoleManager`) e no modelo de segurança RBAC (Role-Based Access Control) do projeto `bcommerce-monolito`, segue a documentação técnica detalhada para as queries de gestão de acessos.

---

### **QRY-08-A: Obter Funções do Usuário (GetUserRolesQuery)**

**Descrição Técnica:**
Esta query retorna a lista de funções (Roles) atribuídas a um usuário específico. É fundamental para a interface de administração de usuários (Backoffice), permitindo que administradores visualizem o nível de acesso atual de um membro. A consulta utiliza o `UserManager` do ASP.NET Core Identity para resolver as associações na tabela `AspNetUserRoles`.

#### **1. Request (Input)**

| Campo | Tipo | Obrigatório | Descrição |
| --- | --- | --- | --- |
| `userId` | Guid | Sim | O identificador único do usuário alvo da consulta. |

**Exemplo de JSON (Request - Query String/Route):**

```json
// GET /api/users/{userId}/roles
{
  "userId": "3fa85f64-5717-4562-b3fc-2c963f66afa6"
}

```

#### **2. Regras de Negócio (Business Rules)**

* **RN-01 (Autorização Elevada):** Apenas usuários com permissão de gerenciamento de usuários (`Users.Read` ou `Roles.Manage`) podem consultar as roles de terceiros.
* **RN-02 (Validação de Existência):** O `userId` deve corresponder a um registro válido na tabela de usuários. Caso contrário, retorna `Error.NotFound`.
* **RN-03 (Formato de Retorno):** A lista deve retornar os nomes normalizados das roles (strings) ou objetos DTO simples contendo ID e Nome.

#### **3. Fluxo de Processamento (Workflow)**

1. **Verificação de Permissões:** Valida se o solicitante tem acesso ao recurso (Policy).
2. **Recuperação do Usuário:**
* Invoca `UserManager.FindByIdAsync(userId)`.
* Se nulo, retorna erro `User.NotFound`.


3. **Consulta de Roles:**
* Invoca `UserManager.GetRolesAsync(user)`.
* O Identity realiza o *Join* interno com a tabela `AspNetUserRoles` e `AspNetRoles`.


4. **Mapeamento:** Converte a lista de strings retornada em um `UserRolesDto`.
5. **Retorno:** Devolve a lista.

#### **4. Response (Output)**

**Exemplo de JSON (Sucesso - HTTP 200 OK):**

```json
{
  "isSuccess": true,
  "value": {
    "userId": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
    "roles": [
      "Customer",
      "SupportAgent"
    ]
  }
}

```

---

### **QRY-08-B: Obter Permissões Efetivas do Usuário (GetUserPermissionsQuery)**

**Descrição Técnica:**
Esta query calcula a "matriz de permissões efetiva" de um usuário. Diferente das roles (que são agrupadores), esta query retorna a lista granular de Claims do tipo `Permission` (ex: "Catalog.Write", "Orders.Read"). O sistema deve agregar todas as permissões das Roles atribuídas ao usuário e, opcionalmente, permissões atribuídas diretamente ao usuário (se suportado), removendo duplicatas.

#### **1. Request (Input)**

| Campo | Tipo | Obrigatório | Descrição |
| --- | --- | --- | --- |
| `userId` | Guid | Sim | O identificador do usuário. |

**Exemplo de JSON (Request):**

```json
// GET /api/users/{userId}/permissions
{
  "userId": "3fa85f64-5717-4562-b3fc-2c963f66afa6"
}

```

#### **2. Regras de Negócio (Business Rules)**

* **RN-01 (Agregação de Claims):** O sistema deve iterar sobre todas as roles do usuário e extrair as Claims associadas (`AspNetRoleClaims`).
* **RN-02 (Deduplicação):** Se o usuário possui duas roles (ex: "Manager" e "Editor") e ambas concedem a permissão "Content.Edit", o retorno deve conter esta string apenas uma vez (`Distinct`).
* **RN-03 (Performance):** Devido à complexidade potencial (N+1 queries se não otimizado), recomenda-se o uso de uma query SQL direta com `JOIN` nas tabelas de Identity ou carregamento ávido (Eager Loading) das Claims das Roles.

#### **3. Fluxo de Processamento (Workflow)**

1. **Contexto:** Identifica o usuário alvo.
2. **Consulta SQL Otimizada (Exemplo Conceitual):**
* Seleciona `ClaimValue` da tabela `AspNetRoleClaims`.
* Faz Join com `AspNetRoles`.
* Faz Join com `AspNetUserRoles` onde `UserId == request.UserId`.
* Filtra onde `ClaimType == "Permission"`.


3. **Processamento em Memória (Alternativa via Identity):**
* Obtém Roles do usuário.
* Para cada Role, obtém Claims (`RoleManager.GetClaimsAsync`).
* Adiciona a um `HashSet<string>` para garantir unicidade.


4. **Retorno:** Lista de strings de permissão.

#### **4. Response (Output)**

**Exemplo de JSON (Sucesso - HTTP 200 OK):**

```json
{
  "isSuccess": true,
  "value": {
    "permissions": [
      "Catalog.Read",
      "Catalog.Write",
      "Orders.Read",
      "Users.Read"
    ]
  }
}

```

---

### **QRY-08-C: Listar Todas as Funções (GetAllRolesQuery)**

**Descrição Técnica:**
Esta query retorna o catálogo mestre de todas as Roles disponíveis no sistema. É utilizada primariamente em telas de cadastro ou edição de usuários, populando componentes de seleção (Dropdowns/Checkboxes). A consulta deve retornar metadados da Role, incluindo sua descrição e, opcionalmente, a lista de permissões que ela engloba.

#### **1. Request (Input)**

| Campo | Tipo | Obrigatório | Descrição |
| --- | --- | --- | --- |
| `includePermissions` | Boolean | Não | Se `true`, expande a resposta para incluir a lista de Claims de cada Role. Default: `false`. |

**Exemplo de JSON (Request):**

```json
// GET /api/roles?includePermissions=true
{
  "includePermissions": true
}

```

#### **2. Regras de Negócio (Business Rules)**

* **RN-01 (Projeção Leve):** Por padrão, deve retornar apenas ID e Nome para otimizar o payload. A lista de permissões detalhada só deve ser carregada se explicitamente solicitada.
* **RN-02 (Ordenação):** A lista deve ser ordenada alfabeticamente pelo Nome da Role.
* **RN-03 (Segurança):** Apenas administradores podem visualizar a estrutura completa de segurança do sistema.

#### **3. Fluxo de Processamento (Workflow)**

1. **Acesso ao RoleManager:** Acessa a queryable `RoleManager.Roles`.
2. **Filtragem/Projeção:**
* Executa `AsNoTracking()`.
* Ordena `.OrderBy(r => r.Name)`.
* Se `includePermissions == true`, realiza o *Left Join* com `RoleClaims`.


3. **Mapeamento:** Transforma as entidades `ApplicationRole` em `RoleDto`.
4. **Retorno:** Coleção de DTOs.

#### **4. Response (Output)**

**Exemplo de JSON (Sucesso - HTTP 200 OK):**

```json
{
  "isSuccess": true,
  "value": [
    {
      "id": "e3b0c442-98fc-1c14-9af2-1234567890ab",
      "name": "Admin",
      "description": "Acesso total ao sistema.",
      "permissions": ["*"] // Se solicitado
    },
    {
      "id": "f5a1b2c3-d4e5-6789-0123-4567890abcde",
      "name": "Customer",
      "description": "Usuário padrão da loja.",
      "permissions": ["Cart.Read", "Order.Create"]
    }
  ]
}

```
=============================================================================================================================================================
Com base na necessidade de monitoramento estratégico e na estrutura de dados do módulo `Users`, segue a documentação técnica detalhada para a query de métricas de crescimento.

---

### **QRY-09: Métricas de Crescimento de Usuários (GetUserGrowthMetricsQuery)**

**Descrição Técnica:**
Esta query analítica é responsável por agregar dados históricos da tabela `users.users` para gerar relatórios de aquisição de clientes. Utilizada exclusivamente pelo Backoffice (Dashboard Administrativo), ela processa o campo `CreatedAt` para agrupar registros por períodos (Diário, Semanal ou Mensal), calculando o volume de novos cadastros e a taxa de variação (crescimento/queda) em relação ao período anterior. Devido à natureza computacional de agregações em grandes volumes de dados, esta query é uma candidata ideal para estratégias de *Caching* de curta duração.

#### **1. Request (Input)**

A requisição deve definir a janela de tempo e a granularidade desejada para o agrupamento dos dados.

| Campo | Tipo | Obrigatório | Descrição |
| --- | --- | --- | --- |
| `startDate` | Date | Sim | Data de início do período de análise (UTC). |
| `endDate` | Date | Sim | Data de fim do período de análise (UTC). |
| `granularity` | Enum | Sim | Nível de agrupamento: `Daily`, `Weekly`, `Monthly`. |
| `includeInactive` | Boolean | Não | Se `true`, inclui usuários desativados ou banidos na contagem (Default: `false`). |

**Exemplo de JSON (Request):**

```json
{
  "startDate": "2024-01-01T00:00:00Z",
  "endDate": "2024-01-31T23:59:59Z",
  "granularity": "Daily",
  "includeInactive": false
}

```

#### **2. Regras de Negócio (Business Rules)**

* **RN-01 (Restrição de Acesso):** Esta query é estritamente reservada para usuários com a Role `Admin` ou `Manager`. Deve ser bloqueada via `AuthorizationBehavior` para usuários comuns.
* **RN-02 (Limite de Janela):** Para evitar degradação de performance do banco de dados, o intervalo entre `startDate` e `endDate` não deve exceder limites lógicos para a granularidade escolhida (ex: Máximo 365 dias para visualização `Daily`).
* **RN-03 (Fuso Horário e Agrupamento):** O agrupamento deve considerar que os dados estão persistidos em UTC. O retorno deve manter o padrão UTC, delegando ao front-end a conversão para o fuso local do administrador.
* **RN-04 (Preenchimento de Lacunas - Gap Filling):** Se não houver registros em um determinado dia/mês dentro do intervalo, a query deve retornar o ponto com valor `0` (Zero) em vez de omitir a data, garantindo a continuidade visual em gráficos de linha.
* **RN-05 (Taxa de Conversão/Variação):** O DTO de retorno deve incluir, se possível, o percentual de crescimento em relação ao ponto de dados imediatamente anterior (`GrowthRate = (Current - Previous) / Previous`).

#### **3. Fluxo de Processamento (Workflow)**

1. **Validação de Segurança:** O pipeline verifica se o usuário possui a permissão `Metrics.Read`.
2. **Preparação da Query (IQueryable):**
* Acessa o `UsersDbContext` com `AsNoTracking()`.
* Aplica filtro de data: `u.CreatedAt >= startDate && u.CreatedAt <= endDate`.
* Aplica filtro de status: `!request.IncludeInactive ? u.IsActive : true`.


3. **Agregação (Database Side):**
* Executa um `GroupBy` baseado na granularidade.
* Exemplo SQL/LINQ: `GroupBy(u => u.CreatedAt.Date)` para diário.


* Seleciona a contagem: `Count()`.


4. **Execução e Materialização:** Recupera os dados sumarizados do banco de dados para a memória.
5. **Pós-Processamento (In-Memory):**
* Itera sobre o intervalo de datas solicitado.
* Preenche as lacunas (dias sem registros) com contagem 0 (RN-04).
* Calcula a porcentagem de variação comparando com o registro anterior da lista.


6. **Mapeamento:** Transforma a coleção em `List<MetricPointDto>`.
7. **Retorno:** Retorna a lista ordenada cronologicamente.

#### **4. Response (Output)**

**Exemplo de JSON (Sucesso - HTTP 200 OK):**

```json
{
  "isSuccess": true,
  "isFailure": false,
  "value": {
    "totalNewUsers": 150,
    "period": "Jan 2024",
    "points": [
      {
        "date": "2024-01-01T00:00:00Z",
        "count": 10,
        "growthPercentage": 0.0
      },
      {
        "date": "2024-01-02T00:00:00Z",
        "count": 15,
        "growthPercentage": 50.0
      },
      {
        "date": "2024-01-03T00:00:00Z",
        "count": 12,
        "growthPercentage": -20.0
      },
      {
        "date": "2024-01-04T00:00:00Z",
        "count": 0,
        "growthPercentage": -100.0
      }
    ]
  },
  "error": {
    "code": "None",
    "message": "",
    "type": 0
  }
}

```

**Exemplo de JSON (Erro - Intervalo Inválido - HTTP 400 Bad Request):**

```json
{
  "isSuccess": false,
  "isFailure": true,
  "value": null,
  "error": {
    "code": "Metrics.InvalidDateRange",
    "message": "O intervalo para visualização diária não pode exceder 365 dias.",
    "type": 2
  }
}

```
=============================================================================================================================================================
Com base na entidade `Session`, no campo `LastActiveAt` e na necessidade de monitoramento em tempo real para o Backoffice, segue a documentação técnica detalhada para a query de estatísticas de sessão.

---

### **QRY-10: Obter Estatísticas de Sessões Ativas (GetActiveSessionsStatsQuery)**

**Descrição Técnica:**
Esta query fornece métricas de observabilidade em tempo real sobre a utilização da plataforma. Ela calcula o número de sessões simultâneas ("Simultaneous Users") considerando uma janela de tempo deslizante (padrão de 5 minutos) baseada na última atividade registrada (`LastActiveAt`). Esta informação é crítica para monitoramento de carga, dimensionamento de infraestrutura (Scaling) e análise de impacto durante campanhas de marketing. A consulta é otimizada para contar registros sem necessariamente carregar os dados das entidades (Count projection).

#### **1. Request (Input)**

A requisição permite ajustar a janela de tempo, embora o padrão de mercado para "tempo real" seja de 5 a 15 minutos.

| Campo | Tipo | Obrigatório | Descrição |
| --- | --- | --- | --- |
| `timeWindowInMinutes` | Int | Não | Janela de tempo em minutos para considerar um usuário como "Online". Default: 5. |

**Exemplo de JSON (Request):**

```json
// GET /api/admin/metrics/sessions?timeWindowInMinutes=5
{
  "timeWindowInMinutes": 5
}

```

#### **2. Regras de Negócio (Business Rules)**

* **RN-01 (Definição de Atividade):** Uma sessão é considerada "Ativa" se a propriedade `LastActiveAt` for maior ou igual a `DateTime.UtcNow - timeWindow`. Sessões com última atividade anterior a este marco são consideradas ociosas ou desconectadas para fins desta métrica.
* **RN-02 (Exclusão de Revogados):** Sessões marcadas como `IsRevoked = true` ou expiradas (`ExpiresAt < Now`) devem ser excluídas da contagem, independentemente do `LastActiveAt`.
* **RN-03 (Segurança - RBAC):** Esta query expõe dados sensíveis de volumetria de negócio. O acesso deve ser restrito estritamente a usuários com a Role `Admin` ou política `Dashboard.Read`.
* **RN-04 (Distinção Sessão vs Usuário):** A query deve retornar dois contadores distintos:
1. **Total Active Sessions:** Número total de dispositivos conectados (um usuário pode estar no PC e Mobile).
2. **Unique Active Users:** Número de usuários distintos (`Distinct UserId`) conectados.



#### **3. Fluxo de Processamento (Workflow)**

1. **Autorização:** O pipeline (`AuthorizationBehavior`) verifica se o usuário possui permissão administrativa.
2. **Cálculo da Janela:**
* Determina o `threshold`: `DateTime.UtcNow.AddMinutes(-request.TimeWindowInMinutes)`.


3. **Consulta de Agregação (Database):**
* Acessa `ISessionRepository` (ou `DbSet<Session>`).
* Aplica filtros: `.Where(s => !s.IsRevoked && s.ExpiresAt > DateTime.UtcNow && s.LastActiveAt >= threshold)`.


4. **Projeção de Métricas:**
* Executa `CountAsync()` para obter o total de sessões.
* Executa `Select(s => s.UserId).Distinct().CountAsync()` para obter usuários únicos.
* *Nota de Performance:* Se o volume de dados for muito alto, essas duas contagens podem ser feitas em uma única query SQL usando `GROUP BY` ou `COUNT(DISTINCT UserId)`.


5. **Montagem do DTO:** Preenche o `ActiveSessionsStatsDto`.
6. **Retorno:** Retorna os dados para o dashboard.

#### **4. Response (Output)**

**Exemplo de JSON (Sucesso - HTTP 200 OK):**

```json
{
  "isSuccess": true,
  "isFailure": false,
  "value": {
    "totalActiveSessions": 1250,
    "uniqueActiveUsers": 980,
    "timeWindowMinutes": 5,
    "measuredAt": "2024-03-15T14:35:00Z"
  },
  "error": { "code": "None", "message": "", "type": 0 }
}

```

**Exemplo de JSON (Erro - HTTP 403 Forbidden):**

```json
{
  "isSuccess": false,
  "isFailure": true,
  "value": null,
  "error": {
    "code": "Auth.Forbidden",
    "message": "Acesso negado às métricas administrativas.",
    "type": 4
  }
}

```
========================================================================================================================================================================================================================================================================================================================================================================================================================================================================================================================================================================================================================================================================================================================================================================================================================================================================================================================================================================================================================================================================================================================================================================================================================================================================================================================================================================================================================================================================================================================================================================================================================================================================================================================================================================================================================================================================================================================================================================================================================================================================================================================================================================================================================================================================================================================================================================================================================================================================================================================================================================================================================================================================================================================================================================================================================================================================================================================================================================================================================================================================================================================================================================================================================================================================================================================================================================================================================================================================================================================================================================================================================================================================================================================================================================================================================================================================================================================================================================================================================================================================================================================================================================================================================================================================================================================================================================================================================================================================================================================================================================================================================================================================================================================================================================================================================================================================