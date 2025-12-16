# 📋 Tasks - BCommerce Modular Monolith

> Lista completa de Commands, Queries, Events, Integration Events e Event Handlers separados por tipo de usuário.

---

## 📊 Resumo por Papel

| Papel        | Commands | Queries | Total de Operações |
| ------------ | -------- | ------- | ------------------ |
| **CUSTOMER** | 22       | 18      | 40                 |
| **ADMIN**    | 28       | 13      | 41                 |

---

# 👤 OPERAÇÕES DO CUSTOMER

## 🔐 Módulo Users - Customer

### Commands

---

#### 1. `RegisterUserCommand` ✅

**Descrição**: Registrar novo usuário na plataforma.

**Algoritmo**:

```
1. RECEBER email, password, firstName, lastName
2. VALIDAR formato do email
3. VALIDAR força da senha (mínimo 8 chars, 1 maiúscula, 1 número)
4. VERIFICAR se email já existe no banco
   4.1. SE existe → RETORNAR erro "Email já cadastrado"
5. CRIAR hash da senha usando BCrypt
6. CRIAR entidade User com dados
7. ADICIONAR domain event UserCreatedEvent
8. PERSISTIR usuário no banco
9. PUBLICAR UserCreatedIntegrationEvent
   9.1. Cart: Criar carrinho vazio para o usuário
10. ENVIAR email de confirmação
11. RETORNAR userId
```

**Integration Events**:

- **Publica**: `UserCreatedIntegrationEvent` → Cart Module

---

#### 2. `LoginCommand` ✅

**Descrição**: Autenticar usuário e criar sessão.

**Algoritmo**:

```
1. RECEBER email, password, deviceInfo
2. BUSCAR usuário por email
   2.1. SE não existe → RETORNAR erro "Credenciais inválidas"
3. VERIFICAR se conta está bloqueada
   3.1. SE bloqueada → RETORNAR erro "Conta bloqueada"
4. VALIDAR senha com hash armazenado
   4.1. SE inválida:
        a. INCREMENTAR accessFailedCount
        b. SE accessFailedCount >= 5 → BLOQUEAR conta
        c. REGISTRAR LoginHistory com success=false
        d. RETORNAR erro "Credenciais inválidas"
5. RESETAR accessFailedCount para 0
6. GERAR access token JWT (expiração: 15 min)
7. GERAR refresh token (expiração: 7 dias)
8. CRIAR sessão com deviceInfo
9. ADICIONAR domain event SessionCreatedEvent
10. REGISTRAR LoginHistory com success=true
11. RETORNAR { accessToken, refreshToken, expiresIn }
```

---

#### 3. `RefreshTokenCommand`

**Descrição**: Renovar tokens de acesso.

**Algoritmo**:

```
1. RECEBER refreshToken
2. VALIDAR assinatura do refresh token
3. BUSCAR sessão pelo hash do refresh token
   3.1. SE não existe → RETORNAR erro "Sessão inválida"
4. VERIFICAR se sessão foi revogada
   4.1. SE revogada → RETORNAR erro "Sessão revogada"
5. VERIFICAR se sessão expirou
   5.1. SE expirada → RETORNAR erro "Sessão expirada"
6. GERAR novo access token
7. GERAR novo refresh token
8. ATUALIZAR sessão com novo refresh token hash
9. ATUALIZAR lastActivityAt da sessão
10. RETORNAR { accessToken, refreshToken, expiresIn }
```

---

#### 4. `LogoutCommand`

**Descrição**: Encerrar sessão atual.

**Algoritmo**:

```
1. RECEBER sessionId do token atual
2. BUSCAR sessão por ID
3. MARCAR sessão como revogada
4. DEFINIR revokedReason = "USER_LOGOUT"
5. ADICIONAR domain event SessionRevokedEvent
6. PERSISTIR alterações
7. RETORNAR sucesso
```

---

#### 5. `CreateProfileCommand`

**Descrição**: Criar perfil estendido após registro.

**Algoritmo**:

```
1. RECEBER userId, firstName, lastName, birthDate, cpf, gender
2. VERIFICAR se perfil já existe para userId
   2.1. SE existe → RETORNAR erro "Perfil já existe"
3. VALIDAR formato do CPF
4. VERIFICAR se CPF já cadastrado
   4.1. SE existe → RETORNAR erro "CPF já cadastrado"
5. CRIAR entidade Profile com dados
6. ADICIONAR domain event ProfileCreatedEvent
7. PERSISTIR perfil
8. RETORNAR profileId
```

---

#### 6. `UpdateProfileCommand`

**Descrição**: Atualizar dados do perfil.

**Algoritmo**:

```
1. RECEBER userId, campos a atualizar
2. BUSCAR perfil por userId
   2.1. SE não existe → RETORNAR erro "Perfil não encontrado"
3. SE cpf alterado:
   3.1. VALIDAR formato
   3.2. VERIFICAR se já cadastrado
4. ATUALIZAR campos modificados
5. ADICIONAR domain event ProfileUpdatedEvent
6. PERSISTIR alterações
7. RETORNAR sucesso
```

---

#### 7. `AddAddressCommand` ✅

**Descrição**: Adicionar novo endereço.

**Algoritmo**:

```
1. RECEBER userId, label, recipientName, street, number, complement, neighborhood, city, state, postalCode, isDefault
2. VALIDAR formato do CEP
3. VALIDAR estado (2 letras maiúsculas)
4. SE isDefault = true:
   4.1. BUSCAR endereço padrão atual
   4.2. SE existe → REMOVER flag isDefault
5. CRIAR entidade Address com dados
6. ADICIONAR domain event AddressAddedEvent
7. PERSISTIR endereço
8. RETORNAR addressId
```

---

#### 8. `UpdateAddressCommand`

**Descrição**: Atualizar endereço existente.

**Algoritmo**:

