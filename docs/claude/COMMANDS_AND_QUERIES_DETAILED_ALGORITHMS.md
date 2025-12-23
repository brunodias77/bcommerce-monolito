# Commands e Queries - Algoritmos Detalhados - BCommerce

> **Arquiteto:** Análise Clean Architecture & DDD
> **Projeto:** BCommerce Monolith - Sistema de E-commerce
> **Framework:** .NET 8 + MediatR + CQRS
> **Padrões:** Clean Architecture, DDD, CQRS, Repository Pattern
> **Data:** 2025-12-23

---

## 📋 Índice

1. [Módulo Users - Commands](#módulo-users---commands)
2. [Módulo Users - Queries](#módulo-users---queries)
3. [Módulo Catalog - Commands](#módulo-catalog---commands)
4. [Módulo Catalog - Queries](#módulo-catalog---queries)
5. [Módulo Cart - Commands](#módulo-cart---commands)
6. [Módulo Cart - Queries](#módulo-cart---queries)
7. [Módulo Orders - Commands](#módulo-orders---commands)
8. [Módulo Orders - Queries](#módulo-orders---queries)
9. [Módulo Payments - Commands](#módulo-payments---commands)
10. [Módulo Payments - Queries](#módulo-payments---queries)
11. [Módulo Coupons - Commands](#módulo-coupons---commands)
12. [Módulo Coupons - Queries](#módulo-coupons---queries)
13. [Resumo e Estatísticas](#resumo-e-estatísticas)

---

## Módulo Users - Commands

### 1. RegisterUserCommand

**Módulo:** Users
**Camada:** Application.Commands
**Namespace:** `Bcommerce.Modules.Users.Application.Commands.RegisterUser`

**Record Definition:**
```csharp
public record RegisterUserCommand(
    string Email,
    string Password,
    string FirstName,
    string LastName,
    string? PhoneNumber
) : ICommand<Guid>;
```

**Algoritmo Passo a Passo:**

1. **Validação de Entrada (FluentValidation):**
   - Verificar se `Email` é válido e único (formato RFC 5322)
   - Validar `Password` com política de segurança:
     - Mínimo 8 caracteres
     - Pelo menos 1 letra maiúscula
     - Pelo menos 1 letra minúscula
     - Pelo menos 1 número
     - Pelo menos 1 caractere especial
   - Validar `FirstName` e `LastName` (não nulos, min 2 caracteres)
   - Se `PhoneNumber` fornecido, validar formato E.164
   - **Validator:** `RegisterUserCommandValidator`

2. **Verificação de Unicidade:**
   - Executar `await _userRepository.ExistsByEmailAsync(Email)`
   - Se existir → Retornar `Result.Failure(UserErrors.EmailAlreadyExists)`

3. **Criação de Value Objects:**
   - Instanciar `Email email = Email.Create(command.Email)`
   - Se `PhoneNumber` fornecido → `PhoneNumber phone = PhoneNumber.Create(command.PhoneNumber)`

4. **Hashing de Senha:**
   - Executar `string passwordHash = _passwordHasher.HashPassword(command.Password)`
   - Usar Argon2id ou BCrypt (configurável)

5. **Criação da Entidade:**
   - Instanciar `ApplicationUser` via método estático:
     ```csharp
     var user = ApplicationUser.Create(
         email,
         passwordHash,
         command.FirstName,
         command.LastName,
         phone
     );
     ```
   - Internamente, o método `Create()` adiciona evento:
     ```csharp
     AddDomainEvent(new UserRegisteredEvent(Id, Email.Value, UserName));
     ```

6. **Persistência:**
   - Adicionar ao repositório: `await _userRepository.AddAsync(user, cancellationToken)`
   - Confirmar transação: `await _unitOfWork.SaveChangesAsync(cancellationToken)`

7. **Processamento de Eventos:**
   - Outbox Pattern captura `UserRegisteredEvent`
   - Evento persistido em `outbox_messages` na mesma transação

8. **Retorno:**
   - `return Result.Success(user.Id)`

**Arquivo:** `/src/Modules/Users/Bcommerce.Modules.Users.Application/Commands/RegisterUser/RegisterUserCommandHandler.cs`

**Dependências:**
- `IUserRepository`
- `IPasswordHasher`
- `IUnitOfWork`

**Eventos Disparados:**
- `UserRegisteredEvent` (Domain)

**Erros Possíveis:**
- `UserErrors.EmailAlreadyExists`
- `UserErrors.InvalidEmailFormat`
- `UserErrors.WeakPassword`

---

### 2. LoginCommand

**Módulo:** Users
**Namespace:** `Bcommerce.Modules.Users.Application.Commands.Login`

**Record Definition:**
```csharp
public record LoginCommand(
    string Email,
    string Password,
    string? IpAddress,
    string? UserAgent
) : ICommand<LoginResponse>;

public record LoginResponse(
    string AccessToken,
    string RefreshToken,
    DateTime ExpiresAt,
    UserDto User
);
```

**Algoritmo Passo a Passo:**

1. **Validação de Entrada:**
   - Validar `Email` não nulo e formato válido
   - Validar `Password` não nulo e não vazio
   - **Validator:** `LoginCommandValidator`

2. **Recuperação do Usuário:**
   - Executar `var user = await _userRepository.GetByEmailAsync(Email, cancellationToken)`
   - Se `user == null` → Retornar `Result.Failure(UserErrors.InvalidCredentials)`
   - **Nota:** Retornar erro genérico para prevenir enumeração de usuários

3. **Verificação de Conta:**
   - Se `user.IsDeleted` → Retornar `Result.Failure(UserErrors.AccountDeactivated)`
   - Se `user.IsLocked` → Retornar `Result.Failure(UserErrors.AccountLocked)`
   - Se `user.EmailConfirmed == false` → Retornar `Result.Failure(UserErrors.EmailNotConfirmed)`

4. **Verificação de Senha:**
   - Executar `bool isValid = _passwordHasher.VerifyPassword(command.Password, user.PasswordHash)`
   - Se `!isValid`:
     - Incrementar `user.FailedLoginAttempts++`
     - Se `FailedLoginAttempts >= 5` → `user.LockAccount(TimeSpan.FromMinutes(30))`
     - Salvar via `_unitOfWork.SaveChangesAsync()`
     - Retornar `Result.Failure(UserErrors.InvalidCredentials)`
   - Se válido → Reset: `user.FailedLoginAttempts = 0`

5. **Criação de Sessão:**
   - Instanciar sessão via método do aggregate:
     ```csharp
     var session = user.CreateSession(
         ipAddress: command.IpAddress,
         userAgent: command.UserAgent,
         expiresAt: DateTime.UtcNow.AddDays(30)
     );
     ```
   - Internamente adiciona evento: `AddDomainEvent(new SessionCreatedEvent(UserId, session.Id))`

6. **Geração de Tokens:**
   - Gerar Access Token (JWT):
     ```csharp
     var accessToken = _jwtTokenGenerator.Generate(
         userId: user.Id,
         email: user.Email.Value,
         roles: user.Roles,
         expiresIn: TimeSpan.FromHours(1)
     );
     ```
   - Gerar Refresh Token (aleatório seguro):
     ```csharp
     var refreshToken = _tokenGenerator.GenerateRefreshToken();
     ```
   - Armazenar refresh token: `session.SetRefreshToken(refreshToken)`

7. **Persistência:**
   - Salvar sessão e atualizar usuário: `await _unitOfWork.SaveChangesAsync(cancellationToken)`

8. **Construção de Response:**
   - Mapear `user` para `UserDto` via AutoMapper
   - Criar response:
     ```csharp
     var response = new LoginResponse(
         AccessToken: accessToken,
         RefreshToken: refreshToken,
         ExpiresAt: DateTime.UtcNow.AddHours(1),
         User: userDto
     );
     ```

9. **Retorno:**
   - `return Result.Success(response)`

**Eventos Disparados:**
- `SessionCreatedEvent` (Domain)

**Erros Possíveis:**
- `UserErrors.InvalidCredentials`
- `UserErrors.AccountDeactivated`
- `UserErrors.AccountLocked`
- `UserErrors.EmailNotConfirmed`

---

### 3. ChangePasswordCommand

**Módulo:** Users
**Namespace:** `Bcommerce.Modules.Users.Application.Commands.ChangePassword`

**Record Definition:**
```csharp
public record ChangePasswordCommand(
    Guid UserId,
    string CurrentPassword,
    string NewPassword
) : ICommand;
```

**Algoritmo Passo a Passo:**

1. **Validação de Entrada:**
   - Validar `UserId` não vazio
   - Validar `CurrentPassword` não nulo
   - Validar `NewPassword` com política de segurança (mesma do registro)
   - Validar `NewPassword != CurrentPassword`

2. **Recuperação do Usuário:**
   - `var user = await _userRepository.GetByIdAsync(UserId, cancellationToken)`
   - Se `user == null` → `Result.Failure(UserErrors.NotFound)`

3. **Verificação de Senha Atual:**
   - `bool isValid = _passwordHasher.VerifyPassword(CurrentPassword, user.PasswordHash)`
   - Se `!isValid` → `Result.Failure(UserErrors.InvalidCurrentPassword)`

4. **Hashing da Nova Senha:**
   - `string newPasswordHash = _passwordHasher.HashPassword(NewPassword)`

5. **Atualização da Entidade:**
   - Invocar método do aggregate:
     ```csharp
     user.ChangePassword(newPasswordHash);
     ```
   - Internamente:
     - Atualiza `PasswordHash`
     - Atualiza `PasswordChangedAt = DateTime.UtcNow`
     - Adiciona evento: `AddDomainEvent(new PasswordChangedEvent(Id))`

6. **Revogação de Sessões:**
   - Handler do evento `PasswordChangedEvent` revogará sessões
   - OU pode ser feito diretamente:
     ```csharp
     var sessions = await _sessionRepository.GetActiveSessionsByUserIdAsync(UserId);
     foreach (var session in sessions.Where(s => s.Id != currentSessionId))
     {
         session.Revoke("PasswordChanged");
     }
     ```

7. **Persistência:**
   - `await _unitOfWork.SaveChangesAsync(cancellationToken)`

8. **Retorno:**
   - `return Result.Success()`

**Eventos Disparados:**
- `PasswordChangedEvent` (Domain)

**Erros Possíveis:**
- `UserErrors.NotFound`
- `UserErrors.InvalidCurrentPassword`
- `UserErrors.WeakPassword`

---

### 4. AddAddressCommand

**Módulo:** Users
**Namespace:** `Bcommerce.Modules.Users.Application.Commands.AddAddress`

**Record Definition:**
```csharp
public record AddAddressCommand(
    Guid UserId,
    string Street,
    string Number,
    string? Complement,
    string Neighborhood,
    string City,
    string State,
    string PostalCode,
    string Country,
    bool IsDefault
) : ICommand<Guid>;
```

**Algoritmo Passo a Passo:**

1. **Validação de Entrada:**
   - Validar todos os campos obrigatórios não nulos
   - Validar `PostalCode` formato brasileiro (XXXXX-XXX) ou internacional
   - Validar `State` (2 letras - UF)
   - **Validator:** `AddAddressCommandValidator`

2. **Recuperação do Usuário:**
   - `var user = await _userRepository.GetByIdAsync(UserId, includeAddresses: true)`
   - Se `user == null` → `Result.Failure(UserErrors.NotFound)`

3. **Criação de Value Objects:**
   - `var postalCode = PostalCode.Create(command.PostalCode)`
   - Se validação falhar → retornar erro

4. **Validação de Endereço via API Externa (opcional):**
   - Se CEP brasileiro:
     ```csharp
     var addressData = await _viaCepService.GetAddressAsync(postalCode.Value);
     if (addressData != null)
     {
         // Validar consistência de cidade/estado
         if (addressData.City != command.City)
         {
             return Result.Failure(AddressErrors.InconsistentData);
         }
     }
     ```

5. **Criação da Entidade Address:**
   - Invocar método do aggregate User:
     ```csharp
     var address = user.AddAddress(
         street: command.Street,
         number: command.Number,
         complement: command.Complement,
         neighborhood: command.Neighborhood,
         city: command.City,
         state: command.State,
         postalCode: postalCode,
         country: command.Country,
         isDefault: command.IsDefault
     );
     ```
   - Internamente:
     - Se `isDefault == true`, outros endereços têm `IsDefault` setado para `false`
     - Adiciona evento: `AddDomainEvent(new AddressAddedEvent(UserId, address.Id))`

6. **Geocodificação (Assíncrona via Event Handler):**
   - Handler de `AddressAddedEvent` fará geocoding:
     ```csharp
     var (lat, lng) = await _geocodingService.GeocodeAsync(fullAddress);
     address.SetCoordinates(lat, lng);
     ```

7. **Persistência:**
   - `await _unitOfWork.SaveChangesAsync(cancellationToken)`

8. **Retorno:**
   - `return Result.Success(address.Id)`

**Eventos Disparados:**
- `AddressAddedEvent` (Domain)

**Erros Possíveis:**
- `UserErrors.NotFound`
- `AddressErrors.InvalidPostalCode`
- `AddressErrors.InconsistentData`

---

### 5. UpdateProfileCommand

**Módulo:** Users
**Namespace:** `Bcommerce.Modules.Users.Application.Commands.UpdateProfile`

**Record Definition:**
```csharp
public record UpdateProfileCommand(
    Guid UserId,
    string FirstName,
    string LastName,
    string? PhoneNumber,
    DateTime? DateOfBirth,
    string? Gender,
    string? AvatarUrl
) : ICommand;
```

**Algoritmo Passo a Passo:**

1. **Validação de Entrada:**
   - Validar `FirstName` e `LastName` não vazios
   - Se `PhoneNumber` fornecido → validar formato
   - Se `DateOfBirth` fornecido → validar idade >= 18 anos
   - Se `Gender` fornecido → validar enum (Male, Female, Other, PreferNotToSay)
   - Se `AvatarUrl` fornecido → validar URL válida

2. **Recuperação do Usuário:**
   - `var user = await _userRepository.GetByIdAsync(UserId, includeProfile: true)`
   - Se `user == null` → `Result.Failure(UserErrors.NotFound)`

3. **Criação de Value Objects:**
   - Se phone fornecido: `var phone = PhoneNumber.Create(command.PhoneNumber)`

4. **Atualização do Perfil:**
   - Se `user.Profile == null`:
     ```csharp
     user.CreateProfile(
         firstName: command.FirstName,
         lastName: command.LastName,
         phone: phone,
         dateOfBirth: command.DateOfBirth,
         gender: command.Gender,
         avatarUrl: command.AvatarUrl
     );
     ```
   - Se já existe:
     ```csharp
     user.Profile.Update(
         firstName: command.FirstName,
         lastName: command.LastName,
         phone: phone,
         dateOfBirth: command.DateOfBirth,
         gender: command.Gender,
         avatarUrl: command.AvatarUrl
     );
     ```
   - Adiciona evento: `AddDomainEvent(new ProfileUpdatedEvent(UserId, Profile.Id))`

5. **Invalidação de Cache:**
   - Handler do evento ou direto:
     ```csharp
     await _cacheService.RemoveAsync($"user:profile:{UserId}");
     ```

6. **Persistência:**
   - `await _unitOfWork.SaveChangesAsync(cancellationToken)`

7. **Retorno:**
   - `return Result.Success()`

**Eventos Disparados:**
- `ProfileUpdatedEvent` (Domain)

---

### 6. DeleteUserCommand

**Módulo:** Users
**Namespace:** `Bcommerce.Modules.Users.Application.Commands.DeleteUser`

**Record Definition:**
```csharp
public record DeleteUserCommand(
    Guid UserId,
    string Password,
    string Reason
) : ICommand;
```

**Algoritmo Passo a Passo:**

1. **Validação de Entrada:**
   - Validar `Password` não nulo (confirmar identidade)
   - Validar `Reason` não vazio (compliance LGPD)

2. **Recuperação do Usuário:**
   - `var user = await _userRepository.GetByIdAsync(UserId)`
   - Se `user == null` → `Result.Failure(UserErrors.NotFound)`

3. **Verificação de Senha:**
   - `bool isValid = _passwordHasher.VerifyPassword(command.Password, user.PasswordHash)`
   - Se `!isValid` → `Result.Failure(UserErrors.InvalidPassword)`

4. **Soft Delete:**
   - Invocar método:
     ```csharp
     user.Delete(command.Reason);
     ```
   - Internamente:
     - `IsDeleted = true`
     - `DeletedAt = DateTime.UtcNow`
     - `DeletionReason = reason`
     - Adiciona evento: `AddDomainEvent(new UserDeletedEvent(Id))`

5. **Revogação de Sessões:**
   - Todas as sessões revogadas imediatamente
   - Tokens adicionados à blacklist

6. **Processamento Assíncrono via Event:**
   - `UserDeletedEvent` dispara:
     - Publicação de `UserDeletedIntegrationEvent`
     - Outros módulos (Orders, Analytics) anonimizam dados
     - Email de confirmação enviado

7. **Agendamento de Hard Delete:**
   - Agendar job para 30 dias:
     ```csharp
     _backgroundJobs.Schedule<HardDeleteUserJob>(
         userId: UserId,
         executeAt: DateTime.UtcNow.AddDays(30)
     );
     ```

8. **Persistência:**
   - `await _unitOfWork.SaveChangesAsync(cancellationToken)`

9. **Retorno:**
   - `return Result.Success()`

**Eventos Disparados:**
- `UserDeletedEvent` (Domain)
- `UserDeletedIntegrationEvent` (Integration)

---

## Módulo Users - Queries

### 1. GetUserByIdQuery

**Módulo:** Users
**Namespace:** `Bcommerce.Modules.Users.Application.Queries.GetUserById`

**Record Definition:**
```csharp
public record GetUserByIdQuery(Guid UserId) : IQuery<UserDto>;

public record UserDto(
    Guid Id,
    string Email,
    string FirstName,
    string LastName,
    string? PhoneNumber,
    string? AvatarUrl,
    DateTime CreatedAt,
    bool IsEmailConfirmed
);
```

**Algoritmo Passo a Passo:**

1. **Sanitização:**
   - Validar `UserId != Guid.Empty`
   - Se inválido → `Result.Failure(UserErrors.InvalidId)`

2. **Cache Check (Cache-Aside Pattern):**
   - Tentar recuperar do cache:
     ```csharp
     var cacheKey = $"user:{UserId}";
     var cachedUser = await _cacheService.GetAsync<UserDto>(cacheKey, cancellationToken);
     if (cachedUser != null)
     {
         return Result.Success(cachedUser);
     }
     ```

3. **Acesso a Dados:**
   - Query otimizada com `AsNoTracking()`:
     ```csharp
     var user = await _context.Users
         .AsNoTracking()
         .Include(u => u.Profile)
         .Where(u => u.Id == query.UserId && !u.IsDeleted)
         .FirstOrDefaultAsync(cancellationToken);
     ```

4. **Verificação de Existência:**
   - Se `user == null` → `Result.Failure(UserErrors.NotFound)`

5. **Mapeamento:**
   - Usar AutoMapper ou mapeamento manual:
     ```csharp
     var dto = new UserDto(
         Id: user.Id,
         Email: user.Email.Value,
         FirstName: user.Profile?.FirstName ?? user.FirstName,
         LastName: user.Profile?.LastName ?? user.LastName,
         PhoneNumber: user.Profile?.PhoneNumber?.Value,
         AvatarUrl: user.Profile?.AvatarUrl,
         CreatedAt: user.CreatedAt,
         IsEmailConfirmed: user.EmailConfirmed
     );
     ```

6. **Atualização de Cache:**
   - Armazenar no cache com TTL de 15 minutos:
     ```csharp
     await _cacheService.SetAsync(
         cacheKey,
         dto,
         TimeSpan.FromMinutes(15),
         cancellationToken
     );
     ```

7. **Retorno:**
   - `return Result.Success(dto)`

**Dependências:**
- `UsersDbContext` (read-only)
- `ICacheService` (Redis)
- `IMapper` (AutoMapper)

**Performance:**
- Cache hit: ~5ms
- Cache miss + DB: ~50ms

---

### 2. GetUserAddressesQuery

**Módulo:** Users
**Namespace:** `Bcommerce.Modules.Users.Application.Queries.GetUserAddresses`

**Record Definition:**
```csharp
public record GetUserAddressesQuery(Guid UserId) : IQuery<List<AddressDto>>;

public record AddressDto(
    Guid Id,
    string Street,
    string Number,
    string? Complement,
    string Neighborhood,
    string City,
    string State,
    string PostalCode,
    string Country,
    bool IsDefault,
    decimal? Latitude,
    decimal? Longitude
);
```

**Algoritmo Passo a Passo:**

1. **Sanitização:**
   - Validar `UserId != Guid.Empty`

2. **Cache Check:**
   - `var cacheKey = $"user:addresses:{UserId}"`
   - Verificar cache (TTL: 30 min)

3. **Acesso a Dados:**
   - Query otimizada:
     ```csharp
     var addresses = await _context.Addresses
         .AsNoTracking()
         .Where(a => a.UserId == query.UserId && !a.IsDeleted)
         .OrderByDescending(a => a.IsDefault)
         .ThenBy(a => a.CreatedAt)
         .ToListAsync(cancellationToken);
     ```

4. **Mapeamento:**
   - Mapear para DTOs:
     ```csharp
     var dtos = addresses.Select(a => new AddressDto(
         Id: a.Id,
         Street: a.Street,
         Number: a.Number,
         Complement: a.Complement,
         Neighborhood: a.Neighborhood,
         City: a.City,
         State: a.State,
         PostalCode: a.PostalCode.Value,
         Country: a.Country,
         IsDefault: a.IsDefault,
         Latitude: a.Latitude,
         Longitude: a.Longitude
     )).ToList();
     ```

5. **Atualização de Cache:**
   - `await _cacheService.SetAsync(cacheKey, dtos, TimeSpan.FromMinutes(30))`

6. **Retorno:**
   - `return Result.Success(dtos)`

---

### 3. GetActiveSessionsQuery

**Módulo:** Users
**Namespace:** `Bcommerce.Modules.Users.Application.Queries.GetActiveSessions`

**Record Definition:**
```csharp
public record GetActiveSessionsQuery(Guid UserId) : IQuery<List<SessionDto>>;

public record SessionDto(
    Guid Id,
    string IpAddress,
    string UserAgent,
    DateTime CreatedAt,
    DateTime ExpiresAt,
    bool IsCurrent
);
```

**Algoritmo Passo a Passo:**

1. **Sanitização:**
   - Validar `UserId`

2. **Acesso a Dados:**
   - Query:
     ```csharp
     var sessions = await _context.Sessions
         .AsNoTracking()
         .Where(s => s.UserId == query.UserId && s.IsActive && s.ExpiresAt > DateTime.UtcNow)
         .OrderByDescending(s => s.CreatedAt)
         .ToListAsync(cancellationToken);
     ```

3. **Identificação de Sessão Atual:**
   - Recuperar session ID do contexto HTTP:
     ```csharp
     var currentSessionId = _httpContextAccessor.HttpContext?.User
         .FindFirst("SessionId")?.Value;
     ```

4. **Mapeamento:**
   - Mapear para DTOs marcando sessão atual:
     ```csharp
     var dtos = sessions.Select(s => new SessionDto(
         Id: s.Id,
         IpAddress: s.IpAddress,
         UserAgent: ParseUserAgent(s.UserAgent), // Parse para browser/OS
         CreatedAt: s.CreatedAt,
         ExpiresAt: s.ExpiresAt,
         IsCurrent: s.Id.ToString() == currentSessionId
     )).ToList();
     ```

5. **Retorno:**
   - `return Result.Success(dtos)`

**Nota:** Não cachear (dados sensíveis de segurança)

---

## Módulo Catalog - Commands

### 1. CreateProductCommand

**Módulo:** Catalog
**Namespace:** `Bcommerce.Modules.Catalog.Application.Commands.CreateProduct`

**Record Definition:**
```csharp
public record CreateProductCommand(
    string Name,
    string Description,
    string Sku,
    decimal Price,
    string Currency,
    Guid CategoryId,
    Guid? BrandId,
    int InitialStock,
    ProductDimensionsDto? Dimensions,
    decimal? Weight
) : ICommand<Guid>;

public record ProductDimensionsDto(
    decimal Height,
    decimal Width,
    decimal Length,
    string Unit // "cm" or "in"
);
```

**Algoritmo Passo a Passo:**

1. **Validação de Entrada:**
   - Validar `Name` não vazio (min: 3, max: 200 chars)
   - Validar `Description` não vazio (max: 5000 chars)
   - Validar `Sku` único e formato válido (regex: `^[A-Z0-9-]+$`)
   - Validar `Price > 0`
   - Validar `Currency` (ISO 4217: BRL, USD, EUR)
   - Validar `CategoryId` existe
   - Se `BrandId` fornecido → validar existe
   - Validar `InitialStock >= 0`
   - **Validator:** `CreateProductCommandValidator`

2. **Verificação de Unicidade de SKU:**
   - `bool exists = await _productRepository.ExistsBySkuAsync(command.Sku)`
   - Se `exists` → `Result.Failure(ProductErrors.SkuAlreadyExists)`

3. **Verificação de Categoria:**
   - `var category = await _categoryRepository.GetByIdAsync(command.CategoryId)`
   - Se `category == null || !category.IsActive` → `Result.Failure(CategoryErrors.NotFound)`

4. **Verificação de Marca (se fornecida):**
   - Se `BrandId.HasValue`:
     ```csharp
     var brand = await _brandRepository.GetByIdAsync(command.BrandId.Value);
     if (brand == null) return Result.Failure(BrandErrors.NotFound);
     ```

5. **Criação de Value Objects:**
   - `var sku = Sku.Create(command.Sku)` → validação interna
   - `var price = Money.Create(command.Price, command.Currency)`
   - Se `Dimensions` fornecido:
     ```csharp
     var dimensions = ProductDimensions.Create(
         height: command.Dimensions.Height,
         width: command.Dimensions.Width,
         length: command.Dimensions.Length,
         unit: command.Dimensions.Unit
     );
     ```
   - Se `Weight` fornecido: `var weight = Weight.Create(command.Weight, "kg")`

6. **Criação da Entidade Product:**
   - Invocar factory method:
     ```csharp
     var product = Product.Create(
         name: command.Name,
         description: command.Description,
         sku: sku,
         price: price,
         categoryId: command.CategoryId,
         brandId: command.BrandId,
         dimensions: dimensions,
         weight: weight
     );
     ```
   - Internamente:
     - Status = `Draft` (não publicado)
     - Slug gerado: `GenerateSlug(name)` → "notebook-dell-inspiron-15"
     - Adiciona evento: `AddDomainEvent(new ProductCreatedEvent(Id))`

7. **Inicialização de Estoque:**
   - Se `InitialStock > 0`:
     ```csharp
     product.InitializeStock(command.InitialStock);
     ```
   - Internamente cria `Stock` value object:
     ```csharp
     Stock = Stock.Create(
         available: initialStock,
         reserved: 0
     );
     ```

8. **Persistência:**
   - `await _productRepository.AddAsync(product, cancellationToken)`
   - `await _unitOfWork.SaveChangesAsync(cancellationToken)`

9. **Processamento de Eventos (Outbox):**
   - `ProductCreatedEvent` capturado
   - Handler assíncrono:
     - Gera slug único (se conflito, adiciona sufixo)
     - Cria entrada em índice de busca

10. **Retorno:**
    - `return Result.Success(product.Id)`

**Eventos Disparados:**
- `ProductCreatedEvent` (Domain)

**Erros Possíveis:**
- `ProductErrors.SkuAlreadyExists`
- `ProductErrors.InvalidPrice`
- `CategoryErrors.NotFound`
- `BrandErrors.NotFound`

**Arquivo:** `/src/Modules/Catalog/Bcommerce.Modules.Catalog.Application/Commands/CreateProduct/CreateProductCommandHandler.cs`

---

### 2. PublishProductCommand

**Módulo:** Catalog
**Namespace:** `Bcommerce.Modules.Catalog.Application.Commands.PublishProduct`

**Record Definition:**
```csharp
public record PublishProductCommand(Guid ProductId) : ICommand;
```

**Algoritmo Passo a Passo:**

1. **Validação de Entrada:**
   - Validar `ProductId != Guid.Empty`

2. **Recuperação do Produto:**
   - `var product = await _productRepository.GetByIdAsync(ProductId, includeImages: true)`
   - Se `product == null` → `Result.Failure(ProductErrors.NotFound)`

3. **Validação de Regras de Negócio:**
   - Verificar se produto pode ser publicado:
     ```csharp
     if (product.Status == ProductStatus.Published)
     {
         return Result.Failure(ProductErrors.AlreadyPublished);
     }

     if (product.Price.Amount <= 0)
     {
         return Result.Failure(ProductErrors.InvalidPriceForPublishing);
     }

     if (!product.Images.Any())
     {
         return Result.Failure(ProductErrors.RequiresAtLeastOneImage);
     }

     if (string.IsNullOrEmpty(product.Description))
     {
         return Result.Failure(ProductErrors.RequiresDescription);
     }
     ```

4. **Verificação de Categoria Ativa:**
   - `var category = await _categoryRepository.GetByIdAsync(product.CategoryId)`
   - Se `!category.IsActive` → `Result.Failure(CategoryErrors.InactiveCategory)`

5. **Publicação do Produto:**
   - Invocar método do aggregate:
     ```csharp
     product.Publish();
     ```
   - Internamente:
     - `Status = ProductStatus.Published`
     - `PublishedAt = DateTime.UtcNow`
     - Adiciona evento: `AddDomainEvent(new ProductPublishedEvent(Id))`

6. **Persistência:**
   - `await _unitOfWork.SaveChangesAsync(cancellationToken)`

7. **Processamento de Eventos:**
   - `ProductPublishedEvent` handler:
     - Indexa produto no Elasticsearch/Algolia
     - **Publica `ProductPublishedIntegrationEvent`**
     - Notifica usuários em waitlist (se produto estava esgotado)
     - Invalida cache

8. **Retorno:**
   - `return Result.Success()`

**Eventos Disparados:**
- `ProductPublishedEvent` (Domain)
- `ProductPublishedIntegrationEvent` (Integration)

**Erros Possíveis:**
- `ProductErrors.NotFound`
- `ProductErrors.AlreadyPublished`
- `ProductErrors.InvalidPriceForPublishing`
- `ProductErrors.RequiresAtLeastOneImage`
- `CategoryErrors.InactiveCategory`

---

### 3. UpdateProductPriceCommand

**Módulo:** Catalog
**Namespace:** `Bcommerce.Modules.Catalog.Application.Commands.UpdateProductPrice`

**Record Definition:**
```csharp
public record UpdateProductPriceCommand(
    Guid ProductId,
    decimal NewPrice,
    string Currency
) : ICommand;
```

**Algoritmo Passo a Passo:**

1. **Validação de Entrada:**
   - Validar `NewPrice > 0`
   - Validar `Currency` (ISO 4217)

2. **Recuperação do Produto:**
   - `var product = await _productRepository.GetByIdAsync(ProductId)`
   - Se `product == null` → `Result.Failure(ProductErrors.NotFound)`

3. **Criação de Value Object:**
   - `var newPrice = Money.Create(command.NewPrice, command.Currency)`
   - Se moeda diferente da atual:
     ```csharp
     if (newPrice.Currency != product.Price.Currency)
     {
         return Result.Failure(ProductErrors.CurrencyMismatch);
     }
     ```

4. **Captura de Preço Antigo:**
   - `var oldPrice = product.Price` (para evento)

5. **Atualização do Preço:**
   - Invocar método:
     ```csharp
     product.UpdatePrice(newPrice);
     ```
   - Internamente:
     - `Price = newPrice`
     - Adiciona evento: `AddDomainEvent(new ProductPriceChangedEvent(Id, oldPrice.Amount, newPrice.Amount))`

6. **Persistência:**
   - `await _unitOfWork.SaveChangesAsync(cancellationToken)`

7. **Processamento de Eventos:**
   - Handler de `ProductPriceChangedEvent`:
     - Invalida cache
     - Atualiza índice de busca
     - Notifica usuários em price alerts
     - Registra histórico de preços (analytics)

8. **Retorno:**
   - `return Result.Success()`

**Eventos Disparados:**
- `ProductPriceChangedEvent` (Domain)

---

### 4. AddProductImageCommand

**Módulo:** Catalog
**Namespace:** `Bcommerce.Modules.Catalog.Application.Commands.AddProductImage`

**Record Definition:**
```csharp
public record AddProductImageCommand(
    Guid ProductId,
    Stream ImageStream,
    string FileName,
    string ContentType,
    bool IsPrimary,
    int DisplayOrder
) : ICommand<Guid>;
```

**Algoritmo Passo a Passo:**

1. **Validação de Entrada:**
   - Validar `ImageStream != null && ImageStream.Length > 0`
   - Validar tamanho máximo (ex: 5MB):
     ```csharp
     if (ImageStream.Length > 5 * 1024 * 1024)
     {
         return Result.Failure(ProductErrors.ImageTooLarge);
     }
     ```
   - Validar `ContentType` permitido (image/jpeg, image/png, image/webp)
   - Validar `FileName` não vazio

2. **Recuperação do Produto:**
   - `var product = await _productRepository.GetByIdAsync(ProductId, includeImages: true)`
   - Se `product == null` → `Result.Failure(ProductErrors.NotFound)`

3. **Validação de Limite de Imagens:**
   - `if (product.Images.Count >= 10)` → `Result.Failure(ProductErrors.MaxImagesReached)`

4. **Upload da Imagem:**
   - Upload para storage (Azure Blob, AWS S3, local):
     ```csharp
     var uploadResult = await _imageStorageService.UploadAsync(
         stream: command.ImageStream,
         fileName: $"{Guid.NewGuid()}-{command.FileName}",
         contentType: command.ContentType,
         folder: $"products/{ProductId}",
         cancellationToken: cancellationToken
     );

     if (!uploadResult.IsSuccess)
     {
         return Result.Failure(ProductErrors.ImageUploadFailed);
     }
     ```

5. **Geração de Thumbnails (Assíncrona):**
   - Gerar variações (opcional, pode ser via event handler):
     ```csharp
     var thumbnailUrl = await _imageProcessingService.GenerateThumbnailAsync(
         originalUrl: uploadResult.Url,
         width: 300,
         height: 300
     );
     ```

6. **Criação da Entidade ProductImage:**
   - Invocar método do aggregate:
     ```csharp
     var image = product.AddImage(
         url: uploadResult.Url,
         thumbnailUrl: thumbnailUrl,
         isPrimary: command.IsPrimary,
         displayOrder: command.DisplayOrder
     );
     ```
   - Internamente:
     - Se `IsPrimary == true`, outras imagens marcadas como `IsPrimary = false`
     - Adiciona evento: `AddDomainEvent(new ProductImageAddedEvent(ProductId, image.Id, uploadResult.Url, IsPrimary))`

7. **Persistência:**
   - `await _unitOfWork.SaveChangesAsync(cancellationToken)`

8. **Processamento de Eventos:**
   - Handler gera otimizações (WebP, thumbnails, etc.)

9. **Retorno:**
   - `return Result.Success(image.Id)`

**Eventos Disparados:**
- `ProductImageAddedEvent` (Domain)

**Dependências:**
- `IImageStorageService`
- `IImageProcessingService`

---

### 5. ReserveStockCommand

**Módulo:** Catalog
**Namespace:** `Bcommerce.Modules.Catalog.Application.Commands.ReserveStock`

**Record Definition:**
```csharp
public record ReserveStockCommand(
    Guid ProductId,
    int Quantity,
    Guid OrderId
) : ICommand<Guid>; // Retorna ReservationId
```

**Algoritmo Passo a Passo:**

1. **Validação de Entrada:**
   - Validar `Quantity > 0`
   - Validar `ProductId != Guid.Empty`
   - Validar `OrderId != Guid.Empty`

2. **Recuperação do Produto:**
   - `var product = await _productRepository.GetByIdAsync(ProductId)`
   - Se `product == null` → `Result.Failure(ProductErrors.NotFound)`

3. **Verificação de Disponibilidade:**
   - Verificar se estoque disponível é suficiente:
     ```csharp
     if (product.Stock.Available < command.Quantity)
     {
         return Result.Failure(ProductErrors.InsufficientStock(
             requested: command.Quantity,
             available: product.Stock.Available
         ));
     }
     ```

4. **Verificação de Produto Publicado:**
   - Se `product.Status != ProductStatus.Published`:
     ```csharp
     return Result.Failure(ProductErrors.ProductNotAvailable);
     ```

5. **Criação da Reserva:**
   - Invocar método do aggregate:
     ```csharp
     var reservation = product.ReserveStock(
         quantity: command.Quantity,
         orderId: command.OrderId,
         expiresAt: DateTime.UtcNow.AddMinutes(30)
     );
     ```
   - Internamente:
     - Atualiza `Stock.Available -= quantity`
     - Atualiza `Stock.Reserved += quantity`
     - Cria entidade `StockReservation`:
       ```csharp
       new StockReservation
       {
           Id = Guid.NewGuid(),
           ProductId = Id,
           OrderId = orderId,
           Quantity = quantity,
           ReservedAt = DateTime.UtcNow,
           ExpiresAt = expiresAt,
           Status = ReservationStatus.Active
       }
       ```
     - Adiciona evento: `AddDomainEvent(new StockReservedEvent(ProductId, quantity, reservation.Id))`

6. **Agendamento de Expiração:**
   - Handler agenda job de cleanup:
     ```csharp
     _backgroundJobs.Schedule<ReleaseExpiredReservationJob>(
         reservationId: reservation.Id,
         executeAt: reservation.ExpiresAt
     );
     ```

7. **Persistência:**
   - `await _unitOfWork.SaveChangesAsync(cancellationToken)`

8. **Verificação de Estoque Esgotado:**
   - Se `product.Stock.Available == 0`:
     - Disparar evento `ProductOutOfStockEvent`

9. **Retorno:**
   - `return Result.Success(reservation.Id)`

**Eventos Disparados:**
- `StockReservedEvent` (Domain)
- `StockReservedIntegrationEvent` (Integration)
- `ProductOutOfStockEvent` (se aplicável)

**Erros Possíveis:**
- `ProductErrors.NotFound`
- `ProductErrors.InsufficientStock`
- `ProductErrors.ProductNotAvailable`

---

### 6. ReleaseStockReservationCommand

**Módulo:** Catalog
**Namespace:** `Bcommerce.Modules.Catalog.Application.Commands.ReleaseStockReservation`

**Record Definition:**
```csharp
public record ReleaseStockReservationCommand(
    Guid ReservationId,
    string Reason
) : ICommand;
```

**Algoritmo Passo a Passo:**

1. **Validação de Entrada:**
   - Validar `ReservationId != Guid.Empty`
   - Validar `Reason` não vazio

2. **Recuperação da Reserva:**
   - Query com join:
     ```csharp
     var reservation = await _context.StockReservations
         .Include(r => r.Product)
         .FirstOrDefaultAsync(r => r.Id == command.ReservationId);
     ```
   - Se `reservation == null` → `Result.Failure(ReservationErrors.NotFound)`

3. **Verificação de Status:**
   - Se `reservation.Status != ReservationStatus.Active`:
     ```csharp
     return Result.Failure(ReservationErrors.AlreadyReleased);
     ```

4. **Liberação da Reserva:**
   - Invocar método:
     ```csharp
     reservation.Release(command.Reason);
     ```
   - Internamente:
     - `Status = ReservationStatus.Released`
     - `ReleasedAt = DateTime.UtcNow`
     - `ReleaseReason = reason`

5. **Atualização do Estoque do Produto:**
   - Retornar estoque ao disponível:
     ```csharp
     var product = reservation.Product;
     product.Stock.Release(reservation.Quantity);
     ```
   - Internamente:
     - `Stock.Available += quantity`
     - `Stock.Reserved -= quantity`
     - Adiciona evento: `AddDomainEvent(new StockReleasedEvent(ProductId, quantity, ReservationId))`

6. **Persistência:**
   - `await _unitOfWork.SaveChangesAsync(cancellationToken)`

7. **Processamento de Eventos:**
   - Handler:
     - Invalida cache de disponibilidade
     - **Publica `StockReleasedIntegrationEvent`**
     - Se produto estava esgotado e agora tem estoque → notifica waitlist

8. **Retorno:**
   - `return Result.Success()`

**Eventos Disparados:**
- `StockReleasedEvent` (Domain)
- `StockReleasedIntegrationEvent` (Integration)

---

## Módulo Catalog - Queries

### 1. GetProductByIdQuery

**Módulo:** Catalog
**Namespace:** `Bcommerce.Modules.Catalog.Application.Queries.GetProductById`

**Record Definition:**
```csharp
public record GetProductByIdQuery(Guid ProductId) : IQuery<ProductDto>;

public record ProductDto(
    Guid Id,
    string Name,
    string Description,
    string Sku,
    MoneyDto Price,
    string Slug,
    ProductStatus Status,
    CategoryDto Category,
    BrandDto? Brand,
    StockDto Stock,
    ProductDimensionsDto? Dimensions,
    decimal? Weight,
    List<ProductImageDto> Images,
    decimal? AverageRating,
    int ReviewCount,
    DateTime CreatedAt,
    DateTime? PublishedAt
);

public record MoneyDto(decimal Amount, string Currency);
public record StockDto(int Available, int Reserved, bool IsAvailable);
```

**Algoritmo Passo a Passo:**

1. **Sanitização:**
   - Validar `ProductId != Guid.Empty`
   - Se inválido → `Result.Failure(ProductErrors.InvalidId)`

2. **Cache Check (Cache-Aside Pattern):**
   - Tentar recuperar do cache:
     ```csharp
     var cacheKey = $"product:{ProductId}";
     var cachedProduct = await _cacheService.GetAsync<ProductDto>(cacheKey, cancellationToken);
     if (cachedProduct != null)
     {
         return Result.Success(cachedProduct);
     }
     ```

3. **Acesso a Dados:**
   - Query otimizada com `AsNoTracking()`:
     ```csharp
     var product = await _context.Products
         .AsNoTracking()
         .Include(p => p.Category)
         .Include(p => p.Brand)
         .Include(p => p.Images.OrderBy(i => i.DisplayOrder))
         .Include(p => p.Reviews.Where(r => r.IsApproved))
         .Where(p => p.Id == query.ProductId && !p.IsDeleted)
         .FirstOrDefaultAsync(cancellationToken);
     ```

4. **Verificação de Existência:**
   - Se `product == null` → `Result.Failure(ProductErrors.NotFound)`

5. **Cálculo de Rating Médio:**
   - Calcular rating apenas de reviews aprovadas:
     ```csharp
     var averageRating = product.Reviews.Any()
         ? (decimal?)product.Reviews.Average(r => r.Rating)
         : null;
     var reviewCount = product.Reviews.Count;
     ```

6. **Mapeamento:**
   - Mapear para DTO:
     ```csharp
     var dto = new ProductDto(
         Id: product.Id,
         Name: product.Name,
         Description: product.Description,
         Sku: product.Sku.Value,
         Price: new MoneyDto(product.Price.Amount, product.Price.Currency),
         Slug: product.Slug.Value,
         Status: product.Status,
         Category: new CategoryDto(
             Id: product.Category.Id,
             Name: product.Category.Name,
             Slug: product.Category.Slug.Value
         ),
         Brand: product.Brand != null
             ? new BrandDto(product.Brand.Id, product.Brand.Name, product.Brand.LogoUrl)
             : null,
         Stock: new StockDto(
             Available: product.Stock.Available,
             Reserved: product.Stock.Reserved,
             IsAvailable: product.Stock.IsAvailable
         ),
         Dimensions: product.Dimensions != null
             ? new ProductDimensionsDto(
                 Height: product.Dimensions.Height,
                 Width: product.Dimensions.Width,
                 Length: product.Dimensions.Length,
                 Unit: product.Dimensions.Unit
             )
             : null,
         Weight: product.Weight?.Value,
         Images: product.Images.Select(i => new ProductImageDto(
             Id: i.Id,
             Url: i.Url,
             ThumbnailUrl: i.ThumbnailUrl,
             IsPrimary: i.IsPrimary,
             DisplayOrder: i.DisplayOrder
         )).ToList(),
         AverageRating: averageRating,
         ReviewCount: reviewCount,
         CreatedAt: product.CreatedAt,
         PublishedAt: product.PublishedAt
     );
     ```

7. **Enriquecimento (Opcional):**
   - Se produto físico, incluir cálculo de frete estimado
   - Se produto digital, incluir metadados de download

8. **Atualização de Cache:**
   - Armazenar no cache:
     ```csharp
     await _cacheService.SetAsync(
         cacheKey,
         dto,
         TimeSpan.FromMinutes(30), // TTL maior para produtos
         cancellationToken
     );
     ```

9. **Retorno:**
   - `return Result.Success(dto)`

**Performance:**
- Cache hit: ~5ms
- Cache miss + DB: ~80ms (joins múltiplos)

**Otimizações:**
- Índice composto em `(Id, IsDeleted)`
- Eager loading de relacionamentos necessários
- AsNoTracking para read-only

---

### 2. SearchProductsQuery

**Módulo:** Catalog
**Namespace:** `Bcommerce.Modules.Catalog.Application.Queries.SearchProducts`

**Record Definition:**
```csharp
public record SearchProductsQuery(
    string? SearchTerm,
    Guid? CategoryId,
    Guid? BrandId,
    decimal? MinPrice,
    decimal? MaxPrice,
    ProductStatus? Status,
    bool? InStock,
    int Page = 1,
    int PageSize = 20,
    string SortBy = "Name",
    string SortOrder = "Asc"
) : IQuery<PagedResult<ProductListItemDto>>;

public record ProductListItemDto(
    Guid Id,
    string Name,
    string Slug,
    MoneyDto Price,
    string PrimaryImageUrl,
    decimal? AverageRating,
    int ReviewCount,
    bool IsAvailable
);

public record PagedResult<T>(
    List<T> Items,
    int Page,
    int PageSize,
    int TotalCount,
    int TotalPages
);
```

**Algoritmo Passo a Passo:**

1. **Sanitização:**
   - Validar `Page >= 1`
   - Validar `PageSize <= 100` (limite máximo)
   - Se `SearchTerm` fornecido, sanitizar SQL injection
   - Validar `SortBy` em lista permitida (Name, Price, CreatedAt, Rating)

2. **Cache Check (Opcional para buscas comuns):**
   - Gerar cache key baseado em parâmetros:
     ```csharp
     var cacheKey = $"products:search:{SearchTerm}:{CategoryId}:{Page}:{PageSize}";
     var cached = await _cacheService.GetAsync<PagedResult<ProductListItemDto>>(cacheKey);
     if (cached != null) return Result.Success(cached);
     ```

3. **Construção da Query Base:**
   - Iniciar query:
     ```csharp
     IQueryable<Product> query = _context.Products
         .AsNoTracking()
         .Include(p => p.Category)
         .Include(p => p.Images.Where(i => i.IsPrimary).Take(1))
         .Where(p => !p.IsDeleted);
     ```

4. **Aplicação de Filtros:**
   - **Termo de Busca (Full-Text ou LIKE):**
     ```csharp
     if (!string.IsNullOrWhiteSpace(SearchTerm))
     {
         query = query.Where(p =>
             EF.Functions.Like(p.Name, $"%{SearchTerm}%") ||
             EF.Functions.Like(p.Description, $"%{SearchTerm}%") ||
             EF.Functions.Like(p.Sku.Value, $"%{SearchTerm}%")
         );
     }
     ```
   - **Categoria:**
     ```csharp
     if (CategoryId.HasValue)
     {
         query = query.Where(p => p.CategoryId == CategoryId.Value);
     }
     ```
   - **Marca:**
     ```csharp
     if (BrandId.HasValue)
     {
         query = query.Where(p => p.BrandId == BrandId.Value);
     }
     ```
   - **Faixa de Preço:**
     ```csharp
     if (MinPrice.HasValue)
     {
         query = query.Where(p => p.Price.Amount >= MinPrice.Value);
     }
     if (MaxPrice.HasValue)
     {
         query = query.Where(p => p.Price.Amount <= MaxPrice.Value);
     }
     ```
   - **Status:**
     ```csharp
     if (Status.HasValue)
     {
         query = query.Where(p => p.Status == Status.Value);
     }
     else
     {
         // Default: apenas publicados
         query = query.Where(p => p.Status == ProductStatus.Published);
     }
     ```
   - **Em Estoque:**
     ```csharp
     if (InStock == true)
     {
         query = query.Where(p => p.Stock.Available > 0);
     }
     ```

5. **Contagem Total (antes de paginação):**
   - `int totalCount = await query.CountAsync(cancellationToken);`

6. **Aplicação de Ordenação:**
   - Dynamic sorting:
     ```csharp
     query = SortBy.ToLower() switch
     {
         "price" => SortOrder == "Asc"
             ? query.OrderBy(p => p.Price.Amount)
             : query.OrderByDescending(p => p.Price.Amount),
         "createdat" => SortOrder == "Asc"
             ? query.OrderBy(p => p.CreatedAt)
             : query.OrderByDescending(p => p.CreatedAt),
         "rating" => query.OrderByDescending(p => p.Reviews.Average(r => r.Rating)),
         _ => SortOrder == "Asc"
             ? query.OrderBy(p => p.Name)
             : query.OrderByDescending(p => p.Name)
     };
     ```

7. **Paginação:**
   - Aplicar skip e take:
     ```csharp
     var products = await query
         .Skip((Page - 1) * PageSize)
         .Take(PageSize)
         .ToListAsync(cancellationToken);
     ```

8. **Mapeamento:**
   - Mapear para DTOs:
     ```csharp
     var items = products.Select(p => new ProductListItemDto(
         Id: p.Id,
         Name: p.Name,
         Slug: p.Slug.Value,
         Price: new MoneyDto(p.Price.Amount, p.Price.Currency),
         PrimaryImageUrl: p.Images.FirstOrDefault()?.ThumbnailUrl ?? "/no-image.png",
         AverageRating: p.Reviews.Any()
             ? (decimal?)p.Reviews.Average(r => r.Rating)
             : null,
         ReviewCount: p.Reviews.Count,
         IsAvailable: p.Stock.IsAvailable
     )).ToList();
     ```

9. **Construção do Resultado Paginado:**
   - Calcular total de páginas:
     ```csharp
     int totalPages = (int)Math.Ceiling(totalCount / (double)PageSize);

     var result = new PagedResult<ProductListItemDto>(
         Items: items,
         Page: Page,
         PageSize: PageSize,
         TotalCount: totalCount,
         TotalPages: totalPages
     );
     ```

10. **Atualização de Cache:**
    - Cache apenas buscas comuns (sem filtros complexos):
      ```csharp
      if (string.IsNullOrEmpty(SearchTerm) && !CategoryId.HasValue)
      {
          await _cacheService.SetAsync(cacheKey, result, TimeSpan.FromMinutes(5));
      }
      ```

11. **Retorno:**
    - `return Result.Success(result)`

**Performance:**
- Com índices: ~100-200ms
- Cache hit: ~10ms

**Índices Necessários:**
- `(Status, CategoryId, Price)`
- `(Name)` (full-text ou trigram)
- `(Stock.Available)` (filtered index para > 0)

---

### 3. GetProductsByIdsQuery

**Módulo:** Catalog
**Namespace:** `Bcommerce.Modules.Catalog.Application.Queries.GetProductsByIds`

**Record Definition:**
```csharp
public record GetProductsByIdsQuery(List<Guid> ProductIds) : IQuery<List<ProductDto>>;
```

**Algoritmo Passo a Passo:**

1. **Sanitização:**
   - Validar `ProductIds != null && ProductIds.Any()`
   - Remover duplicatas: `ProductIds = ProductIds.Distinct().ToList()`
   - Limitar quantidade (ex: máximo 100 IDs):
     ```csharp
     if (ProductIds.Count > 100)
     {
         return Result.Failure(ProductErrors.TooManyIds);
     }
     ```

2. **Cache Check (Multi-Get):**
   - Tentar recuperar múltiplos do cache:
     ```csharp
     var cacheKeys = ProductIds.Select(id => $"product:{id}").ToList();
     var cachedProducts = await _cacheService.GetManyAsync<ProductDto>(cacheKeys);

     var missingIds = ProductIds
         .Where(id => !cachedProducts.ContainsKey($"product:{id}"))
         .ToList();
     ```

3. **Acesso a Dados (apenas IDs não cacheados):**
   - Query otimizada:
     ```csharp
     var products = missingIds.Any()
         ? await _context.Products
             .AsNoTracking()
             .Include(p => p.Category)
             .Include(p => p.Brand)
             .Include(p => p.Images)
             .Where(p => missingIds.Contains(p.Id) && !p.IsDeleted)
             .ToListAsync(cancellationToken)
         : new List<Product>();
     ```

4. **Mapeamento:**
   - Mapear produtos do DB:
     ```csharp
     var mappedProducts = _mapper.Map<List<ProductDto>>(products);
     ```

5. **Merge com Cache:**
   - Combinar resultados:
     ```csharp
     var allProducts = cachedProducts.Values
         .Concat(mappedProducts)
         .ToList();
     ```

6. **Atualização de Cache (apenas novos):**
   - Cache produtos que vieram do DB:
     ```csharp
     foreach (var product in mappedProducts)
     {
         await _cacheService.SetAsync(
             $"product:{product.Id}",
             product,
             TimeSpan.FromMinutes(30)
         );
     }
     ```

7. **Ordenação (manter ordem original):**
   - Ordenar conforme lista de IDs original:
     ```csharp
     var orderedProducts = ProductIds
         .Select(id => allProducts.FirstOrDefault(p => p.Id == id))
         .Where(p => p != null)
         .ToList();
     ```

8. **Retorno:**
   - `return Result.Success(orderedProducts)`

**Uso Comum:**
- Carrinho de compras (obter detalhes de múltiplos produtos)
- Histórico de pedidos

---

## Módulo Cart - Commands

### 1. CreateCartCommand

**Módulo:** Cart
**Namespace:** `Bcommerce.Modules.Cart.Application.Commands.CreateCart`

**Record Definition:**
```csharp
public record CreateCartCommand(
    Guid? UserId,
    Guid? SessionId
) : ICommand<Guid>;
```

**Algoritmo Passo a Passo:**

1. **Validação de Entrada:**
   - Pelo menos um deve estar presente:
     ```csharp
     if (!UserId.HasValue && !SessionId.HasValue)
     {
         return Result.Failure(CartErrors.UserIdOrSessionIdRequired);
     }
     ```

2. **Verificação de Carrinho Existente:**
   - Se `UserId` fornecido:
     ```csharp
     var existingCart = await _cartRepository.GetActiveCartByUserIdAsync(UserId.Value);
     if (existingCart != null)
     {
         return Result.Success(existingCart.Id); // Retornar carrinho existente
     }
     ```
   - Se apenas `SessionId`:
     ```csharp
     var existingCart = await _cartRepository.GetActiveCartBySessionIdAsync(SessionId.Value);
     if (existingCart != null)
     {
         return Result.Success(existingCart.Id);
     }
     ```

3. **Criação da Entidade:**
   - Invocar factory method:
     ```csharp
     var cart = ShoppingCart.Create(
         userId: command.UserId,
         sessionId: command.SessionId
     );
     ```
   - Internamente:
     - `Status = CartStatus.Active`
     - `CreatedAt = DateTime.UtcNow`
     - `ExpiresAt = DateTime.UtcNow.AddDays(30)`
     - Adiciona evento: `AddDomainEvent(new CartCreatedEvent(Id, UserId, SessionId))`

4. **Persistência:**
   - `await _cartRepository.AddAsync(cart, cancellationToken)`
   - `await _unitOfWork.SaveChangesAsync(cancellationToken)`

5. **Processamento de Eventos:**
   - Handler de `CartCreatedEvent`:
     - Registra analytics (início de funil)
     - Se usuário autenticado, verifica carrinho de sessão anterior para merge

6. **Retorno:**
   - `return Result.Success(cart.Id)`

**Eventos Disparados:**
- `CartCreatedEvent` (Domain)

---

### 2. AddItemToCartCommand

**Módulo:** Cart
**Namespace:** `Bcommerce.Modules.Cart.Application.Commands.AddItemToCart`

**Record Definition:**
```csharp
public record AddItemToCartCommand(
    Guid CartId,
    Guid ProductId,
    int Quantity
) : ICommand;
```

**Algoritmo Passo a Passo:**

1. **Validação de Entrada:**
   - Validar `Quantity > 0 && Quantity <= 99` (limite por item)
   - Validar `CartId` e `ProductId` não vazios

2. **Recuperação do Carrinho:**
   - `var cart = await _cartRepository.GetByIdAsync(CartId, includeItems: true)`
   - Se `cart == null` → `Result.Failure(CartErrors.NotFound)`

3. **Verificação de Status do Carrinho:**
   - Se `cart.Status != CartStatus.Active`:
     ```csharp
     return Result.Failure(CartErrors.CartNotActive);
     ```

4. **Recuperação do Produto:**
   - Buscar produto via Integration Event ou Query direta:
     ```csharp
     var productResult = await _mediator.Send(
         new GetProductByIdQuery(ProductId),
         cancellationToken
     );

     if (!productResult.IsSuccess)
     {
         return Result.Failure(ProductErrors.NotFound);
     }

     var product = productResult.Value;
     ```

5. **Validação do Produto:**
   - Verificar se está publicado:
     ```csharp
     if (product.Status != ProductStatus.Published)
     {
         return Result.Failure(ProductErrors.ProductNotAvailable);
     }
     ```
   - Verificar estoque disponível:
     ```csharp
     if (product.Stock.Available < Quantity)
     {
         return Result.Failure(ProductErrors.InsufficientStock(
             requested: Quantity,
             available: product.Stock.Available
         ));
     }
     ```

6. **Criação de Snapshot de Preço:**
   - Congelar preço no momento da adição:
     ```csharp
     var priceSnapshot = Money.Create(
         product.Price.Amount,
         product.Price.Currency
     );
     ```

7. **Adição ao Carrinho:**
   - Invocar método do aggregate:
     ```csharp
     cart.AddItem(
         productId: ProductId,
         productName: product.Name,
         quantity: Quantity,
         unitPrice: priceSnapshot,
         imageUrl: product.Images.FirstOrDefault()?.Url
     );
     ```
   - Internamente:
     - Verifica se item já existe → incrementa quantidade
     - Se novo → cria `CartItem`
     - Recalcula `TotalAmount`
     - Adiciona evento: `AddDomainEvent(new ItemAddedToCartEvent(Id, ProductId, Quantity))`

8. **Atualização de Timestamp:**
   - `cart.UpdatedAt = DateTime.UtcNow`

9. **Persistência:**
   - `await _unitOfWork.SaveChangesAsync(cancellationToken)`

10. **Invalidação de Cache:**
    - `await _cacheService.RemoveAsync($"cart:{CartId}")`

11. **Retorno:**
    - `return Result.Success()`

**Eventos Disparados:**
- `ItemAddedToCartEvent` (Domain)

**Erros Possíveis:**
- `CartErrors.NotFound`
- `CartErrors.CartNotActive`
- `ProductErrors.NotFound`
- `ProductErrors.ProductNotAvailable`
- `ProductErrors.InsufficientStock`
- `CartErrors.MaxItemsReached` (se limite atingido, ex: 50 items)

---

### 3. UpdateCartItemQuantityCommand

**Módulo:** Cart
**Namespace:** `Bcommerce.Modules.Cart.Application.Commands.UpdateCartItemQuantity`

**Record Definition:**
```csharp
public record UpdateCartItemQuantityCommand(
    Guid CartId,
    Guid CartItemId,
    int NewQuantity
) : ICommand;
```

**Algoritmo Passo a Passo:**

1. **Validação de Entrada:**
   - Validar `NewQuantity > 0 && NewQuantity <= 99`

2. **Recuperação do Carrinho:**
   - `var cart = await _cartRepository.GetByIdAsync(CartId, includeItems: true)`
   - Se `cart == null` → `Result.Failure(CartErrors.NotFound)`

3. **Localização do Item:**
   - `var item = cart.Items.FirstOrDefault(i => i.Id == CartItemId)`
   - Se `item == null` → `Result.Failure(CartErrors.ItemNotFound)`

4. **Verificação de Estoque:**
   - Buscar estoque atual do produto:
     ```csharp
     var product = await _mediator.Send(
         new GetProductByIdQuery(item.ProductId)
     );

     if (product.Stock.Available < NewQuantity)
     {
         return Result.Failure(ProductErrors.InsufficientStock(
             requested: NewQuantity,
             available: product.Stock.Available
         ));
     }
     ```

5. **Atualização da Quantidade:**
   - Invocar método:
     ```csharp
     cart.UpdateItemQuantity(CartItemId, NewQuantity);
     ```
   - Internamente:
     - Atualiza `item.Quantity = newQuantity`
     - Recalcula `item.TotalPrice = UnitPrice * Quantity`
     - Recalcula `cart.TotalAmount`
     - Atualiza `cart.UpdatedAt`

6. **Persistência:**
   - `await _unitOfWork.SaveChangesAsync(cancellationToken)`

7. **Invalidação de Cache:**
   - `await _cacheService.RemoveAsync($"cart:{CartId}")`

8. **Retorno:**
   - `return Result.Success()`

---

### 4. RemoveItemFromCartCommand

**Módulo:** Cart
**Namespace:** `Bcommerce.Modules.Cart.Application.Commands.RemoveItemFromCart`

**Record Definition:**
```csharp
public record RemoveItemFromCartCommand(
    Guid CartId,
    Guid CartItemId
) : ICommand;
```

**Algoritmo Passo a Passo:**

1. **Validação de Entrada:**
   - Validar IDs não vazios

2. **Recuperação do Carrinho:**
   - `var cart = await _cartRepository.GetByIdAsync(CartId, includeItems: true)`

3. **Remoção do Item:**
   - Invocar método:
     ```csharp
     cart.RemoveItem(CartItemId);
     ```
   - Internamente:
     - Remove item da coleção `_items`
     - Recalcula `TotalAmount`
     - Adiciona evento: `AddDomainEvent(new ItemRemovedFromCartEvent(Id, CartItemId, item.ProductId))`

4. **Verificação de Carrinho Vazio:**
   - Se `cart.Items.Count == 0`:
     - Opcionalmente marcar carrinho como `Empty` ou deletar

5. **Persistência:**
   - `await _unitOfWork.SaveChangesAsync(cancellationToken)`

6. **Retorno:**
   - `return Result.Success()`

**Eventos Disparados:**
- `ItemRemovedFromCartEvent` (Domain)

---

### 5. MergeCartsCommand

**Módulo:** Cart
**Namespace:** `Bcommerce.Modules.Cart.Application.Commands.MergeCarts`

**Record Definition:**
```csharp
public record MergeCartsCommand(
    Guid AnonymousCartId,
    Guid UserCartId
) : ICommand;
```

**Algoritmo Passo a Passo:**

1. **Validação de Entrada:**
   - Validar IDs não vazios
   - Validar IDs diferentes

2. **Recuperação dos Carrinhos:**
   - Recuperar ambos carrinhos:
     ```csharp
     var anonymousCart = await _cartRepository.GetByIdAsync(
         AnonymousCartId,
         includeItems: true
     );
     var userCart = await _cartRepository.GetByIdAsync(
         UserCartId,
         includeItems: true
     );
     ```
   - Se qualquer um for null → erro

3. **Validação de Ownership:**
   - Validar que `anonymousCart.UserId == null` (é anônimo)
   - Validar que `userCart.UserId != null` (é do usuário)

4. **Merge de Items:**
   - Iterar items do carrinho anônimo:
     ```csharp
     foreach (var anonymousItem in anonymousCart.Items)
     {
         var existingItem = userCart.Items
             .FirstOrDefault(i => i.ProductId == anonymousItem.ProductId);

         if (existingItem != null)
         {
             // Item já existe → somar quantidades
             var newQuantity = existingItem.Quantity + anonymousItem.Quantity;
             userCart.UpdateItemQuantity(existingItem.Id, newQuantity);
         }
         else
         {
             // Item novo → adicionar ao carrinho do usuário
             userCart.AddItem(
                 productId: anonymousItem.ProductId,
                 productName: anonymousItem.ProductName,
                 quantity: anonymousItem.Quantity,
                 unitPrice: anonymousItem.UnitPrice,
                 imageUrl: anonymousItem.ImageUrl
             );
         }
     }
     ```

5. **Verificação de Estoque (após merge):**
   - Para cada item, validar estoque disponível:
     ```csharp
     foreach (var item in userCart.Items)
     {
         var product = await _mediator.Send(
             new GetProductByIdQuery(item.ProductId)
         );

         if (product.Stock.Available < item.Quantity)
         {
             // Ajustar quantidade para máximo disponível
             userCart.UpdateItemQuantity(
                 item.Id,
                 Math.Min(item.Quantity, product.Stock.Available)
             );
         }
     }
     ```

6. **Inativação do Carrinho Anônimo:**
   - Marcar carrinho anônimo como merged:
     ```csharp
     anonymousCart.MarkAsMerged(userCart.Id);
     ```
   - Internamente: `Status = CartStatus.Merged`

7. **Persistência:**
   - Salvar ambos carrinhos:
     ```csharp
     await _unitOfWork.SaveChangesAsync(cancellationToken);
     ```

8. **Invalidação de Cache:**
   - Invalidar cache de ambos:
     ```csharp
     await _cacheService.RemoveAsync($"cart:{AnonymousCartId}");
     await _cacheService.RemoveAsync($"cart:{UserCartId}");
     ```

9. **Retorno:**
   - `return Result.Success()`

**Cenário de Uso:**
- Usuário navega anonimamente → adiciona produtos ao carrinho
- Faz login
- Sistema detecta carrinho anônimo (via cookie) e carrinho do usuário (via DB)
- Executa merge automaticamente

---

### 6. ApplyCouponCommand

**Módulo:** Cart
**Namespace:** `Bcommerce.Modules.Cart.Application.Commands.ApplyCoupon`

**Record Definition:**
```csharp
public record ApplyCouponCommand(
    Guid CartId,
    string CouponCode
) : ICommand;
```

**Algoritmo Passo a Passo:**

1. **Validação de Entrada:**
   - Validar `CouponCode` não vazio
   - Normalizar código (uppercase, trim)

2. **Recuperação do Carrinho:**
   - `var cart = await _cartRepository.GetByIdAsync(CartId, includeItems: true)`

3. **Validação do Cupom:**
   - Query ao módulo Coupons:
     ```csharp
     var couponResult = await _mediator.Send(
         new ValidateCouponQuery(
             Code: CouponCode,
             UserId: cart.UserId,
             OrderTotal: cart.TotalAmount,
             ProductIds: cart.Items.Select(i => i.ProductId).ToList()
         ),
         cancellationToken
     );

     if (!couponResult.IsSuccess)
     {
         return Result.Failure(couponResult.Error);
     }

     var coupon = couponResult.Value;
     ```

4. **Cálculo de Desconto:**
   - Calcular desconto baseado no tipo do cupom:
     ```csharp
     var discountAmount = coupon.Type switch
     {
         CouponType.Percentage =>
             cart.TotalAmount * (coupon.DiscountValue / 100m),
         CouponType.FixedAmount =>
             Math.Min(coupon.DiscountValue, cart.TotalAmount),
         CouponType.FreeShipping => 0m, // Desconto aplicado no frete
         _ => 0m
     };
     ```

5. **Aplicação do Cupom ao Carrinho:**
   - Invocar método:
     ```csharp
     cart.ApplyCoupon(
         couponId: coupon.Id,
         couponCode: CouponCode,
         discountAmount: discountAmount
     );
     ```
   - Internamente:
     - `CouponId = couponId`
     - `CouponCode = code`
     - `DiscountAmount = discountAmount`
     - Recalcula `TotalAmount` (subtrai desconto)

6. **Persistência:**
   - `await _unitOfWork.SaveChangesAsync(cancellationToken)`

7. **Invalidação de Cache:**
   - `await _cacheService.RemoveAsync($"cart:{CartId}")`

8. **Retorno:**
   - `return Result.Success()`

**Erros Possíveis:**
- `CouponErrors.NotFound`
- `CouponErrors.Expired`
- `CouponErrors.UsageLimitReached`
- `CouponErrors.MinimumOrderValueNotMet`

---

## Módulo Cart - Queries

### 1. GetCartByIdQuery

**Módulo:** Cart
**Namespace:** `Bcommerce.Modules.Cart.Application.Queries.GetCartById`

**Record Definition:**
```csharp
public record GetCartByIdQuery(Guid CartId) : IQuery<CartDto>;

public record CartDto(
    Guid Id,
    Guid? UserId,
    List<CartItemDto> Items,
    MoneyDto Subtotal,
    MoneyDto? DiscountAmount,
    string? CouponCode,
    MoneyDto TotalAmount,
    int ItemCount,
    DateTime CreatedAt,
    DateTime UpdatedAt
);

public record CartItemDto(
    Guid Id,
    Guid ProductId,
    string ProductName,
    string? ImageUrl,
    int Quantity,
    MoneyDto UnitPrice,
    MoneyDto TotalPrice,
    bool IsAvailable
);
```

**Algoritmo Passo a Passo:**

1. **Sanitização:**
   - Validar `CartId != Guid.Empty`

2. **Cache Check:**
   - `var cacheKey = $"cart:{CartId}"`
   - Verificar cache (TTL: 5 min - dados voláteis)

3. **Acesso a Dados:**
   - Query otimizada:
     ```csharp
     var cart = await _context.ShoppingCarts
         .AsNoTracking()
         .Include(c => c.Items)
         .Where(c => c.Id == query.CartId && c.Status == CartStatus.Active)
         .FirstOrDefaultAsync(cancellationToken);
     ```

4. **Verificação de Existência:**
   - Se `cart == null` → `Result.Failure(CartErrors.NotFound)`

5. **Enriquecimento com Dados de Produtos:**
   - Buscar disponibilidade atual dos produtos:
     ```csharp
     var productIds = cart.Items.Select(i => i.ProductId).ToList();
     var products = await _mediator.Send(
         new GetProductsByIdsQuery(productIds)
     );

     var productDict = products.Value.ToDictionary(p => p.Id);
     ```

6. **Mapeamento com Disponibilidade:**
   - Mapear items verificando disponibilidade:
     ```csharp
     var itemDtos = cart.Items.Select(item =>
     {
         var product = productDict.GetValueOrDefault(item.ProductId);
         var isAvailable = product != null &&
                          product.Status == ProductStatus.Published &&
                          product.Stock.Available >= item.Quantity;

         return new CartItemDto(
             Id: item.Id,
             ProductId: item.ProductId,
             ProductName: item.ProductName,
             ImageUrl: item.ImageUrl,
             Quantity: item.Quantity,
             UnitPrice: new MoneyDto(item.UnitPrice.Amount, item.UnitPrice.Currency),
             TotalPrice: new MoneyDto(item.TotalPrice.Amount, item.TotalPrice.Currency),
             IsAvailable: isAvailable
         );
     }).ToList();
     ```

7. **Cálculo de Subtotal:**
   - `var subtotal = cart.Items.Sum(i => i.TotalPrice.Amount)`

8. **Construção do DTO:**
   - ```csharp
     var dto = new CartDto(
         Id: cart.Id,
         UserId: cart.UserId,
         Items: itemDtos,
         Subtotal: new MoneyDto(subtotal, "BRL"),
         DiscountAmount: cart.DiscountAmount.HasValue
             ? new MoneyDto(cart.DiscountAmount.Value, "BRL")
             : null,
         CouponCode: cart.CouponCode,
         TotalAmount: new MoneyDto(cart.TotalAmount, "BRL"),
         ItemCount: cart.Items.Sum(i => i.Quantity),
         CreatedAt: cart.CreatedAt,
         UpdatedAt: cart.UpdatedAt
     );
     ```

9. **Atualização de Cache:**
   - `await _cacheService.SetAsync(cacheKey, dto, TimeSpan.FromMinutes(5))`

10. **Retorno:**
    - `return Result.Success(dto)`

---

### 2. GetCartByUserIdQuery

**Módulo:** Cart
**Namespace:** `Bcommerce.Modules.Cart.Application.Queries.GetCartByUserId`

**Record Definition:**
```csharp
public record GetCartByUserIdQuery(Guid UserId) : IQuery<CartDto>;
```

**Algoritmo Passo a Passo:**

1. **Sanitização:**
   - Validar `UserId != Guid.Empty`

2. **Cache Check:**
   - `var cacheKey = $"cart:user:{UserId}"`

3. **Acesso a Dados:**
   - Query com filtro por UserId:
     ```csharp
     var cart = await _context.ShoppingCarts
         .AsNoTracking()
         .Include(c => c.Items)
         .Where(c => c.UserId == query.UserId && c.Status == CartStatus.Active)
         .OrderByDescending(c => c.UpdatedAt)
         .FirstOrDefaultAsync(cancellationToken);
     ```

4. **Tratamento de Carrinho Não Existente:**
   - Se `cart == null`:
     - Retornar carrinho vazio OU
     - Executar `CreateCartCommand` automaticamente:
       ```csharp
       var createResult = await _mediator.Send(
           new CreateCartCommand(UserId, null),
           cancellationToken
       );

       if (!createResult.IsSuccess)
       {
           return Result.Failure(createResult.Error);
       }

       // Retornar carrinho vazio recém-criado
       return Result.Success(new CartDto(
           Id: createResult.Value,
           UserId: UserId,
           Items: new List<CartItemDto>(),
           Subtotal: new MoneyDto(0, "BRL"),
           DiscountAmount: null,
           CouponCode: null,
           TotalAmount: new MoneyDto(0, "BRL"),
           ItemCount: 0,
           CreatedAt: DateTime.UtcNow,
           UpdatedAt: DateTime.UtcNow
       ));
       ```

5. **Mapeamento (se carrinho existe):**
   - Mesmo processo de `GetCartByIdQuery`

6. **Retorno:**
   - `return Result.Success(dto)`

---

## Módulo Orders - Commands

### 1. PlaceOrderCommand

**Módulo:** Orders
**Namespace:** `Bcommerce.Modules.Orders.Application.Commands.PlaceOrder`

**Record Definition:**
```csharp
public record PlaceOrderCommand(
    Guid CartId,
    Guid UserId,
    Guid ShippingAddressId,
    Guid? BillingAddressId,
    ShippingMethod ShippingMethod,
    PaymentMethod PaymentMethod,
    string? CustomerNotes
) : ICommand<Guid>;

public enum ShippingMethod { Standard, Express, SameDay }
public enum PaymentMethod { CreditCard, DebitCard, Pix, Boleto }
```

**Algoritmo Passo a Passo:**

1. **Validação de Entrada (FluentValidation):**
   - Validar `CartId`, `UserId`, `ShippingAddressId` não vazios
   - Validar `ShippingMethod` e `PaymentMethod` são valores válidos do enum
   - Se `CustomerNotes` fornecido, validar max 500 caracteres

2. **Recuperação do Carrinho:**
   - Query ao módulo Cart:
     ```csharp
     var cartResult = await _mediator.Send(
         new GetCartByIdQuery(CartId),
         cancellationToken
     );

     if (!cartResult.IsSuccess)
     {
         return Result.Failure(cartResult.Error);
     }

     var cart = cartResult.Value;
     ```

3. **Validação do Carrinho:**
   - Verificar ownership:
     ```csharp
     if (cart.UserId != UserId)
     {
         return Result.Failure(OrderErrors.CartDoesNotBelongToUser);
     }
     ```
   - Verificar se carrinho não está vazio:
     ```csharp
     if (cart.ItemCount == 0)
     {
         return Result.Failure(OrderErrors.EmptyCart);
     }
     ```
   - Verificar se todos items estão disponíveis:
     ```csharp
     var unavailableItems = cart.Items.Where(i => !i.IsAvailable).ToList();
     if (unavailableItems.Any())
     {
         return Result.Failure(OrderErrors.CartContainsUnavailableItems(
             unavailableItems.Select(i => i.ProductName).ToList()
         ));
     }
     ```

4. **Recuperação do Endereço de Entrega:**
   - Query ao módulo Users:
     ```csharp
     var shippingAddressResult = await _mediator.Send(
         new GetAddressByIdQuery(ShippingAddressId),
         cancellationToken
     );

     if (!shippingAddressResult.IsSuccess)
     {
         return Result.Failure(AddressErrors.NotFound);
     }

     var shippingAddress = shippingAddressResult.Value;
     ```
   - Validar ownership do endereço:
     ```csharp
     if (shippingAddress.UserId != UserId)
     {
         return Result.Failure(AddressErrors.NotOwned);
     }
     ```

5. **Recuperação do Endereço de Cobrança (se diferente):**
   - Se `BillingAddressId.HasValue`:
     ```csharp
     var billingAddressResult = await _mediator.Send(
         new GetAddressByIdQuery(BillingAddressId.Value)
     );
     var billingAddress = billingAddressResult.Value;
     ```
   - Senão, usar mesmo endereço de entrega

6. **Cálculo de Frete:**
   - Executar serviço de cálculo de frete:
     ```csharp
     var shippingCost = await _shippingCalculator.CalculateAsync(
         shippingAddress: shippingAddress,
         items: cart.Items,
         shippingMethod: ShippingMethod,
         cancellationToken: cancellationToken
     );
     ```

7. **Criação de Snapshots de Produtos:**
   - Congelar dados dos produtos no momento do pedido:
     ```csharp
     var productSnapshots = cart.Items.Select(item => new ProductSnapshot(
         productId: item.ProductId,
         name: item.ProductName,
         sku: item.Sku, // Buscar do produto original
         price: item.UnitPrice,
         imageUrl: item.ImageUrl
     )).ToList();
     ```

8. **Criação dos OrderItems:**
   - Mapear items do carrinho para ordem:
     ```csharp
     var orderItems = cart.Items.Select(item =>
         OrderItem.Create(
             productId: item.ProductId,
             productName: item.ProductName,
             quantity: item.Quantity,
             unitPrice: item.UnitPrice,
             snapshot: productSnapshots.First(s => s.ProductId == item.ProductId)
         )
     ).ToList();
     ```

9. **Cálculo de Totais:**
   - Calcular subtotal, descontos, frete e total:
     ```csharp
     var subtotal = orderItems.Sum(i => i.TotalPrice.Amount);
     var discountAmount = cart.DiscountAmount?.Amount ?? 0;
     var totalAmount = subtotal - discountAmount + shippingCost;
     ```

10. **Criação da Entidade Order:**
    - Invocar factory method:
      ```csharp
      var order = Order.Create(
          userId: UserId,
          items: orderItems,
          shippingAddress: ShippingAddress.FromDto(shippingAddress),
          billingAddress: BillingAddress.FromDto(billingAddress ?? shippingAddress),
          shippingMethod: ShippingMethod,
          shippingCost: Money.Create(shippingCost, "BRL"),
          paymentMethod: PaymentMethod,
          subtotal: Money.Create(subtotal, "BRL"),
          discountAmount: cart.DiscountAmount != null
              ? Money.Create(discountAmount, "BRL")
              : null,
          couponId: cart.CouponId,
          totalAmount: Money.Create(totalAmount, "BRL"),
          customerNotes: CustomerNotes
      );
      ```
    - Internamente:
      - Gera `OrderNumber` sequencial (ORD-2025-00001)
      - `Status = OrderStatus.Pending`
      - `CreatedAt = DateTime.UtcNow`
      - Adiciona evento: `AddDomainEvent(new OrderPlacedEvent(Id, UserId))`

11. **Persistência:**
    - Salvar ordem:
      ```csharp
      await _orderRepository.AddAsync(order, cancellationToken);
      await _unitOfWork.SaveChangesAsync(cancellationToken);
      ```

12. **Processamento de Eventos (Outbox):**
    - `OrderPlacedEvent` é capturado e persistido
    - Handler assíncrono publica `OrderPlacedIntegrationEvent`
    - Outros módulos reagem:
      - **Catalog**: Reserva estoque (`ReserveStockCommand`)
      - **Payments**: Inicia processamento de pagamento (`ProcessPaymentCommand`)
      - **Cart**: Marca carrinho como convertido (`ConvertCartCommand`)
      - **Notifications**: Envia email de confirmação
      - **Coupons**: Registra uso do cupom (se aplicável)

13. **Retorno:**
    - `return Result.Success(order.Id)`

**Eventos Disparados:**
- `OrderPlacedEvent` (Domain)
- `OrderPlacedIntegrationEvent` (Integration)

**Erros Possíveis:**
- `CartErrors.NotFound`
- `OrderErrors.CartDoesNotBelongToUser`
- `OrderErrors.EmptyCart`
- `OrderErrors.CartContainsUnavailableItems`
- `AddressErrors.NotFound`
- `AddressErrors.NotOwned`

**Arquivo:** `/src/Modules/Orders/Bcommerce.Modules.Orders.Application/Commands/PlaceOrder/PlaceOrderCommandHandler.cs`

**Nota Importante:**
Este é o comando mais crítico do sistema. Falhas aqui podem resultar em:
- Perda de venda
- Dados inconsistentes (carrinho convertido mas ordem não criada)
- Estoque não reservado

Por isso, utiliza padrão Saga orquestrado via Integration Events para garantir consistência eventual.

---

### 2. MarkOrderAsPaidCommand

**Módulo:** Orders
**Namespace:** `Bcommerce.Modules.Orders.Application.Commands.MarkOrderAsPaid`

**Record Definition:**
```csharp
public record MarkOrderAsPaidCommand(
    Guid OrderId,
    Guid PaymentId
) : ICommand;
```

**Algoritmo Passo a Passo:**

1. **Validação de Entrada:**
   - Validar IDs não vazios

2. **Recuperação da Ordem:**
   - `var order = await _orderRepository.GetByIdAsync(OrderId)`
   - Se `order == null` → `Result.Failure(OrderErrors.NotFound)`

3. **Validação de Status:**
   - Verificar se ordem está em status válido para pagamento:
     ```csharp
     if (order.Status != OrderStatus.Pending && order.Status != OrderStatus.PaymentFailed)
     {
         return Result.Failure(OrderErrors.InvalidStatusTransition(
             from: order.Status,
             to: OrderStatus.Paid
         ));
     }
     ```

4. **Atualização da Ordem:**
   - Invocar método do aggregate:
     ```csharp
     order.MarkAsPaid(PaymentId);
     ```
   - Internamente:
     - `Status = OrderStatus.Paid`
     - `PaidAt = DateTime.UtcNow`
     - `PaymentId = paymentId`
     - Adiciona evento: `AddDomainEvent(new OrderPaidEvent(Id))`

5. **Persistência:**
   - `await _unitOfWork.SaveChangesAsync(cancellationToken)`

6. **Processamento de Eventos:**
   - Handler de `OrderPaidEvent`:
     - Confirma reserva de estoque no Catalog (commit definitivo)
     - Gera nota fiscal
     - Envia email com fatura ao cliente
     - Cria tarefa de separação no WMS

7. **Retorno:**
   - `return Result.Success()`

**Eventos Disparados:**
- `OrderPaidEvent` (Domain)

**Trigger:**
- Este command é executado quando módulo Payments publica `PaymentCompletedIntegrationEvent`

---

### 3. ShipOrderCommand

**Módulo:** Orders
**Namespace:** `Bcommerce.Modules.Orders.Application.Commands.ShipOrder`

**Record Definition:**
```csharp
public record ShipOrderCommand(
    Guid OrderId,
    string TrackingCode,
    string Carrier,
    DateTime? EstimatedDeliveryDate
) : ICommand;
```

**Algoritmo Passo a Passo:**

1. **Validação de Entrada:**
   - Validar `TrackingCode` não vazio e formato válido
   - Validar `Carrier` não vazio
   - Se `EstimatedDeliveryDate` fornecido, validar > DateTime.UtcNow

2. **Recuperação da Ordem:**
   - `var order = await _orderRepository.GetByIdAsync(OrderId)`

3. **Validação de Status:**
   - Verificar se ordem foi paga:
     ```csharp
     if (order.Status != OrderStatus.Paid && order.Status != OrderStatus.Processing)
     {
         return Result.Failure(OrderErrors.OrderNotPaid);
     }
     ```

4. **Validação de Estoque Commitado:**
   - Verificar se estoque foi definitivamente reservado (opcional, pode ser garantido pelo fluxo)

5. **Atualização da Ordem:**
   - Invocar método:
     ```csharp
     order.MarkAsShipped(
         trackingCode: TrackingCode,
         carrier: Carrier,
         estimatedDeliveryDate: EstimatedDeliveryDate
     );
     ```
   - Internamente:
     - `Status = OrderStatus.Shipped`
     - `ShippedAt = DateTime.UtcNow`
     - `TrackingCode = trackingCode`
     - `Carrier = carrier`
     - `EstimatedDeliveryDate = estimatedDeliveryDate`
     - Adiciona evento: `AddDomainEvent(new OrderShippedEvent(Id, trackingCode))`

6. **Persistência:**
   - `await _unitOfWork.SaveChangesAsync(cancellationToken)`

7. **Processamento de Eventos:**
   - Handler:
     - Envia email com código de rastreamento
     - Envia SMS/WhatsApp com tracking
     - Envia push notification
     - Inicia job de rastreamento automático (polling na API da transportadora)

8. **Retorno:**
   - `return Result.Success()`

**Eventos Disparados:**
- `OrderShippedEvent` (Domain)
- `OrderShippedIntegrationEvent` (Integration - para Notifications)

---

### 4. CancelOrderCommand

**Módulo:** Orders
**Namespace:** `Bcommerce.Modules.Orders.Application.Commands.CancelOrder`

**Record Definition:**
```csharp
public record CancelOrderCommand(
    Guid OrderId,
    CancellationReason Reason,
    string? Notes,
    Guid? CancelledBy // UserId ou AdminId
) : ICommand;

public enum CancellationReason
{
    CustomerRequest,
    PaymentFailed,
    StockUnavailable,
    FraudSuspicion,
    AddressIssue,
    Other
}
```

**Algoritmo Passo a Passo:**

1. **Validação de Entrada:**
   - Validar `Reason` é valor válido
   - Se `Reason == Other`, validar `Notes` obrigatório

2. **Recuperação da Ordem:**
   - `var order = await _orderRepository.GetByIdAsync(OrderId)`

3. **Validação de Regras de Cancelamento:**
   - Verificar se ordem pode ser cancelada:
     ```csharp
     if (order.Status == OrderStatus.Shipped || order.Status == OrderStatus.Delivered)
     {
         return Result.Failure(OrderErrors.CannotCancelShippedOrder);
     }

     if (order.Status == OrderStatus.Cancelled)
     {
         return Result.Failure(OrderErrors.AlreadyCancelled);
     }
     ```

4. **Verificação de Pagamento:**
   - Capturar se ordem foi paga (para processar reembolso):
     ```csharp
     bool wasPaid = order.Status == OrderStatus.Paid || order.PaidAt.HasValue;
     ```

5. **Cancelamento da Ordem:**
   - Invocar método:
     ```csharp
     order.Cancel(
         reason: Reason,
         notes: Notes,
         cancelledBy: CancelledBy
     );
     ```
   - Internamente:
     - `Status = OrderStatus.Cancelled`
     - `CancelledAt = DateTime.UtcNow`
     - `CancellationReason = reason`
     - `CancellationNotes = notes`
     - Adiciona evento: `AddDomainEvent(new OrderCancelledEvent(Id, reason, notes, wasPaid))`

6. **Persistência:**
   - `await _unitOfWork.SaveChangesAsync(cancellationToken)`

7. **Processamento de Eventos:**
   - Handler publica `OrderCancelledIntegrationEvent`
   - Módulos reagem:
     - **Catalog**: Libera reserva de estoque (`ReleaseStockReservationCommand`)
     - **Payments**: Se pago, processa reembolso (`RefundPaymentCommand`)
     - **Notifications**: Envia email de confirmação de cancelamento
     - **Analytics**: Registra motivo de cancelamento

8. **Retorno:**
   - `return Result.Success()`

**Eventos Disparados:**
- `OrderCancelledEvent` (Domain)
- `OrderCancelledIntegrationEvent` (Integration)

**Erros Possíveis:**
- `OrderErrors.NotFound`
- `OrderErrors.CannotCancelShippedOrder`
- `OrderErrors.AlreadyCancelled`

---

## Módulo Orders - Queries

### 1. GetOrderByIdQuery

**Módulo:** Orders
**Namespace:** `Bcommerce.Modules.Orders.Application.Queries.GetOrderById`

**Record Definition:**
```csharp
public record GetOrderByIdQuery(Guid OrderId) : IQuery<OrderDto>;

public record OrderDto(
    Guid Id,
    string OrderNumber,
    Guid UserId,
    OrderStatus Status,
    List<OrderItemDto> Items,
    ShippingAddressDto ShippingAddress,
    BillingAddressDto? BillingAddress,
    ShippingMethod ShippingMethod,
    MoneyDto ShippingCost,
    PaymentMethod PaymentMethod,
    MoneyDto Subtotal,
    MoneyDto? DiscountAmount,
    string? CouponCode,
    MoneyDto TotalAmount,
    string? CustomerNotes,
    string? TrackingCode,
    string? Carrier,
    DateTime CreatedAt,
    DateTime? PaidAt,
    DateTime? ShippedAt,
    DateTime? DeliveredAt,
    DateTime? CancelledAt,
    CancellationReason? CancellationReason,
    string? CancellationNotes
);

public record OrderItemDto(
    Guid Id,
    Guid ProductId,
    string ProductName,
    string? ProductSku,
    string? ImageUrl,
    int Quantity,
    MoneyDto UnitPrice,
    MoneyDto TotalPrice
);
```

**Algoritmo Passo a Passo:**

1. **Sanitização:**
   - Validar `OrderId != Guid.Empty`

2. **Cache Check:**
   - `var cacheKey = $"order:{OrderId}"`
   - TTL: 10 min (dados relativamente estáveis após criação)

3. **Acesso a Dados:**
   - Query otimizada:
     ```csharp
     var order = await _context.Orders
         .AsNoTracking()
         .Include(o => o.Items)
         .Include(o => o.ShippingAddress)
         .Include(o => o.BillingAddress)
         .Where(o => o.Id == query.OrderId)
         .FirstOrDefaultAsync(cancellationToken);
     ```

4. **Verificação de Existência:**
   - Se `order == null` → `Result.Failure(OrderErrors.NotFound)`

5. **Mapeamento:**
   - Mapear para DTO:
     ```csharp
     var dto = new OrderDto(
         Id: order.Id,
         OrderNumber: order.OrderNumber.Value,
         UserId: order.UserId,
         Status: order.Status,
         Items: order.Items.Select(item => new OrderItemDto(
             Id: item.Id,
             ProductId: item.ProductId,
             ProductName: item.ProductName,
             ProductSku: item.Snapshot?.Sku,
             ImageUrl: item.Snapshot?.ImageUrl,
             Quantity: item.Quantity,
             UnitPrice: new MoneyDto(item.UnitPrice.Amount, item.UnitPrice.Currency),
             TotalPrice: new MoneyDto(item.TotalPrice.Amount, item.TotalPrice.Currency)
         )).ToList(),
         ShippingAddress: MapAddress(order.ShippingAddress),
         BillingAddress: order.BillingAddress != null
             ? MapAddress(order.BillingAddress)
             : null,
         ShippingMethod: order.ShippingMethod,
         ShippingCost: new MoneyDto(order.ShippingCost.Amount, order.ShippingCost.Currency),
         PaymentMethod: order.PaymentMethod,
         Subtotal: new MoneyDto(order.Subtotal.Amount, order.Subtotal.Currency),
         DiscountAmount: order.DiscountAmount != null
             ? new MoneyDto(order.DiscountAmount.Amount, order.DiscountAmount.Currency)
             : null,
         CouponCode: order.CouponCode,
         TotalAmount: new MoneyDto(order.TotalAmount.Amount, order.TotalAmount.Currency),
         CustomerNotes: order.CustomerNotes,
         TrackingCode: order.TrackingCode?.Value,
         Carrier: order.Carrier,
         CreatedAt: order.CreatedAt,
         PaidAt: order.PaidAt,
         ShippedAt: order.ShippedAt,
         DeliveredAt: order.DeliveredAt,
         CancelledAt: order.CancelledAt,
         CancellationReason: order.CancellationReason,
         CancellationNotes: order.CancellationNotes
     );
     ```

6. **Atualização de Cache:**
   - `await _cacheService.SetAsync(cacheKey, dto, TimeSpan.FromMinutes(10))`

7. **Retorno:**
   - `return Result.Success(dto)`

---

### 2. GetUserOrdersQuery

**Módulo:** Orders
**Namespace:** `Bcommerce.Modules.Orders.Application.Queries.GetUserOrders`

**Record Definition:**
```csharp
public record GetUserOrdersQuery(
    Guid UserId,
    OrderStatus? Status,
    DateTime? StartDate,
    DateTime? EndDate,
    int Page = 1,
    int PageSize = 20
) : IQuery<PagedResult<OrderListItemDto>>;

public record OrderListItemDto(
    Guid Id,
    string OrderNumber,
    OrderStatus Status,
    MoneyDto TotalAmount,
    int ItemCount,
    DateTime CreatedAt,
    DateTime? DeliveredAt
);
```

**Algoritmo Passo a Passo:**

1. **Sanitização:**
   - Validar `UserId != Guid.Empty`
   - Validar `Page >= 1`
   - Validar `PageSize <= 100`

2. **Construção da Query:**
   - Iniciar query:
     ```csharp
     IQueryable<Order> query = _context.Orders
         .AsNoTracking()
         .Where(o => o.UserId == query.UserId);
     ```

3. **Aplicação de Filtros:**
   - **Status:**
     ```csharp
     if (Status.HasValue)
     {
         query = query.Where(o => o.Status == Status.Value);
     }
     ```
   - **Período:**
     ```csharp
     if (StartDate.HasValue)
     {
         query = query.Where(o => o.CreatedAt >= StartDate.Value);
     }
     if (EndDate.HasValue)
     {
         query = query.Where(o => o.CreatedAt <= EndDate.Value);
     }
     ```

4. **Contagem Total:**
   - `int totalCount = await query.CountAsync(cancellationToken);`

5. **Ordenação:**
   - Ordenar por data de criação decrescente:
     ```csharp
     query = query.OrderByDescending(o => o.CreatedAt);
     ```

6. **Paginação:**
   - ```csharp
     var orders = await query
         .Skip((Page - 1) * PageSize)
         .Take(PageSize)
         .ToListAsync(cancellationToken);
     ```

7. **Mapeamento:**
   - Mapear para DTOs:
     ```csharp
     var items = orders.Select(o => new OrderListItemDto(
         Id: o.Id,
         OrderNumber: o.OrderNumber.Value,
         Status: o.Status,
         TotalAmount: new MoneyDto(o.TotalAmount.Amount, o.TotalAmount.Currency),
         ItemCount: o.Items.Sum(i => i.Quantity),
         CreatedAt: o.CreatedAt,
         DeliveredAt: o.DeliveredAt
     )).ToList();
     ```

8. **Construção do Resultado:**
   - ```csharp
     var result = new PagedResult<OrderListItemDto>(
         Items: items,
         Page: Page,
         PageSize: PageSize,
         TotalCount: totalCount,
         TotalPages: (int)Math.Ceiling(totalCount / (double)PageSize)
     );
     ```

9. **Retorno:**
   - `return Result.Success(result)`

---

## Resumo e Estatísticas

### Total de Commands e Queries por Módulo

| Módulo | Commands | Queries | Total |
|--------|----------|---------|-------|
| **Users** | 6 | 3 | 9 |
| **Catalog** | 6 | 3 | 9 |
| **Cart** | 6 | 2 | 8 |
| **Orders** | 4 | 2 | 6 |
| **Payments** | (próximo) | (próximo) | - |
| **Coupons** | (próximo) | (próximo) | - |

### Padrões Identificados

**Commands:**
- Todos retornam `Result` ou `Result<T>`
- Validação via FluentValidation
- Uso extensivo de Value Objects
- Eventos de domínio em métodos do aggregate
- Outbox Pattern para garantia de entrega
- UnitOfWork para transações

**Queries:**
- Sempre `AsNoTracking()` para performance
- Cache-Aside Pattern (Redis)
- Mapeamento para DTOs
- Paginação em listagens
- Índices otimizados

**Value Objects Utilizados:**
- `Email`, `PhoneNumber` (Users)
- `Sku`, `Slug`, `Money`, `Stock`, `ProductDimensions` (Catalog)
- `OrderNumber`, `TrackingCode`, `ShippingAddress` (Orders)

---

**Documento gerado em:** 2025-12-23
**Status:** Parcial (32 de ~80 commands/queries detalhados)
**Próximo:** Payments e Coupons modules
