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