```
1. RECEBER addressId, userId, novos dados
2. BUSCAR endereço por ID
   2.1. SE não existe → RETORNAR erro "Endereço não encontrado"
3. VERIFICAR se endereço pertence ao userId
   3.1. SE não pertence → RETORNAR erro "Não autorizado"
4. VALIDAR novos dados (CEP, estado)
5. SE isDefault alterado para true:
   5.1. REMOVER isDefault de outros endereços
6. ATUALIZAR campos
7. PERSISTIR alterações
8. RETORNAR sucesso
```

---

#### 9. `DeleteAddressCommand`

**Descrição**: Remover endereço (soft delete).

**Algoritmo**:

```
1. RECEBER addressId, userId
2. BUSCAR endereço por ID
   2.1. SE não existe → RETORNAR erro "Endereço não encontrado"
3. VERIFICAR se pertence ao userId
4. MARCAR deletedAt = now()
5. SE era endereço padrão:
   5.1. BUSCAR próximo endereço válido
   5.2. MARCAR como padrão
6. PERSISTIR alterações
7. RETORNAR sucesso
```

---

#### 10. `ChangePasswordCommand`

**Descrição**: Alterar senha do usuário.

**Algoritmo**:

```
1. RECEBER userId, currentPassword, newPassword
2. BUSCAR usuário por ID
3. VALIDAR senha atual
   3.1. SE inválida → RETORNAR erro "Senha atual incorreta"
4. VALIDAR força da nova senha
5. GERAR hash da nova senha
6. ATUALIZAR passwordHash
7. REVOGAR todas as outras sessões
8. ENVIAR email de confirmação de alteração
9. RETORNAR sucesso
```

---

#### 11. `ConfirmEmailCommand` ✅

**Descrição**: Confirmar email via token.

**Algoritmo**:

```
1. RECEBER token de confirmação
2. VALIDAR e decodificar token
3. BUSCAR usuário pelo userId do token
4. VERIFICAR se token expirou
   4.1. SE expirou → RETORNAR erro "Token expirado"
5. MARCAR emailConfirmed = true
6. ADICIONAR domain event EmailConfirmedEvent
7. PERSISTIR alterações
8. RETORNAR sucesso
```

## use os os metodos await \_userManager.GenerateEmailConfirmationTokenAsync(user); e await \_userManager.ConfirmEmailAsync(user, token) do identity

### Queries - Customer (Users)

---

#### 1. `GetUserProfileQuery`

**Descrição**: Buscar perfil completo do usuário logado.

**Algoritmo**:

```
1. RECEBER userId do token
2. BUSCAR perfil com JOIN em User
3. MAPEAR para ProfileDto
4. RETORNAR ProfileDto
```

---

#### 2. `GetUserAddressesQuery`

**Descrição**: Listar endereços do usuário.

**Algoritmo**:

```
1. RECEBER userId
2. BUSCAR endereços WHERE userId = X AND deletedAt IS NULL
3. ORDENAR por isDefault DESC, createdAt DESC
4. MAPEAR para List<AddressDto>
5. RETORNAR lista
```

---

#### 3. `GetActiveSessionsQuery`

**Descrição**: Listar sessões ativas.

**Algoritmo**:

```
1. RECEBER userId
2. BUSCAR sessões WHERE userId = X AND revokedAt IS NULL AND expiresAt > now()
3. MAPEAR para List<SessionDto>
4. RETORNAR lista
```

---

#### 4. `GetNotificationsQuery`

**Descrição**: Listar notificações (paginado).

**Algoritmo**:

```
1. RECEBER userId, page, pageSize
2. BUSCAR notificações WHERE userId = X
3. ORDENAR por createdAt DESC
4. APLICAR paginação
5. RETORNAR PagedResult<NotificationDto>
```

---

## 🛍️ Módulo Catalog - Customer

### Commands

---

#### 1. `AddToFavoritesCommand`

**Descrição**: Adicionar produto aos favoritos.

**Algoritmo**:

```
1. RECEBER userId, productId
2. BUSCAR produto por ID
   2.1. SE não existe → RETORNAR erro "Produto não encontrado"
3. VERIFICAR se já está nos favoritos
   3.1. SE já está → RETORNAR erro "Produto já favoritado"
4. CRIAR snapshot do produto (nome, preço, imagem)
5. CRIAR entidade UserFavorite
6. PERSISTIR
7. RETORNAR sucesso
```

---

#### 2. `RemoveFromFavoritesCommand`

**Descrição**: Remover produto dos favoritos.

**Algoritmo**:

```
1. RECEBER userId, productId
2. BUSCAR favorito por userId e productId
   2.1. SE não existe → RETORNAR erro "Favorito não encontrado"
3. DELETAR registro
4. RETORNAR sucesso
```

---

#### 3. `CreateReviewCommand`

**Descrição**: Criar avaliação de produto.

**Algoritmo**:

```
1. RECEBER userId, productId, rating, title, comment
2. BUSCAR produto por ID
   2.1. SE não existe → RETORNAR erro "Produto não encontrado"
3. VERIFICAR se usuário já avaliou
   3.1. SE já avaliou → RETORNAR erro "Você já avaliou este produto"
4. CONSULTAR Orders via Mediator: usuário comprou o produto?
   4.1. SE sim → isVerifiedPurchase = true
   4.2. SE não → isVerifiedPurchase = false
5. CRIAR entidade ProductReview
6. ADICIONAR domain event ReviewCreatedEvent
7. PERSISTIR
8. RETORNAR reviewId
```

---

### Queries - Customer (Catalog)

---

#### 1. `GetProductByIdQuery`

**Descrição**: Buscar detalhes de um produto.

**Algoritmo**:

```
1. RECEBER productId
2. BUSCAR produto com JOINs (category, brand, images)
3. VERIFICAR se produto está ativo
   3.1. SE não ativo → RETORNAR erro "Produto não encontrado"
4. BUSCAR estatísticas de avaliação
5. MAPEAR para ProductDetailDto
6. RETORNAR ProductDetailDto
```

---

#### 2. `SearchProductsQuery`

**Descrição**: Buscar produtos com filtros.

**Algoritmo**:

```
1. RECEBER query, categoryId, minPrice, maxPrice, sortBy, page, pageSize
2. CONSTRUIR query base (status = ACTIVE)
3. SE categoryId → FILTRAR por categoria (incluindo subcategorias)
4. SE query → BUSCAR em nome, descrição (full-text search)
5. SE minPrice → FILTRAR price >= minPrice
6. SE maxPrice → FILTRAR price <= maxPrice
7. APLICAR ordenação (relevance, price_asc, price_desc, newest)
8. APLICAR paginação
9. RETORNAR PagedResult<ProductListDto>
```

---

#### 3. `GetCategoriesQuery`

**Descrição**: Listar árvore de categorias.

**Algoritmo**:

```
1. BUSCAR categorias WHERE isActive = true AND deletedAt IS NULL
2. ORDENAR por sortOrder
3. CONSTRUIR estrutura hierárquica (parent/children)
4. RETORNAR List<CategoryTreeDto>
```

---

#### 4. `GetProductReviewsQuery`

**Descrição**: Listar avaliações de um produto.

**Algoritmo**:

```
1. RECEBER productId, page, pageSize
2. BUSCAR reviews WHERE productId = X AND isApproved = true
3. ORDENAR por createdAt DESC
4. APLICAR paginação
5. CALCULAR estatísticas (média, distribuição por estrelas)
6. RETORNAR PagedResult<ReviewDto> com stats
```

---

#### 5. `GetUserFavoritesQuery`

**Descrição**: Listar produtos favoritos do usuário.

**Algoritmo**:

```
1. RECEBER userId, page, pageSize
2. BUSCAR favoritos com JOIN em Product
3. FILTRAR apenas produtos ativos
4. ORDENAR por createdAt DESC
5. APLICAR paginação
6. RETORNAR PagedResult<FavoriteDto>
```

---

## 🛒 Módulo Cart - Customer

### Commands

---

#### 1. `AddItemToCartCommand`

**Descrição**: Adicionar item ao carrinho.

**Algoritmo**:

```
1. RECEBER userId OU sessionId, productId, quantity
2. BUSCAR carrinho ativo do usuário/sessão
   2.1. SE não existe → CRIAR novo carrinho
3. CONSULTAR Catalog via Mediator: GetProductByIdQuery
   3.1. SE produto não existe/inativo → RETORNAR erro
4. VERIFICAR estoque disponível
   4.1. SE insuficiente → RETORNAR erro "Estoque insuficiente"
5. VERIFICAR se produto já está no carrinho
   5.1. SE já está → ATUALIZAR quantidade
   5.2. SE não está → CRIAR CartItem com snapshot do produto
6. ADICIONAR domain event ItemAddedToCartEvent
7. REGISTRAR em ActivityLog
8. PERSISTIR
9. RETORNAR cartId, itemId
```

**Integration (Consome)**:

- `Catalog.GetProductByIdQuery` via Mediator

---

#### 2. `UpdateCartItemCommand`

**Descrição**: Alterar quantidade de item.

**Algoritmo**:

```
1. RECEBER cartId, itemId, newQuantity
2. BUSCAR cart e verificar ownership
3. BUSCAR item por ID
   3.1. SE não existe → RETORNAR erro
4. SE newQuantity = 0 → REMOVER item
5. CONSULTAR Catalog: CheckStockAvailability
   5.1. SE insuficiente → RETORNAR erro
6. ATUALIZAR quantidade
7. ADICIONAR domain event ItemQuantityChangedEvent
8. REGISTRAR em ActivityLog
9. PERSISTIR
10. RETORNAR sucesso
```

---

#### 3. `RemoveItemFromCartCommand`

**Descrição**: Remover item do carrinho.

**Algoritmo**:

```
1. RECEBER cartId, itemId
2. BUSCAR cart e verificar ownership
3. BUSCAR item por ID
4. MARCAR removedAt = now()
5. SE tinha reserva de estoque → LIBERAR
6. ADICIONAR domain event ItemRemovedFromCartEvent
7. REGISTRAR em ActivityLog
8. PERSISTIR
9. RETORNAR sucesso
```

---

#### 4. `ApplyCouponCommand`

**Descrição**: Aplicar cupom de desconto.

**Algoritmo**:

```
1. RECEBER cartId, couponCode
2. BUSCAR cart e verificar ownership
3. MONTAR lista de itens com categoryId e productId
4. CONSULTAR Coupons via Mediator: ValidateCouponCommand
   4.1. SE inválido → RETORNAR erro com mensagem específica
5. ATUALIZAR cart com couponId, couponCode, discountAmount
6. CONSULTAR Coupons: ReserveCouponCommand (reserva temporária)
7. ADICIONAR domain event CouponAppliedEvent
8. PERSISTIR
9. RETORNAR { discountAmount, newTotal }
```

**Integration (Consome)**:

- `Coupons.ValidateCouponCommand` via Mediator
- `Coupons.ReserveCouponCommand` via Mediator

---

#### 5. `RemoveCouponCommand`

**Descrição**: Remover cupom do carrinho.

**Algoritmo**:

```
1. RECEBER cartId
2. BUSCAR cart e verificar ownership
3. BUSCAR reserva de cupom ativa
4. SE existe reserva → LIBERAR (releasedAt = now())
5. LIMPAR couponId, couponCode, discountAmount do cart
6. PERSISTIR
7. RETORNAR sucesso
```

---

#### 6. `MergeCartsCommand`

**Descrição**: Mesclar carrinho anônimo após login.

**Algoritmo**:

```
1. RECEBER userId, sessionId
2. BUSCAR carrinho anônimo por sessionId
   2.1. SE não existe → RETORNAR sucesso (nada a fazer)
3. BUSCAR carrinho do usuário logado
   3.1. SE não existe → CRIAR novo
4. PARA CADA item do carrinho anônimo:
   4.1. VERIFICAR se produto existe no carrinho logado
   4.2. SE existe → SOMAR quantidades
   4.3. SE não existe → MOVER item
5. SE carrinho anônimo tinha cupom:
   5.1. TRANSFERIR para carrinho logado (se não tiver outro)
6. MARCAR carrinho anônimo como MERGED
7. PERSISTIR
8. RETORNAR cartId do usuário
```

---

#### 7. `CheckoutCommand`

**Descrição**: Iniciar processo de checkout.

**Algoritmo**:

```
1. RECEBER cartId, addressId, paymentMethodType
2. BUSCAR cart e verificar ownership
3. VERIFICAR se carrinho tem itens
   3.1. SE vazio → RETORNAR erro "Carrinho vazio"
4. PARA CADA item:
   4.1. CONSULTAR Catalog: ReserveStockCommand
   4.2. SE falhou → ROLLBACK reservas anteriores, RETORNAR erro
   4.3. SALVAR reservationId no item
5. PUBLICAR CartCheckoutStartedIntegrationEvent
6. PERSISTIR
7. RETORNAR checkoutToken
```

**Integration Events**:

- **Publica**: `CartCheckoutStartedIntegrationEvent` → Orders Module
- **Consome**: `Catalog.ReserveStockCommand` via Mediator

---

### Queries - Customer (Cart)

---

#### 1. `GetCartQuery`

**Descrição**: Buscar carrinho atual.

**Algoritmo**:

```
1. RECEBER userId OU sessionId
2. BUSCAR carrinho ativo com itens
3. PARA CADA item:
   3.1. VERIFICAR se preço mudou (currentPrice vs unitPrice)
   3.2. VERIFICAR disponibilidade de estoque
4. CALCULAR subtotal, desconto, total
5. RETORNAR CartDto com alertas de preço/estoque
```

---

## 📦 Módulo Orders - Customer

### Commands

---

#### 1. `CreateOrderCommand`

**Descrição**: Criar pedido a partir do checkout.

**Algoritmo**:

```
1. RECEBER checkoutToken, cartId, shippingAddressId, billingAddressId, paymentMethodType
2. VALIDAR checkoutToken
3. BUSCAR cart e verificar ownership
4. CONSULTAR Users via Mediator: GetUserAddressQuery
5. CRIAR snapshot do endereço de entrega
6. SE billingAddressId diferente → CRIAR snapshot separado
7. GERAR orderNumber (YY-XXXXXX)
8. CALCULAR totais: subtotal, discount, shipping, tax, total
9. CRIAR Order com status PENDING
10. PARA CADA item do cart:
    10.1. CRIAR OrderItem com snapshot do produto
11. ADICIONAR domain event OrderCreatedEvent
12. PUBLICAR OrderCreatedIntegrationEvent
    12.1. Payments: Criar Payment pendente
    12.2. Cart: Marcar como CONVERTED
    12.3. Coupons: Converter reserva em uso
13. PERSISTIR
14. RETORNAR orderId, orderNumber
```

**Integration Events**:

- **Publica**: `OrderCreatedIntegrationEvent` → Payments, Cart, Coupons

---

#### 2. `CancelOrderCommand`

**Descrição**: Cancelar pedido (apenas status permitidos).

**Algoritmo**:

```
1. RECEBER orderId, userId, reason
2. BUSCAR order e verificar ownership
3. VERIFICAR se status permite cancelamento
   3.1. Permitidos: PENDING, PAYMENT_PROCESSING
   3.2. SE não permitido → RETORNAR erro
4. ATUALIZAR status para CANCELLED
5. DEFINIR cancellation_reason, cancellation_notes
6. ADICIONAR domain event OrderCancelledEvent
7. PUBLICAR OrderCancelledIntegrationEvent
   7.1. Payments: Cancelar/estornar pagamento
   7.2. Catalog: Liberar estoque reservado
   7.3. Coupons: Reverter uso do cupom
8. PERSISTIR
9. RETORNAR sucesso
```

**Integration Events**:

- **Publica**: `OrderCancelledIntegrationEvent` → Payments, Catalog, Coupons

---

#### 3. `RequestRefundCommand`

**Descrição**: Solicitar reembolso de pedido entregue.

**Algoritmo**:

```
1. RECEBER orderId, userId, reason, amount
2. BUSCAR order e verificar ownership
3. VERIFICAR se status = DELIVERED
4. VERIFICAR prazo para reembolso (ex: 7 dias)
5. VALIDAR amount <= order.total
6. CRIAR OrderRefund com status PENDING
7. ADICIONAR domain event RefundRequestedEvent
8. PUBLICAR RefundRequestedIntegrationEvent → Payments
9. PERSISTIR
10. RETORNAR refundId
```

---

### Queries - Customer (Orders)

---

#### 1. `GetUserOrdersQuery`

**Descrição**: Listar pedidos do usuário.

**Algoritmo**:

```
1. RECEBER userId, status (opcional), page, pageSize
2. CONSTRUIR query base WHERE userId = X
3. SE status → FILTRAR por status
4. ORDENAR por createdAt DESC
5. APLICAR paginação
6. RETORNAR PagedResult<OrderSummaryDto>
```

---

#### 2. `GetOrderByIdQuery`

**Descrição**: Buscar detalhes de um pedido.

**Algoritmo**:

```
1. RECEBER orderId, userId
2. BUSCAR order com JOINs (items, statusHistory)
3. VERIFICAR ownership
4. MAPEAR para OrderDetailDto
5. RETORNAR OrderDetailDto
```

---

#### 3. `GetOrderTrackingQuery`

**Descrição**: Buscar eventos de rastreamento.

**Algoritmo**:

```
1. RECEBER orderId, userId
2. VERIFICAR ownership
3. BUSCAR tracking_events WHERE orderId = X
4. ORDENAR por occurredAt DESC
5. RETORNAR List<TrackingEventDto>
```

---

## 💳 Módulo Payments - Customer

### Commands

---

#### 1. `ProcessPaymentCommand`

**Descrição**: Processar pagamento do pedido.

**Algoritmo**:

```
1. RECEBER orderId, userId, paymentMethodType, paymentMethodId (se existente), cardData (se novo)
2. BUSCAR Payment pendente do order
3. VERIFICAR ownership
4. SE paymentMethodId:
   4.1. BUSCAR método salvo
   4.2. USAR token do gateway
5. SE cardData:
   5.1. CRIAR token no gateway
   5.2. SE saveCard = true → SALVAR método tokenizado
6. CHAMAR gateway: authorize
7. SE autorizado:
   7.1. ATUALIZAR status para AUTHORIZED
   7.2. CHAMAR gateway: capture (se não é two-step)
   7.3. ATUALIZAR status para CAPTURED
   7.4. PUBLICAR PaymentCapturedIntegrationEvent
8. SE falhou:
   8.1. ATUALIZAR status para FAILED
   8.2. PUBLICAR PaymentFailedIntegrationEvent
9. PERSISTIR
10. RETORNAR paymentStatus, transactionId
```

---

#### 2. `GeneratePixCommand`

**Descrição**: Gerar QR Code PIX para pagamento.

**Algoritmo**:

```
1. RECEBER orderId, userId
2. BUSCAR Payment pendente
3. VERIFICAR ownership
4. CHAMAR gateway: createPixCharge
5. SALVAR pix_qr_code, pix_qr_code_url
6. DEFINIR pix_expiration_at (+ 30 min)
7. PERSISTIR
8. RETORNAR { qrCode, qrCodeUrl, expiresAt }
```

---

#### 3. `GenerateBoletoCommand`

**Descrição**: Gerar boleto para pagamento.

**Algoritmo**:

```
1. RECEBER orderId, userId
2. BUSCAR Payment pendente
3. VERIFICAR ownership
4. CHAMAR gateway: createBoleto
5. SALVAR boleto_url, boleto_barcode
6. DEFINIR boleto_expiration_at (+ 3 dias úteis)
7. PERSISTIR
8. RETORNAR { boletoUrl, barcode, expiresAt }
```

---

#### 4. `SavePaymentMethodCommand`

**Descrição**: Salvar método de pagamento.

**Algoritmo**:

```
1. RECEBER userId, cardData
2. CHAMAR gateway: tokenize
3. CRIAR UserPaymentMethod com dados tokenizados
4. SE setAsDefault = true:
   4.1. REMOVER isDefault de outros métodos
5. PERSISTIR
6. RETORNAR paymentMethodId
```

---

#### 5. `RemovePaymentMethodCommand`

**Descrição**: Remover método de pagamento salvo.

**Algoritmo**:

```
1. RECEBER paymentMethodId, userId
2. BUSCAR método e verificar ownership
3. CHAMAR gateway: deletePaymentMethod (se aplicável)
4. MARCAR deletedAt = now()
5. SE era default:
   5.1. DEFINIR próximo método como default
6. PERSISTIR
7. RETORNAR sucesso
```

---

### Queries - Customer (Payments)

---

#### 1. `GetUserPaymentMethodsQuery`

**Descrição**: Listar métodos de pagamento salvos.

**Algoritmo**:

```
1. RECEBER userId
2. BUSCAR métodos WHERE userId = X AND deletedAt IS NULL
3. ORDENAR por isDefault DESC, lastUsedAt DESC
4. MAPEAR para List<PaymentMethodDto> (mascarar dados sensíveis)
5. RETORNAR lista
```

---

#### 2. `GetPaymentStatusQuery`

**Descrição**: Buscar status do pagamento.

**Algoritmo**:

```
1. RECEBER orderId, userId
2. BUSCAR payment do order
3. VERIFICAR ownership
4. RETORNAR PaymentStatusDto { status, pixQrCode, boletoUrl, etc }
```

---

# 🛠️ OPERAÇÕES DO ADMIN

## 👤 Módulo Users - Admin

### Commands

---

#### 1. `BlockUserCommand`

**Descrição**: Bloquear usuário.

**Algoritmo**:

```
1. RECEBER userId, reason, duration
2. BUSCAR usuário por ID
3. DEFINIR lockoutEnd = now() + duration
4. REVOGAR todas as sessões ativas
5. ADICIONAR domain event UserLockedEvent
6. ENVIAR notificação ao usuário
7. PERSISTIR
8. RETORNAR sucesso
```

---

#### 2. `UnblockUserCommand`

**Descrição**: Desbloquear usuário.

**Algoritmo**:

```
1. RECEBER userId
2. BUSCAR usuário por ID
3. DEFINIR lockoutEnd = null
4. RESETAR accessFailedCount = 0
5. PERSISTIR
6. RETORNAR sucesso
```

---

#### 3. `AssignRoleCommand`

**Descrição**: Atribuir role a usuário.

**Algoritmo**:

```
1. RECEBER userId, roleName
2. BUSCAR usuário e role
3. VERIFICAR se já possui a role
4. CRIAR UserRole
5. PERSISTIR
6. RETORNAR sucesso
```

---

### Queries - Admin (Users)

---

#### 1. `SearchUsersQuery`

**Descrição**: Buscar usuários com filtros.

**Algoritmo**:

```
1. RECEBER query, role, status, page, pageSize
2. CONSTRUIR query base
3. APLICAR filtros
4. APLICAR paginação
5. RETORNAR PagedResult<UserAdminDto>
```

---

#### 2. `GetUserDetailsQuery`

**Descrição**: Ver detalhes completos de um usuário.

**Algoritmo**:

```
1. RECEBER userId
2. BUSCAR user com JOINs (profile, addresses, roles, orders summary)
3. RETORNAR UserDetailsDto
```

---

## 🛍️ Módulo Catalog - Admin

### Commands

---

#### 1. `CreateProductCommand`

**Descrição**: Criar novo produto.

**Algoritmo**:

```
1. RECEBER productData (name, sku, price, categoryId, etc)
2. VALIDAR SKU único
3. GERAR slug a partir do nome
4. VALIDAR slug único
5. CRIAR entidade Product com status DRAFT
6. ADICIONAR domain event ProductCreatedEvent
7. PERSISTIR
8. INDEXAR no ElasticSearch (se ativo)
9. RETORNAR productId
```

---

#### 2. `UpdateProductCommand`

**Descrição**: Atualizar produto.

**Algoritmo**:

```
1. RECEBER productId, novos dados
2. BUSCAR produto
3. SE SKU alterado → VALIDAR unicidade
4. SE nome alterado → ATUALIZAR slug
5. ATUALIZAR campos
6. ADICIONAR domain event ProductUpdatedEvent
7. PUBLICAR ProductUpdatedIntegrationEvent
8. PERSISTIR
9. ATUALIZAR índice ElasticSearch
10. RETORNAR sucesso
```

**Integration Events**:

- **Publica**: `ProductUpdatedIntegrationEvent` → Cart (atualizar snapshots)

---

#### 3. `UpdateProductPriceCommand`

**Descrição**: Alterar preço do produto.

**Algoritmo**:

```
1. RECEBER productId, newPrice, compareAtPrice
2. BUSCAR produto
3. VALIDAR compareAtPrice > newPrice (se informado)
4. SALVAR preço anterior (para histórico)
5. ATUALIZAR price, compare_at_price
6. ADICIONAR domain event ProductPriceChangedEvent
7. PUBLICAR ProductPriceChangedIntegrationEvent
8. PERSISTIR
9. RETORNAR sucesso
```

**Integration Events**:

- **Publica**: `ProductPriceChangedIntegrationEvent` → Cart (alertar mudança de preço)

---

#### 4. `PublishProductCommand`

**Descrição**: Publicar produto (tornar ativo).

**Algoritmo**:

```
1. RECEBER productId
2. BUSCAR produto
3. VALIDAR requisitos mínimos:
   3.1. Tem imagem principal?
   3.2. Tem preço > 0?
   3.3. Tem descrição?
4. ATUALIZAR status para ACTIVE
5. DEFINIR publishedAt = now()
6. ADICIONAR domain event ProductPublishedEvent
7. INDEXAR no ElasticSearch
8. PERSISTIR
9. RETORNAR sucesso
```

---

#### 5. `DeactivateProductCommand`

**Descrição**: Desativar produto.

**Algoritmo**:

```
1. RECEBER productId
2. BUSCAR produto
3. ATUALIZAR status para INACTIVE
4. ADICIONAR domain event ProductDeactivatedEvent
5. PUBLICAR ProductDeactivatedIntegrationEvent
6. REMOVER do índice ElasticSearch
7. PERSISTIR
8. RETORNAR sucesso
```

**Integration Events**:

- **Publica**: `ProductDeactivatedIntegrationEvent` → Cart (remover de carrinhos)

---

#### 6. `CreateCategoryCommand`✅

**Descrição**: Criar categoria.

**Algoritmo**:

```
1. RECEBER name, parentId (opcional), description
2. GERAR slug
3. VALIDAR slug único
4. SE parentId:
   4.1. BUSCAR categoria pai
   4.2. DEFINIR depth = parent.depth + 1
   4.3. DEFINIR path = parent.path + '/' + slug
5. CRIAR entidade Category
6. PERSISTIR
7. RETORNAR categoryId
```

---

#### 7. `UpdateCategoryCommand`

**Descrição**: Atualizar categoria.

**Algoritmo**:

```
1. RECEBER categoryId, novos dados
2. BUSCAR categoria
3. SE nome alterado → ATUALIZAR slug
4. SE parentId alterado:
   4.1. VALIDAR que não é filho de si mesmo
   4.2. ATUALIZAR depth e path
   4.3. ATUALIZAR path de todas subcategorias
5. PERSISTIR
6. RETORNAR sucesso
```

---

#### 8. `AdjustStockCommand`

**Descrição**: Ajustar estoque manualmente.

**Algoritmo**:

```
1. RECEBER productId, quantity (pode ser negativo), reason
2. BUSCAR produto
3. CALCULAR novo estoque = stock + quantity
   3.1. SE < 0 → RETORNAR erro
4. CRIAR StockMovement (ADJUSTMENT)
5. ATUALIZAR product.stock
6. SE stock <= low_stock_threshold:
   6.1. ADICIONAR domain event LowStockAlertEvent
7. PERSISTIR
8. RETORNAR sucesso
```

---

#### 9. `ApproveReviewCommand`

**Descrição**: Aprovar avaliação de produto.

**Algoritmo**:

```
1. RECEBER reviewId
2. BUSCAR review
3. MARCAR isApproved = true
4. ATUALIZAR materialized view de stats
5. PERSISTIR
6. RETORNAR sucesso
```

---

#### 10. `DeleteReviewCommand`

**Descrição**: Remover avaliação (soft delete).

**Algoritmo**:

```
1. RECEBER reviewId
2. BUSCAR review
3. MARCAR deletedAt = now()
4. ATUALIZAR materialized view de stats
5. PERSISTIR
6. RETORNAR sucesso
```

---

### Queries - Admin (Catalog)

---

#### 1. `GetProductsAdminQuery`

**Descrição**: Listar produtos (incluindo inativos).

**Algoritmo**:

```
1. RECEBER filters, page, pageSize
2. BUSCAR produtos (todos status)
3. APLICAR filtros
4. RETORNAR PagedResult<ProductAdminDto>
```

---

#### 2. `GetLowStockProductsQuery`

**Descrição**: Listar produtos com estoque baixo.

**Algoritmo**:

```
1. BUSCAR produtos WHERE stock <= low_stock_threshold AND status = ACTIVE
2. ORDENAR por stock ASC
3. RETORNAR List<LowStockProductDto>
```

---

#### 3. `GetPendingReviewsQuery`

**Descrição**: Listar avaliações pendentes de aprovação.

**Algoritmo**:

```
1. BUSCAR reviews WHERE isApproved = false AND deletedAt IS NULL
2. ORDENAR por createdAt ASC
3. RETORNAR List<PendingReviewDto>
```

---

## 📦 Módulo Orders - Admin

### Commands

---

#### 1. `PrepareOrderCommand`

**Descrição**: Marcar pedido como em preparação.

**Algoritmo**:

```
1. RECEBER orderId
2. BUSCAR order
3. VERIFICAR status = PAID
4. ATUALIZAR status para PREPARING
5. ADICIONAR domain event OrderPreparingEvent
6. PERSISTIR
7. RETORNAR sucesso
```

---

#### 2. `ShipOrderCommand`

**Descrição**: Registrar envio do pedido.

**Algoritmo**:

```
1. RECEBER orderId, trackingCode, shippingCarrier, estimatedDeliveryAt
2. BUSCAR order
3. VERIFICAR status = PREPARING
4. ATUALIZAR status para SHIPPED
5. DEFINIR tracking_code, shipping_carrier, shipped_at
6. ADICIONAR domain event OrderShippedEvent
7. PUBLICAR OrderShippedIntegrationEvent → Users (notificação)
8. PERSISTIR
9. RETORNAR sucesso
```

---

#### 3. `MarkDeliveredCommand`

**Descrição**: Confirmar entrega do pedido.

**Algoritmo**:

```
1. RECEBER orderId
2. BUSCAR order
3. VERIFICAR status IN (SHIPPED, OUT_FOR_DELIVERY)
4. ATUALIZAR status para DELIVERED
5. DEFINIR delivered_at = now()
6. ADICIONAR domain event OrderDeliveredEvent
7. CONSULTAR Catalog: ConfirmStockCommand (converter reserva em venda)
8. PERSISTIR
9. RETORNAR sucesso
```

---

#### 4. `AdminCancelOrderCommand`

**Descrição**: Cancelar pedido (admin pode em qualquer status).

**Algoritmo**:

```
1. RECEBER orderId, reason, notes
2. BUSCAR order
3. VERIFICAR status != DELIVERED (já entregue não pode cancelar)
4. ATUALIZAR status para CANCELLED
5. DEFINIR cancellation_reason, cancellation_notes, cancelled_by
6. PUBLICAR OrderCancelledIntegrationEvent
7. PERSISTIR
8. RETORNAR sucesso
```

---

#### 5. `ApproveRefundCommand`

**Descrição**: Aprovar solicitação de reembolso.

**Algoritmo**:

```
1. RECEBER refundId
2. BUSCAR refund
3. ATUALIZAR status para APPROVED
4. PUBLICAR RefundApprovedIntegrationEvent → Payments
5. PERSISTIR
6. RETORNAR sucesso
```

---

### Queries - Admin (Orders)

---

#### 1. `SearchOrdersQuery`

**Descrição**: Buscar pedidos com filtros avançados.

**Algoritmo**:

```
1. RECEBER filters (status, dateRange, userId, orderNumber)
2. CONSTRUIR query dinâmica
3. APLICAR paginação
4. RETORNAR PagedResult<OrderAdminDto>
```

---

#### 2. `GetPendingOrdersQuery`

**Descrição**: Listar pedidos pendentes de ação.

**Algoritmo**:

```
1. USAR view orders.v_orders_pending_action
2. FILTRAR por alert_status != 'OK'
3. RETORNAR List<PendingActionOrderDto>
```

---

#### 3. `GetOrderMetricsQuery`

**Descrição**: Métricas de pedidos por período.

**Algoritmo**:

```
1. RECEBER startDate, endDate
2. CALCULAR totais por status
3. CALCULAR receita total
4. CALCULAR ticket médio
5. RETORNAR OrderMetricsDto
```

---

## 💳 Módulo Payments - Admin

### Commands

---

#### 1. `ProcessRefundCommand`

**Descrição**: Processar reembolso aprovado.

**Algoritmo**:

```
1. RECEBER refundId
2. BUSCAR refund
3. BUSCAR payment original
4. CHAMAR gateway: refund
5. SE sucesso:
   5.1. ATUALIZAR status para REFUNDED
   5.2. PUBLICAR RefundCompletedIntegrationEvent
6. SE falha:
   6.1. REGISTRAR erro
7. PERSISTIR
8. RETORNAR sucesso
```

---

#### 2. `RespondChargebackCommand`

**Descrição**: Responder a chargeback com evidências.

**Algoritmo**:

```
1. RECEBER chargebackId, evidences
2. BUSCAR chargeback
3. CHAMAR gateway: submitChargebackEvidence
4. ATUALIZAR evidence_submitted = true
5. PERSISTIR
6. RETORNAR sucesso
```

---

### Queries - Admin (Payments)

---

#### 1. `GetPaymentMetricsQuery`

**Descrição**: Métricas de pagamentos.

**Algoritmo**:

```
1. USAR view payments.v_payment_metrics
2. FILTRAR por período
3. AGRUPAR por método de pagamento
4. RETORNAR PaymentMetricsDto
```

---

#### 2. `GetPendingChargebacksQuery`

**Descrição**: Listar chargebacks pendentes.

**Algoritmo**:

```
1. BUSCAR chargebacks WHERE status = 'OPEN'
2. ORDENAR por evidence_due_at ASC
3. RETORNAR List<ChargebackDto>
```

---

## 🎟️ Módulo Coupons - Admin

### Commands

---

#### 1. `CreateCouponCommand`

**Descrição**: Criar cupom de desconto.

**Algoritmo**:

```
1. RECEBER couponData
2. VALIDAR código único (case-insensitive)
3. VALIDAR validUntil > validFrom
4. SE type = PERCENTAGE → VALIDAR value <= 100
5. SE type = BUY_X_GET_Y → VALIDAR buyQuantity e getQuantity
6. CRIAR entidade Coupon com status DRAFT
7. SE scope = CATEGORIES → CRIAR eligible_categories
8. SE scope = PRODUCTS → CRIAR eligible_products
9. SE scope = SPECIFIC_USERS → CRIAR eligible_users
10. ADICIONAR domain event CouponCreatedEvent
11. PERSISTIR
12. RETORNAR couponId
```

---

#### 2. `UpdateCouponCommand`

**Descrição**: Atualizar cupom.

**Algoritmo**:

```
1. RECEBER couponId, novos dados
2. BUSCAR cupom
3. VERIFICAR status permite edição (DRAFT, SCHEDULED)
4. ATUALIZAR campos
5. ATUALIZAR eligible_* se necessário
6. PERSISTIR
7. RETORNAR sucesso
```

---

#### 3. `ActivateCouponCommand`

**Descrição**: Ativar cupom.

**Algoritmo**:

```
1. RECEBER couponId
2. BUSCAR cupom
3. VERIFICAR status = DRAFT ou SCHEDULED
4. ATUALIZAR status para ACTIVE
5. ADICIONAR domain event CouponActivatedEvent
6. PERSISTIR
7. RETORNAR sucesso
```

---

#### 4. `DeactivateCouponCommand`

**Descrição**: Desativar cupom.

**Algoritmo**:

```
1. RECEBER couponId
2. BUSCAR cupom
3. ATUALIZAR status para PAUSED
4. PUBLICAR CouponDeactivatedIntegrationEvent → Cart
5. PERSISTIR
6. RETORNAR sucesso
```

---

### Queries - Admin (Coupons)

---

#### 1. `GetCouponsQuery`

**Descrição**: Listar todos os cupons.

**Algoritmo**:

```
1. RECEBER filters (status, type)
2. BUSCAR cupons
3. APLICAR filtros
4. RETORNAR PagedResult<CouponAdminDto>
```

---

#### 2. `GetCouponMetricsQuery`

**Descrição**: Métricas de uso do cupom.

**Algoritmo**:

```
1. RECEBER couponId
2. USAR view coupons.v_coupon_metrics
3. RETORNAR CouponMetricsDto
```

---

# 🔄 EVENTS E HANDLERS

## Domain Events (Internos)

| Módulo  | Event                  | Handler                      | Ação                                    |
| ------- | ---------------------- | ---------------------------- | --------------------------------------- |
| Users   | `UserCreatedEvent`     | `UserCreatedEventHandler`    | Cria Profile, dispara integration event |
| Users   | `SessionCreatedEvent`  | `SessionCreatedEventHandler` | Registra LoginHistory                   |
| Users   | `UserLockedEvent`      | `UserLockedEventHandler`     | Envia notificação de segurança          |
| Catalog | `ProductCreatedEvent`  | `ProductCreatedEventHandler` | Indexa no ElasticSearch                 |
| Catalog | `StockReservedEvent`   | `StockReservedEventHandler`  | Verifica alerta de estoque baixo        |
| Catalog | `LowStockAlertEvent`   | `LowStockAlertEventHandler`  | Notifica administradores                |
| Cart    | `ItemAddedToCartEvent` | `CartActivityEventHandler`   | Registra no ActivityLog                 |
| Orders  | `OrderCreatedEvent`    | `OrderCreatedEventHandler`   | Registra histórico de status            |

## Integration Events (Entre Módulos)

| Event                                 | Publicado Por | Consumido Por              | Handler                                      | Ação                                           |
| ------------------------------------- | ------------- | -------------------------- | -------------------------------------------- | ---------------------------------------------- |
| `UserCreatedIntegrationEvent`         | Users         | Cart                       | `UserCreatedIntegrationEventHandler`         | Cria carrinho vazio                            |
| `ProductPriceChangedIntegrationEvent` | Catalog       | Cart                       | `ProductPriceChangedIntegrationEventHandler` | Atualiza currentPrice em itens                 |
| `ProductDeactivatedIntegrationEvent`  | Catalog       | Cart                       | `ProductDeactivatedIntegrationEventHandler`  | Remove item de carrinhos                       |
| `CartCheckoutStartedIntegrationEvent` | Cart          | Orders                     | `CartCheckoutStartedIntegrationEventHandler` | Inicia criação do pedido                       |
| `OrderCreatedIntegrationEvent`        | Orders        | Payments, Cart, Coupons    | -                                            | Criar Payment, converter cart, confirmar cupom |
| `OrderCancelledIntegrationEvent`      | Orders        | Payments, Catalog, Coupons | -                                            | Estornar, liberar estoque, reverter cupom      |
| `PaymentCapturedIntegrationEvent`     | Payments      | Orders                     | `PaymentCapturedIntegrationEventHandler`     | Atualiza status para PAID                      |
| `PaymentFailedIntegrationEvent`       | Payments      | Orders                     | `PaymentFailedIntegrationEventHandler`       | Atualiza status para FAILED                    |
| `RefundCompletedIntegrationEvent`     | Payments      | Orders                     | `RefundCompletedIntegrationEventHandler`     | Atualiza status para REFUNDED                  |

---

**Última atualização**: 2025-12-13  
**Versão**: 2.0
